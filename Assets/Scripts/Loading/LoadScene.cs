/* Import the necessary system libraries. */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/* Import the necessary Unity libraries. */
using UnityEngine;
using UnityEngine.Networking;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;

class LoadScene : MonoBehaviour {
    /* Manage the socket.io communication with the server */
    public bool sceneLoaded = false;
    private SQLiteConnectionManager dbManager;
    public bool syncDB = false;
    private SocketIOUnity ws;

    void Update() {
        if (Parameters.LOADED_DATA == true && this.sceneLoaded == false) {
            this.dbManager = new SQLiteConnectionManager();
            socketManagement();
            this.syncDB = true;
        }
        if(syncDB) {
            SyncLocalDatabase();
            this.syncDB = false;
            if (!this.sceneLoaded)
                this.sceneLoaded = true;
        }
    }

    private void compareDataAndDownload(List<Model> modelsFromServer, List<Model> modelsSavedLocally) {
        /* Syncronize the local database with the local storage (verify that no models has been removed from the local storage) */
        for(int i = 0; i != modelsSavedLocally.Count ; i++) {
            for (int j = 0; j != modelsSavedLocally[i].textures.Count ; j++) {
                if (!File.Exists(modelsSavedLocally[i].textures[j].getFileProperties())) {
                    delete(modelsSavedLocally[i].textures[j]);
                    modelsSavedLocally[i].textures.RemoveAt(j);
                    j--;
                }
            }
            if (!File.Exists(modelsSavedLocally[i].getFileProperties())) {
                delete(modelsSavedLocally[i]);
                modelsSavedLocally.RemoveAt(i);
                i--;
            }
        }


        /* Verify if there are new models to download */
        foreach (Model modelFromServer in modelsFromServer) {
            bool modelFound = false;
            foreach (Model modelSavedLocally in modelsSavedLocally) {
                if (modelFromServer.IDModel == modelSavedLocally.IDModel) {
                    modelFound = true;
                    break;
                }
            }

            if (!modelFound)
                save(modelFromServer);
        }

        /* Verify if there are models to remove */
        foreach (Model modelSavedLocally in modelsSavedLocally) {
            bool modelFound = false;
            foreach (Model modelFromServer in modelsFromServer) {
                if (modelFromServer.IDModel == modelSavedLocally.IDModel) {
                    modelFound = true;
                    break;
                }
            }
            if (!modelFound)
                delete(modelSavedLocally);
        }

        /* Verify if there are new textures to download */
        foreach (Model modelFromServer in modelsFromServer) {
            foreach (Model modelSavedLocally in modelsSavedLocally) {
                if (modelFromServer.IDModel == modelSavedLocally.IDModel) {
                    foreach (ModelTexture textureFromServer in modelFromServer.textures) {
                        bool textureFound = false;
                        foreach (ModelTexture textureSavedLocally in modelSavedLocally.textures) {
                            if (textureFromServer.IDTexture == textureSavedLocally.IDTexture) {
                                textureFound = true;
                                break;
                            }
                        }

                        if (!textureFound) {
                            /* Download the texture */
                            save(textureFromServer);
                        }
                    }
                }
            }
        }

        /* Verify if there are textures to remove */
        foreach (Model modelSavedLocally in modelsSavedLocally) {
            foreach (Model modelFromServer in modelsFromServer) {
                if (modelFromServer.IDModel == modelSavedLocally.IDModel) {
                    foreach (ModelTexture textureSavedLocally in modelSavedLocally.textures) {
                        bool textureFound = false;
                        foreach (ModelTexture textureFromServer in modelFromServer.textures) {
                            if (textureFromServer.IDTexture == textureSavedLocally.IDTexture) {
                                textureFound = true;
                                break;
                            }
                        }

                        if (!textureFound)
                            delete(textureSavedLocally);
                    }
                }
            }
        }
    }

    private List<Model> convertModelListJSONToList(string str) {
        int startingPosition = 0;
        int numberOfBrackets = 0;
        List<Model> modelsSavedIntoServer = new List<Model>();

        /* Get the models from the JSON string */
        for (int i = 1; i < str.Length-1; i++) {
            if (str[i] == '{') {
                numberOfBrackets++;
                if (numberOfBrackets == 1) {
                    startingPosition = i;
                }
            } else if (str[i] == '}') {
                numberOfBrackets--;
            }

            if (numberOfBrackets == 0 && startingPosition != 0) {
                string modelString = str.Substring(startingPosition, i - startingPosition + 1);
                modelsSavedIntoServer.Add(JsonUtility.FromJson<Model>(modelString));
                startingPosition = 0;
            }
        }

        /* Return the list of models saved into the server */
        return modelsSavedIntoServer;
    }

    private IEnumerator _SyncLocalDatabase() {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(Parameters.INFO_LIST_ENDPONT)) {
            /* Request and wait for the desired page. */
            yield return webRequest.SendWebRequest();

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    List<Model> modelsFromServer = this.convertModelListJSONToList(webRequest.downloadHandler.text);
                    List<Model> modelsSavedLocally = this.dbManager.getModelsList();
                    this.compareDataAndDownload(modelsFromServer, modelsSavedLocally);
                    break;
            }
        }
    }

    private void SyncLocalDatabase() {
        StartCoroutine(_SyncLocalDatabase());
    }

    private IEnumerator _save(Model modelToSave) {
        /* Create all the needed directories */
        if(Directory.Exists(modelToSave.getDirectory()))
            Directory.Delete(modelToSave.getDirectory(), true);
        Directory.CreateDirectory(modelToSave.getDirectory());

        /* Download the model */
        var webRequest = new UnityWebRequest(modelToSave.getDownloadUrl(), UnityWebRequest.kHttpVerbGET);
        webRequest.downloadHandler = new DownloadHandlerFile(modelToSave.getFileProperties());
        yield return webRequest.SendWebRequest();

        /* Handle the result */
        if (webRequest.result == UnityWebRequest.Result.Success) {
            this.dbManager.saveModelIntoDatabase(modelToSave);
            foreach (ModelTexture textureToDownload in modelToSave.textures)
                save(textureToDownload);
        }
    }

    private void save(Model model) {
        StartCoroutine(_save(model));
    }

    private IEnumerator _save(ModelTexture texture) {
        Debug.Log(texture.getDownloadUrl());
        /* Download the texture */
        var webRequest = new UnityWebRequest(texture.getDownloadUrl(), UnityWebRequest.kHttpVerbGET);
        webRequest.downloadHandler = new DownloadHandlerFile(texture.getFileProperties());
        yield return webRequest.SendWebRequest();

        /* Handle the result */
        if (webRequest.result == UnityWebRequest.Result.Success)
            this.dbManager.saveTextureIntoDatabase(texture);
    }

    private void save(ModelTexture textureToDownload) {
        StartCoroutine(_save(textureToDownload));
    }

    /* Delete function */
    private void delete(Model model) {        
        /* Delete the textures from the database */
        foreach(ModelTexture texture in model.textures)
            delete(texture);
        
        /* Delete the model from the database */
        dbManager.deleteModelFromDatabase(model);

        /* Delete all the material saved locally */
        if(Directory.Exists(model.getDirectory()))
            Directory.Delete(model.getDirectory(), true);
    }

    private void delete(ModelTexture textureToDownload) {
        /* Delete the texture from the database */
        dbManager.deleteTextureFromDatabase(textureToDownload);

        /* Delete the texture from the storage */
        if(File.Exists(textureToDownload.getFileProperties())) {
            /* Delete the file */
            File.Delete(textureToDownload.getFileProperties());
        }
    }

    private void loadModelInScene() {

    }

    private void updateModelInScenePosition() {

    }

    private void setActiveModel(string IDModel, string IDTexture) { 
        Debug.Log("Set active model"); 
        Debug.Log(IDModel);
        Debug.Log(IDTexture);
    }

    private void socketManagement() {
        /* Function needed to manage the json messages from the server */
        string[] prepareJSON(string json) {
            /* Remove some characters that are not useful */
            json = json.Replace("[{", "");
            json = json.Replace("}]", "");
            json = json.Replace("\"", "");

            /* Split and manage the string */
            string[] jsonList = json.Split(new string[] { "," }, StringSplitOptions.None);
            for(int i = 0; i < jsonList.Length; i++)
                jsonList[i] = jsonList[i].Split(new string[] { ":" }, StringSplitOptions.None)[1];
            
            /* Return the result */
            return jsonList;
        }

        /* Start connection */
        ws = new SocketIOUnity(new Uri("http://virtualenv.epfl.ch"), new SocketIOOptions { 
            Path=Parameters.WS_URL,
            EIO = 4, 
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket 
        });
        ws.JsonSerializer = new NewtonsoftJsonSerializer(); // Needed (know bug of the socketio library)
        ws.Connect();
        
        /* Listen to events */
        ws.On("set-active-model", (response) => {
            string[] informations = prepareJSON(response.ToString());
            if(informations.Length != 2) {
                /* Verify that informations has length 2 */
                Debug.LogError("Data from the server not valid.");
                return;
            }
            setActiveModel(informations[0], informations[1]);
        });
        ws.On("new-model", (response) => {
            this.syncDB = true;
        });
        ws.On("update-model", (response) => {
            this.syncDB = true;
        });
        ws.On("delete-model", (response) => {
            this.syncDB = true;
        });
        ws.On("new-texture", (response) => {
            this.syncDB = true;
        });
        ws.On("delete-texture", (response) => {
            this.syncDB = true;
        });
    }
}
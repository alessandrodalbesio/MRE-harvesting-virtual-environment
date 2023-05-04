/* Import the necessary system libraries. */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/* Import the necessary Unity libraries. */
using UnityEngine;
using UnityEngine.Networking;
using SocketIOClient;

class LoadScene : MonoBehaviour {
    /* Manage the socket.io communication with the server */
    private SocketIOUnity socket;
    public bool sceneLoaded = false;
    private SQLiteConnectionManager dbManager;

    void Start() {
        if (Parameters.LOADED_DATA == true && this.sceneLoaded == false) {
            /* Create the connection to the database */
            this.dbManager = new SQLiteConnectionManager();

            /* Syncronize the local database with the remote database */
            //SyncLocalDatabase();
            StartCoroutine(_SyncLocalDatabase());

            /* Generate the socket connection */
            //generateSocket();
            
            /* Set the scene as loaded */
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

            if (!modelFound) {
                Debug.Log("Active");
                StartCoroutine(_save(modelFromServer));
                /* Download the model */
            }
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

    private void save(Model model) {
        StartCoroutine(_save(model));
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

    private void save(ModelTexture textureToDownload) {
        StartCoroutine(_save(textureToDownload));
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

    private void getNewModel() {  }

    private void updateModel() {  }

    private void deleteModel() {  }

    private void getNewTexture() {  }

    private void deleteTexture() {  }

    
    private void generateSocket() {
        var uri = new Uri("http://virtualenv.epfl.ch");
        socket = new SocketIOUnity(uri, new SocketIOOptions { Path="/ws",  EIO = 4, Transport = SocketIOClient.Transport.TransportProtocol.WebSocket });
        socket.OnConnected += (sender, e) => {
            Debug.Log("Connected");
        };
        socket.Connect();
        socket.On("set-active-model", (response) => {
            Debug.Log("set-active-model");
        });
        socket.On("new-model", (response) => {
            Debug.Log("new-model");
        });
        socket.On("update-model", (response) => {
            Debug.Log("update-model");
        });
    }
}
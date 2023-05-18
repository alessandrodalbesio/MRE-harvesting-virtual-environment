﻿/* Import the necessary libraries */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Networking;

class Synchronizer : MonoBehaviour {
    /* Storage manager */
    private StorageManager storageManager;
    
    /* Reference to scene loader */
    public GameObject sceneLoader;

    /* Pooling variables */
    private long poolAtMS = 0;

    private bool isBusy = false;
    private bool IsBusy {
        get { return this.isBusy; }
    }

    /* Methods */
    void Start() {
        this.storageManager = new StorageManager();
        this.SyncLocalDatabase();
    }

    void Update() {
        poolServer();
    }
    
    private void compareDataAndDownload(List<Model> modelsFromServer, List<Model> modelsSavedLocally) {
        /* Syncronize the local database with the local storage (verify that no models has been removed from the local storage) */
        for(int i = 0; i != modelsSavedLocally.Count ; i++) {
            for (int j = 0; j != modelsSavedLocally[i].textures.Count ; j++) {
                if (!File.Exists(modelsSavedLocally[i].textures[j].getFile())) {
                    delete(modelsSavedLocally[i].textures[j]);
                    modelsSavedLocally[i].textures.RemoveAt(j);
                    j--;
                }
            }
            if (!File.Exists(modelsSavedLocally[i].getFile())) {
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

                        if (!textureFound) 
                            save(textureFromServer);
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
                Debug.Log(modelString);
                modelsSavedIntoServer.Add(JsonUtility.FromJson<Model>(modelString));
                startingPosition = 0;
            }
        }

        /* Display the ID of all the textures */
        foreach (Model model in modelsSavedIntoServer) {
            Debug.Log("Model ID" + model.IDModel);
            Debug.Log(model.textures[0].IDTexture);
            foreach (ModelTexture texture in model.textures) {
                Debug.Log("Texture ID" + texture);
            }
        }

        /* Return the list of models saved into the server */
        return modelsSavedIntoServer;
    }

    private IEnumerator _SyncLocalDatabase() {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(Parameters.MODELS_INFO_ENDPOINT)) {
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
                    List<Model> modelsSavedLocally = this.storageManager.getModels();
                    this.compareDataAndDownload(modelsFromServer, modelsSavedLocally);
                    break;
            }
        }
    }

    private void SyncLocalDatabase() {
        this.isBusy = true;
        StartCoroutine(_SyncLocalDatabase());
        this.isBusy = false;
    }

    private IEnumerator _save(Model modelToSave) {
        /* Create all the needed directories */
        if(Directory.Exists(modelToSave.getParentDirectory()))
            Directory.Delete(modelToSave.getParentDirectory(), true);
        Directory.CreateDirectory(modelToSave.getParentDirectory());

        /* Download the model */
        var webRequest = new UnityWebRequest(modelToSave.getDownloadUrl(), UnityWebRequest.kHttpVerbGET);
        webRequest.downloadHandler = new DownloadHandlerFile(modelToSave.getFile());
        yield return webRequest.SendWebRequest();

        /* Handle the result */
        if (webRequest.result == UnityWebRequest.Result.Success) {
            this.storageManager.saveModel(modelToSave);
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
        webRequest.downloadHandler = new DownloadHandlerFile(texture.getFile());
        yield return webRequest.SendWebRequest();

        /* Handle the result */
        if (webRequest.result == UnityWebRequest.Result.Success)
            this.storageManager.saveTexture(texture);
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
        storageManager.deleteModel(model);

        /* Delete all the material saved locally */
        if(Directory.Exists(model.getParentDirectory()))
            Directory.Delete(model.getParentDirectory(), true);
    }

    private void delete(ModelTexture textureToDownload) {
        /* Delete the texture from the database */
        storageManager.deleteTexture(textureToDownload);

        /* Delete the texture from the storage */
        if(File.Exists(textureToDownload.getFile())) {
            /* Delete the file */
            File.Delete(textureToDownload.getFile());
        }
    }

    /* Pooling to verify if there are new updates */
    private IEnumerator _checkForNewUpdate() {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(Parameters.WS_API_NEED_LOCAL_STORAGE_REFRESH_ENDPOINT)) {
            /* Request and wait for the desired page. */
            yield return webRequest.SendWebRequest();

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    bool newContents = webRequest.downloadHandler.text.Split(":")[1].Replace("}", "").Replace("\n", "") == "true";
                    if (newContents)
                        this.SyncLocalDatabase();
                    break;
            }
        }
    }

    private void setActiveModelFromServerResponse(string serverResponse) {
        serverResponse = serverResponse.Replace("\"", "").Replace("{", "").Replace("}", "").Replace("\n", "");
        string IDModel = serverResponse.Split(",")[0].Split(":")[1];
        string IDTexture = serverResponse.Split(",")[1].Split(":")[1];        
        sceneLoader.GetComponent<SceneLoader>().setActiveObjectInScene(IDModel == "null" ? null : IDModel, IDTexture == "null" ? null : IDTexture);
    }

    /* Pooling to verify if there is an active model */
    private IEnumerator _checkForActiveModel() {
        Debug.Log(Parameters.WS_API_GET_ACTIVE_MODEL_ENPOINT);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(Parameters.WS_API_GET_ACTIVE_MODEL_ENPOINT)) {
            /* Request and wait for the desired page. */
            yield return webRequest.SendWebRequest();

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    this.setActiveModelFromServerResponse(webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    private void poolServer() {
        long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (currentTime > poolAtMS) {
            poolAtMS = currentTime + Parameters.TIME_BETWEEN_POLLING_MS; /* Wait at least 5 seconds before checking again to avoid overload on the server */
            StartCoroutine(_checkForActiveModel());
            StartCoroutine(_checkForNewUpdate());
        }
    }
}
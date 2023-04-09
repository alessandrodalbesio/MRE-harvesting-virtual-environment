/* Import the necessary system libraries. */
using System.Collections;
using System.Collections.Generic;
using System.IO;

/* Import the necessary Unity libraries. */
using UnityEngine;
using UnityEngine.Networking;


/* Define the class for loading the models from the server. */
class filesLoader : MonoBehaviour
{
    /* Define the connection manager for the database. */
    private SQLiteConnectionManager dbManager;


    /* Define the containers for the error and loading text. */
    public GameObject errorContainer, loadingContainer, loadingLogContainer;


    /* Starting method */
    void Start() {
        /* Initialize the database manager. */
        this.dbManager = new SQLiteConnectionManager();

        /* Get the data from the server */
        StartCoroutine(this.GetDataFromServer());
    }


    /* Display an error if something goes wrong */
    public void displayErrorText(string errorText) {
        errorContainer.SetActive(true);
        loadingContainer.SetActive(false);
        if(loadingLogContainer != null) {
            loadingLogContainer.SetActive(false);
        }
    }


    /* Convert the JSON string from the server to a list of models */
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



    /* Syncronize the data from the server with the local data */
    private void compareDataAndDownload(List<Model> modelsFromServer, List<Model> modelsSavedLocally) {
        /* Syncronize the local database with the local storage */
        for(int i = 0; ; i++) {
            if(i == modelsSavedLocally.Count)
                break;
            for (int j = 0; ; j++) {
                if (j == modelsSavedLocally[i].textures.Count)
                    break;
                if (!File.Exists(modelsSavedLocally[i].textures[j].getFileDirectoryAndName())) {
                    modelsSavedLocally[i].textures[j].deleteTexture();
                    modelsSavedLocally[i].textures.RemoveAt(j);
                    j--;
                }
            }
            if (!File.Exists(modelsSavedLocally[i].getFileDirectoryAndName())) {
                modelsSavedLocally[i].deleteModel();
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
                /* Download the model */
                StartCoroutine(this.DownloadAndSave(modelFromServer));
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
                modelSavedLocally.deleteModel();
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
                            StartCoroutine(this.DownloadAndSave(textureFromServer));
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
                            textureSavedLocally.deleteTexture();
                    }
                }
            }
        }
    }


    IEnumerator GetDataFromServer()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(Parameters.GET_MODELS_LIST_ENDPOINT))
        {
            /* Request and wait for the desired page. */
            yield return webRequest.SendWebRequest();

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    displayErrorText("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    List<Model> modelsFromServer = this.convertModelListJSONToList(webRequest.downloadHandler.text);
                    List<Model> modelsSavedLocally = this.dbManager.getModelsList();
                    this.compareDataAndDownload(modelsFromServer, modelsSavedLocally);
                    break;
            }
        }
    }


    IEnumerator DownloadAndSave(Model modelToDownload) {
        /* Create all the needed directories */
        if (!Directory.Exists(Path.GetDirectoryName(modelToDownload.getDirectory())))
            Directory.CreateDirectory(Path.GetDirectoryName(modelToDownload.getDirectory()));
        if (!Directory.Exists(Path.GetDirectoryName(modelToDownload.getTexturesDirectory())))
            Directory.CreateDirectory(Path.GetDirectoryName(modelToDownload.getTexturesDirectory()));
        

        /* Download the model */
        var webRequest = new UnityWebRequest(modelToDownload.getDownloadUrl(), UnityWebRequest.kHttpVerbGET);
        webRequest.downloadHandler = new DownloadHandlerFile(modelToDownload.getFileDirectoryAndName());
        yield return webRequest.SendWebRequest();


        /* Handle the result */
        if (webRequest.result != UnityWebRequest.Result.Success) {
            /* Display the error informations */
            Debug.LogError(webRequest.error);

            /* Delete all the created directories */
            if (Directory.Exists(Path.GetDirectoryName(modelToDownload.getDirectory())))
                Directory.Delete(Path.GetDirectoryName(modelToDownload.getDirectory()), true);
            if (Directory.Exists(Path.GetDirectoryName(modelToDownload.getTexturesDirectory())))
                Directory.Delete(Path.GetDirectoryName(modelToDownload.getTexturesDirectory()), true);
        }
        else {
            this.dbManager.saveModelIntoDatabase(modelToDownload);
            foreach (ModelTexture textureToDownload in modelToDownload.textures) {
                /* Download the texture */
                StartCoroutine(this.DownloadAndSave(textureToDownload));
            }
        }
    }

    IEnumerator DownloadAndSave(ModelTexture textureToDownload) {
        /* Download the texture */
        var webRequest = new UnityWebRequest(textureToDownload.getDownloadUrl(), UnityWebRequest.kHttpVerbGET);
        webRequest.downloadHandler = new DownloadHandlerFile(textureToDownload.getFileDirectoryAndName());
        yield return webRequest.SendWebRequest();

        /* Handle the result */
        if (webRequest.result != UnityWebRequest.Result.Success) {
            /* Display the error informations */
            Debug.Log(webRequest.error);
        }
        else 
            this.dbManager.saveTextureIntoDatabase(textureToDownload);
    }
}

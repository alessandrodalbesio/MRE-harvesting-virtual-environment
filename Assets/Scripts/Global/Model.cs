/* Import the necessary system libraries. */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/* Import the necessary Unity libraries. */
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
class Model {
    public string IDModel;
    public string modelName;
    public string modelExtension;
    public List<ModelTexture> textures;

    /* Model informations */
    public string getDownloadUrl() {
        return Parameters.SERVER_MODELS_PATH + this.IDModel + "/model." + this.modelExtension;
    }

    private string getLocalPath() {
        return Application.dataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/";
    }

    public string getFileProperties(bool withExtension = true) {
        if(withExtension)
            return Application.dataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/model." + this.modelExtension;
        else
            return Application.dataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/model";
    }

    public string getDirectory() {
        return Application.dataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel;
    }

    public string getFileName(bool withExtension = true) {
        if(withExtension)
            return "model." + this.modelExtension;
        else
            return "model";
    }

    public string getTexturesDirectory() {
        /* Get the directory where the textures are saved */
        return Application.dataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/textures/";
    }

}
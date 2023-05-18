/* Import the necessary system libraries. */
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
class Model {
    public string ID;
    public string name;
    public string extension;
    public List<ModelTexture> textures;

    /* Model informations */
    public string getDownloadUrl() {
        return Parameters.DOWLOAD_ENDPOINT + this.ID + "/" + Parameters.DEFAULT_MODEL_NAME + "." + this.extension;
    }

    public string getFile() {
        return Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + this.ID + "/" + Parameters.DEFAULT_MODEL_NAME + "." + this.extension;
    }

    public string getParentDirectory() {
        return Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + this.ID;
    }

    public string getFileName() {
        return Parameters.DEFAULT_MODEL_NAME + "." + this.extension;
    }

    public string getTexturesParentDirectory() {
        return Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + this.ID + "/textures/";
    }

}
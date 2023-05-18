/* Import the necessary system libraries. */
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
class Model {
    public string IDModel;
    public string modelName;
    public string modelExtension;
    public List<ModelTexture> textures;

    /* Model informations */
    public string getDownloadUrl() {
        return Parameters.DOWLOAD_ENDPOINT + this.IDModel + "/" + Parameters.DEFAULT_MODEL_NAME + "." + this.modelExtension;
    }

    public string getFile() {
        return Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/" + Parameters.DEFAULT_MODEL_NAME + "." + this.modelExtension;
    }

    public string getParentDirectory() {
        return Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel;
    }

    public string getFileName() {
        return Parameters.DEFAULT_MODEL_NAME + "." + this.modelExtension;
    }

    public string getTexturesParentDirectory() {
        return Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/textures/";
    }

}
/* Import the necessary system libraries. */
using System.Collections;
using System.IO;
using System;

/* Import the necessary Unity libraries. */
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
class ModelTexture {
    public string IDTexture;
    public string IDModel;
    public bool isDefault;
    public bool isColor;
    public string colorHex;
    public bool isImage;
    public string extension;

    /* Model informations */
    public string getDownloadUrl() {
        return Parameters.SERVER_MODELS_PATH + this.IDModel + "/" + this.IDTexture + "." + this.extension;
    }

    private string getLocalPath() {
        return Application.dataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/";
    }

    public string getFileProperties(bool withExtension = true) {
        if(withExtension)
            return Application.dataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/textures/" + this.IDTexture + "." + this.extension;
        else
            return Application.dataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/textures/" + this.IDTexture;
    }

    public string getDirectory() {
        return Application.dataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/textures";
    }

    public string getFileName() {
        return this.IDTexture + "." + this.extension;
    }
}
/* Import the necessary system libraries */
using System;
using UnityEngine;

[Serializable]
class ModelTexture {
    public string IDTexture;
    public string IDModel;
    public bool isDefault;
    public bool isColor;
    public string colorHex;
    public bool isImage;
    public string extension;

    
    public string getDownloadUrl() {
        /* Get the download url */
        return Parameters.DOWLOAD_ENDPOINT + this.IDModel + "/" + this.IDTexture + "." + this.extension;
    }

    public string getFile() {
        return Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/textures/" + this.IDTexture + "." + this.extension;
    }

    public string getParentDirectory() {
        return Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + this.IDModel + "/textures/";
    }

    public string getFileName() {
        return this.IDTexture + "." + this.extension;
    }
}
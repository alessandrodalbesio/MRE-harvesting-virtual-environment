using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

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
        return Parameters.MODEL_PATH + this.IDModel + "/" + this.IDTexture + "." + this.extension;
    }

    public string getFileDirectoryAndName(bool withExtension = true) {
        if(withExtension)
            return Application.dataPath + Parameters.SAVE_PATH + this.IDModel + "/textures/" + this.IDTexture + "." + this.extension;
        else
            return Application.dataPath + Parameters.SAVE_PATH + this.IDModel + "/textures/" + this.IDTexture;
    }

    public string getDirectory(bool withEndSlash = true) {
        if(withEndSlash)
            return Application.dataPath + Parameters.SAVE_PATH + this.IDModel + "/textures/";
        else
            return Application.dataPath + Parameters.SAVE_PATH + this.IDModel + "/textures";
    }

    public string getFileName() {
        /* Get the file name */
        return this.IDTexture + "." + this.extension;
    }

    public void deleteTexture() {
        /* Create the connection to the database */
        SQLiteConnectionManager dbManager = new SQLiteConnectionManager();

        /* Delete the texture from the database */
        dbManager.deleteTextureFromDatabase(this);

        /* Delete the texture from the storage */
        if(File.Exists(this.getFileDirectoryAndName())) {
            /* Delete the file */
            File.Delete(this.getFileDirectoryAndName());
        }
    }
}
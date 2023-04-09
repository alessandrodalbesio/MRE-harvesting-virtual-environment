using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

[Serializable]
class Model {
    public string IDModel;
    public string modelName;
    public string modelExtension;
    public List<ModelTexture> textures;

    public string getDownloadUrl() {
        /* Get the download url */
        return Parameters.MODEL_PATH + this.IDModel + "/model." + this.modelExtension;
    }
    public string getFileDirectoryAndName(bool withExtension = true) {
        if(withExtension)
            return Application.dataPath + Parameters.SAVE_PATH + this.IDModel + "/model." + this.modelExtension;
        else
            return Application.dataPath + Parameters.SAVE_PATH + this.IDModel + "/model";
    }

    public string getDirectory(bool withEndSlash = true) {
        if(withEndSlash)
            return Application.dataPath + Parameters.SAVE_PATH + this.IDModel + "/";
        else
            return Application.dataPath + Parameters.SAVE_PATH + this.IDModel;
    }

    public string getTexturesDirectory() {
        /* Get the directory where the textures are saved */
        return Application.dataPath + Parameters.SAVE_PATH + this.IDModel + "/textures/";
    }

    public string getFileName() {
        /* Get the file name */
        return "model." + this.modelExtension;
    }

    /* Delete the model from the storage and the database */
    public void deleteModel() {
        /* Create the connection to the database */
        SQLiteConnectionManager dbManager = new SQLiteConnectionManager();
        
        /* Delete the textures from the database */
        foreach(ModelTexture texture in this.textures)
            texture.deleteTexture();
        
        /* Delete the model from the database */
        dbManager.deleteModelFromDatabase(this);

        /* Delete the model from the storage */
        if(Directory.Exists(this.getDirectory())) {
            /* Delete the directory */
            Directory.Delete(this.getDirectory(), true);
        }
    }
}
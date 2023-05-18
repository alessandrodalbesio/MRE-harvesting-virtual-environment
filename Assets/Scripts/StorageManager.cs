using UnityEngine;
using System.Collections.Generic;
using System.IO;

class StorageManager {
    public StorageManager() {
        /* Create the main directory if it doesn't exist */
        if (!Directory.Exists(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH))
            Directory.CreateDirectory(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH);

        /* Create the files if they don't exist */
        if (!File.Exists(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.MODEL_LOCAL_DATA_FILE))
            File.Create(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.MODEL_LOCAL_DATA_FILE);
        if (!File.Exists(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.TEXTURE_LOCAL_DATA_FILE))
            File.Create(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.TEXTURE_LOCAL_DATA_FILE);
    }


    /* Get a list of all the models in the local storage */
    public List<Model> getModels() {
        /* Read the file */
        string[] lines = File.ReadAllLines(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.MODEL_LOCAL_DATA_FILE);

        List<Model> models = new List<Model>();
        
        foreach (string line in lines) {
            string[] values = line.Split(';');
            Model model = new Model();
            model.IDModel = values[0];
            model.modelName = values[1];
            model.modelExtension = values[2];
            model.textures = new List<ModelTexture>();
            models.Add(model);
        }

        for (int i = 0; i < models.Count; i++) {
            /* Read the file */
            string[] linesTextures = File.ReadAllLines(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.TEXTURE_LOCAL_DATA_FILE);

            foreach (string line in linesTextures) {
                string[] values = line.Split(';');
                if (values[1] == models[i].IDModel) {
                    ModelTexture texture = new ModelTexture();
                    texture.IDTexture = values[0];
                    texture.IDModel = values[1];
                    texture.isDefault = bool.Parse(values[2]);
                    texture.isColor = bool.Parse(values[3]);
                    texture.colorHex = values[4];
                    texture.isImage = bool.Parse(values[5]);
                    texture.extension = values[6];
                    models[i].textures.Add(texture);
                }
            }
        }

        return models;
    }

    /* Get a list of all the textures in the local storage */
    public List<ModelTexture> getTextures() {
        /* Read the file */
        string[] lines = File.ReadAllLines(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.TEXTURE_LOCAL_DATA_FILE);

        List<ModelTexture> textures = new List<ModelTexture>();

        foreach (string line in lines) {
            string[] values = line.Split(';');
            ModelTexture texture = new ModelTexture();
            texture.IDTexture = values[0];
            texture.IDModel = values[1];
            texture.isDefault = bool.Parse(values[2]);
            texture.isColor = bool.Parse(values[3]);
            texture.colorHex = values[4];
            texture.isImage = bool.Parse(values[5]);
            texture.extension = values[6];
            textures.Add(texture);
        }

        return textures;
    }

    /* Get a list of the textures of a specified model */
    public List<ModelTexture> getTextures(string IDModel) {
        /* Read the file */
        string[] lines = File.ReadAllLines(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.TEXTURE_LOCAL_DATA_FILE);

        List<ModelTexture> textures = new List<ModelTexture>();

        foreach (string line in lines) {
            string[] values = line.Split(';');
            if (values[1] == IDModel) {
                ModelTexture texture = new ModelTexture();
                texture.IDTexture = values[0];
                texture.IDModel = values[1];
                texture.isDefault = bool.Parse(values[2]);
                texture.isColor = bool.Parse(values[3]);
                texture.colorHex = values[4];
                texture.isImage = bool.Parse(values[5]);
                texture.extension = values[6];
                textures.Add(texture);
            }
        }

        return textures;
    }

    /* Get the informations of a specified model */
    public Model getModel(string IDModel) {
        List<Model> models = this.getModels();
        foreach (Model model in models) {
            if (model.IDModel == IDModel)
                return model;
        }
        return null;
    }

    /* Get the informations of a specified texture of a specified model */
    public ModelTexture getTexture(string IDModel, string IDTexture) {
        Model model = this.getModel(IDModel);
        foreach (ModelTexture texture in model.textures) {
            if (texture.IDTexture == IDTexture)
                return texture;
        }
        return null;
    }


    /* Save the model into the local storage */
    public void saveModel(Model model) {
        /* Each model is saved into the file as ID_MODEL;MODEL_NAME;MODEL_EXTENSION */

        /* Write the model informations into the file */
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.MODEL_LOCAL_DATA_FILE, true);
        sw.WriteLine(model.IDModel + ";" + model.modelName + ";" + model.modelExtension);
        sw.Close();

        /* Save the textures */
        foreach (ModelTexture texture in model.textures)
            this.saveTexture(texture);
    }

    /* Save the texture into the local storage */
    public void saveTexture(ModelTexture texture) {
        /* Each texture is saved into the file as ID_TEXTURE;ID_MODEL;IS_DEFAULT;IS_COLOR;COLOR_HEX;IS_IMAGE;EXTENSION */

        /* Write the texture informations into the file */
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.TEXTURE_LOCAL_DATA_FILE, true);
        sw.WriteLine(texture.IDTexture + ";" + texture.IDModel + ";" + texture.isDefault + ";" + texture.isColor + ";" + texture.colorHex + ";" + texture.isImage + ";" + texture.extension);
        sw.Close();
    }


    /* Delete the specified model from the local storage */
    public void deleteModel(Model model) {
        /* Read the file */
        string[] lines = File.ReadAllLines(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.MODEL_LOCAL_DATA_FILE);

        /* Write the file */
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.MODEL_LOCAL_DATA_FILE, false);

        foreach (string line in lines) {
            string[] values = line.Split(';');
            if (values[0] != model.IDModel)
                sw.WriteLine(line);
        }

        sw.Close();

        /* Delete the textures */
        foreach (ModelTexture texture in model.textures)
            this.deleteTexture(texture);
    }

    /* Delete the specified texture from the local storage */
    public void deleteTexture(ModelTexture texture) {
        /* Read the file */
        string[] lines = File.ReadAllLines(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.TEXTURE_LOCAL_DATA_FILE);

        /* Write the file */
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.TEXTURE_LOCAL_DATA_FILE, false);

        foreach (string line in lines) {
            string[] values = line.Split(';');
            if (values[0] != texture.IDTexture)
                sw.WriteLine(line);
        }

        sw.Close();
    }
}
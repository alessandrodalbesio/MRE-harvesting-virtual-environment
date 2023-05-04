using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.IO;

class SQLiteConnectionManager {
    private string dbConnectionString;

    /* Manage the connection to the database */
    public SQLiteConnectionManager() {
        this.dbConnectionString = "URI=file:" + Application.dataPath + Parameters.LOCAL_SAVE_PATH + "/" + Parameters.LOCAL_DB_NAME;
        if (!Directory.Exists(Application.dataPath + Parameters.LOCAL_SAVE_PATH))
            Directory.CreateDirectory(Application.dataPath + Parameters.LOCAL_SAVE_PATH);
        this.setUpTables();
    }
    private void setUpTables() {
        /* Define the queries for creating the model table and the texture table */
        string modelTableQuery = @"CREATE TABLE IF NOT EXISTS 'model' (
            'IDModel' CHAR("+Parameters.MODEL_ID_LENGTH+@") NOT NULL PRIMARY KEY,
            'nameModel' VARCHAR("+Parameters.MAX_MODEL_NAME_LENGTH+@") NOT NULL UNIQUE,
            'extension' VARCHAR(4) NOT NULL
        )";
        string modelTexturesQuery = @"CREATE TABLE IF NOT EXISTS 'texture' (
            'IDTexture' CHAR("+Parameters.TEXTURE_ID_LENGTH+@") PRIMARY KEY, 
            'IDModel' CHAR("+Parameters.MODEL_ID_LENGTH+@") NOT NULL,
            'isDefault' BOOLEAN NOT NULL DEFAULT 0,
            'isColor' BOOLEAN NOT NULL DEFAULT 0,
            'colorHex' CHAR(7) DEFAULT NULL,
            'isImage' BOOLEAN NOT NULL DEFAULT 0,
            'extension' VARCHAR(4) DEFAULT NULL
        )";

        /* Open the connection with the database */
        IDbConnection dbcon = new SqliteConnection(this.dbConnectionString);
        dbcon.Open();

        /* Execute the queries */
        IDbCommand dbCommand = dbcon.CreateCommand();
        dbCommand.CommandText = modelTableQuery;
        dbCommand.ExecuteNonQuery();
        dbCommand.CommandText = modelTexturesQuery;
        dbCommand.ExecuteNonQuery();

        /* Close the connection */
        dbCommand.Dispose();
        dbcon.Close();
    }


    /* Get the model list from the database */
    public List<Model> getModelsList() {
        /* Open the connection */
        IDbConnection dbcon = new SqliteConnection(this.dbConnectionString);
        dbcon.Open();

        /* Create the list of models */
        List<Model> models = new List<Model>();

        /* Select all the models from the database */
        using(IDbCommand dbCommand = dbcon.CreateCommand()) {
            string query = "SELECT * FROM model";

            /* Execute the query */
            dbCommand.CommandText = query;

            /* Get the results */
            using(IDataReader reader = dbCommand.ExecuteReader()) {
                /* Save the models to the list */
                while(reader.Read()) {
                    Model model = new Model();
                    model.IDModel = reader.GetString(0);
                    model.modelName = reader.GetString(1);
                    model.modelExtension = reader.GetString(2);
                    model.textures = new List<ModelTexture>();
                    models.Add(model);
                }
                reader.Dispose();
            }
            dbCommand.Dispose();
        }

        /* Select all the textures from the database and save them to the associated model */
        for (int i = 0; i < models.Count; i++) {
            using(IDbCommand dbCommand = dbcon.CreateCommand()) {
                string query = "SELECT * FROM texture WHERE IDModel = @IDModel";

                /* Execute the query */
                dbCommand.CommandText = query;
                dbCommand.Parameters.Add(new SqliteParameter("@IDModel", models[i].IDModel));
                
                /* Get the results */
                using(IDataReader reader = dbCommand.ExecuteReader()) {
                    /* Save the textures to the model */
                    while(reader.Read()) {
                        ModelTexture texture = new ModelTexture();
                        texture.IDTexture = reader.GetString(0);
                        texture.IDModel = reader.GetString(1);
                        texture.isDefault = reader.GetBoolean(2);
                        texture.isColor = reader.GetBoolean(3);
                        texture.colorHex = reader.GetString(4);
                        texture.isImage = reader.GetBoolean(5);
                        texture.extension = reader.GetString(6);
                        models[i].textures.Add(texture);
                    }
                    reader.Dispose();
                }
                dbCommand.Dispose();
            }
        }

        /* Close the connection */
        dbcon.Close();

        /* Return the list of models */
        return models;
    }


    /* Save the model into the database */
    public void saveModelIntoDatabase(Model model) {
        /* Open the connection */
        SqliteConnection dbcon = new SqliteConnection(this.dbConnectionString);
        dbcon.Open();

        /* Create the model into the database */
        using(IDbCommand dbCommand = dbcon.CreateCommand()) {
            /* Define the query */
            string query = "INSERT INTO model (IDModel, nameModel, extension) VALUES (@IDModel, @nameModel, @extension)";

            /* Execute the query */
            dbCommand.CommandText = query;
            dbCommand.Parameters.Add(new SqliteParameter("@IDModel", model.IDModel));
            dbCommand.Parameters.Add(new SqliteParameter("@nameModel", model.modelName));
            dbCommand.Parameters.Add(new SqliteParameter("@extension", model.modelExtension));
            dbCommand.ExecuteNonQuery();

            /* Close the connection */
            dbCommand.Dispose();
        }

        /* Close the connection */
        dbcon.Close();
    }


    /* Save the texture into the database */
    public void saveTextureIntoDatabase(ModelTexture texture) {
        /* Open the connection */
        SqliteConnection dbcon = new SqliteConnection(this.dbConnectionString);
        dbcon.Open();

        /* Create the texture into the database */
        using(IDbCommand dbCommand = dbcon.CreateCommand()) {
            /* Define the query */
            string query = "INSERT INTO texture (IDTexture, IDModel, isDefault, isColor, colorHex, isImage, extension) VALUES (@IDTexture, @IDModel, @isDefault, @isColor, @colorHex, @isImage, @extension)";

            /* Execute the query */
            dbCommand.CommandText = query;
            dbCommand.Parameters.Add(new SqliteParameter("@IDTexture", texture.IDTexture));
            dbCommand.Parameters.Add(new SqliteParameter("@IDModel", texture.IDModel));
            dbCommand.Parameters.Add(new SqliteParameter("@isDefault", texture.isDefault));
            dbCommand.Parameters.Add(new SqliteParameter("@isColor", texture.isColor));
            dbCommand.Parameters.Add(new SqliteParameter("@colorHex", texture.colorHex));
            dbCommand.Parameters.Add(new SqliteParameter("@isImage", texture.isImage));
            dbCommand.Parameters.Add(new SqliteParameter("@extension", texture.extension));
            dbCommand.ExecuteNonQuery();

            dbCommand.Dispose();
        }

        /* Close the connection */
        dbcon.Close();
    }


    /* Delete the model from the database */
    public void deleteModelFromDatabase(Model model) {
        /* Open the connection */
        IDbConnection dbcon = new SqliteConnection(this.dbConnectionString);
        dbcon.Open();

        /* Create all the needed parameters */
        using(IDbCommand dbCommand = dbcon.CreateCommand()) {
            /* Define the query */
            string query = "DELETE FROM model WHERE IDModel = @IDModel";

            /* Execute the query */
            dbCommand.CommandText = query;
            dbCommand.Parameters.Add(new SqliteParameter("@IDModel", model.IDModel));
            dbCommand.ExecuteNonQuery();

            /* Close the connection */
            dbCommand.Dispose();
        }

        /* Close the connection */
        dbcon.Close();
    }

    public void deleteTextureFromDatabase(ModelTexture texture) {
        /* Open the connection */
        IDbConnection dbcon = new SqliteConnection(this.dbConnectionString);
        dbcon.Open();

        /* Create all the needed parameters */
        using(IDbCommand dbCommand = dbcon.CreateCommand()) {
            /* Define the query */
            string query = "DELETE FROM texture WHERE IDTexture = @IDTexture";

            /* Execute the query */
            dbCommand.CommandText = query;
            dbCommand.Parameters.Add(new SqliteParameter("@IDTexture", texture.IDTexture));
            dbCommand.ExecuteReader();

            /* Close the connection */
            dbCommand.Dispose();
        }

        /* Close the connection */ 
        dbcon.Close();
    }

}
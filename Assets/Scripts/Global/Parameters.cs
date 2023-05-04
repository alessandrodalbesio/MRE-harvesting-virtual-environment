using UnityEngine;

class Parameters : MonoBehaviour {
    public static bool LOADED_DATA = true;
    public const string API_URL = "http://virtualenv.epfl.ch/api";
    public const string WS_URL = "http://virtualenv.epfl.ch/ws";
    public const string SERVER_MODELS_PATH = "http://virtualenv.epfl.ch/models/";
    public const string LOCAL_SAVE_PATH = "/Models/"; // Path where the models are saved

    public const string INFO_LIST_ENDPONT = API_URL + "/models";

    public const int MODEL_ID_LENGTH = 32;
    public const int TEXTURE_ID_LENGTH = 32;
    public const int MAX_MODEL_NAME_LENGTH = 100;

    public const string LOCAL_DB_NAME = "Database.db";
}
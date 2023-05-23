using UnityEngine;

class Parameters : MonoBehaviour {
    /* Urls */
    public const string SERVER_URL = "http://virtualenv.epfl.ch/";
    public const string API_URL = SERVER_URL+"api/";
    public const string WS_API_URL = SERVER_URL + "ws_api/";
    public const string WS_API_NEED_LOCAL_STORAGE_REFRESH_ENDPOINT = WS_API_URL + "need-local-database-refresh";
    public const string WS_API_GET_ACTIVE_MODEL_ENPOINT = WS_API_URL + "active-model";
    public const string DOWLOAD_ENDPOINT = SERVER_URL+"models/";
    public const string MODELS_INFO_ENDPOINT = API_URL + "models";


    /* Constants for storage */
    public const string DEFAULT_MODEL_NAME = "model";
    public const string MODEL_LOCAL_DATA_FILE = "model.csv";
    public const string TEXTURE_LOCAL_DATA_FILE = "texture.csv";
    public const string LOCAL_SAVE_PATH = "/Models/";


    /* Consts for synchronization */
    public const long TIME_BETWEEN_POLLING_MS = 2500;


    /* Parameters for ROS */
    public const string ROS_PUB_NAME = "color";


    /* Parameters for scene loading */
    


    /* General constants */
    public const bool DEBUG_MODEL = true;
}
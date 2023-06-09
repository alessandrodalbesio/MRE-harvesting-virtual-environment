using UnityEngine;

class Parameters : MonoBehaviour {
    /* Urls */
    public const string SERVER_URL = "http://virtualenv.epfl.ch/";
    public const string WEBSOCKET_SERVER_URL = "ws://virtualenv.epfl.ch/ws";
    public const string API_URL = SERVER_URL+"api/";
    public const string DOWLOAD_ENDPOINT = SERVER_URL+"models/";
    public const string MODELS_INFO_ENDPOINT = API_URL + "models";


    /* Constants for storage */
    public const string DEFAULT_MODEL_NAME = "model";
    public const string MODEL_LOCAL_DATA_FILE = "model.csv";
    public const string TEXTURE_LOCAL_DATA_FILE = "texture.csv";
    public const string LOCAL_SAVE_PATH = "/Models/";


    /* Parameter for tracker synchronization */
    public const string RASPBERRY_REFERENCE_NAME = "RASPBERRY";
    public const string HOLDER_REFERENCE_NAME = "HOLDER";
    public const int RASPBERRY_ID = 101;
    public const int HOLDER_ID = 100;

    /* General constants */
    public const bool DEBUG_MODEL = true;
}
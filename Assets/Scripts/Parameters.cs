using UnityEngine;

class Parameters {
    public const string URL = "http://127.0.0.1:5000/"; // URL of the server
    public const string GET_MODELS_LIST_ENDPOINT = Parameters.URL + "models?device=unity"; // Endpoint for getting the models list

    public const string MODEL_PATH = "http://127.0.0.1:5001/website/models/";

    public const bool DEBUG_MODE = true; // Debug mode

    public const string SAVE_PATH = "/Models/"; // Path where the models are saved

    public const int MAXIMUM_NUMBER_OF_TEXTURES = 5;
    public string IDTexture;
    public string IDModel;
    public string isDefault;
    public string isColor;
    public string colorHex;
    public string isImage;
    public string extension;
}
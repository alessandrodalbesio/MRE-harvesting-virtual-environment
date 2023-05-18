/* This file manage the loading of the scene */
using System.Collections.Generic;
using UnityEngine;
using Dummiesman;

public class SceneLoader : MonoBehaviour {
    /* Real object parameters */
    public string realObjectNameModel = "calyx real"; /* This is the sub object that should be used as a reference for making the resizing */
    public Vector3 realObjectDimensions = new Vector3(0.1f,0.1f,0.1f); /* Real size of the object in cm */

    /* Table parameters */
    public Material table_material;
    public float TABLE_LEG_DIMENSION = 0.1f;
    public float TABLE_TOP_HEIGHT = 0.1f;

    /* Holder parameters */    
    public Material holder_material;
    public float HOLDER_DIMENSION = 0.1f;

    /* Environment options */
    public float MIN_LENGTH = 1.5f;
    public float LENGTH_OFFSET = 0.5f;
    public float CEILING_HEIGHT = 2.5f;
    public float WIDTH_OFFSET = 0.2f;

    /* Internal variables */
    private bool isSceneReady = false;
    public bool IsSceneReady {
        get { return isSceneReady; }
    } 
    private Vector3 tableTopCenterPosition;

    /* Active model info */
    private string currentActiveModelID = null;
    private string currentActiveTextureID = null;
    private GameObject activeModel = null;

    /* Default material for the creation of the objects */
    public Material defaultMaterial;


    void Start() {
        /* To remove this part when the tracking part has been done */
        Dictionary<string,Vector3> tableTrackers = new Dictionary<string, Vector3>();
        Dictionary<string,Vector3> holderTrackers = new Dictionary<string, Vector3>();
        Dictionary<string,Vector3> headsetTrackers = new Dictionary<string, Vector3>();
        Dictionary<string,Vector3> objectTrackers = new Dictionary<string, Vector3>();

        tableTrackers.Add("front-left", new Vector3(0,1.0f,0));
        tableTrackers.Add("back-right", new Vector3(1.0f,1.0f,0.5f));
        holderTrackers.Add("front-left-low", new Vector3(0.3f,1.05f,0));
        holderTrackers.Add("back-right-low", new Vector3(0.7f,1.05f,0.5f));
        holderTrackers.Add("top-center-front", new Vector3(0.5f,1.6f,0));
        holderTrackers.Add("top-center-back", new Vector3(0.5f,1.6f,0.3f));
        holderTrackers.Add("rope-attach-point", new Vector3(0.5f,1.55f,0));

        this.createTableInScene(tableTrackers);
        this.createHolderInScene(holderTrackers);
    }

    private void createTableInScene(Dictionary<string,Vector3> tableTrackers) {
        /* Get the values of the trackers */
        Vector3 FrontLeftTableTracker = tableTrackers["front-left"];
        Vector3 BackRightTableTracker = tableTrackers["back-right"];

        /* Compute the center of the table top (used to construct other components) */
        tableTopCenterPosition = new Vector3((FrontLeftTableTracker.x + BackRightTableTracker.x)/2, FrontLeftTableTracker.y, (FrontLeftTableTracker.z + BackRightTableTracker.z)/2);

        /* Create a GameObject that will contain the table top and the legs */
        GameObject table = new GameObject();
        table.name = "table";
        table.transform.parent = this.gameObject.transform;

        /* Define the position of the legs */
        Dictionary<string,Vector3[]> tableLegs = new Dictionary<string, Vector3[]>();

        Vector3 flLegPos = new Vector3(FrontLeftTableTracker.x + TABLE_LEG_DIMENSION/2, FrontLeftTableTracker.y / 2, FrontLeftTableTracker.z + TABLE_LEG_DIMENSION/2);
        Vector3 flLegSize = new Vector3(Mathf.Abs(TABLE_LEG_DIMENSION), Mathf.Abs(FrontLeftTableTracker.y), TABLE_LEG_DIMENSION);
        tableLegs.Add("front-left-table-leg", new Vector3[] {flLegPos, flLegSize});

        Vector3 frLegPos = new Vector3(FrontLeftTableTracker.x + TABLE_LEG_DIMENSION/2, FrontLeftTableTracker.y / 2, BackRightTableTracker.z - TABLE_LEG_DIMENSION/2);
        Vector3 frLegSize = new Vector3(Mathf.Abs(TABLE_LEG_DIMENSION), Mathf.Abs(FrontLeftTableTracker.y), TABLE_LEG_DIMENSION);
        tableLegs.Add("front-right-table-leg", new Vector3[] {frLegPos, frLegSize});

        Vector3 blLegPos = new Vector3(BackRightTableTracker.x - TABLE_LEG_DIMENSION/2, BackRightTableTracker.y / 2, FrontLeftTableTracker.z + TABLE_LEG_DIMENSION/2);
        Vector3 blLegSize = new Vector3(TABLE_LEG_DIMENSION, Mathf.Abs(BackRightTableTracker.y), TABLE_LEG_DIMENSION);
        tableLegs.Add("back-left-table-leg", new Vector3[] {blLegPos, blLegSize});

        Vector3 brLegPos = new Vector3(BackRightTableTracker.x - TABLE_LEG_DIMENSION/2, BackRightTableTracker.y / 2, BackRightTableTracker.z - TABLE_LEG_DIMENSION/2);
        Vector3 brLegSize = new Vector3(TABLE_LEG_DIMENSION, Mathf.Abs(BackRightTableTracker.y), TABLE_LEG_DIMENSION);
        tableLegs.Add("back-right-table-leg", new Vector3[] {brLegPos, brLegSize});

        /* Create the legs into scene */
        foreach (KeyValuePair<string,Vector3[]> leg in tableLegs) {
            GameObject legObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            legObj.name = leg.Key;
            legObj.transform.parent = table.transform;
            legObj.transform.position = leg.Value[0];
            legObj.transform.localScale = leg.Value[1];
            legObj.GetComponent<Renderer>().material = table_material;
        }

        /* Create the table top */
        Vector3 tableTopPos = new Vector3((BackRightTableTracker.x + FrontLeftTableTracker.x) / 2, FrontLeftTableTracker.y - TABLE_TOP_HEIGHT/2, (BackRightTableTracker.z + FrontLeftTableTracker.z) / 2);
        Vector3 tableTopSize = new Vector3(Mathf.Abs(BackRightTableTracker.x - FrontLeftTableTracker.x), TABLE_TOP_HEIGHT, Mathf.Abs(BackRightTableTracker.z - FrontLeftTableTracker.z));
        GameObject tableTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tableTop.name = "table-top";
        tableTop.transform.position = tableTopPos;
        tableTop.transform.localScale = tableTopSize;
        tableTop.transform.parent = table.transform;
        tableTop.GetComponent<Renderer>().material = table_material;
    }

    private void createHolderInScene(Dictionary<string,Vector3> holderTrackers) {
        /* Define global variables */
        Vector3 holderFrontLeftLow = holderTrackers["front-left-low"];
        Vector3 holderBackRightLow = holderTrackers["back-right-low"];
        Vector3 holderTopCenterFront = holderTrackers["top-center-front"];
        Vector3 holderTopCenterBack = holderTrackers["top-center-back"];
        Vector3 ropeAttachPoint = holderTrackers["rope-attach-point"];

        /* Create the gameobject holder */
        GameObject holder = new GameObject();
        holder.name = "Holder";
        holder.transform.parent = this.gameObject.transform;

        /* Create the holder base */
        Vector3 holderBaseCenterPos = new Vector3((holderFrontLeftLow.x + holderBackRightLow.x)/2, (holderFrontLeftLow.y + tableTopCenterPosition.y)/2, (holderFrontLeftLow.z + holderBackRightLow.z)/2);
        Vector3 holderBaseSize = new Vector3(Mathf.Abs(holderFrontLeftLow.x - holderBackRightLow.x), Mathf.Abs(holderFrontLeftLow.y - tableTopCenterPosition.y), Mathf.Abs(holderBackRightLow.z - holderFrontLeftLow.z));
        GameObject holderBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        holderBase.name = "holder-base";
        holderBase.transform.parent = holder.transform;
        holderBase.transform.position = holderBaseCenterPos;
        holderBase.transform.localScale = holderBaseSize;
        holderBase.GetComponent<Renderer>().material = holder_material;
        float holderHeight = Mathf.Abs(holderTopCenterFront.y - ropeAttachPoint.y);

        /* Create the holder arm */
        Vector3 holderArmPos = new Vector3(holderTopCenterBack.x, (holderFrontLeftLow.y + holderTopCenterFront.y) / 2, holderTopCenterBack.z - holderHeight/2);
        Vector3 holderArmSize = new Vector3(Mathf.Abs(holderHeight), Mathf.Abs(holderTopCenterBack.y - holderBackRightLow.y), Mathf.Abs(holderHeight));
        GameObject holderArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        holderArm.name = "holder-arm-1";
        holderArm.transform.parent = holder.transform;
        holderArm.transform.position = holderArmPos;
        holderArm.transform.localScale = holderArmSize;
        holderArm.GetComponent<Renderer>().material = holder_material;

        /* Create the second holder arm */
        Vector3 holderArm2Pos = new Vector3(holderTopCenterFront.x, (ropeAttachPoint.y + holderTopCenterFront.y) / 2, (holderTopCenterFront.z + holderTopCenterBack.z )/2);
        Vector3 holderArm2Size = new Vector3(Mathf.Abs(holderHeight), Mathf.Abs(holderHeight), Mathf.Abs(holderTopCenterFront.z - holderTopCenterBack.z));
        GameObject holderArm2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        holderArm2.name = "holder-arm-2";
        holderArm2.transform.parent = holder.transform;
        holderArm2.transform.position = holderArm2Pos;
        holderArm2.transform.localScale = holderArm2Size;
        holderArm2.GetComponent<Renderer>().material = holder_material;
    }

    public Vector3 getEstimatedObjectCenter(Dictionary<string,Vector3> objectTrackers) {
        return new Vector3(0.5f, 1.40f, 0.05f);
    }

    public Quaternion getEstimatedObjectRotation(Dictionary<string,Vector3> objectTrackers) {
        return new Quaternion(0,0,0,0);
    }
    
    private void manageObjectInScene(string IDModel, string IDTexture) {
        /* Get the variables needed to get the active model */
        StorageManager storageManager = new StorageManager();
        OBJLoader objLoader = new OBJLoader();
        objLoader.setDefaultMaterial(defaultMaterial);

        Model model = storageManager.getModel(IDModel);
        /* Get the model from the database and create it */
        Debug.Log("GET FILE: " + model.getFile());
        activeModel = objLoader.Load(model.getFile());
        Debug.Log("Active model loaded");
        GameObject refObject = GameObject.Find(this.realObjectNameModel);
        Vector3 loadedModelDimensions = refObject.GetComponent<Renderer>().bounds.size;
        Vector3 scalingFactor = new Vector3(realObjectDimensions.x / loadedModelDimensions.x, realObjectDimensions.y / loadedModelDimensions.y, realObjectDimensions.z / loadedModelDimensions.z);
        activeModel.transform.localScale = scalingFactor;
        activeModel.transform.parent = this.gameObject.transform;
        activeModel.name = "active-model";
        activeModel.transform.position = getEstimatedObjectCenter(new Dictionary<string, Vector3>());
        activeModel.transform.rotation = getEstimatedObjectRotation(new Dictionary<string, Vector3>());

        /* Get the texture from the database and apply it to the model based on its type */
        ModelTexture texture = storageManager.getTexture(IDModel, IDTexture);
        if (texture.isColor == true) {
            Color newColor;
            if(ColorUtility.TryParseHtmlString(texture.colorHex, out newColor)) {
                // Add the color to all the childs of the model
                foreach (Transform child in activeModel.transform) {
                    child.GetComponent<Renderer>().material.color = newColor;
                }
            }
        }
    }

    public void setActiveObjectInScene(string IDModel, string IDTexture) {
        if (IDModel == null && currentActiveModelID != null) {
            currentActiveModelID = null;
            currentActiveTextureID = null;
            Destroy(activeModel);
            activeModel = null;
        }
        else if (IDModel != null && (currentActiveModelID == null || currentActiveModelID != IDModel || IDTexture != currentActiveTextureID)) {
            StorageManager storageManager = new StorageManager();
            currentActiveModelID = IDModel;
            currentActiveTextureID = IDTexture;
            if (activeModel != null) {
                Destroy(activeModel);
                activeModel = null;
            }
            manageObjectInScene(IDModel, IDTexture);
        }
    }

    /* Refresh the scene with the new trackers positions */
    public void refreshScene(Dictionary<string,Vector3> tableTrackers, Dictionary<string,Vector3> holderTrackers, Dictionary<string,Vector3> headsetTrackers, Dictionary<string,Vector3> objectTrackers) {
        if (!isSceneReady)
            this.createScene(tableTrackers, holderTrackers, headsetTrackers);
        else
            this.updateScene(headsetTrackers, objectTrackers);
    }


    /* Create the scene with the given trackers positions */
    public void createScene(Dictionary<string,Vector3> tableTrackers, Dictionary<string,Vector3> holderTrackers, Dictionary<string,Vector3> headsetTrackers) {
        if (isSceneReady == true) 
            return;

        /* Create the table */
        this.createTableInScene(tableTrackers);

        /* Create the holder */
        this.createHolderInScene(holderTrackers);

        this.isSceneReady = true;

    }

    /* Update the scene with the given trackers positions (only headsets and objects ones) */
    public void updateScene(Dictionary<string,Vector3> headsetTrackers, Dictionary<string,Vector3> objectTrackers) {

    }

    /* Utilities functions */
    public Vector3 quaternionToEuler(Quaternion q) {
        Vector3 euler = new Vector3();
        euler.x = Mathf.Atan2(2 * (q.w * q.x + q.y * q.z), 1 - 2 * (Mathf.Pow(q.x, 2) + Mathf.Pow(q.y, 2)));
        euler.y = Mathf.Asin(2 * (q.w * q.y - q.z * q.x));
        euler.z = Mathf.Atan2(2 * (q.w * q.z + q.x * q.y), 1 - 2 * (Mathf.Pow(q.y, 2) + Mathf.Pow(q.z, 2)));
        return euler;
    }

    public Quaternion eulerToQuaternion(Vector3 e) {
        Quaternion q = new Quaternion();
        q.w = Mathf.Cos(e.x / 2) * Mathf.Cos(e.y / 2) * Mathf.Cos(e.z / 2) + Mathf.Sin(e.x / 2) * Mathf.Sin(e.y / 2) * Mathf.Sin(e.z / 2);
        q.x = Mathf.Sin(e.x / 2) * Mathf.Cos(e.y / 2) * Mathf.Cos(e.z / 2) - Mathf.Cos(e.x / 2) * Mathf.Sin(e.y / 2) * Mathf.Sin(e.z / 2);
        q.y = Mathf.Cos(e.x / 2) * Mathf.Sin(e.y / 2) * Mathf.Cos(e.z / 2) + Mathf.Sin(e.x / 2) * Mathf.Cos(e.y / 2) * Mathf.Sin(e.z / 2);
        q.z = Mathf.Cos(e.x / 2) * Mathf.Cos(e.y / 2) * Mathf.Sin(e.z / 2) - Mathf.Sin(e.x / 2) * Mathf.Sin(e.y / 2) * Mathf.Cos(e.z / 2);
        return q;
    }
}

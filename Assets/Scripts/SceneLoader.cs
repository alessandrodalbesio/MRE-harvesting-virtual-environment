/* This file manage the loading of the scene */
using System.Collections.Generic;
using UnityEngine;
using Dummiesman;

class SceneLoader : MonoBehaviour {
    /* Real object parameters */
    public string realObjectNameModel = "calyx real"; /* This is the sub object that should be used as a reference for making the resizing */
    public Vector3 realObjectDimensions = new Vector3(0.05f,0.05f,0.05f); /* Real size of the object in cm */

    /* Table parameters */
    public Material table_material;
    public float TABLE_LEG_DIMENSION = 0.1f;
    public float TABLE_TOP_HEIGHT = 0.1f;

    /* Holder parameters */    
    public Material holder_material;
    public float HOLDER_DIMENSION = 0.1f;

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

    void Update() {
        /* Check if the trackers are ready */
        if(this.GetComponent<TrackersSynchronizer>().IsReady == false)
            return;

        /* Refresh the scene */
        this.refreshScene();

        /* Check if there is a new model from the synchronization */
        if (currentActiveModelID != this.GetComponent<DataSynchronizer>().ActiveModelID || currentActiveTextureID != this.GetComponent<DataSynchronizer>().ActiveTextureID) {
            Destroy(this.activeModel);
            if (this.GetComponent<DataSynchronizer>().ActiveModelID != null) {
                this.createModelInScene(this.GetComponent<DataSynchronizer>().ActiveModelID, this.GetComponent<DataSynchronizer>().ActiveTextureID);
            } else {
                this.currentActiveModelID = null;
                this.currentActiveTextureID = null;
            }
        }
    }

    private void createTableInScene() {
        /* Get the table trackers */
        Dictionary<string,Vector3> tableTrackers = this.GetComponent<TrackersSynchronizer>().TableTrackers;

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

    private void createHolderInScene() {
        /* Get the holder trackers */
        Dictionary<string,Vector3> holderTrackers = this.GetComponent<TrackersSynchronizer>().HolderTrackers;

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
    
    private void createModelInScene(string IDModel, string IDTexture) {
        /* Get the variables needed to get the active model */
        StorageManager storageManager = new StorageManager();
        OBJLoader objLoader = new OBJLoader();
        objLoader.setDefaultMaterial(defaultMaterial);
        Model model = storageManager.getModel(IDModel);

        /* Get the model from the database and create it */
        activeModel = objLoader.Load(model.getFile());
        GameObject refObject = GameObject.Find(this.realObjectNameModel);
        Vector3 loadedModelDimensions = refObject.GetComponent<Renderer>().bounds.size;
        Vector3 scalingFactor = new Vector3(realObjectDimensions.x / loadedModelDimensions.x, realObjectDimensions.y / loadedModelDimensions.y, realObjectDimensions.z / loadedModelDimensions.z);
        activeModel.transform.localScale = scalingFactor;
        activeModel.transform.parent = this.gameObject.transform;
        activeModel.name = "active-model";

        /* Get the position and rotation of the model */
        activeModel.transform.position = this.GetComponent<TrackersSynchronizer>().getObjectPosition();
        activeModel.transform.rotation = this.GetComponent<TrackersSynchronizer>().getObjectRotation();

        /* Get the texture from the database and apply it to the model based on its type */
        ModelTexture texture = storageManager.getTexture(IDModel, IDTexture);
        if (texture.isColor == true) {
            Color newColor;
            if(ColorUtility.TryParseHtmlString(texture.colorHex, out newColor)) {
                foreach (Transform child in activeModel.transform) {
                    child.GetComponent<Renderer>().material.color = newColor;
                }
            }
        }
    }

    /* Refresh the scene with the new trackers positions */
    public void refreshScene() {
        if (!isSceneReady)
            this.createScene();
        else
            this.updateScene();
    }


    /* Create the scene with the given trackers positions */
    public void createScene() {
        if (isSceneReady == true) 
            return;

        /* Create the table */
        this.createTableInScene();

        /* Create the holder */
        this.createHolderInScene();

        this.isSceneReady = true;

    }

    /* Update the scene with the given trackers positions (only headsets and objects ones) */
    public void updateScene() {

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

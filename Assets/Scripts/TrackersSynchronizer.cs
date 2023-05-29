/* This script at each iteration check that the position of the trackers is well represented in the simulation */
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class TrackersSynchronizer : MonoBehaviour {
    /* Used to de-serialize the data from the websocket server */
    [System.Serializable]
    protected class Tracker {
        public Vector3 position {
            get {
                return new Vector3(-pos[0], pos[1], pos[2]);
            }
        }
        public List<float> pos;
        public int id;
        public float residual;
        public string name {
            get {
                return "tracker" + id;
            }
        }
    }

    /* General settings variables */
    private bool isReady = false;
    public bool IsReady { get { return isReady; } }

    /* Trackers settings */
    public GameObject trackersPrefab;
    public Material trackersMaterial;
    public bool showTrackersInScene = true;
    private GameObject trackersParent;

    /* Informations to compute the center of the object */
    private Vector3 refVector;
    public Vector3 offsetToP1 = new Vector3(0,-0.1f,0); // To measure

    /* Trackers positions */
    private Dictionary<string,Vector3> tableTrackers = new Dictionary<string,Vector3>();
    private Dictionary<string,Vector3> holderTrackers = new Dictionary<string,Vector3>();
    private Dictionary<string,Vector3> headsetTrackers = new Dictionary<string,Vector3>();
    private Dictionary<string,Vector3> objectTrackers = new Dictionary<string,Vector3>();

    private List<Tracker> trackers = new List<Tracker>();

    /* Websocket management */
    private WebSocket ws;

    void Start() {
        /* Generate the trackers parent */
        trackersParent = new GameObject("Trackers");
        trackersParent.name = "Trackers";

        /* Manage the websocket */
        ws = new WebSocket(Parameters.WEBSOCKET_SERVER_URL);

        ws.OnOpen += (sender, e) => {
            ws.Send("{\"type\": \"join-optitrack-room\"}");
        };

        ws.OnMessage += (sender, e) => {
            if (e.Data.Contains("optitrack-data"))
                this.convertDataFromServer(e.Data);
        };

        ws.Connect();
    }
    
    private void convertDataFromServer(string jsonData) {
        jsonData = jsonData.Replace("\\", "");
        int numberOfCurlyBrackets = 0, numberOfSquaredBrackets = 0;
        int initialIndex = 0;
        List<Tracker> trackers = new List<Tracker>();
        for (int x = jsonData.IndexOf("["); ; x++) {
            if (jsonData[x] == '[')
                numberOfSquaredBrackets++;
            if (jsonData[x] == ']') {
                numberOfSquaredBrackets--;
                if (numberOfSquaredBrackets == 0)
                    break;
            }
            if (jsonData[x] == '{') {
                if (numberOfCurlyBrackets == 0)
                    initialIndex = x;
                numberOfCurlyBrackets++;
            }
            if (jsonData[x] == '}') {
                numberOfCurlyBrackets--;
                if (numberOfCurlyBrackets == 0) {
                    string json = jsonData.Substring(initialIndex, x - initialIndex + 1);
                    trackers.Add(JsonUtility.FromJson<Tracker>(json));
                }
            }
        }
        makeTrackerDataCompatible(trackers);
        this.isReady = true;
    }

    public void Update() {
        if (this.isReady)
            this.updateTrackersInSimulation();
    }

    private void makeTrackerDataCompatible(List<Tracker> trackers) {
        this.trackers = trackers;
    }


    /* Calibrate the ref vector */
    public void calibrateRefVector() {
        /* Compute the reference points */
        Vector3 refP1 = (objectTrackers["right"] + objectTrackers["left"]) / 2;
        Vector3 refP2 = objectTrackers["top"];
        refVector = refP2 - refP1;
    }

    /* Get object position */
    public Vector3 getObjectPosition() {
        Vector3 measuredP1 = (objectTrackers["right"] + objectTrackers["left"]) / 2;

        Quaternion rotation = this.getObjectRotation();

        /* Compute the object center position */
        Vector3 objectCenterPosition = measuredP1 + rotation * offsetToP1;
        return objectCenterPosition;
    }

    /* Get object rotation */
    public Quaternion getObjectRotation() {
        /* Compute the measured vector */
        Vector3 measuredP1 = (objectTrackers["right"] + objectTrackers["left"]) / 2;
        Vector3 measuredP2 = objectTrackers["top"];
        Vector3 measuredVector = measuredP2 - measuredP1;

        /* Compute the rotational matrix of the vector to respect to the refVector */
        float angle = Vector3.SignedAngle(refVector, measuredVector, Vector3.forward);
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        return rotation;
    }

    /* Update the trackers positions in the simulation */
    public void updateTrackersInSimulation() {
        if (!showTrackersInScene) return;
        for (int x = 0; ; x++) {
            Tracker tracker = trackers[x];
            GameObject trackerObject = GameObject.Find(tracker.name);
            if (trackerObject == null) {
                trackerObject = Instantiate(trackersPrefab, tracker.position, Quaternion.identity);
                trackerObject.name = tracker.name;
                trackerObject.transform.parent = trackersParent.transform;
                trackerObject.GetComponent<MeshRenderer>().material = trackersMaterial;
            }
            else
                trackerObject.transform.position = tracker.position;
            
            if (x == trackers.Count - 1) break;
        }
        foreach (KeyValuePair<string,Vector3> tracker in holderTrackers) {
            GameObject trackerObject = GameObject.Find(tracker.Key);
            if (trackerObject == null) {
                trackerObject = Instantiate(trackersPrefab, tracker.Value, Quaternion.identity);
                trackerObject.name = tracker.Key;
                trackerObject.transform.parent = trackersParent.transform;
                trackerObject.GetComponent<MeshRenderer>().material = trackersMaterial;
            }
            else
                trackerObject.transform.position = tracker.Value;
        }
        foreach (KeyValuePair<string,Vector3> tracker in headsetTrackers) {
            GameObject trackerObject = GameObject.Find(tracker.Key);
            if (trackerObject == null) {
                trackerObject = Instantiate(trackersPrefab, tracker.Value, Quaternion.identity);
                trackerObject.name = tracker.Key;
                trackerObject.transform.parent = trackersParent.transform;
                trackerObject.GetComponent<MeshRenderer>().material = trackersMaterial;
            }
            else
                trackerObject.transform.position = tracker.Value;
        }
        foreach (KeyValuePair<string,Vector3> tracker in objectTrackers) {
            GameObject trackerObject = GameObject.Find(tracker.Key);
            if (trackerObject == null) {
                trackerObject = Instantiate(trackersPrefab, tracker.Value, Quaternion.identity);
                trackerObject.name = tracker.Key;
                trackerObject.transform.parent = trackersParent.transform;
                trackerObject.GetComponent<MeshRenderer>().material = trackersMaterial;
            }
            else
                trackerObject.transform.position = tracker.Value;
        }
    }

    void OnDestroy() {
        if (this.ws != null)
            ws.Close();
    }

}

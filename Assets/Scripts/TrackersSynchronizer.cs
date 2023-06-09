/* This script at each iteration check that the position of the trackers is well represented in the simulation */
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class TrackersSynchronizer : MonoBehaviour {
    /* General settings variables */
    private bool isReady = false;
    public bool IsReady { get { return isReady; } }

    /* Trackers settings */
    public GameObject trackersPrefab;
    public Material trackersMaterial;
    public bool showTrackersInScene = true;
    private GameObject trackersParent;

    /* Rigidbody settings */
    public GameObject rigidbodiesPrefab;
    public Material rigidbodiesMaterial;
    public bool showRigidbodiesInScene = true;
    private GameObject rigidbodiesParent;

    /* Trackers positions */
    private Dictionary<string,Vector3> holderTrackers = new Dictionary<string,Vector3>();
    private Dictionary<string,Vector3> headsetTrackers = new Dictionary<string,Vector3>();
    private Dictionary<string,Vector3> objectTrackers = new Dictionary<string,Vector3>();

    private List<TrackerOptitrack> trackers = new List<TrackerOptitrack>();
    private List<RigidbodyOptitrack> rigidbodies = new List<RigidbodyOptitrack>();

    private RigidbodyOptitrack objectTracker;
    private Vector3 centerOffset = new Vector3(0f, -0.025f, 0f);

    /* Websocket management */
    private WebSocket ws;

    void Start() {
        /* Generate the trackers parent */
        trackersParent = new GameObject("Trackers");
        trackersParent.name = "Trackers";

        rigidbodiesParent = new GameObject("Rigidbodies");
        rigidbodiesParent.name = "Rigidbodies";

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
        /* Get the data from the server and make them compatible with the C# code */
        jsonData = jsonData.Replace("\\", "");
        int numberOfCurlyBrackets = 0, numberOfSquaredBrackets = 0;
        int initialIndex = 0;
        List<TrackerOptitrack> trackers = new List<TrackerOptitrack>();
        List<RigidbodyOptitrack> rigidbodies = new List<RigidbodyOptitrack>();
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
                    if (json.Contains("marker"))
                        trackers.Add(JsonUtility.FromJson<TrackerOptitrack>(json));
                    else if (json.Contains("rigidBody")) {
                        RigidbodyOptitrack rigidbody = JsonUtility.FromJson<RigidbodyOptitrack>(json);
                        if (rigidbody.ID == Parameters.RASPBERRY_ID.ToString())
                            this.objectTracker = rigidbody;
                        rigidbodies.Add(JsonUtility.FromJson<RigidbodyOptitrack>(json));
                    }
                }
            }
        }
        this.trackers = trackers;
        this.rigidbodies = rigidbodies;
        /* Divide the trackers into the various dictionaries */
        // this.makeTrackersCompatible(trackers);

        this.isReady = true;
    }

    /*private void makeTrackersCompatible(List<Tracker> trackers) {
        List<Tracker> holderTrackers = new List<Tracker>();
        // Divide the trackers based on the object
        foreach (Tracker tracker in trackers) {
            if (tracker.object == Parameters.HOLDER_REFERENCE_NAME)
                holderTrackers.Add(tracker);
        }

        // Set the trackers into the dictionaries
        // Get the tracker in holderTrackers with the highest y position
        int indexHighestTracker = 0;
        for(int x = 0; x < holderTrackers.Count; x++) {
            if (holderTrackers[x].y > holderTrackers[indexHighestTracker].y)
                indexHighestTracker = x;
        }
        Tracker topTracker = holderTrackers[indexHighestTracker];
        holderTrackers.RemoveAt(indexHighestTracker);
        // Order the remaining trackers in a clockwise order
        List<Tracker> orderedTrackers = new List<Tracker>();
        orderedTrackers.Add(topTracker);
        while (holderTrackers.Count > 0) {
            int indexTracker = 0;
            for (int x = 0; x < holderTrackers.Count; x++) {
                if (holderTrackers[x].x > holderTrackers[indexTracker].x)
                    indexTracker = x;
            }
            orderedTrackers.Add(holderTrackers[indexTracker]);
            holderTrackers.RemoveAt(indexTracker);
        }
    }*/

    public void Update() {
        if (this.isReady)
            this.updateTrackersInSimulation();
    }

    public Vector3 getObjectPosition() {
        return objectTracker.position + centerOffset;
    }

    public Quaternion getObjectRotation() {
        return objectTracker.rotation;
    }

    /* Update the trackers positions in the simulation */
    public void updateTrackersInSimulation() {
        if (!showTrackersInScene) return;

        foreach (RigidbodyOptitrack rigidbody in rigidbodies) {
            GameObject rigidbodyObject = GameObject.Find(rigidbody.name);
            if (rigidbodyObject == null) {
                rigidbodyObject = Instantiate(rigidbodiesPrefab, rigidbody.position, rigidbody.rotation);
                rigidbodyObject.name = rigidbody.name;
                rigidbodyObject.transform.parent = rigidbodiesParent.transform;
                rigidbodyObject.GetComponent<MeshRenderer>().material = rigidbodiesMaterial;
            } else {
                rigidbodyObject.transform.position = rigidbody.position;
            }
        }

        foreach (TrackerOptitrack tracker in trackers) {
            GameObject trackerObject = GameObject.Find(tracker.name);
            if (trackerObject == null) {
                trackerObject = Instantiate(trackersPrefab, tracker.position, Quaternion.identity);
                trackerObject.name = tracker.name;
                trackerObject.transform.parent = trackersParent.transform;
                trackerObject.GetComponent<MeshRenderer>().material = trackersMaterial;
            } else {
                trackerObject.transform.position = tracker.position;
            }
        }
    }

    void OnDestroy() {
        if (this.ws != null)
            ws.Close();
    }

}

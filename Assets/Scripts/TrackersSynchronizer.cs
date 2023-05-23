/* This script at each iteration check that the position of the trackers is well represented in the simulation */
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using ROSMessageType = RosMessageTypes.Std.EmptyMsg; // Need to understand the message type

public class TrackersSynchronizer : MonoBehaviour {
    /* Trackers settings */
    public GameObject trackersPrefab;
    public Material trackersMaterial;
    public bool showTrackersInScene = true;
    private GameObject trackersParent;

    /* Informations to compute the center of the object */
    private Vector3 refVector;
    public Vector3 offsetToP1 = new Vector3(0,-0.1f,0); // To measure

    /* Trackers */
    private Dictionary<string,Vector3> tableTrackers = new Dictionary<string, Vector3>(); // This is generated from the holder trackers
    public Dictionary<string,Vector3> TableTrackers { get { return tableTrackers; } }
    
    private Dictionary<string,Vector3> holderTrackers = new Dictionary<string, Vector3>(); /* Trackers relative to the structure */
    public Dictionary<string,Vector3> HolderTrackers { get { return holderTrackers; } }
    
    private Dictionary<string,Vector3> headsetTrackers = new Dictionary<string, Vector3>(); /* Trackers relative to the headset to be able to synchronize the position of the headset with the position of the system */
    public Dictionary<string,Vector3> HeadsetTrackers { get { return headsetTrackers; } }
    
    private Dictionary<string,Vector3> objectTrackers = new Dictionary<string, Vector3>(); /* Trackers relative to the object (the raspberry) */
    public Dictionary<string,Vector3> ObjectTrackers { get { return objectTrackers; } }

    private bool isReady = false;
    public bool IsReady { get { return isReady; } }

    /* Start method */
    void Start() {
        /* Generate the parent gameobject */
        trackersParent = new GameObject("Trackers");
        trackersParent.name = "Trackers";

        /* Set the connection with the ROS server */
        //ROSConnection.GetOrCreateInstance().Subscribe<ROSMessageType>(Parameters.ROS_PUB_NAME, refreshTrackers);
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

        foreach (KeyValuePair<string,Vector3> tracker in tableTrackers) {
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

    /* Refresh the trackers positions */
    public void refreshTrackers() { //ROSMessageType serverResponse) {
        /* Estimate the various trackers positions */
        // TO IMPLEMENT

        /* Table trackers */
        tableTrackers.Add("front-left", new Vector3(0,1.0f,0));
        tableTrackers.Add("back-right", new Vector3(1.0f,1.0f,0.5f));
        
        /* Holder trackers */
        holderTrackers.Add("front-left-low", new Vector3(0.3f,1.05f,0));
        holderTrackers.Add("back-right-low", new Vector3(0.7f,1.05f,0.5f));
        holderTrackers.Add("rope-attach-point", new Vector3(0.5f,1.55f,0));
        Vector3 topCenterFrontTracker = holderTrackers["rope-attach-point"] + new Vector3(0,0.5f,0);
        holderTrackers.Add("top-center-front", topCenterFrontTracker);
        Vector3 topCenterBackTracker = new Vector3(holderTrackers["rope-attach-point"].x,holderTrackers["rope-attach-point"].y + 0.5f,holderTrackers["back-right-low"].z - 0.1f);
        holderTrackers.Add("top-center-back", topCenterBackTracker);

        /* Calibrate the reference vector */
        if (refVector == null) calibrateRefVector();

        /* Update the trackers in the simulation */
        updateTrackersInSimulation();
    }
}

/* This script at each iteration check that the position of the trackers is well represented in the simulation */
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using ROSMessageType = RosMessageTypes.Std.EmptyMsg; // Need to understand the message type

public class TrackersSynchronizer : MonoBehaviour {
    public GameObject trackersPrefab;
    public Material trackersMaterial;
    public bool showTrackersInScene = true;
    public GameObject trackersParent;

    public GameObject sceneLoader;

    /* Saved trackers */
    private Dictionary<string,Vector3> tableTrackers = new Dictionary<string, Vector3>();
    private Dictionary<string,Vector3> holderTrackers = new Dictionary<string, Vector3>();
    private Dictionary<string,Vector3> headsetTrackers = new Dictionary<string, Vector3>();
    private Dictionary<string,Vector3> objectTrackers = new Dictionary<string, Vector3>();

    // Start is called before the first frame update
    void Start() {
        /* Create an empty game object to contain the trackers */
        trackersParent = new GameObject("Trackers");
        trackersParent.name = "Trackers";
        trackersParent.transform.parent = this.transform;

        tableTrackers.Add("TABLE_FRONTLEFT", new Vector3(0,1.0f,0));
        tableTrackers.Add("TABLE_BACKRIGHT", new Vector3(1.0f,1.0f,0.5f));
        holderTrackers.Add("HOLDER_FRONTLEFTLOW", new Vector3(0.3f,1.10f,0));
        holderTrackers.Add("HOLDER_BACKRIGHTLOW", new Vector3(0.7f,1.10f,0.5f));
        holderTrackers.Add("HOLDER_TOPCENTERFRONT", new Vector3(0.5f,1.6f,0));
        holderTrackers.Add("HOLDER_TOPCENTERBACK", new Vector3(0.5f,1.6f,0.3f));
        holderTrackers.Add("HOLDER_ROPEATTACH", new Vector3(0.5f,1.55f,0));
        headsetTrackers.Add("HEADSET_CENTER", new Vector3(-1f, 0f, -1f));

        //ROSConnection.GetOrCreateInstance().Subscribe<ROSMessageType>(Parameters.ROS_PUB_NAME, refreshTrackers);
    }

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

    public void refreshTrackers(ROSMessageType serverResponse) {
        /* Estimate the various trackers positions */
        // TO DO 

        /* Update the trackers in the simulation */
        updateTrackersInSimulation();

        /* Refresh the scene */
        sceneLoader.GetComponent<SceneLoader>().refreshScene(tableTrackers, holderTrackers, headsetTrackers, objectTrackers);
    }
}

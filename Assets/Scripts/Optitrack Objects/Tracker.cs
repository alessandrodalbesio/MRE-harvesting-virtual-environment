using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TrackerOptitrack {
    public string type;
    public string ID;
    public string rigidBody;
    public string name {
        get {
            return "Marker "+ ID + " - Rigidbody " + rigidBody;
        }
    }
    public float x;
    public float y;
    public float z;
    public Vector3 position {
        get {
            return new Vector3(-x, y, z);
        }
    }
}
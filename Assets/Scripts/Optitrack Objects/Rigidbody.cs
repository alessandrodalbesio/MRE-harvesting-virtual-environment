using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class RigidbodyOptitrack {
    public string type;
    public string ID;
    public float x;
    public float y;
    public float z;
    public Vector3 position {
        get {
            return new Vector3(-x, y, z);
        }
    }
    public string name {
        get {
            return "Rigidbody " + ID;
        }
    }
    public float qx;
    public float qy;
    public float qz;
    public float qw;
    public Quaternion rotation {
        get {
            return new Quaternion(-qx, qy, qz, qw);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Tracker {
    private Vector3 position;
    public List<float> pos {
        set {
            position = new Vector3(-value[0], value[1], value[2]);
        }
    }
    public int _id;
    public string id {
        set {
            _id = int.Parse(value);
        }
        get {
            return "Tracker ID: " + this._id.ToString();
        }
    }
    public float residual;
}
using UnityEngine;
using System.Collections;

public class AppliedForce {
    public Vector3 point { get; set; }
    public float magnitude { get; set; }

    public AppliedForce(Vector3 point, float magnitude) {
        this.point = point;
        this.magnitude = magnitude;
    }

    public override string ToString() {
        return point + " " + magnitude;
    }
}

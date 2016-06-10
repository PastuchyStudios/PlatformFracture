using UnityEngine;
using System.Collections;

public abstract class ForceReceiver : MonoBehaviour {
    public abstract void receiveHit(AppliedForce hit);
}

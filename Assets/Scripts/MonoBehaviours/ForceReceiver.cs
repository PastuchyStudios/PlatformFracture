using UnityEngine;
using System.Collections;


abstract class ForceReceiver : MonoBehaviour {
    public abstract void receive(PlatformHit hit);
}

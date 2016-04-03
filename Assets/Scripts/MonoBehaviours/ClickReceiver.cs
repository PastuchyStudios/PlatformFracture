using UnityEngine;
using System.Collections;

public abstract class ClickReceiver : MonoBehaviour {
    public abstract void receive(RaycastHit hit);
}

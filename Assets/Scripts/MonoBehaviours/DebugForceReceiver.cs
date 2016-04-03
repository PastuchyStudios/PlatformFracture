using UnityEngine;
using System.Collections;

class DebugForceReceiver : ForceReceiver {
    public override void receive(PlatformHit hit) {
        Debug.Log(hit);
    }
}

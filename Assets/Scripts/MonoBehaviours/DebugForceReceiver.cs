using UnityEngine;
using System.Collections;

class DebugForceReceiver : ForceReceiver {
    public override void receiveHit(PlatformHit hit) {
        Debug.Log(hit);
    }
}

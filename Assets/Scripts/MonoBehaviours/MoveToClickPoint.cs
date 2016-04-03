using UnityEngine;
using System.Collections;

class MoveToClickPoint : ClickReceiver {
    public override void receive(RaycastHit hit) {
        transform.position = new Vector3(hit.point.x, transform.position.y, hit.point.z);
    }
}

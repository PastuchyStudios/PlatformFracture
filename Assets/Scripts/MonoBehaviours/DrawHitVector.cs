using UnityEngine;
using System.Collections;

class DrawHitVector : ClickReceiver {
    public float visualScale = 1;
    public float outputScale = 1;
    public float minLength = 0;
    public ForceReceiver forceReceiver;

    private bool drawing = false;
    private Vector3 clickPosition;
    private Vector3 hitPosition;

    public override void receive(RaycastHit hit) {
        if (drawing) {
            return;
        }
        drawing = true;
//vectorObject.GetComponent<MeshRenderer>().enabled = true;
        clickPosition = Input.mousePosition;
        hitPosition = hit.point;
    }

    void Update() {
        if (!drawing) {
            return;
        }

        float deltaY = Mathf.Max(0, minLength, Input.mousePosition.y - clickPosition.y);

        Vector3 newScale = new Vector3(
            transform.localScale.x,
            deltaY * visualScale,
            transform.localScale.z);
        Vector3 newPosition = new Vector3(
            hitPosition.x,
            newScale.y,
            hitPosition.z);

        transform.localScale = newScale;
        transform.position = newPosition;

        if (Input.GetMouseButtonUp(0)) {
            drawing = false;
            //vectorObject.GetComponent<MeshRenderer>().enabled = false;
            if (forceReceiver != null) {
                forceReceiver.receive(new PlatformHit(hitPosition, deltaY * outputScale));
            }
        }
    }
}

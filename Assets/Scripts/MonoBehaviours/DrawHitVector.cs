using UnityEngine;
using System.Collections;

class DrawHitVector : ClickReceiver {
    public float visualScale = 1;
    public float outputScale = 1;
    public float minLength = 0;

    private bool drawing = false;
    private Vector3 clickPosition;
    private RaycastHit hit;

    public override void receive(RaycastHit hit) {
        if (drawing) {
            return;
        }
        drawing = true;
        GetComponent<MeshRenderer>().enabled = true;
        clickPosition = Input.mousePosition;
        this.hit = hit;
    }

    void Start() {
        GetComponent<MeshRenderer>().enabled = false;
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
            hit.point.x,
            newScale.y,
            hit.point.z);

        transform.localScale = newScale;
        transform.position = newPosition;

        if (Input.GetMouseButtonUp(0)) {
            drawing = false;
            GetComponent<MeshRenderer>().enabled = false;
            ForceReceiver forceReceiver = hit.transform.gameObject.GetComponent<ForceReceiver>();
            if (forceReceiver != null) {
                forceReceiver.receiveHit(new AppliedForce(hit.point, deltaY * outputScale));
            }
        }
    }
}

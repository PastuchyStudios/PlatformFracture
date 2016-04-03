using UnityEngine;
using System.Collections;

public class MouseCameraControl : MonoBehaviour {
    public Transform rotationHandle;
    public Transform translationHandle;
    public int mouseButton;

    public float nearDistance;
    public float farDistance;

    public float rotationRate;
    public float zoomingRate;

    private Vector3? dragOrigin = null;

    void Start() {
        if (rotationHandle == null) {
            rotationHandle = transform;
        }

        if (translationHandle == null) {
            translationHandle = transform;
        }
    }

    void LateUpdate() {
        if (dragOrigin == null && Input.GetMouseButtonDown(mouseButton)) {
            dragOrigin = Input.mousePosition;
        } else if (dragOrigin != null && Input.GetMouseButtonUp(mouseButton)) {
            dragOrigin = null;
        }

        if (dragOrigin != null) {
            Vector3 newDragOrigin = Input.mousePosition;
            Vector3 delta = newDragOrigin - (Vector3) dragOrigin;

            rotationHandle.Rotate(Vector3.up, delta.x * rotationRate);
            var zValue = translationHandle.localPosition.z + delta.y * zoomingRate;
            float clampedZValue = Mathf.Clamp(zValue, farDistance, nearDistance);
            translationHandle.localPosition = new Vector3(0, 0, clampedZValue);

            dragOrigin = newDragOrigin;
        }
    }
}

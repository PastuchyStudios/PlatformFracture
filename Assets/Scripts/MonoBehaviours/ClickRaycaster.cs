using UnityEngine;
using System.Collections;

public class ClickRaycaster : MonoBehaviour {
    public string filterTag;
    public ClickReceiver clickReceiver;
    
    void Update() {
        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 20) && hit.transform != null) {
                if (hit.transform.CompareTag(filterTag)) {
                    clickReceiver.receive(hit);
                }
                Transform hitAncestor = hit.transform;
                while (hitAncestor.parent != null) {
                    hitAncestor = hitAncestor.parent;
                    if (hitAncestor.CompareTag(filterTag)) {
                        clickReceiver.receive(hit);
                    }
                }
            }
        }
    }
}

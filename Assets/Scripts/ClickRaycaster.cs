using UnityEngine;
using System.Collections;

public class ClickRaycaster : MonoBehaviour {
    public Transform objectFilter;
    public ClickReceiver clickReceiver;
    
    void Update() {
        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 20) && hit.transform == objectFilter) {
                clickReceiver.receive(hit);
            }
        }
    }
}

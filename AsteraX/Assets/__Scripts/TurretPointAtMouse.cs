
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretPointAtMouse : MonoBehaviour {
    
    private Vector3 mousePoint3D;
    
    void Update()
    {
        PointAtMouse();
    }
    
    void PointAtMouse()
    {
        mousePoint3D = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.back * Camera.main.transform.position.z);
        transform.LookAt(mousePoint3D, Vector3.back);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When a GameObject exits the bounds of the OnScreenBounds, screen wrap it.
/// </summary>
public class OffScreenWrapper : MonoBehaviour {

	Bullet bulletScript;
	
	void Start(){
        bulletScript = gameObject.GetComponent<Bullet>();
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!enabled)
        {
            return;
        }
        
        ScreenBounds bounds = other.GetComponent<ScreenBounds>();
        if (bounds == null) {
            bounds = other.GetComponentInParent<ScreenBounds>();
            if (bounds == null) { // If bounds is still null, return
            return;
            } else {
                
                Vector3 pos = transform.position.ComponentDivide(other.transform.localScale);
                pos.z = 0; 
                transform.position = pos;
                Debug.LogWarning("OffScreenWrapper:OnTriggerExit() - Runaway object caught by ExtraBounds: "+gameObject.name);
            }
        }

        ScreenWrap(bounds);
    }

    
    private void ScreenWrap(ScreenBounds bounds) {
        
        Vector3 relativeLoc = bounds.transform.InverseTransformPoint(transform.position);
        
        if (Mathf.Abs(relativeLoc.x) > 0.5f)
        {
            relativeLoc.x *= -1;
        }
        if (Mathf.Abs(relativeLoc.y) > 0.5f)
        {
            relativeLoc.y *= -1;
        }
        transform.position = bounds.transform.TransformPoint(relativeLoc);
        
        if (bulletScript != null)
        {
            bulletScript.bDidWrap = true;
        }

    }

}
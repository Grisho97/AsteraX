using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof(Text) )]
public class JumpsGT : MonoBehaviour {
    Text    txt;
    
	void Start () {
        txt = GetComponent<Text>();
	}
	
	void Update () {
		txt.text = (PlayerShip.JUMPS >= 0) ? PlayerShip.JUMPS+" Jumps" : "";
	}
}

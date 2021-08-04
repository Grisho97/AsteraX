using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveOnlyDuringSomeGameStates : MonoBehaviour {

    
    [EnumFlags] 
    public AsteraX.eGameState   activeStates = AsteraX.eGameState.all;

	
	public virtual void Awake () {
       
        AsteraX.GAME_STATE_CHANGE_DELEGATE += DetermineActive;
        
        DetermineActive();
	}

    protected void OnDestroy()
    {
        AsteraX.GAME_STATE_CHANGE_DELEGATE -= DetermineActive;
    }


    protected virtual void DetermineActive()
    {
        bool shouldBeActive = (activeStates & AsteraX.GAME_STATE) == AsteraX.GAME_STATE;

        gameObject.SetActive(shouldBeActive);
    }
    
}

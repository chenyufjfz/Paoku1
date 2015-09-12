#pragma strict

/*
*	FUNCITON:
*	This script checks if there is a terrain layer under the player character.
*	If there isn't the player will fall and its energy will be reduced to zero.
*	
*	USED BY: This script is part of the "Player" prefab.
*
*/

private var tPlayer : Transform;
private var bPitFallingStart : boolean = false;
private var fCurrentEnergyDepletionSpeed : float = 10.0;

private var hInGameScript : InGameScript;
private var hControllerScript : ControllerScript;

function Start()
{
	tPlayer = GameObject.Find("Player").transform;
	bPitFallingStart = false;
	
	hInGameScript = this.GetComponent(InGameScript) as InGameScript;
	hControllerScript = this.GetComponent(ControllerScript) as ControllerScript;
}

function Update()
{
	if(hInGameScript.isGamePaused()==true)
		return;
	
	if(bPitFallingStart)
	{
		hInGameScript.decrementEnergy( (hInGameScript.getCurrentEnergy()/10.0) + Time.deltaTime*100);
	}
}

/*
*	FUNCTION: Reduce energy if player fell int a pit
*/
public function setPitValues()
{
	bPitFallingStart = true;
		
	hControllerScript.setPitFallLerpValue(Time.time);
	hControllerScript.setPitFallForwardSpeed(hControllerScript.getCurrentForwardSpeed());
			
	print("Fell in a Pit");
}

public function isFallingInPit() { return bPitFallingStart; }
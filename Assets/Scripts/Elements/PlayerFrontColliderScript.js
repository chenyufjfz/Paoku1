#pragma strict

/*
*	FUNCTION: Controls all frontal collisions.
*
*	USED BY: This script is part of the PlayerFrontCollider prefab.
*
*/

private var bFrontColliderFlag : boolean;

private var hPlayerSidesColliderScript : PlayerSidesColliderScript;
private var hInGameScript : InGameScript;

function Start()
{
	bFrontColliderFlag = true;
	
	hPlayerSidesColliderScript = GameObject.Find("PlayerSidesCollider").GetComponent(PlayerSidesColliderScript) as PlayerSidesColliderScript;
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
}

function OnCollisionEnter(collision : Collision)
{		
	if (bFrontColliderFlag == true)
	{
		hPlayerSidesColliderScript.deactivateSidesCollider();	//dont detect stumbles on death
		hInGameScript.collidedWithObstacle();	//play the death scene
	}
}

public function isFrontColliderActive() { return bFrontColliderFlag; }
public function activateFrontCollider() { bFrontColliderFlag = true; }
public function deactivateFrontCollider() { bFrontColliderFlag = false; }
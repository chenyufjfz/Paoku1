#pragma strict

/*
*	FUNCTION:
*	- This script controls the Enemy (Police Car) based on the player’s movement.
*	- It controls the enemy's animatations and it's behavior if the player stumbles.
*
*	USED BY:
*	This script is a part of the “Enemy” prefab.
*
*/

private var tEnemy : Transform;	//enemy transform
private var tPlayer : Transform;//player transform

private var iEnemyState : int = 0;
private var fDeathRotation : float = 0.0;
private var fCosLerp : float = 0.0;	//used for Lerp

//script references
private var hInGameScript : InGameScript;
private var hControllerScript : ControllerScript;
private var hSoundManager : SoundManager;

//enemy logic
private var fEnemyPosition : float = 0.0;
private var fEnemyPositionX : float = -5;
private var fEnemyPositionY : float = 0;
private var fStumbleStartTime : float;
private var fChaseTime : float = 5;

function Start()
{	
	tPlayer = GameObject.Find("Player").transform;
	tEnemy = this.transform;
	
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
	hControllerScript = GameObject.Find("Player").GetComponent(ControllerScript) as ControllerScript;
	hSoundManager = GameObject.Find("SoundManager").GetComponent(SoundManager) as SoundManager;	
}

/*
*	FUNCTION: Starting the chasing sequence
*	CALLED BY: ControllerScript.launchGame()
*/
public function launchEnemy():void
{
	iEnemyState = 2;
}

function FixedUpdate ()
{
	if(hInGameScript.isGamePaused()==true)
		return;
		
	//set the position of guard in current frame
	tEnemy.position.x = Mathf.Lerp(tEnemy.position.x, (tPlayer.position.x - fEnemyPosition), Time.deltaTime*10);
		
	if (!hControllerScript.isInAir())//follow the player in y-axis if he's not jumping (cars cant jump)
		tEnemy.position.y = Mathf.Lerp(tEnemy.position.y, tPlayer.position.y + fEnemyPositionY, Time.deltaTime*8);
	
	//ignore y-axis rotation and horizontal movement in idle and death state
	if (iEnemyState < 4)
	{
		tEnemy.position.z = Mathf.Lerp(tEnemy.position.z, tPlayer.position.z, Time.deltaTime*10);
		tEnemy.localEulerAngles.y = -hControllerScript.getCurrentPlayerRotation();
	}
	
	if (iEnemyState == 1)//hide the chasing character
	{
		fCosLerp += (Time.deltaTime/10);
		fEnemyPosition = Mathf.Lerp(fEnemyPosition, fEnemyPositionX + 45, Mathf.Cos(fCosLerp)/1000);
		
		if (fCosLerp >= 0.7)
		{
			fCosLerp = 0.0;
			iEnemyState = 0;
			
			hSoundManager.stopSound(EnemySounds.Siren);
		}
	}
	else if (iEnemyState == 2)//show the chasing character
	{
		hSoundManager.playSound(EnemySounds.Siren);
		
		fCosLerp += (Time.deltaTime/4);
		fEnemyPosition = Mathf.Lerp(fEnemyPosition, fEnemyPositionX, Mathf.Cos(fCosLerp));
		
		if (fCosLerp >= 1.5)
		{
			fCosLerp = 0.0;
			iEnemyState = 3;
		}
	}
	else if (iEnemyState == 3)//wait for 'fChaseTime' after showing character
	{
		if ( (Time.time - fStumbleStartTime)%60 >= fChaseTime)
			iEnemyState = 1;
	}
	
	//DEATH SEQUENCE
	else if (iEnemyState == 4)//on death
	{	
		tEnemy.localEulerAngles.y = 350;//to ensure correct rotation animation
		
		hSoundManager.playSound(EnemySounds.TiresSqueal);
		iEnemyState = 5;
	}
	else if (iEnemyState == 5)//pin behind the player
	{
		fEnemyPosition = Mathf.Lerp(fEnemyPosition, fEnemyPositionX+20, Time.fixedDeltaTime*50);//vertical position after skid
		tEnemy.position.z = Mathf.Lerp(tEnemy.position.z, tPlayer.position.z + 20, Time.deltaTime*10);//horizontal position after skid
		
		tEnemy.localEulerAngles = Vector3.Lerp(tEnemy.localEulerAngles, Vector3(0,260,0), Time.deltaTime*10);//90 degree rotation
		if (tEnemy.localEulerAngles.y <= 261)
			iEnemyState = 6;
	}
	else if (iEnemyState == 6)
	{
		hSoundManager.stopSound(EnemySounds.Siren);
	}
}//end of Update

/*
*	FUNCTION: Animate enemy
*	RETURNS:	'true' if the enemy was already chasing player
*				'false' if the enemy was not chasing the player
*	CALLED BY: ControllerScript.processStumble()
*/
public function processStumble():boolean
{
	if (isEnemyActive())//if enemy is already chasing player
	{
		iEnemyState = 0;		
		return true;
	}
	else
	{
		fStumbleStartTime = Time.time;
		iEnemyState = 2;		
		return false;
	}
}

public function playDeathAnimation():void { iEnemyState = 4; }
public function hideEnemy() { iEnemyState = 1; }

/*
*	FUNCTION: Check if the enemy is chasing the player
*/
public function isEnemyActive():boolean
{
	if (iEnemyState == 2 || iEnemyState == 3)
		return true;
	else
		return false;
}
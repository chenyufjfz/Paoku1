#pragma strict

/*
*	FUNCTION: This script defines the behavior of powerups and currency units.
*	
*	USED BY: This script is a part of every powerup and currency unit.
*
*/

var powerupType : PowerUps;
var frequency : int;	//occurance frequency

private var tPlayer : Transform;//player transform
private var PUState : int = 0;
private var StartTime : float = 0.0;

//script references
private var hInGameScript : InGameScript;
private var hPowerupsMainController : PowerupsMainController;

private var v3StartPosition : Vector3;
private var bDestroyWhenFarFlag : boolean = false;
private var v3DistanceVector : Vector3;
private var fCatchRadius : float = 200;//the radius at which Power Ups are pulled towards the character
private var v3CurrencyLerpPosition : Vector3;

/*
*	FUNCTION: Make arrangements for reuse of the object
*/
function initPowerupScript()
{
	PUState = 0;
	bDestroyWhenFarFlag = false;
	transform.localScale = Vector3(1,1,1);
	StartTime = 0.0;
	v3DistanceVector = Vector3(0,0,0);
	
	toggleMeshRender(true);
}

function Start()
{
	tPlayer = GameObject.Find("Player").transform;
	
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
	hPowerupsMainController = GameObject.Find("Player").GetComponent(PowerupsMainController) as PowerupsMainController;
}

function Update ()
{
	if(hInGameScript.isGamePaused()==true)
		return;
		
	if(PUState==1)//hide the powerup
	{
		if (hPowerupsMainController.isPowerupActive(PowerUps.Magnetism) == true)	//magnetism powerup is active
		{
			//adjust the currency's height
			v3CurrencyLerpPosition = tPlayer.position;
			v3CurrencyLerpPosition.x += 2;
			v3CurrencyLerpPosition.y += 5;
			
			//pull the currency towards the player
			transform.position = Vector3.Lerp(transform.position,v3CurrencyLerpPosition,(Time.time-StartTime)/0.8);
			transform.localScale = Vector3.Lerp(transform.localScale,Vector3(0.1,0.1,0.1),(Time.time-StartTime)/0.8);
		}
		else//regular cases
		{			
			//pull the currency towards the player
			transform.position = Vector3.Lerp(transform.position,tPlayer.position,(Time.time-StartTime)/0.2);
			transform.localScale = Vector3.Lerp(transform.localScale,Vector3(0.01,0.01,0.01),(Time.time-StartTime)/0.002);
		}
		
		if((Time.time - StartTime)>0.2)
		{	
			//disable currency if magnetism is activated
			if (powerupType == PowerUps.Currency || hPowerupsMainController.isPowerupActive(PowerUps.Magnetism) == true)			
				toggleMeshRender(false);//make currency invisible			
			else			
				this.gameObject.SetActive(false);//deactivate object			
		}
		return;
	}
	
	v3DistanceVector = transform.position - tPlayer.position;
	
	//destroy not collect currency/ powerup
	if(v3DistanceVector.sqrMagnitude<40000.0)	
		bDestroyWhenFarFlag = true;
	
	//destroy currency or powerup if not collected
	if(bDestroyWhenFarFlag==true)
		if(v3DistanceVector.sqrMagnitude>90000.0)
		{
			if (powerupType == PowerUps.Currency)			
				toggleMeshRender(false);			
			else
				this.gameObject.SetActive(false);
		}

	if(powerupType==PowerUps.Currency)//currency pull radius	
		fCatchRadius = hPowerupsMainController.getMagnetismRadius();
		
	if(v3DistanceVector.sqrMagnitude<fCatchRadius)//catch the orb
	{
		PUState = 1;//hide the orb
		StartTime = Time.time;
		
		hPowerupsMainController.collectedPowerup(powerupType);//tell power-up main script what has been collected		
	}
}//end of update

/*
*	FUNCTION: Make the object invisible
*/
private function toggleMeshRender(bState:boolean)
{
	if (powerupType == PowerUps.Currency)
	{
		(this.transform.Find("A_Crystal").GetComponent(MeshRenderer) as MeshRenderer).enabled = bState;
		(this.transform.Find("Shadow").GetComponent(MeshRenderer) as MeshRenderer).enabled = bState;
	}
	else if (powerupType == PowerUps.Magnetism)
	{		
		(this.transform.Find("Center").GetComponent(MeshRenderer) as MeshRenderer).enabled = bState;
	}
}
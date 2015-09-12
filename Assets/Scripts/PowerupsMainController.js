#pragma strict

/*
*	FUNCTION:
*	- This script activates and deactivates powerups.
*	- It defines the execution of powerups.
*	- It handles and triggers events when a currency unit is collected.
*
*/

//all the powerups used in the game
public enum PowerUps
{	
	Magnetism = 0,
	Currency = 1
}

private var tPlayer : Transform;	//player transform

private var iCurrencyUnits : int = 0;	//curency earned in a particular run
private var fMangetismRadius : float;	//when to pull currency if magnetism is active
private var fMagnetismDefaultRadius : float;	//when to pull currency
private var iPowerupCount : int;	//a count of types of powerups

private var bPowerupStatus : boolean[];	//if and which powerup is active
private var fPowerupStartTime : float[];//the time when a powerup is started
private var fPowerupTotalDuration : float[];//total time to keep the powerup active

//script references
private var hInGameScript : InGameScript;
private var hSoundManager : SoundManager;
private var hControllerScript : ControllerScript;

private var tHUDPUMeter : Transform;//the HUD powerup meter
private var tHUDPUMeterBar : Transform;//the bar in the powerup meter on HUD

function Start()
{
	tPlayer = transform;	
	
	//powerup meter visual
	tHUDPUMeter = GameObject.Find("HUDMainGroup/HUDPUMeter").GetComponent(Transform) as Transform;
	tHUDPUMeterBar = GameObject.Find("HUDMainGroup/HUDPUMeter/HUD_PU_Meter_Bar_Parent").GetComponent(Transform) as Transform;
	
	iPowerupCount = PowerUps.GetValues(PowerUps).Length-1;//get the total number of powerups
	
	bPowerupStatus = new boolean[iPowerupCount];
	fPowerupStartTime = new float[iPowerupCount];	
	fPowerupTotalDuration = new float[iPowerupCount];

	hInGameScript = this.GetComponent(InGameScript) as InGameScript;
	hControllerScript = this.GetComponent(ControllerScript) as ControllerScript;
	hSoundManager = GameObject.Find("SoundManager").GetComponent(SoundManager) as SoundManager;
	
	tHUDPUMeter.transform.position.y -= 100;	//hide the powerup meter
	fMagnetismDefaultRadius = 200;
	fMangetismRadius = 200;		//default: pull currency toward the character
	iCurrencyUnits = 0;
	
	for(var i = 0; i <iPowerupCount ; i++)
	{
		bPowerupStatus[i] = false;
		fPowerupTotalDuration[i] = 10.0;//active time duration of the powerups
	}
}

function FixedUpdate ()
{	
	//pause the powerup's time if the game is paused
	if(hInGameScript.isGamePaused()==true)
	{
		for (var j=0; j<iPowerupCount; j++)
		{
			if (bPowerupStatus[j] == true)
				fPowerupStartTime[j] += Time.deltaTime;
		}
		return;
	}

	//count down timer for the active powerup
	for(var i : int = 0; i < iPowerupCount; i++)
	{
		if(bPowerupStatus[i]==true)
		{
			//reduce the meter bar
			PowerupHUDVisual( (Time.time - fPowerupStartTime[i]), fPowerupTotalDuration[i] );
			
			if(Time.time - fPowerupStartTime[i]>=fPowerupTotalDuration[i])//deactivate the PU when time runs out
			{
				deactivatePowerup(i);
			}
		}//end of if PU Active == true
	}//end of for i
}

/*
*	FUNCTION: Add collected currency or activate powerup
*	CALLED BY: PowerupScript.Update()
*/
public function collectedPowerup(index : int)
{
	if(index==PowerUps.Currency)//if a currency unit is collected
	{
		iCurrencyUnits += 1;	//add 1 to the currency count
		hSoundManager.playSound(PowerupSounds.CurrencyCollection);//play collection sound

		return;
	}
	
	fPowerupStartTime[index] = Time.time;	//set the time when powerup collected
	activatePowerUp(index);		//activate powerup if collected
}

/*
*	FUNCTION: Enable the powerup's functionality
*	CALLED BY:	collectedPowerup()
*/
private function activatePowerUp(index : int)
{
	tHUDPUMeter.transform.position.y = -88.6;//dispaly power-up meter
	bPowerupStatus[index] = true;
		
	if(index == PowerUps.Magnetism)//Magnetism Powerup
	{
		fMangetismRadius =  fMagnetismDefaultRadius + 2300;
	}
}

/*
*	FUNCTION: Dactivate powerup when it time expires
*	CALLED BY: Update()
*/
public function deactivatePowerup(index : int)
{	
	tHUDPUMeter.transform.position.y = 5000;//hide power-up meter
	bPowerupStatus[index] = false;
	
	if(index == PowerUps.Magnetism)//Magnetism Powerup
	{
		fMangetismRadius = fMagnetismDefaultRadius;
	}	
}

/*
*	FUNCTION: Deactivate all active powerups
*	CALLED BY:	InGameScript.Update()
*/
public function deactivateAllPowerups()
{
	for (var i=0; i< (PowerUps.GetValues(PowerUps).Length-2); i++)
	{
		if (bPowerupStatus[i] == true)
			deactivatePowerup(i);
	}
}

/*
*	FUNCTION: Reduce the powerup meter's bar when a powerup is activated
*	CALLED BY: Update()
*/
private function PowerupHUDVisual(fCurrentTime:float,fTotalTime:float)
{
	var iBarLength : float = tHUDPUMeterBar.transform.localScale.x;
	
	if (fCurrentTime <= 0)
		return;
	
	iBarLength = (fTotalTime-fCurrentTime)/fTotalTime;//calculate powerup meter bar's length
	tHUDPUMeterBar.transform.localScale.x = iBarLength;//set the length
}

/*
*	FUNCTION: Get the radius of magnetism effect
*/
public function getMagnetismRadius() { return fMangetismRadius; }

/*
*	FUNCTION: Get the currency collected in current run
*/
public function getCurrencyUnits() { return iCurrencyUnits; }

/*
*	FUNCTION: Check if any powerup is active
*	CALLED BY:	ElementsGenerator.getRandomElement()
*/
public function isPowerupActive():boolean
{
	for (var i=0; i<iPowerupCount; i++)
	{
		if (bPowerupStatus[i] == true)
			return true;
	}
	
	return false;
}

/*
*	FUNCTION: Check if a particular powerup is active
*	PARAMETER 1: The powerup which needs to be checked
*	CALLED BY:	PowerupScript.Update()
*/
public function isPowerupActive(ePUType:PowerUps):boolean
{
	return bPowerupStatus[ePUType];
}
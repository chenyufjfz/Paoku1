#pragma strict
/*
*	FUNCTION:
*	- Controls the HUD display which includes the score and currency.
*	
*	USED BY: This script is a part of the "Player" prefab.
*
*/

private var tPlayer : Transform;	//player transfrom

//script references
private var hInGameScript : InGameScript;
private var hPowerupsMainController : PowerupsMainController;
private var hControllerScript : ControllerScript;

private var tmHUDCurrencyText : TextMesh;
private var tmHUDScoreText : TextMesh;
private var tHUDScoreContainerMid : Transform;
private var tHUDCurrencyContainerMid : Transform;

//Calculate Score
private var fPreviousDistance : float = 0.0;	//mileage in the last frame
private var fCurrentDistance : float = 0.0;		//mileage in the current frame
private var fCurrentTime : float = 0.0;
private var fPreviousTime : float = 0.0;

//HUD element Container sizes
private var iDivisorScore : int;
private var iDivisorCurrency : int;
private var iDivisorMultiplier : int;

function Start()
{		
	tPlayer = GameObject.Find("Player").transform;
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;	
	hControllerScript = GameObject.Find("Player").GetComponent(ControllerScript) as ControllerScript;
	hPowerupsMainController = GameObject.Find("Player").GetComponent(PowerupsMainController) as PowerupsMainController;

	tmHUDCurrencyText = GameObject.Find("HUDMainGroup/HUDGroup/HUDCurrencyGroup/HUD_Currency_Text").GetComponent("TextMesh") as TextMesh;
	tmHUDScoreText = GameObject.Find("HUDMainGroup/HUDGroup/HUDScoreGroup/HUD_Score_Text").GetComponent("TextMesh") as TextMesh;
		
	tHUDScoreContainerMid = GameObject.Find("HUDMainGroup/HUDGroup/HUDScoreGroup/HUD_Score_BG").GetComponent(Transform) as Transform;	//	HUD Score Container	
	tHUDCurrencyContainerMid = GameObject.Find("HUDMainGroup/HUDGroup/HUDCurrencyGroup/HUD_Currency_BG").GetComponent(Transform) as Transform;	//	HUD Currency Container
		
	//get time difference to calculate score
	fCurrentTime = Time.time;
	fPreviousTime = Time.time;
	
	fPreviousDistance = 0;
	fCurrentDistance = 0;
	fCurrentTime = 0;
	fPreviousTime = 0;
	
	iDivisorScore = 10;
	iDivisorCurrency = 10;
	iDivisorMultiplier = 10;
	
	tHUDScoreContainerMid.localScale.z = 0.45;
	tHUDCurrencyContainerMid.localScale.z = 0.45;
	
	//call the resize Dight Container function every .5 seconds
	InvokeRepeating("resizeDigitContainer", 1, 0.5);
	resizeDigitContainer();
}

function FixedUpdate()
{	
	if(hInGameScript.isGamePaused()==true)
		return;

	UpdateHUDStats();
	
}//end of Update

/*
* 	FUNCTION: The score is calculated and added up in Level_Score variable
*	CALLED BY:	FixedUpdate()
*/
private function UpdateHUDStats()
{	
	yield WaitForEndOfFrame();
	
	//skip time and check the difference in milage in the duration
	if ( (fCurrentTime - fPreviousTime) >= 0.1 )
	{
		var iCurrentFrameScore = (fCurrentDistance - fPreviousDistance);
		hInGameScript.incrementLevelScore(iCurrentFrameScore);
		
		fPreviousDistance = fCurrentDistance;
		fCurrentDistance = hControllerScript.getCurrentMileage();
		
		fPreviousTime = fCurrentTime;
		fCurrentTime = Time.time;
	}
	else
	{
		fCurrentDistance = hControllerScript.getCurrentMileage();	//get the current mileage
		fCurrentTime = Time.time;
	}	
		
	tmHUDCurrencyText.text = hPowerupsMainController.getCurrencyUnits().ToString();	//update Currency on HUD
	tmHUDScoreText.text = hInGameScript.getLevelScore().ToString();				//update Score on HUD
}

/*
*	FUNCTION: Resize HUD Score and Currency containers according to digit count
*	CALLED BY:	Start() (invoke repeating)
*/
private function resizeDigitContainer()
{
	var fScore : int = hInGameScript.getLevelScore();
	var fCurrency : int = hPowerupsMainController.getCurrencyUnits();
		
	if ( (fScore / iDivisorScore) >= 1 )
	{
		tHUDScoreContainerMid.localScale.z += 0.4;	//expand the Score Container Mid
		iDivisorScore *= 10;
	}
	
	if ( (fCurrency / iDivisorCurrency) >= 1 )
	{
		tHUDCurrencyContainerMid.localScale.z += 0.4;		//expand the Currency Container Mid
		iDivisorCurrency *= 10;
	}
}
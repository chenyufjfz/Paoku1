#pragma strict
/*
*	FUNCTION:
*	- This script handles the menu instantiation, destruction and button event handling.
*	- Each menu item is always present in the scene. The inactive menus are hidden from the 
*	HUD Camera by setting their y position to 1000 units.
*	- To show a menu, the menu prefab's y position is set to 0 units and it appears in front of
*	the HUD Camera.
*
*	USED BY: This script is part of the MenuGroup prefab.
*
*	INFO:
*	To add a new menu, do the following in Editor:
*	- Add its name in the MenuIDs enum.
*	- Set its transform in tMenuTransforms array.
*	- Set its buttons' transforms in an array.
*	- Create its event handler. (E.g. handlerMainMenu(), handlerPauseMenu())
*	- Write implementation of each of the buttons in the handler function.
*	- Add its case in the listenerClicks() function.
*	- Add its case in the ShowMenu(...) function.
*	- Add its case in the CloseMenu(...) function.
*
*	ADDITIONAL INFO:
*	Unity's default GUI components are not used in the UI implementation to reduce 
*	performace overhead.
*
*/

//the available menus
public enum MenuIDs
{
	MainMenu = 0, 
	PauseMenu = 1,
	GameOverMenu = 2,
	InstructionsMenu = 3,
	SettingsMenu = 4
}

//events/ buttons on the pause menu
public enum PauseMenuEvents
{
	Resume = 0,
	MainMenu = 1
}

//events/ buttons on the game over menu
public enum GameOverMenuEvents
{
	Back = 0,
	Play = 1
}

private var tMenuGroup : Transform;//get the menu group parent
private var CurrentMenu:int = -1;	//current menu index
private var iTapState:int = 0;//state of tap on screen

private var aspectRatio : float = 0.0;
private var fResolutionFactor : float;	//displacement of menu elements based on device aspect ratio
public function getResolutionFactor():float { return fResolutionFactor; }

private var HUDCamera : Camera;//the menu camera

//script references
private var hControllerScript : ControllerScript;
private var hSoundManagerScript : SoundManager;
private var hInGameScript : InGameScript;

private var tMenuTransforms : Transform[];	//menu prefabs
//Main Menu
private var tMainMenuButtons:Transform[];	//main menu buttons' transform
private var iMainMenuButtonsCount:int = 3;	//total number of buttons on main menu
//Pause Menu
private var tPauseButtons:Transform[];	//pause menu buttons transforms
private var iPauseButtonsCount:int = 2;	//total number of buttons on pause menu
//Game Over Menu
private var tGameOverButtons:Transform[];
private var iGameOverButtonsCount:int = 1;
//instructions menu
private var tInstructionsButtons : Transform[];
private var iInstructionsButtonsCount : int = 1;
//settings menu
private var tSettingsButtons : Transform[];
private var iSettingsButtonsCount : int = 7;

//meshrenderers of all the radio buttons in settings menu
private var mrSwipeControls : MeshRenderer;
private var mrGyroControls : MeshRenderer;
private var mrMusicON : MeshRenderer;
private var mrMusicOFF : MeshRenderer;
private var mrSoundON : MeshRenderer;
private var mrSoundOFF : MeshRenderer;

//resume game countdown
private var iResumeGameState : int = 0;
private var iResumeGameStartTime: int = 0;
private var tmPauseCountdown : TextMesh;	//count down numbers after resume


function Start ()
{
	HUDCamera = GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(Camera) as Camera;
	hControllerScript = GameObject.Find("Player").GetComponent(ControllerScript) as ControllerScript;
	hSoundManagerScript = GameObject.Find("SoundManager").GetComponent(SoundManager) as SoundManager;
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
		
	//the fResolutionFactor can be used to adjust components according to screen size
	aspectRatio = ( (Screen.height * 1.0)/(Screen.width * 1.0) - 1.77);	
	fResolutionFactor = (43.0 * (aspectRatio));
		
	tMenuGroup = GameObject.Find("MenuGroup").transform;
	tMenuTransforms = new Transform[MenuIDs.GetValues(MenuIDs).Length];
	
	//main menu initialization
	tMenuTransforms[MenuIDs.MainMenu] = tMenuGroup.Find("MainMenu").GetComponent(Transform) as Transform;
	tMainMenuButtons = new Transform[iMainMenuButtonsCount];
	tMainMenuButtons[0] = tMenuTransforms[MenuIDs.MainMenu].Find("Buttons/Button_TapToPlay");
	tMainMenuButtons[1] = tMenuTransforms[MenuIDs.MainMenu].Find("Buttons/Button_Instructions");
	tMainMenuButtons[2] = tMenuTransforms[MenuIDs.MainMenu].Find("Buttons/Button_Settings");
	
	//pause menu initialization
	tMenuTransforms[MenuIDs.PauseMenu] = tMenuGroup.Find("PauseMenu").GetComponent(Transform) as Transform;
	tPauseButtons = new Transform[iPauseButtonsCount];
	tPauseButtons[0] = tMenuTransforms[MenuIDs.PauseMenu].Find("Buttons/Button_Back");
	tPauseButtons[1] = tMenuTransforms[MenuIDs.PauseMenu].Find("Buttons/Button_Resume");
		
	//game over menu initialization
	tMenuTransforms[MenuIDs.GameOverMenu] = tMenuGroup.Find("GameOver").GetComponent(Transform) as Transform;
	tGameOverButtons = new Transform[iGameOverButtonsCount];
	tGameOverButtons[0] = tMenuTransforms[MenuIDs.GameOverMenu].Find("Buttons/Button_Back");
	
	//instructions menu initialization
	tMenuTransforms[MenuIDs.InstructionsMenu] = tMenuGroup.Find("Instructions").GetComponent(Transform) as Transform;
	tInstructionsButtons = new Transform[iInstructionsButtonsCount];
	tInstructionsButtons[0] = tMenuTransforms[MenuIDs.InstructionsMenu].Find("Buttons/Button_Back").GetComponent(Transform) as Transform;
	
	//settings menu initialization
	tMenuTransforms[MenuIDs.SettingsMenu] = tMenuGroup.Find("Settings").GetComponent(Transform) as Transform;
	tSettingsButtons = new Transform[iSettingsButtonsCount];
	tSettingsButtons[0] = tMenuTransforms[MenuIDs.SettingsMenu].Find("Buttons/Button_Back");
	tSettingsButtons[1] = tMenuTransforms[MenuIDs.SettingsMenu].Find("ControlType/Button_Swipe/RadioButton_Background").GetComponent(Transform) as Transform;
	tSettingsButtons[2] = tMenuTransforms[MenuIDs.SettingsMenu].Find("ControlType/Button_Gyro/RadioButton_Background").GetComponent(Transform) as Transform;
	tSettingsButtons[3] = tMenuTransforms[MenuIDs.SettingsMenu].Find("Music/Button_ON/RadioButton_Background").GetComponent(Transform) as Transform;
	tSettingsButtons[4] = tMenuTransforms[MenuIDs.SettingsMenu].Find("Music/Button_OFF/RadioButton_Background").GetComponent(Transform) as Transform;
	tSettingsButtons[5] = tMenuTransforms[MenuIDs.SettingsMenu].Find("Sound/Button_ON/RadioButton_Background").GetComponent(Transform) as Transform;
	tSettingsButtons[6] = tMenuTransforms[MenuIDs.SettingsMenu].Find("Sound/Button_OFF/RadioButton_Background").GetComponent(Transform) as Transform;
					
	mrSwipeControls = tMenuTransforms[MenuIDs.SettingsMenu].Find("ControlType/Button_Swipe/RadioButton_Foreground").GetComponent(MeshRenderer) as MeshRenderer;
	mrGyroControls = tMenuTransforms[MenuIDs.SettingsMenu].Find("ControlType/Button_Gyro/RadioButton_Foreground").GetComponent(MeshRenderer) as MeshRenderer;
	mrMusicON = tMenuTransforms[MenuIDs.SettingsMenu].Find("Music/Button_ON/RadioButton_Foreground").GetComponent(MeshRenderer) as MeshRenderer;
	mrMusicOFF = tMenuTransforms[MenuIDs.SettingsMenu].Find("Music/Button_OFF/RadioButton_Foreground").GetComponent(MeshRenderer) as MeshRenderer;
	mrSoundON = tMenuTransforms[MenuIDs.SettingsMenu].Find("Sound/Button_ON/RadioButton_Foreground").GetComponent(MeshRenderer) as MeshRenderer;
	mrSoundOFF = tMenuTransforms[MenuIDs.SettingsMenu].Find("Sound/Button_OFF/RadioButton_Foreground").GetComponent(MeshRenderer) as MeshRenderer;
	
	///////HUD//////
	(GameObject.Find("HUDMainGroup/HUDPauseCounter").GetComponent(MeshRenderer) as MeshRenderer).enabled = false;
	
	//set the HUD position according to the screen resolution
	(GameObject.Find("HUDMainGroup/HUDGroup/HUDCurrencyGroup").GetComponent(Transform) as Transform).transform.Translate(-fResolutionFactor,0,0);
	(GameObject.Find("HUDMainGroup/HUDGroup/HUDScoreGroup").GetComponent(Transform) as Transform).transform.Translate(-fResolutionFactor,0,0);
	(GameObject.Find("HUDMainGroup/HUDGroup/HUDPause").GetComponent(Transform) as Transform).transform.Translate(fResolutionFactor,0,0);
		
	ShowMenu(MenuIDs.MainMenu);	//show Main Menu on game launch
}

/*
*	FUNCTION: Show the pause menu
*	CALLED BY:	InGameScript.Update()
*/
public function displayPauseMenu()
{
	ShowMenu(MenuIDs.PauseMenu);
}

/*
*	FUNCTION: Show the game over menu
*	CALLED BY:	InGameScript.Update()
*/
public function displayGameOverMenu()
{	
	ShowMenu(MenuIDs.GameOverMenu);	
}

function FixedUpdate()
{		
	//display countdown timer on Resume
	if (iResumeGameState == 0)
		;
	else if (iResumeGameState == 1)//display the counter
	{
		tmPauseCountdown = GameObject.Find("HUDMainGroup/HUDPauseCounter").GetComponent("TextMesh") as TextMesh;
		(GameObject.Find("HUDMainGroup/HUDPauseCounter").GetComponent(MeshRenderer) as MeshRenderer).enabled = true;
		
		iResumeGameStartTime = Time.time;		
		iResumeGameState = 2;
	}
	else if (iResumeGameState == 2)//count down
	{
		tmPauseCountdown.text = Mathf.Round(4 - (Time.time - iResumeGameStartTime)).ToString();
		
		if ( (Time.time - iResumeGameStartTime) >= 3)//resume the game when time expires
		{
			tmPauseCountdown.text = String.Empty;
			hInGameScript.processClicksPauseMenu(PauseMenuEvents.Resume);
			iResumeGameStartTime = 0;			
			iResumeGameState = 0;
		}
	}	
}//end of fixed update

function OnGUI()
{
	listenerClicks();//listen for clicks
}

/*
*	FUNCTION: Detect taps and call the relevatn event handler.
*	CALLED BY:	The FixedUpdate() function.
*/
private var hit : RaycastHit;
private function listenerClicks()
{
	if (Input.GetMouseButtonDown(0) && iTapState == 0)//detect taps
	{	
		iTapState = 1;
		
	}//end of if get mouse button
	else if (iTapState == 1)//call relevent handler
	{
		if (Physics.Raycast(HUDCamera.ScreenPointToRay(Input.mousePosition),hit))//if a button has been tapped
		{
			//call the listner function of the active menu
			if (CurrentMenu == MenuIDs.MainMenu)
				handlerMainMenu(hit.transform);
			else if (CurrentMenu == MenuIDs.PauseMenu)
				handlerPauseMenu(hit.transform);
			else if (CurrentMenu == MenuIDs.GameOverMenu)
				handlerGameOverMenu(hit.transform);
			else if (CurrentMenu == MenuIDs.InstructionsMenu)
				handlerInstructionsMenu(hit.transform);
			else if (CurrentMenu == MenuIDs.SettingsMenu)
				handlerSettingsMenu(hit.transform);
		}//end of if raycast
		
		iTapState = 2;
	}
	else if (iTapState == 2)//wait for user to release before detcting next tap
	{
		if (Input.GetMouseButtonUp(0))
			iTapState = 0;
	}
}//end of listner function

/*
*	FUNCTION: Handle clicks on Main Menu
*/
private function handlerMainMenu(buttonTransform : Transform)
{		
	if (tMainMenuButtons[0] == buttonTransform)//Tap to Play button
	{
		CloseMenu(MenuIDs.MainMenu);
		
		hInGameScript.launchGame();	//start the gameplay
		setMenuScriptStatus(false);
	}
	else if (tMainMenuButtons[1] == buttonTransform)//information button
	{
		CloseMenu(MenuIDs.MainMenu);
		ShowMenu(MenuIDs.InstructionsMenu);
		CurrentMenu = MenuIDs.InstructionsMenu;
	}
	else if (tMainMenuButtons[2] == buttonTransform)//settings button
	{
		CloseMenu(MenuIDs.MainMenu);
		ShowMenu(MenuIDs.SettingsMenu);		
	}
}//end of handler main menu function

/*
*	FUNCTION: Handle clicks on pause menu.
*/
private function handlerPauseMenu(buttonTransform : Transform)
{
	if (tPauseButtons[0] == buttonTransform)//back button handler
	{
		hInGameScript.processClicksPauseMenu(PauseMenuEvents.MainMenu);
		
		CloseMenu(MenuIDs.PauseMenu);		
		ShowMenu(MenuIDs.MainMenu);
	}
	else if (tPauseButtons[1] == buttonTransform)//resume button handler
	{
		CloseMenu(MenuIDs.PauseMenu);
		iResumeGameState = 1;//begin the counter to resume
	}
}

/*
*	FUNCTION: Handle clicks on Game over menu.
*/
private function handlerGameOverMenu(buttonTransform : Transform)
{
	if (tGameOverButtons[0] == buttonTransform)//main menu button
	{
		hInGameScript.procesClicksDeathMenu(GameOverMenuEvents.Back);
		CloseMenu(MenuIDs.GameOverMenu);
		ShowMenu(MenuIDs.MainMenu);		
	}
	else if (tGameOverButtons[1] == buttonTransform)//play button
	{
		hInGameScript.procesClicksDeathMenu(GameOverMenuEvents.Play);		
		CloseMenu(CurrentMenu);
	}	
}

/*
*	FUNCTION: Handle the clicks on Information menu.
*/
private function handlerInstructionsMenu(buttonTransform : Transform)
{
	if (tInstructionsButtons[0] == buttonTransform)//main menu button
	{
		CloseMenu(MenuIDs.InstructionsMenu);
		ShowMenu(MenuIDs.MainMenu);		
	}	
}

/*
*	FUNCTION: Handle the clicks on Information menu.
*	CALLED BY:	listenerClicks()
*/
private function handlerSettingsMenu(buttonTransform : Transform)
{
	if (tSettingsButtons[0] == buttonTransform)//home button
	{
		CloseMenu(MenuIDs.SettingsMenu);
		ShowMenu(MenuIDs.MainMenu);
	}
	else if (tSettingsButtons[1] == buttonTransform)//swipe controls
	{		
		if (mrSwipeControls.enabled == false)
		{
			mrSwipeControls.enabled = true;
			mrGyroControls.enabled = false;
			hControllerScript.toggleSwipeControls(true);
		}		
	}
	else if (tSettingsButtons[2] == buttonTransform)//gyro controls
	{		
		if (mrGyroControls.enabled == false)
		{
			mrGyroControls.enabled = true;
			mrSwipeControls.enabled = false;
			hControllerScript.toggleSwipeControls(false);
		}		
	}
	else if (tSettingsButtons[3] == buttonTransform)//music ON radio button
	{
		if (mrMusicON.enabled == false)
		{
			mrMusicON.enabled = true;
			mrMusicOFF.enabled = false;
			hSoundManagerScript.toggleMusicEnabled(true);
		}
	}
	else if (tSettingsButtons[4] == buttonTransform)//music OFF radio button
	{
		if (mrMusicON.enabled == true)
		{
			mrMusicON.enabled = false;
			mrMusicOFF.enabled = true;
			hSoundManagerScript.toggleMusicEnabled(false);
		}
	}
	else if (tSettingsButtons[5] == buttonTransform)//music ON radio button
	{
		if (mrSoundON.enabled == false)
		{
			mrSoundON.enabled = true;
			mrSoundOFF.enabled = false;
			hSoundManagerScript.toggleSoundEnabled(true);
		}
	}
	else if (tSettingsButtons[6] == buttonTransform)//music ON radio button
	{
		if (mrSoundON.enabled == true)
		{
			mrSoundON.enabled = false;
			mrSoundOFF.enabled = true;
			hSoundManagerScript.toggleSoundEnabled(false);
		}
	}
}

/*
*	FUNCTION: Set the menu to show in front of the HUD Camera
*/
public function ShowMenu(index : int)
{
	yield WaitForFixedUpdate();
	
	if (MenuIDs.MainMenu == index)	
		tMenuTransforms[MenuIDs.MainMenu].position.y = 0;
	else if (MenuIDs.PauseMenu == index)
		tMenuTransforms[MenuIDs.PauseMenu].position.y = 0;
	else if (MenuIDs.GameOverMenu == index)
		tMenuTransforms[MenuIDs.GameOverMenu].position.y = 0;
	else if (MenuIDs.InstructionsMenu == index)
		tMenuTransforms[MenuIDs.InstructionsMenu].position.y = 0;
	else if (MenuIDs.SettingsMenu == index)
	{
		//check which type of controls are active and 
		//set the appropriate radio button 
		if ( hControllerScript.isSwipeControlEnabled() )
		{
			mrSwipeControls.enabled = true;
			mrGyroControls.enabled = false;
		}
		else
		{
			mrSwipeControls.enabled = false;
			mrGyroControls.enabled = true;
		}
		
		//check if the music is enabled or disabled and
		//set the appropriate radio button
		if (hSoundManagerScript.isMusicEnabled())
		{
			mrMusicON.enabled = true;
			mrMusicOFF.enabled = false;
		}
		else
		{
			mrMusicON.enabled = false;
			mrMusicOFF.enabled = true;
		}
		
		//check if the sound is ON or OFF and se the
		//appropriate radio button
		if (hSoundManagerScript.isSoundEnabled())
		{
			mrSoundON.enabled = true;
			mrSoundOFF.enabled = false;
		}
		else
		{
			mrSoundON.enabled = false;
			mrSoundOFF.enabled = true;
		}
		
		tMenuTransforms[MenuIDs.SettingsMenu].position.y = 0;
	}
	
	CurrentMenu = index;
	hideHUDElements();	//hide the HUD
	hSoundManagerScript.playSound(MenuSounds.ButtonTap);
}

/*
*	FUNCTION: Send the menu away from the HUD Camera
*/
private function CloseMenu(index : int)
{
	if (index == MenuIDs.MainMenu)		
		tMenuTransforms[MenuIDs.MainMenu].position.y = 1000;
	else if (index == MenuIDs.PauseMenu)
		tMenuTransforms[MenuIDs.PauseMenu].position.y = 1000;
	else if (index == MenuIDs.GameOverMenu)
		tMenuTransforms[MenuIDs.GameOverMenu].position.y = 1000;
	else if (MenuIDs.InstructionsMenu == index)
		tMenuTransforms[MenuIDs.InstructionsMenu].position.y = 1000;
	else if (MenuIDs.SettingsMenu == index)		
		tMenuTransforms[MenuIDs.SettingsMenu].position.y = 1000;
	
	CurrentMenu = -1;
}

public function hideHUDElements()
{
	(GameObject.Find("HUDMainGroup/HUDGroup").GetComponent(Transform) as Transform).position.y = 1000;
}

public function showHUDElements()
{	
	(GameObject.Find("HUDMainGroup/HUDGroup").GetComponent(Transform) as Transform).position.y = 0;
}

/*
*	FUNCTION: Enable/ disable MenuScript.
*	CALLED BY: InGameScript.Update()
*	ADDITIONAL INFO: The MenuScript is disabled during gameplay for improved performance.
*/
public function setMenuScriptStatus(flag:boolean)
{
	if (flag != this.enabled)
		this.enabled = flag;
}
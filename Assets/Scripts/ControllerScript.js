#pragma strict
/*
*	FUNCTION:
*	- This script detects inputs (swipes and gyro) and controls the 
*	player accordingly.	
*	- It also defines the physics that defines
*	the user's actions.	
*	- It is responsible for handling all player animations.
*	- It is responsible for process all collisions.
*	- It is responsible for initiating the death scene.
*	
*	USED BY: This script is a part of the "Player" prefab.
*
*/

enum StrafeDirection {left = 0, right = 1}

private var tPlayer : Transform;	//the main character transform
private var tPlayerRotation : Transform;	//Player child transform to rotate it in game
private var aPlayer : Animation;				//character animation
private var tPlayerSidesCollider : Transform;	//sides collider transform (detects stumble)
private var tFrontCollider : Transform;			//front collider transfrom (detects collisions)
private var v3BNCDefaultScale : Vector3;
private var v3BFCDefaultScale : Vector3;

//Variables
private var fCurrentWalkSpeed : float;

private var tCurrentAngle : float = 0.0;	//current rotation along Y axis
private var fJumpForwardFactor : float = 0.0;	//movement speed increase on jump
private var fCurrentUpwardVelocity : float = 0.0;	//speed during the duration of jump
private var fCurrentHeight : float = 0.0;
private var fContactPointY : float = 0.0;	//y-axis location of the path

//player state during gameplay
private var bInAir : boolean = false;
private var bJumpFlag : boolean = false;
private var bInJump : boolean = false;
private var bInDuck : boolean = false;				//true if the character is sliding
private var bDiveFlag : boolean = false;			//force character to dive during jump
private var bExecuteLand : boolean = false;
private var bInStrafe : boolean = false;

private var fForwardAccleration : float = 0.0;
private var tBlobShadowPlane : Transform;	//the shadow under the player
private var CurrentDirection : Vector3;//set player rotation according to path

//script references
private var hPatchesRandomizer : PatchesRandomizer;
private var hCheckPointsMain : CheckPointsMain;
private var hInGameScript : InGameScript;
private var hPitsMainController : PitsMainController;
private var hSoundManager : SoundManager;
private var hCameraController : CameraController;
private var hPowerupScript : PowerupsMainController;
private var hEnemyController : EnemyController;
private var hMenuScript : MenuScript;
private var hPlayerFrontColliderScript : PlayerFrontColliderScript;
private var hPlayerSidesColliderScript : PlayerSidesColliderScript;

private var hitInfo : RaycastHit;	//whats under the player character
private var bGroundhit : boolean = false;	//is that an object under the player character
private var fHorizontalDistance : float = 0.0;	//calculate player's horizontal distance on path

private var fCurrentForwardSpeed : float = .5;	//sets movement based on spline
private var fCurrentDistance : float = 0.0;//distance between the start and current position during the run
private var fCurrentMileage : float = 0.0;//used to calculate the score based on distance covered

//detect if there is a terrain_lyr under the player
private var fPitFallLerpValue : float = 0.0;
private var fPitFallForwardSpeed : float = 0.0;
private var fPitPositionX : float = 0.0;	//check the position of the pit in x-axis
private var iDeathAnimStartTime : int = 0;
private var iDeathAnimEndTime : int = 3;	//duration wait for death scene

private var JumpAnimationFirstTime : boolean = true;	//play death animation once
private var HUDCamera : Camera;

private var tPauseButton : Transform;
private var tHUDGroup : Transform;
private var swipeLogic : SwipeControls;
private var iLanePosition : int;						//current lane number -- -1, 0 or 1
private var iLastLanePosition : int; 					//stores the previous lane on lane change
private var bMouseReleased : boolean = true;
private var bControlsEnabled : boolean = true;

//action queue
private var directionQueue : SwipeDirection;
private var bDirectionQueueFlag : boolean = false;

//Physics Constants
//change these to adjust the initial and final movement speed
private var fStartingWalkSpeed : float = 150.0;//when player starts running
private var fEndingWalkSpeed : float = 230.0;	//final speed after acclerating
private var fCurrentWalkAccleration : float = 0.5;	//rate of accleartion

//change these to adjust the jump height and displacement
private var fJumpPush : float = 185;			//force with which player pushes the ground on jump
private function getAccleration() { return 500; }	//accleration and deceleration on jump

//the initial distance of the player character at launch
//from the start of the path
private var fCurrentDistanceOnPath : float = 0.0;

//Is used to switch between gyro and swipe controls
private var swipeControlsEnabled : boolean = true;
public function isSwipeControlEnabled() { return swipeControlsEnabled; }

public function toggleSwipeControls(state:boolean)
{
	swipeControlsEnabled = state;
	
	//permanently save user preference of controls
	PlayerPrefs.SetInt("ControlsType", (state == true ? 1 : 0));
	PlayerPrefs.Save();
}

function Start()
{
	hMenuScript = GameObject.Find("MenuGroup").GetComponent(MenuScript) as MenuScript;
	hPatchesRandomizer = this.GetComponent(PatchesRandomizer) as PatchesRandomizer;
	hPlayerSidesColliderScript = GameObject.Find("PlayerSidesCollider").GetComponent(PlayerSidesColliderScript) as PlayerSidesColliderScript;
	hPlayerFrontColliderScript = GameObject.Find("PlayerFrontCollider").GetComponent(PlayerFrontColliderScript) as PlayerFrontColliderScript;
	hSoundManager = GameObject.Find("SoundManager").GetComponent(SoundManager) as SoundManager;
	hInGameScript = this.GetComponent(InGameScript) as InGameScript;	
	hPitsMainController = this.GetComponent(PitsMainController) as PitsMainController;
	hCheckPointsMain = this.GetComponent(CheckPointsMain) as CheckPointsMain;
	hPowerupScript = this.GetComponent(PowerupsMainController) as PowerupsMainController;
	hEnemyController = GameObject.Find("Enemy").GetComponent(EnemyController) as EnemyController;
	hPowerupScript = this.GetComponent(PowerupsMainController) as PowerupsMainController;
	hCameraController = camera.main.gameObject.GetComponent(CameraController) as CameraController;
	swipeLogic = transform.GetComponent(SwipeControls) as SwipeControls;
	
	tPlayer = transform;
	tPlayerRotation = transform.Find("PlayerRotation");
	
	//get the animation component of the player character
	aPlayer = this.transform.Find("PlayerRotation/PlayerMesh/model").GetComponent(Animation) as Animation;
	tBlobShadowPlane = transform.Find("BlobShadowPlane");
	
	tPlayerSidesCollider = GameObject.Find("PlayerSidesCollider").transform;	
	tFrontCollider = GameObject.Find("PlayerFrontCollider").transform;
	tHUDGroup = GameObject.Find("HUDMainGroup/HUDGroup").transform;
	tPauseButton = GameObject.Find("HUDMainGroup/HUDGroup/HUDPause").transform;
	
	HUDCamera = GameObject.Find("HUDCamera").camera;
	
	v3BNCDefaultScale = tFrontCollider.localScale;	
	v3BFCDefaultScale = tPlayerSidesCollider.localScale;
	
	bInAir = false;
	fCurrentDistanceOnPath = 50.0;	//inital distance with respect to spline
	fCurrentDistance = 0.0;
	fCurrentMileage = 0.0;
	tCurrentAngle = 0.0;	
	fPitFallLerpValue = 0.0;
	fPitFallForwardSpeed = 0.0;
	fPitPositionX = 0.0;
	iDeathAnimStartTime = 0;
	bGroundhit = false;	
	bJumpFlag = false;
	bInJump = false;
	fCurrentUpwardVelocity = 0;
	fCurrentHeight = 0;
	
	bDirectionQueueFlag = false;
	directionQueue = SwipeDirection.Null;
	iLanePosition = 0;	//set current lane to mid	
	fCurrentWalkSpeed = fStartingWalkSpeed;
	
	
	if (PlayerPrefs.HasKey("ControlsType"))
		swipeControlsEnabled = PlayerPrefs.GetInt("ControlsType") == 1 ? true : false;
	else
		PlayerPrefs.SetInt("ControlsType", (swipeControlsEnabled == true ? 1 : 0));
		
	hSoundManager.stopSound(CharacterSounds.Footsteps);
	StartCoroutine("playIdleAnimations");//start playing idle animations
}//end of Start()

/*
 * FUNCTION:	Play and alternate between the two idle animations
 * 				when the game is launched/ restarted.
 * CALLED BY:	Start()
 * */
private function playIdleAnimations()
{
	while(true)
	{
		yield WaitForFixedUpdate();
/*	
		if (!aPlayer.IsPlaying("Idle_1") && !aPlayer.IsPlaying("Idle_2"))
		{
			aPlayer.animation.Play("Idle_1");
			aPlayer.PlayQueued("Idle_2", QueueMode.CompleteOthers);
		}
*/
	}//end of while
}

/*
*	FUNCTION: Enable controls, start player animation and movement
*/
public function launchGame()
{
	StopCoroutine("playIdleAnimations");//stop idle animations
	hEnemyController.launchEnemy();

	aPlayer["run"].speed = Mathf.Clamp( (fCurrentWalkSpeed/fStartingWalkSpeed)/1.1, 0.8, 1.2 );
	aPlayer.Play("run");

	hSoundManager.playSound(CharacterSounds.Footsteps);//play the footsteps sound
}

function Update()
{
	if(hInGameScript.isGamePaused()==true)
		return;
	
	if (hInGameScript.isEnergyZero())
		if(DeathScene())
			return;
	
	getClicks();	//get taps/clicks for pause menu etc.
	
	if (bControlsEnabled)
		SwipeMovementControl();
}//end of update()

function FixedUpdate()
{
	if(hInGameScript.isGamePaused() == true)
		return;
		
	setForwardSpeed();
	SetTransform();
	setShadow();
		
	if(!bInAir)
	{
		if(bExecuteLand)
		{
			hSoundManager.playSound(CharacterSounds.JumpLand);
			bExecuteLand = false;
			JumpAnimationFirstTime = true;
		}
	}//end of if not in air
	else
	{		
		if(JumpAnimationFirstTime&&bInJump==true)
		{
            /*
			aPlayer.Rewind("jump");*/
			JumpAnimationFirstTime = false;
			bInDuck = false;
						
			aPlayer.CrossFade("jump", 0.1);
		}
	}//end of else !in air

	if(bJumpFlag==true)
	{		
		bJumpFlag = false;
		bExecuteLand = true;
		bInJump = true;
		bInAir = true;
		fCurrentUpwardVelocity = fJumpPush;
		fCurrentHeight = tPlayer.position.y;
	}
		
	//acclerate movement speed with time
	if(fCurrentWalkSpeed<fEndingWalkSpeed)
		fCurrentWalkSpeed += (fCurrentWalkAccleration * Time.fixedDeltaTime);
		
	aPlayer["run"].speed = Mathf.Clamp( (fCurrentWalkSpeed/fStartingWalkSpeed)/1.1, 0.8, 1.2 );	//set run animation speed according to current speed
}//end of Fixed Update

/*
*	FUNCTION: Check if pause button is tapped in-game
*	CALLED BY:	Update()
*/
private function getClicks()
{
	if(Input.GetMouseButtonUp(0) && bMouseReleased==true)
	{
		var screenPoint : Vector3;
		var buttonSize : Vector2;
		var Orb_Rect : Rect;
		
		if (tHUDGroup.localPosition.z==0)
		{
			buttonSize = Vector3(Screen.width/6,Screen.width/6,0.0);
			screenPoint = HUDCamera.WorldToScreenPoint( tPauseButton.position );
			
			Orb_Rect = Rect (screenPoint.x - ( buttonSize.x * 0.5 ), screenPoint.y - ( buttonSize.y * 0.5 ), buttonSize.x, buttonSize.y);
			if(Orb_Rect.Contains(Input.mousePosition))
			{				
				hInGameScript.pauseGame();
			}
		}
		
		Orb_Rect = Rect (screenPoint.x - ( buttonSize.x * 0.5 ), screenPoint.y - ( buttonSize.y * 0.5 ), buttonSize.x, buttonSize.y);
	}//end of mouserelease == true if
		
}//end of get clicks function

/*
*	FUNCITON: Set the position of the shadow under the player and of the
*				colliders to make them move with the character mesh.
*	CALLED BY:	FixedUpdate()
*/
private function setShadow()
{	
	tBlobShadowPlane.up = hitInfo.normal;
	tBlobShadowPlane.position.y = fContactPointY+0.7;//set shadow's position
	tBlobShadowPlane.localEulerAngles.y = tPlayerRotation.localEulerAngles.y;//set shadow's rotation
	
	//set side collider's position and rotation
	tPlayerSidesCollider.position = tPlayer.position + Vector3(0,5,0);
	tPlayerSidesCollider.localEulerAngles = tBlobShadowPlane.localEulerAngles;//set 
	
	//set front collider's position and rotation
	tFrontCollider.position = tPlayer.position + Vector3(7,5,0);
	tFrontCollider.localEulerAngles = tBlobShadowPlane.localEulerAngles;
}

/*
*	FUNCTION: Set the player's position the path with reference to the spline
*	CALLED BY:	FixedUpdate()
*/
function SetTransform()
{
	if (bControlsEnabled)
		var iStrafeDirection : int = getLeftRightInput();	//get the current lane (-1, 0 or 1)
	
	fCurrentDistanceOnPath = hCheckPointsMain.SetNextMidPointandRotation(fCurrentDistanceOnPath, fCurrentForwardSpeed);//distance on current patch
	fCurrentDistance = fCurrentDistanceOnPath + hPatchesRandomizer.getCoveredDistance();//total distance since the begining of the run
	fCurrentMileage = fCurrentDistance/12.0;//calculate milage to display score on HUD
	
	tCurrentAngle = hCheckPointsMain.getCurrentAngle();//get the angle according to the position on path
	tPlayerRotation.localEulerAngles.y = -tCurrentAngle;//set player rotation according to the current player position on the path's curve (if any)
	
	CurrentDirection = hCheckPointsMain.getCurrentDirection();
	var Desired_Horinzontal_Pos : Vector3 = calculateHorizontalPosition(iStrafeDirection);
	
	bGroundhit = Physics.Linecast(Desired_Horinzontal_Pos + Vector3(0,20,0),Desired_Horinzontal_Pos + Vector3(0,-100,0),hitInfo,(1<<9));	
	
	if(bGroundhit && hPitsMainController.isFallingInPit()==false)//calculate player position in y-axis
		fContactPointY = hitInfo.point.y;
	else//call death if player in not on Terrain_lyr
	{
		fContactPointY = -10000.0;
		if(!bInAir)
		{
			if(!bInJump)
			{
				if(reConfirmPitFalling(Desired_Horinzontal_Pos,iStrafeDirection)==true)
				{
					hPitsMainController.setPitValues();
				}
			}
			bInAir = true;
			fCurrentUpwardVelocity = 0;
			fCurrentHeight = tPlayer.position.y;
		}
	}
	
	if(!bInAir)//set player position when not in air
	{
		tPlayer.position.y = fContactPointY+0.6;
	}
	else//set player position if in air
	{
		if (bDiveFlag)	//dive during jump
		{
			setCurrentDiveHeight();
			tPlayer.position.y = fCurrentHeight;
		}
		else			//JUMP
		{
			setCurrentJumpHeight();
			tPlayer.position.y = fCurrentHeight;
		}
	}
	
	tPlayer.position.x = Desired_Horinzontal_Pos.x;//set player position in x-axis
	tPlayer.position.z = Desired_Horinzontal_Pos.z;//set player position in y-axis
	
}//end of Set Transform()

/*
*	FUNCTION: Set the height of the player during jump
*	CALLED BY:	SetTransform()
*/
private function setCurrentJumpHeight()		//set height during jump
{
	fCurrentUpwardVelocity-=Time.fixedDeltaTime*getAccleration();
	fCurrentUpwardVelocity = Mathf.Clamp(fCurrentUpwardVelocity,-fJumpPush,fJumpPush);
	fCurrentHeight+=fCurrentUpwardVelocity*(Time.fixedDeltaTime/1.4);
	
	if(fCurrentHeight<fContactPointY)
	{
		fCurrentHeight = fContactPointY;
		bInAir = false;
		bInJump = false;
		
		if (bDiveFlag)	//do not resume run animation on Dive
			return;
				
		if (!hInGameScript.isEnergyZero())
		{		
			aPlayer.CrossFade("run", 0.1);
		}//end of if current energy > 0
	}
}

/*
*	FUNCITON: Pull the player down faster if user swipes down int the middle of jump
*	CALLED BY:	SetTransform()
*/
private function setCurrentDiveHeight()	//set height after dive called
{
	fCurrentUpwardVelocity-=Time.fixedDeltaTime*2000;
	fCurrentUpwardVelocity = Mathf.Clamp(fCurrentUpwardVelocity,-fJumpPush,fJumpPush);
	if(hPitsMainController.isFallingInPit() == false)
		fCurrentHeight+=fCurrentUpwardVelocity*Time.fixedDeltaTime;
	else
	{
		fCurrentHeight-=40.0*Time.fixedDeltaTime;
		hMenuScript.hideHUDElements();
	}	
	
	if(fCurrentHeight<=fContactPointY)
	{
		fCurrentHeight = fContactPointY;//bring character down completely
			
		bInAir = false;
		bInJump = false;
		
		duckPlayer();//make the character slide
		bDiveFlag = false;		//dive complete
	}//end of if
}

/*
*	FUNCTION: 	Make sure that there is no terrain under the player
*				before making it fall
*	CALLED BY:	SetTransform()
*/
private function reConfirmPitFalling(Desired_Horinzontal_Pos : Vector3, iStrafeDirection : float) : boolean
{
	var bGroundhit : boolean = false;
	
	if(iStrafeDirection>=0)
		bGroundhit = Physics.Linecast(Desired_Horinzontal_Pos + Vector3(1,20,5),Desired_Horinzontal_Pos + Vector3(0,-100,5),hitInfo,1<<9);
	else
		bGroundhit = Physics.Linecast(Desired_Horinzontal_Pos + Vector3(1,20,-5),Desired_Horinzontal_Pos + Vector3(0,-100,-5),hitInfo,1<<9);
	
	if(!bGroundhit)
		return true;
	else
		return false;
}

/*
*	FUNCTION: Called when user runs out of energy
*	CALLED BY:	Update()
*/
function DeathScene() : boolean
{
	bInAir = false;
	tPlayerRotation.localEulerAngles = Vector3(0,0,0);
	
	if (iDeathAnimStartTime == 0)
	{
		hSoundManager.stopSound(CharacterSounds.Footsteps);
		bControlsEnabled = false;
				
		var v3EffectPosition = this.transform.position;
		v3EffectPosition.x += 15;
		v3EffectPosition.y += 5;		
	
		aPlayer.CrossFade("death",0.1);
		hEnemyController.playDeathAnimation();
		
		hMenuScript.hideHUDElements();
		iDeathAnimStartTime = Time.time;	
	}	
	else if (iDeathAnimStartTime != 0 && (Time.time - iDeathAnimStartTime) >= iDeathAnimEndTime)
	{		
		hInGameScript.setupDeathMenu();
		return true;
	}
	
	return false;
}

/*
*	FUNCTION: Called when player hits an obstacle sideways
*	CALLED BY: PlayerSidesColliderScript.OnCollisionEnter()
*/
public function processStumble()
{
	hCameraController.setCameraShakeImpulseValue(1);
	iLanePosition = iLastLanePosition;	//stop strafe
		
	if (hEnemyController.processStumble())
	{	
		hInGameScript.collidedWithObstacle();//call death if player stumbled twice in unit time
	}
	else
	{
		aPlayer.PlayQueued("run", QueueMode.CompleteOthers);
		
		//enable colliders if they were disabled
		hPlayerFrontColliderScript.activateFrontCollider();
		hPlayerSidesColliderScript.activateSidesCollider();
	}	
}

/*
*	FUNCTION: Returns horizontal the position to move to
*	CALLED BY: SetTransform()
*/
private function getLeftRightInput()	//change lane
{
	if (swipeControlsEnabled == true)//swipe direction
		return iLanePosition;
	else//gyro direction
	{
		var fMovement : float = 0.0;
		var fSign : float = 1.0;
		
		if(Screen.orientation == ScreenOrientation.Portrait)
			fSign = 1.0;
		else
			fSign = -1.0;
		
		if(Application.isEditor)//map gyro controls on mouse in editor mode
		{
			fMovement = (Input.mousePosition.x - (Screen.height/2.0))/(Screen.height/2.0) * 4.5;
		}
		else
		{
			fMovement = (fSign * Input.acceleration.x * 4.5);
		}
		
		return fMovement;
	}
}

/*
*	FUNCTION: Set the movement speed
*	CALLED BY: FixedUpdate()
*/
private function setForwardSpeed()
{
	//if the player is not on Terrain_lyr
	if(hPitsMainController.isFallingInPit() == true)
	{		
		if(transform.position.x>fPitPositionX)
			fCurrentForwardSpeed = 0.0;
		else
			fCurrentForwardSpeed = Mathf.Lerp(fPitFallForwardSpeed,0.01,(Time.time-fPitFallLerpValue)*3.5);
		return;
	}
	
	if (hInGameScript.isEnergyZero())//on death
	{
		fCurrentForwardSpeed = 0;
		return;
	}
	
	if(bInAir)
		fForwardAccleration = 1.0;
	else
		fForwardAccleration = 2.0;
		
	fJumpForwardFactor = 1 + ((1/fCurrentWalkSpeed)*50);
		
	if(bInJump==true)
		fCurrentForwardSpeed = Mathf.Lerp(fCurrentForwardSpeed,fCurrentWalkSpeed*Time.fixedDeltaTime*fJumpForwardFactor,Time.fixedDeltaTime*fForwardAccleration);
	else
		fCurrentForwardSpeed = Mathf.Lerp(fCurrentForwardSpeed,(fCurrentWalkSpeed)*Time.fixedDeltaTime,Time.fixedDeltaTime*fForwardAccleration);
}

/*
*	FUNCTION: Make the player change lanes
*	CALLED BY:	SetTransform()
*/
private var fCurrentStrafePosition : float = 0.0;	//keeps track of strafe position at each frame
private var fSpeedMultiplier : float = 5.0;	//how fast to strafe/ change lane
private function calculateHorizontalPosition(iStrafeDirection : int)
{
	if (swipeControlsEnabled == true)
	{
		var SideDirection_Vector2 : Vector2 = rotateAlongZAxis(Vector2(CurrentDirection.x,CurrentDirection.z),90.0);
		SideDirection_Vector2.Normalize();
			
		if(iStrafeDirection==-1)//strafe left from center
		{
			if(fCurrentStrafePosition>-1)
			{
				fCurrentStrafePosition-= fSpeedMultiplier*Time.fixedDeltaTime;
				if(fCurrentStrafePosition<=-1.0)
				{
					fCurrentStrafePosition = -1.0;
					switchStrafeToSprint();
				}
			}
		}
		else if(iStrafeDirection==1)//strafe right from center
		{
			if(fCurrentStrafePosition<1)
			{
				fCurrentStrafePosition+= fSpeedMultiplier*Time.fixedDeltaTime;
				if(fCurrentStrafePosition>=1.0)
				{
					fCurrentStrafePosition = 1.0;
					switchStrafeToSprint();
				}
			}
		}
		else if(iStrafeDirection==0&&fCurrentStrafePosition!=0.0)//strafe from left or right lane to center
		{	
			if(fCurrentStrafePosition<0)
			{
				fCurrentStrafePosition+= fSpeedMultiplier*Time.fixedDeltaTime;
				if(fCurrentStrafePosition>=0.0)
				{
					fCurrentStrafePosition = 0.0;
					switchStrafeToSprint();
				}
			}
			else if(fCurrentStrafePosition>0)
			{
				fCurrentStrafePosition-= fSpeedMultiplier*Time.fixedDeltaTime;
				if(fCurrentStrafePosition<=0.0)
				{
					fCurrentStrafePosition = 0.0;
					switchStrafeToSprint();
				}
			}
		}//end of else
			
		fHorizontalDistance = -fCurrentStrafePosition*16.0;	
		fHorizontalDistance = Mathf.Clamp(fHorizontalDistance,-20.0,20.0);
		
		var fHorizontalPoint : Vector2 = hCheckPointsMain.getCurrentMidPoint() + SideDirection_Vector2*fHorizontalDistance;
			
		return Vector3(fHorizontalPoint.x,tPlayerRotation.position.y,fHorizontalPoint.y);
	}
	else
	{
		SideDirection_Vector2 = rotateAlongZAxis(Vector2(CurrentDirection.x,CurrentDirection.z),90.0);
		SideDirection_Vector2.Normalize();
		
		fHorizontalDistance = Mathf.Lerp(fHorizontalDistance,-iStrafeDirection * 40.0, 0.05*fCurrentForwardSpeed);		
		fHorizontalDistance = Mathf.Clamp(fHorizontalDistance,-20.0,20.0);		
		fHorizontalPoint = hCheckPointsMain.getCurrentMidPoint() + SideDirection_Vector2*fHorizontalDistance;
				
		return Vector3(fHorizontalPoint.x,tPlayerRotation.position.y,fHorizontalPoint.y);
	}//end of else
}

/*
*	FUNCTION: Determine the rotation of the player character
*/
private function rotateAlongZAxis(inputVector : Vector2, angletoRotate : float)
{
	var FinalVector : Vector2 = Vector2.zero;
	angletoRotate = angletoRotate/57.3;
	FinalVector.x = Mathf.Cos(angletoRotate) * inputVector.x - Mathf.Sin(angletoRotate) * inputVector.y;
	FinalVector.y = Mathf.Sin(angletoRotate) * inputVector.x + Mathf.Cos(angletoRotate) * inputVector.y;
	
	return FinalVector;
}

/*
*	FUNCTION: Play the "run" animation
*	CALLED BY:	calculateHorizontalPosition()
*/
private function switchStrafeToSprint():void
{
	if (!hInGameScript.isEnergyZero() && !isInAir())
	{
		aPlayer.CrossFade("run", 0.1);
		bInStrafe = false;
	}	
}

/*
*	FUNCITON: Detect swipes on screen
*	CALLED BY: Update()
*/
function SwipeMovementControl()
{	
	//check and execute two jump or duck commands simultaneously
	if (bDirectionQueueFlag)
	{
		if(!bInAir && directionQueue == SwipeDirection.Jump)		//queue JUMP
		{
			bJumpFlag = true;			
			bDirectionQueueFlag = false;
		}//end of jump queue
		if (directionQueue == SwipeDirection.Duck && !bInDuck)		//queue SLIDE
		{
			duckPlayer();			
			bDirectionQueueFlag = false;
		}//end of duck queue
		
	}//end of direction queue

	//swipe controls
	var direction = swipeLogic.getSwipeDirection();	//get the swipe direction	
	if (direction != SwipeDirection.Null)
	{
		bMouseReleased = false;//disallow taps on swipe
		
		if (direction == SwipeDirection.Jump)	//JUMP
		{
			if(!bInAir)
			{					
				bJumpFlag = true;
			}
			if (bInAir)	//queue the second jump if player swipes up in the middle of a jump
			{
				bDirectionQueueFlag = true;
				directionQueue = SwipeDirection.Jump;
			}
		}//end of if direction is jump
		if (direction == SwipeDirection.Right && swipeControlsEnabled == true)	//RIGHT swipe
		{
			if (iLanePosition != 1) 
			{
				iLastLanePosition = iLanePosition;
				iLanePosition++;
				
				strafePlayer(StrafeDirection.right);
				
			}//end of lane check if
		}//end of swipe direction if
		if (direction == SwipeDirection.Left && swipeControlsEnabled == true)	//LEFT swipe
		{
			if (iLanePosition != -1) 
			{
				iLastLanePosition = iLanePosition;
				iLanePosition--;
				
				strafePlayer(StrafeDirection.left);
				
			}//end of lane check if
		}//end of swipe direction if
		if (direction == SwipeDirection.Duck && bInDuck)//SLIDE: queue the second duck command if player is in the middle of slide animation
		{
			bDirectionQueueFlag = true;
			directionQueue = SwipeDirection.Duck;
		}
		if (direction == SwipeDirection.Duck && !bInAir && !bInDuck)//SLIDE: on ground
		{
			duckPlayer();
		}
		if (direction == SwipeDirection.Duck && bInAir && !bInDuck)//SLIDE/ DIVE: in air
		{				
			bDiveFlag = true;	//used by Set Transform() to make the character dive
		}//end of slide in air if
		
		//swipeLogic.iTouchStateFlag = 2;
	}//end of if	
	if (Input.GetMouseButtonUp(0))	//allow taps on mouse/ tap release
	{
		bMouseReleased = true;
	}
		
	if (!isPlayingDuck() && bInDuck == true)	//restore the size of the collider after slide ends
	{
		hSoundManager.playSound(CharacterSounds.Footsteps);
		
		tPlayerRotation.localEulerAngles.z = 0;//rotation correction after DIVE
		tBlobShadowPlane.localPosition.x = 0;//translation correction after DIVE (to fix mysterious bug :S)
	
		bInDuck = false;
		tFrontCollider.localScale = v3BNCDefaultScale;
		tPlayerSidesCollider.localScale = v3BFCDefaultScale;		//restore far collider
		
		if (bDiveFlag)	//do not resume run animation on Dive
			return;
				
		aPlayer.CrossFadeQueued("run", 0.5, QueueMode.CompleteOthers);
	}
	
	//keyboard controls (DEBUG)
	if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))//Up/ jump
	{
		if(!bInAir)
		{					
			bJumpFlag = true;
		}
		if (bInAir)
		{
			bDirectionQueueFlag = true;
			directionQueue = SwipeDirection.Jump;
		}
	}
	else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))//Right
	{
		if (iLanePosition != 1) 
		{
			iLastLanePosition = iLanePosition;
			iLanePosition++;
			
			strafePlayer(StrafeDirection.right);
			
		}//end of lane check if
	}
	else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))//Left
	{
		if (iLanePosition != -1) 
		{
			iLastLanePosition = iLanePosition;
			iLanePosition--;
			
			strafePlayer(StrafeDirection.left);
			
		}//end of lane check if
	}
	else if ( (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && bInDuck)
	{
		bDirectionQueueFlag = true;
		directionQueue = SwipeDirection.Duck;
	}
	else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !bInAir && !bInDuck)
	{
		duckPlayer();
	}
	else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && bInAir && !bInDuck)
	{
		bDiveFlag = true;	//used by Set Transform() to make the character dive
	}
	
}//end of Movement Control function

/*
*	FUNCTION: make the character slide
*	CALLED BY: SwipeMovementControl()
*/
function duckPlayer()
{
	bInDuck = true;
	hSoundManager.stopSound(CharacterSounds.Footsteps);
	
	aPlayer["slide"].speed = 1.4;
	aPlayer.CrossFade("slide", 0.1);
	
	tFrontCollider.localScale = v3BNCDefaultScale/2;	//reduce the near collider size
	tPlayerSidesCollider.localScale = v3BFCDefaultScale/2;	//reduce the far collider size
}

/*
*	FUNCTION: Check if the user is sliding
*/
function isPlayingDuck():boolean
{
	if (hInGameScript.isEnergyZero())
		return false;
	
	if (aPlayer.IsPlaying("slide"))
		return true;
	else
		return false;
        
}

/*
*	FUNCTION: strafe charater right or left
*	INPUT: "right" OR "left"
*	OUTPUT: move the character left or right
*/
function strafePlayer(strafeDirection : StrafeDirection)
{
    
	if (isInAir())
	{	
		aPlayer[strafeDirection.ToString()].speed = 2;
		aPlayer.Play(strafeDirection.ToString());		
	}
	else if (aPlayer.IsPlaying(strafeDirection.ToString()))	//if strafed while already strafing
	{
		aPlayer.Stop(strafeDirection.ToString());
		
		aPlayer[strafeDirection.ToString()].speed = 1.75;
		aPlayer.CrossFade(strafeDirection.ToString(),0.01);
		
		bInStrafe = true;
	}
	else
	{
		aPlayer[strafeDirection.ToString()].speed = 1.75;
		aPlayer.CrossFade(strafeDirection.ToString(),0.01);
		
		bInStrafe = true;
	}
}

public function getCurrentMileage():float { return fCurrentMileage; }
public function getCurrentForwardSpeed():float { return fCurrentForwardSpeed; }
public function getCurrentLane():int { return iLanePosition; }
public function getCurrentPlayerRotation():float { return tCurrentAngle; }
public function getCurrentWalkSpeed():float { return fCurrentWalkSpeed; }
public function isInAir():boolean
{
	if (bInAir || bJumpFlag || bInJump || bDiveFlag)
		return true;
	else
		return false;
}

public function setCurrentDistanceOnPath(iValue:float) { fCurrentDistanceOnPath = iValue; }
public function setPitFallLerpValue(iValue:float) { fPitFallLerpValue = iValue; }
public function setPitFallForwardSpeed(iValue:float) { fPitFallForwardSpeed = iValue; }

/*
*	FUNCTION: Turn player animations On or Off
*/
public function togglePlayerAnimation(bValue:boolean):void { aPlayer.enabled = bValue; }
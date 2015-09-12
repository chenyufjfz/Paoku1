#pragma strict

/*
*	FUNCTION:
*	This script controls the camera movements based on the Player's movements.
*
*	USED BY: This script is part of the "Main Camera" prefab.
*
*/

private var tPlayer : Transform;	//player transform
private var tCamera : Transform;	//Main Camera transform
private var tPlayerMesh : Transform;	//player's mesh

//script references
private var hInGameScript : InGameScript;
private var hControllerScript : ControllerScript;

//defines the distance between the player and the camera
//set in the CameraMain() function
private var fCameraLerpValue : float;

//camera position variables
//all these variables are changed continuously during runtime
//to set the camera position
private var fCameraDistance : float = 30;//distance between player and camera
private var v3CamDirection : Vector3;	//camera direction
private var fCurrentCamDir : float = 90.0;	//camera rotation based on player's rotation
private var fCameraRotationX : float = 0.0;	//camera x rotation
private var fCameraRotationZ : float = 0.0;	//camera z rotation
private var fCameraPositionY : float = 35;	//camera Y position
private var fCameraPositionX : float = -10;	//camera X position

private var iCameraState : int = 0;	//camera state
private var fCamShakeImpulse : float = 0.0;	//Camera Shake Impulse

function Start()
{
	tCamera = camera.main.transform;
	tPlayerMesh = GameObject.Find("PlayerRotation/PlayerMesh").transform;
	tPlayer = GameObject.Find("Player").transform;
	
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
	hControllerScript = GameObject.Find("Player").GetComponent(ControllerScript) as ControllerScript;
			
	fCameraRotationX = tCamera.localEulerAngles.x;
	fCameraRotationZ = tCamera.localEulerAngles.z;
	
	iCameraState = 0;	
	fCamShakeImpulse = 0.0;
}

/*
*	FUNCTION: Start following the player
*	CALLED BY: InGameScript.launchGame()
*/
public function launchGame()
{	
	iCameraState = 1;
}

function Update ()
{
	if(hInGameScript.isGamePaused()==true)		
		return;
	
	if (hInGameScript.isEnergyZero())	//switch to death camera state on depletion of energy
		iCameraState = 2;
}

function FixedUpdate()
{
	CameraMain();//camera transitions
}

/*
*	FUNCTION: Controls camera movements
*	CALLED BY: FixedUpdate()
*/
private function CameraMain()
{
	fCameraDistance = Mathf.Lerp(fCameraDistance,fCameraLerpValue,Time.deltaTime*1.5);
	fCurrentCamDir = Mathf.Lerp(fCurrentCamDir,-hControllerScript.getCurrentPlayerRotation()+90.0,Time.deltaTime*4.0);
	tCamera.localEulerAngles = Vector3(fCameraRotationX, fCurrentCamDir, fCameraRotationZ);
	v3CamDirection = rotateAlongY(Vector3(-1,0,0),-hControllerScript.getCurrentPlayerRotation());
		
	if (iCameraState == 1)	//regular gameplay
	{
		fCameraLerpValue = 35;//maintain a static distance between camera and the player
		tCamera.position.x = tPlayerMesh.position.x + v3CamDirection.x*fCameraDistance + fCameraPositionX;
		tCamera.position.z = Mathf.Lerp(tCamera.position.z, (tPlayerMesh.position.z + v3CamDirection.z*fCameraDistance), Time.deltaTime*50);		
		tCamera.position.y = Mathf.Lerp(tCamera.position.y, tPlayerMesh.position.y + fCameraPositionY, Time.deltaTime*70);		
	}	
	else if(iCameraState == 2)	//Camera on death 
	{	
		fCameraLerpValue = 60;//increase the distance between the camera and the player
		tCamera.position = tPlayerMesh.position + v3CamDirection*fCameraDistance;
		tCamera.position.y+= 30.0;//increase the height of the camera
		
		tCamera.localEulerAngles.x = Mathf.Lerp(tCamera.localEulerAngles.x, 40, Time.deltaTime*25);//change the camera angle (look at the death scene)
	}
	
	//make the camera shake if the fCamShakeImpulse is not zero
	if(fCamShakeImpulse>0.0)
		shakeCamera();
}

/*
*	FUNCTION: Calculate camera rotation vector based on player movement
*	CALLED BY: CameraMain()
*	PARAMETER 1: Camera rotation vector
*	PARAMETER 2: Player rotation value
*/
private function rotateAlongY(inputVector : Vector3, angletoRotate : float):Vector3
{
	var FinalVector : Vector3 = Vector3.zero;
	angletoRotate = angletoRotate/57.3;
	FinalVector.x = Mathf.Cos(-angletoRotate) * inputVector.x - Mathf.Sin(-angletoRotate) * inputVector.z;
	FinalVector.z = Mathf.Sin(-angletoRotate) * inputVector.x + Mathf.Cos(-angletoRotate) * inputVector.z;
	
	return FinalVector;
}

/*
*	FUNCTION: Set the intensity of camera vibration
*	PARAMETER 1: Intensity value of the vibration
*/
public function setCameraShakeImpulseValue(iShakeValue : int)
{
	if(iShakeValue==1)
		fCamShakeImpulse = 1.0;
	else if(iShakeValue==2)
		fCamShakeImpulse = 2.0;
	else if(iShakeValue==3)
		fCamShakeImpulse = 1.3;
	else if(iShakeValue==4)
		fCamShakeImpulse = 1.5;
	else if(iShakeValue==5)
		fCamShakeImpulse = 1.3;
}

/*
*	FUNCTION: Make the camera vibrate. Used for visual effects
*/
private function shakeCamera()
{
	tCamera.position.y+=Random.Range(-fCamShakeImpulse,fCamShakeImpulse);
	tCamera.position.z+=Random.Range(-fCamShakeImpulse,fCamShakeImpulse);
	fCamShakeImpulse-=Time.deltaTime * fCamShakeImpulse*4.0;
	if(fCamShakeImpulse<0.01)
		fCamShakeImpulse = 0.0;
}

#pragma strict

/*
*	FUNCTION: Detects and controls the swipes during gameplay.
*	
*	USED BY: This script is a part of the "Player" prefab.
*
*	INFO: Call the getSwipeDirection() function in the Update() function of your script responsible for character control
*
*/

public enum SwipeDirection 
{
	Null = 0,//no swipe detected
	Duck = 1,//swipe down detected
	Jump = 2,//swipe up detected
	Right = 3,//swipe right detected
	Left = 4//swipe left detected
}

//Constants
private var fSensitivity : float = 15;

//VARIABLES
//distance calculation
private var fInitialX : float;
private var fInitialY : float;
private var fFinalX : float;
private var fFinalY : float;

private var inputX : float;				//x-coordinate
private var inputY : float;				//y-coordinate
private var slope : float;				//slope (m) of the the 
private var fDistance : float;			//magnitude of distance between two positions
private var iTouchStateFlag : int;	//flag to check
private var sSwipeDirection : SwipeDirection;			//string to receive swipe output

function Start ()
{
	fInitialX = 0.0f;
	fInitialY = 0.0f;
	fFinalX = 0.0f;
	fFinalY = 0.0f;
	
	inputX = 0.0f;
	inputY = 0.0f;
	
	iTouchStateFlag = 0;
	sSwipeDirection = SwipeDirection.Null;
}

function Update()
{
	if (iTouchStateFlag == 0 && Input.GetMouseButtonDown(0))	//state 1 of swipe control
	{		
		fInitialX = Input.mousePosition.x;	//get the initial x mouse/ finger value
		fInitialY = Input.mousePosition.y;	//get the initial y mouse/ finger value
		
		sSwipeDirection = SwipeDirection.Null;
		iTouchStateFlag = 1;
	}		
	if (iTouchStateFlag == 1)	//state 2 of swipe control
	{
		fFinalX = Input.mousePosition.x;
		fFinalY = Input.mousePosition.y;
		
		sSwipeDirection = swipeDirection();	//get the swipe direction
		if (sSwipeDirection != SwipeDirection.Null)
			iTouchStateFlag = 2;
	}//end of state 1		
	if (iTouchStateFlag == 2 || Input.GetMouseButtonUp(0))	//state 3 of swipe control
	{
		iTouchStateFlag = 0;
	}//end of M.R. swipe control
}

/*
*	FUNCTION: Return swipe direction.
*	RETURNS: Returns NULL if no swipes are detected.
*			  Returns SwipeDirection if a swipe is detected
*/
public function getSwipeDirection():SwipeDirection
{
	if (sSwipeDirection != SwipeDirection.Null)//if a swipe is detected
	{
		var etempSwipeDirection = sSwipeDirection;
		sSwipeDirection = SwipeDirection.Null;
		
		return etempSwipeDirection;
	}
	else
		return SwipeDirection.Null;//if no swipe was detected
}

/*
*	FUNCTION: Calculate the swipe direction
*/
private function swipeDirection()
{
	//calculate the slope of the swipe
	inputX = fFinalX - fInitialX;
	inputY = fFinalY - fInitialY;
	slope = inputY / inputX;
	
	//calculate the distance of tap start and end
	fDistance = Mathf.Sqrt( Mathf.Pow((fFinalY-fInitialY), 2) + Mathf.Pow((fFinalX-fInitialX), 2) );
	
	if (fDistance <= (Screen.width/fSensitivity))//higher the dividing factor higher the sensitivity
		return SwipeDirection.Null;
	
	if (inputX >= 0 && inputY > 0 && slope > 1)//first octant JUMP
	{		
		return SwipeDirection.Jump;
	}
	else if (inputX <= 0 && inputY > 0 && slope < -1)//eighth octant  JUMP
	{
		return SwipeDirection.Jump;
	}
	else if (inputX > 0 && inputY >= 0 && slope < 1 && slope >= 0)//second octant  RIGHT
	{
		return SwipeDirection.Right;
	}
	else if (inputX > 0 && inputY <= 0 && slope > -1 && slope <= 0)//third octant  RIGHT
	{
		return SwipeDirection.Right;
	}
	else if (inputX < 0 && inputY >= 0 && slope > -1 && slope <= 0)//seventh octant  LEFT
	{
		return SwipeDirection.Left;
	}
	else if (inputX < 0 && inputY <= 0 && slope >= 0 && slope < 1)//sixth octant  LEFT
	{
		return SwipeDirection.Left;
	}
	else if (inputX >= 0 && inputY < 0 && slope < -1)//fourth octant  DUCK
	{
		return SwipeDirection.Duck;
	}
	else if (inputX <= 0 && inputY < 0 && slope > 1)//fifth octant  DUCK
	{
		return SwipeDirection.Duck;
	}//end of else if
	
	return SwipeDirection.Null;	
}//end of SwipeDirection function


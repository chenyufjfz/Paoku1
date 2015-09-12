#pragma strict

/*
*	FUNCTION:
*	- This script keeps a record of the CPs of active patches.
*	- This script translates the player position (controlled by ControllerScript.js) 
*	on the spline into Vector3 cooridiantes.
*	- It calculates the angle of player and obstacles according to the spline.
*	- It supports the ElementsGenerator.js to generate obstacles on the path also by
*	translating position on spline to Vector3 coordinates.
*
*	USED BY:
*	This script is a part of the "Player" prefab.
*
*	INFO: Understanding of algorithms that make the calculation based on spline is not
*	necessary. A high level undertstanding of each function can however can be handy.
*
*/

static var fPathLength : float = 0.0;//length displacement of current patch
static var fNextPathLength : float = 0.0;//length displacement of next patch
private var defaultPathLength : float = 3000.0;

private var CurrentAngle : float = 0.0;
private var CurrentDir : Vector3;
private var Current_MidPoint : Vector2;

private var CPPositions : Vector3[];
private var NextCPPositions : Vector3[];

private var WaypointAngle : float = 0.0;
private  var CurrentPercent : float = 0.0;

private var goCPsGroup : GameObject;
private var goNextCPsGroup : GameObject;
private var tCPsGroup : Transform;
private var tNextCPsGroup : Transform;

//script references
private var hInGameScript : InGameScript;
private var hControllerScript : ControllerScript;
private var hPatchesRandomizer : PatchesRandomizer;
private var hElementsGenerator : ElementsGenerator;

private var PatchNumber : int = 0;

function Start()
{	
	WaypointAngle = 0.0;
	fPathLength = defaultPathLength;
	fNextPathLength = defaultPathLength;
	CurrentPercent = 0.0;
	
	hInGameScript = this.GetComponent(InGameScript) as InGameScript;
	hControllerScript = this.GetComponent(ControllerScript) as ControllerScript;
	hPatchesRandomizer = this.GetComponent(PatchesRandomizer) as PatchesRandomizer;
	hElementsGenerator = this.GetComponent(ElementsGenerator) as ElementsGenerator;
}

/*
*	FUNCTION: Get the CP gameobjects of the currently active patches.
*	USED BY:	PatchesRandomizer.Start()
*				PatchesRandomizer.Start()
*/
public function setChildGroups()
{
	for (var child:Transform in hPatchesRandomizer.getCurrentPatch().transform)
	{
		if(child.name.Contains("CheckPoints"))
			goCPsGroup = child.gameObject;
	}
	for (var child:Transform in hPatchesRandomizer.getNextPatch().transform)
	{
		if(child.name.Contains("CheckPoints"))
			goNextCPsGroup = child.gameObject;
	}
}

/*
*	FUNCTION: Get the CP transforms of the currently active patch and store them in array.
*	USED BY: PatchesRandomizer.Start()
*/
public function SetCurrentPatchCPs()
{	
	var i : int = 0;
	
	CurrentAngle = 90.0;
	
	tCPsGroup = goCPsGroup.transform;
	fPathLength = (tCPsGroup.GetComponent(PathLineDrawer) as PathLineDrawer).fPathLength;
	
	CPPositions = [];
	CPPositions = new Vector3[(tCPsGroup.GetComponent(PathLineDrawer) as PathLineDrawer).Parameterized_CPPositions.length];
	for(i=0;i<CPPositions.length;i++)
	{
		CPPositions[i] = (tCPsGroup.GetComponent(PathLineDrawer) as PathLineDrawer).Parameterized_CPPositions[i];
		CPPositions[i].x = CPPositions[i].x + PatchNumber * defaultPathLength;
	}
	
	PatchNumber++;	
}

/*
*	FUNCTION: Get the CP transforms of the next active patch and store them in array.
*	USED BY: PatchesRandomizer.Start()
*/
public function SetNextPatchCPs()
{
	var i : int = 0;
	
	tNextCPsGroup = goNextCPsGroup.transform;
	fNextPathLength = (tNextCPsGroup.GetComponent(PathLineDrawer) as PathLineDrawer).fPathLength;
	
	NextCPPositions = [];
	NextCPPositions = new Vector3[(tNextCPsGroup.GetComponent(PathLineDrawer) as PathLineDrawer).Parameterized_CPPositions.length];
	for(i=0;i<NextCPPositions.length;i++)
	{
		NextCPPositions[i] = (tNextCPsGroup.GetComponent(PathLineDrawer) as PathLineDrawer).Parameterized_CPPositions[i];
		NextCPPositions[i].x = NextCPPositions[i].x + PatchNumber * defaultPathLength;
	}
}

/*
*	FUNCTION: Gets the vector position from the location on the spline.
*	
*	PARAMETER 1: The array of CPs of the patch.
*	PATAMETER 2: Location on spline.
*/
//andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
private function Interp(pts : Vector3[] , t : float) : Vector3
{
	t = Mathf.Clamp(t,0.0,2.0);
	var numSections : int  = pts.Length - 3;
	var currPt : int  = Mathf.Min(Mathf.FloorToInt(t * parseFloat(numSections)), numSections - 1);
	var u : float = t * parseFloat(numSections) - parseFloat(currPt);
	var a : Vector3 = pts[currPt];
	var b : Vector3 = pts[currPt + 1];
	var c : Vector3 = pts[currPt + 2];
	var d : Vector3 = pts[currPt + 3];
	
	return .5f * (
		(-a + 3f * b - 3f * c + d) * (u * u * u)
		+ (2f * a - 5f * b + 4f * c - d) * (u * u)
		+ (-a + c) * u
		+ 2f * b
	);
}

/*
*	FUNCTION: Set distance on patch based on the spline.
*	CALLED BY: ControllerScript.SetTransform()
*/
public function SetNextMidPointandRotation(CurrentDistanceOnPath : float, CurrentForwardSpeed : float) : float
{
	CurrentPercent = (CurrentDistanceOnPath+CurrentForwardSpeed)/fPathLength;
	
	if(CurrentPercent>=1.0)
	{
		var PreviousPathLength : float = fPathLength;
		(GameObject.Find("Player").GetComponent(PatchesRandomizer) as PatchesRandomizer).createNewPatch();
		
		CurrentDistanceOnPath = (CurrentDistanceOnPath+CurrentForwardSpeed) - PreviousPathLength;
		CurrentDistanceOnPath = Mathf.Abs(CurrentDistanceOnPath);
		hControllerScript.setCurrentDistanceOnPath(CurrentDistanceOnPath);
		SetCurrentPatchCPs();
		SetNextPatchCPs();
		
		CurrentPercent = (CurrentDistanceOnPath+CurrentForwardSpeed)/fPathLength;
	}
	
	var MidPointVector3 : Vector3 = Interp(CPPositions,CurrentPercent);

	Current_MidPoint.x = MidPointVector3.x;
	Current_MidPoint.y = MidPointVector3.z;
	
	var ForwardPointVector3 : Vector3 = Interp(CPPositions,CurrentPercent+0.001);
	var BackPointVector3 : Vector3 = Interp(CPPositions,CurrentPercent-0.001);
	CurrentDir = ForwardPointVector3 - BackPointVector3;
	CurrentAngle = PosAngleofVector(CurrentDir);
	if(CurrentAngle>180.0)
		CurrentAngle = CurrentAngle-360.0;
		
	return (CurrentDistanceOnPath+CurrentForwardSpeed);
}

/*
*	FUNCTION:	Calculate the rotation along y-axis based on the position.
*	USED BY:	SetNextMidPointandRotation(...)
*				SetNextMidPointandRotation(...)
*				getCurrentWSPointBasedOnPercent(...)
*				getNextWSPointBasedOnPercent(...)
*	
*/
private function PosAngleofVector(InputVector : Vector3)
{
	var AngleofInputVector : float = 57.3 * (Mathf.Atan2(InputVector.z,InputVector.x));
	if(AngleofInputVector<0.0)
		AngleofInputVector = AngleofInputVector + 360.0;
	return AngleofInputVector;
}

/*
*	FUNCTION: The angle the spline makes on the percentile position on current patch.
*	USED BY: ControllerScript.SetTransform()
*	INFO: The CurrentAngle is calculated byte the SetNextMidPointandRotation(...) and
*	returned to the ControllerScript using this getter function in each frame.
*/
public function getCurrentAngle():float
{
	return CurrentAngle;
}

/*
*	FUNCTION: Get the Vector3 position based on area covered on spline of current patch.
*	USED BY: ElementsGenerator.getPosition(...)
*	INFO: The spline is assumed to be 1. The starting point is 0.0 and
*	the end point is 1.0. Any value between 0 and 1 is the percentile position on
*	the spline. 
*/
public function getCurrentWSPointBasedOnPercent(Percent_Val: float):Vector3
{
	var NextSideDir : Vector3 = Vector3(0,0,1);
	
	var ForwardPointVector3 : Vector3 = Interp(CPPositions,(Percent_Val+0.001));
	var BackPointVector3 : Vector3 = Interp(CPPositions,(Percent_Val-0.001));
	var NextCurrentDir : Vector3 = ForwardPointVector3 - BackPointVector3;
	NextSideDir = RotateY0Vector(NextCurrentDir, 90.0);
	NextSideDir.Normalize();
	WaypointAngle = PosAngleofVector(NextCurrentDir);
	if(WaypointAngle>180.0)
		WaypointAngle = WaypointAngle-360.0;
	
	return Interp(CPPositions,Percent_Val);
}

/*
*	FUNCTION: Get the Vector3 position based on area covered on spline of next patch.
*	USED BY: ElementsGenerator.getPosition(...)
*	INFO: The spline is assumed to be 1. The starting point is 0.0 and
*	the end point is 1.0. Any value between 0 and 1 is the percentile position on
*	the spline. 
*/
public function getNextWSPointBasedOnPercent(Percent_Val: float):Vector3
{
	var NextSideDir : Vector3 = Vector3(0,0,1);
	
	var ForwardPointVector3 : Vector3 = Interp(NextCPPositions,(Percent_Val+0.001));
	var BackPointVector3 : Vector3 = Interp(NextCPPositions,(Percent_Val-0.001));
	var NextCurrentDir : Vector3 = ForwardPointVector3 - BackPointVector3;
	NextSideDir = RotateY0Vector(NextCurrentDir, 90.0);
	NextSideDir.Normalize();
	WaypointAngle = PosAngleofVector(NextCurrentDir);
	if(WaypointAngle>180.0)
		WaypointAngle = WaypointAngle-360.0;
	
	return Interp(NextCPPositions,Percent_Val);
}

/*
*	FUNCTION: The angle the spline makes on the percentile position on current patch.
*	USED BY: ElementsGenerator.generateElements(...)
*	INFO: The WaypointAngle is calculated int the getNextWSPointBasedOnPercent(...)
*	function and used by ElementsGenerator whenever an element is placed on the path.
*/
public function getWaypointAngle():float
{
	return WaypointAngle;
}

/*
*	FUNCTION: Get the current direction of the player on the spline.
*	USED BY: ControllerScript.SetTransform()
*/
public function getCurrentDirection():Vector3 { return CurrentDir; }

/*
*	FUNCTION: Get the exact position under the spline.
*	USED BY: ControllerScript.calculateHorizontalPosition(...)
*	INFO: This is used to position the player character in one of the three lanes.
*/
public function getCurrentMidPoint():Vector2 { return Current_MidPoint; }

/*
*	FUNCTION: Calculate the needed rotation based on spline direction.
*	USED BY: 	getCurrentWSPointBasedOnPercent(...)
*				getNextWSPointBasedOnPercent(...)
*/
private function RotateY0Vector(inputVector : Vector3, angletoRotate : float)
{
	var FinalVector : Vector3 = Vector3.zero;
	angletoRotate = angletoRotate/57.3;
	FinalVector.x = Mathf.Cos(angletoRotate) * inputVector.x - Mathf.Sin(angletoRotate) * inputVector.z;
	FinalVector.z = Mathf.Sin(angletoRotate) * inputVector.x + Mathf.Cos(angletoRotate) * inputVector.z;
	
	return FinalVector;
}
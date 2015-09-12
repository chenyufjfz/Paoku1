#pragma strict
/*
*	FUNCTION:
*	- This script is responsible for drawing the spline visually in the editor.
*	
*	USED BY: 
*	This script is part of the CP_Straight and CP_Curve prefab which are used with
*	environment patches.
*
*/

var Parameterized_CPPositions : Vector3[] = new Vector3[53];
var fPathLength : float = 3000.0;	//default length displacement of each patch

function OnDrawGizmos ()
{
	if(transform.Find("CP_01"))
	{
		var ParentGroup : Transform = transform;
		var i : int = 0;
		for (var child : Transform in ParentGroup)
			i++;
		var NumCPs : int = i;
		var vectorsArray : Vector3[] = new Vector3[NumCPs];
		for(i=0;i<NumCPs;i++)
		{
			if(i<9)
				vectorsArray[i] = ParentGroup.Find("CP_0"+(i+1)).position;
			else
				vectorsArray[i] = ParentGroup.Find("CP_"+(i+1)).position;
		}
	    iTween.DrawPath(vectorsArray);
    }
}

function SetCPValues()
{
	SetCurrentPatchCPs();
}

private function SetCurrentPatchCPs()
{
	var i : int = 0;
	
	var tempCPPositions : Vector3[];
	var AllCPs : Transform[];
	Parameterized_CPPositions = [];
	
	var CPs_Group : Transform = transform;
	
	for (var child : Transform in CPs_Group)
		i++;
	var NumCPs : int = i;
	
	i = 0;
	
	AllCPs = new Transform[NumCPs];
	tempCPPositions = new Vector3[NumCPs];
	
	i = 0;
	for (var child : Transform in CPs_Group)
	{
		AllCPs[i] = child;
		i++;
	}
	
	AllCPs = SortCPsbyName(AllCPs, 0,NumCPs);
	
	for(i=0;i<NumCPs;i++)
	{
		tempCPPositions[i] = AllCPs[i].position;
		tempCPPositions[i].y = 0;
	}	
	
	Parameterized_CPPositions = PathControlPointGenerator(tempCPPositions);
	Parameterized_CPPositions = ParameterizeCPs(Parameterized_CPPositions);
	
	fPathLength = PathLength(Parameterized_CPPositions);
}

private function ParameterizeCPs(pts : Vector3[]) : Vector3[]
{
	var i : float = 0.0;
	var Current_TD : float = 0.0;	 //Current total distance
	var TotalPathLength : float = PathLength(pts);
	
	var CP_Increment : float = TotalPathLength/50.0;
	var PreviousPoint : Vector3 = pts[1];
	var CurrentPoint : Vector3 = PreviousPoint;
	var FinalPoints : Vector3[] = new Vector3[51];
	var Index : int = 0;
	FinalPoints[Index] = pts[1];
	Index++;
	for(i=0;i<=1.0;i+=0.000001)
	{
		CurrentPoint = Interp(pts,i);
		Current_TD+=Vector3.Distance(CurrentPoint,PreviousPoint);
		if(Current_TD>=CP_Increment)
		{
			FinalPoints[Index] = CurrentPoint;
			Current_TD = 0;
			Index++;
		}
		PreviousPoint = CurrentPoint;
	}
	FinalPoints[50] = pts[pts.length-2];
	FinalPoints = PathControlPointGenerator(FinalPoints);

	return FinalPoints;
}

private function PathControlPointGenerator(path : Vector3[]) : Vector3[]
{
		var suppliedPath : Vector3[];
		var vector3s : Vector3[];
		
		//create and store path points:
		suppliedPath = path;

		//populate calculate path;
		var offset : int  = 2;
		vector3s = new Vector3[suppliedPath.Length+offset];
		System.Array.Copy(suppliedPath,0,vector3s,1,suppliedPath.Length);
		
		//populate start and end control points:
		vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
		vector3s[vector3s.Length-1] = vector3s[vector3s.Length-2] + (vector3s[vector3s.Length-2] - vector3s[vector3s.Length-3]);
		
		//is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
		if(vector3s[1] == vector3s[vector3s.Length-2]){
			var tmpLoopSpline : Vector3[]  = new Vector3[vector3s.Length];
			System.Array.Copy(vector3s,tmpLoopSpline,vector3s.Length);
			tmpLoopSpline[0]=tmpLoopSpline[tmpLoopSpline.Length-3];
			tmpLoopSpline[tmpLoopSpline.Length-1]=tmpLoopSpline[2];
			vector3s=new Vector3[tmpLoopSpline.Length];
			System.Array.Copy(tmpLoopSpline,vector3s,tmpLoopSpline.Length);
		}
		
		return(vector3s);
}
	
//andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
private function Interp(pts : Vector3[] , t : float) : Vector3
{
		t = Mathf.Clamp(t,0.0,2.0);
		//t = ActualPercentage(t);
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

private function PathLength(pathPoints : Vector3[]) : float
{
		var pathLength : float  = 0;
		
		var vector3s : Vector3[]  = pathPoints;
		
		//Line Draw:
		var prevPt : Vector3  = Interp(vector3s,0);
		var SmoothAmount : int  = pathPoints.Length*20;
		for (var i : int = 1; i <= SmoothAmount; i++)
		{
			var pm : float = parseFloat(i) / SmoothAmount;
			var currPt : Vector3  = Interp(vector3s,pm);
			pathLength += Vector3.Distance(prevPt,currPt);
			prevPt = currPt;
		}
		
		return pathLength;
}

private function SortCPsbyName(CPs : Transform[], startIndex : int, endIndex : int) : Transform[]
{
	var names = new Array();
	//var final = new Array();
    
    var tempCPs : Transform[] = new Transform[endIndex-startIndex];
    var j : int = 0;
    var i : int = 0;
    for(i=startIndex;i<endIndex;i++)
    {
    	tempCPs[j] = CPs[i];
    	j++;
    }
    
	for (var go : Transform in tempCPs)
		names.Push(go.name);
	
	names.Sort();
	
	i=startIndex;
	for (var name : String in names)
	{
		for (var go : Transform in tempCPs)
		{
			if(go.name==name)
			{
				CPs[i] = go;
				break;
			}
    	}
    	i++;
    }
	return CPs;
}

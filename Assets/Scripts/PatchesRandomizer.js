#pragma strict

/*
*	FUNCTION:
*	- This scirpt handles the creation and destruction of the environment patches.
*
*	USED BY:
*	This script is a part of the "Player" prefab.
*
*/

var patchesPrefabs : GameObject[];//patches that will be generated

private var goPreviousPatch : GameObject;//the patch the the player passed
private var goCurrentPatch : GameObject;//the patch the player is currently on
private var goNextPatch : GameObject;//the next patch located immediatly after current patch
private var fPatchDistance : float = 3000.0;//default displacement of patch
private var tPlayer : Transform;//player transform

private var fPreviousTotalDistance : float = 0.0;//total displacement covered
private var iCurrentPNum : int = 1;//number of patches generated

//script references
private var hInGameScript : InGameScript;
private var hElementsGenerator : ElementsGenerator;
private var hCheckPointsMain : CheckPointsMain;

//get the current path length
public function getCoveredDistance():float { return fPreviousTotalDistance; } 

function Start()
{
	iCurrentPNum = 1;	
	fPreviousTotalDistance = 0.0;
	
	hInGameScript = this.GetComponent(InGameScript) as InGameScript;
	hCheckPointsMain = GetComponent(CheckPointsMain) as CheckPointsMain;
	hElementsGenerator = this.GetComponent(ElementsGenerator) as ElementsGenerator;
	
	instantiateStartPatch();	
	goPreviousPatch = goCurrentPatch;	
	
	tPlayer = GameObject.Find("Player").transform;
	hCheckPointsMain.setChildGroups();
	
	hCheckPointsMain.SetCurrentPatchCPs();
	hCheckPointsMain.SetNextPatchCPs();
}

function Update()
{
	if(hInGameScript.isGamePaused()==true)
		return;
	
	if(tPlayer.position.x>(iCurrentPNum*fPatchDistance)+100.0)
	{
		Destroy(goPreviousPatch);
		iCurrentPNum++;
	}
}//end of update

/*
*	FUNCTION: Create a new Patch after the player reaches goNextPatch
*/
public function createNewPatch()
{
	goPreviousPatch = goCurrentPatch;
	goCurrentPatch = goNextPatch;
	
	instantiateNextPatch();	
	hCheckPointsMain.setChildGroups();
	
	fPreviousTotalDistance += CheckPointsMain.fPathLength;
	
	hElementsGenerator.generateElements();	//generate obstacles on created patch
}

private function instantiateNextPatch()
{	
	goNextPatch = Instantiate(patchesPrefabs[Random.Range(0,patchesPrefabs.length)],Vector3(fPatchDistance*(iCurrentPNum+1),0,0),Quaternion());
}

/*
*	FUNCTION: Instantiate the first patch on start of the game.
*	CALLED BY: Start()
*/
private function instantiateStartPatch()
{
	goCurrentPatch = Instantiate(patchesPrefabs[Random.Range(0,patchesPrefabs.length)], Vector3(0,0,0),Quaternion());
	goNextPatch = Instantiate(patchesPrefabs[Random.Range(0,patchesPrefabs.length)],Vector3(fPatchDistance,0,0),Quaternion());
}

public function getCurrentPatch():GameObject { return goCurrentPatch; }
public function getNextPatch():GameObject { return goNextPatch; }
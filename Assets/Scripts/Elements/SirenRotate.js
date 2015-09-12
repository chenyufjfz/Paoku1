#pragma strict

/*
*	FUNCTION: Rotate the siren on the police car.
*
*/

private var tBackgroundRotation : Transform;
private var fBackgroundRotateValue : float = 0.0;

function Start () 
{
	tBackgroundRotation = this.transform;
}

function FixedUpdate () 
{
	fBackgroundRotateValue = Mathf.Lerp(fBackgroundRotateValue, 8.0, Time.deltaTime);
	tBackgroundRotation.transform.Rotate(0,fBackgroundRotateValue,0);
}
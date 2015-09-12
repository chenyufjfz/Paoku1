/*/ Attach this to a GUIText to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
*/

private var updateInterval : float = 0.5;
private var accum : float = 0.0; // FPS accumulated over the interval
private var frames : int = 0; // Frames drawn over the interval
private var timeleft : float; // Left time for current interval

private var FPS : float = 0.0;
private var FPS_Text_Ref : GUIText;

function Start()
{
    timeleft = updateInterval;
    FPS_Text_Ref = GetComponent(GUIText) as GUIText;
}

function Update()
{
    timeleft -= Time.deltaTime;
    accum += Time.timeScale/Time.deltaTime;
    ++frames;
    
    // Interval ended - update GUI text and start new interval
    if( timeleft <= 0.0 )
    {
        // display two fractional digits (f2 format)
        FPS = (accum/frames);
        timeleft = updateInterval;
        accum = 0.0;
        frames = 0;
        FPS_Text_Ref.text = System.String.Empty+FPS;
    }
    
}
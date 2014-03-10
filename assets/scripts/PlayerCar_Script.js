// ----------- CAR TUTORIAL SAMPLE PROJECT, ? Andrew Gotow 2009 -----------------

// Here's the basic car script described in my tutorial at www.gotow.net/andrew/blog.
// A Complete explaination of how this script works can be found at the link above, along
// with detailed instructions on how to write one of your own, and tips on what values to 
// assign to the script variables for it to work well for your application.

// Contact me at Maxwelldoggums@Gmail.com for more information.

// This variable is for sound

// This variables are to control the steering visuals.
var SteeringWheel : Transform;

//This variables are to control the dashboard pointers
var VelocityPointer : Transform;
var RPMPointer : Transform;
// These variables allow the script to power the wheels of the car.
var FrontLeftWheel : WheelCollider;
var FrontRightWheel : WheelCollider;

// These variables are for the gears, the array is the list of ratios. The script
// uses the defined gear ratios to determine how much torque to apply to the wheels.
var GearRatio : float[];
var CurrentGear : int = 0;

// These variables are just for applying torque to the wheels and shifting gears.
// using the defined Max and Min Engine RPM, the script can determine what gear the
// car needs to be in.
var EngineTorque : float = 600.0;
var MaxEngineRPM : float = 3000.0;
var MinEngineRPM : float = 1000.0;
private var EngineRPM : float = 0.0;

//
var Velocimeter_display: GUIText; 

public var DEBUG: boolean =false;
public var INPUT_DEBUG: boolean =false;



function Start () {
	// I usually alter the center of mass to make the car more stable. I'ts less likely to flip this way.
	rigidbody.centerOfMass = Vector3 (0, -0.5148, 0); //However this makes the vehicle drift one side
	
}

function Update () {
	
	// This is to limith the maximum speed of the car, adjusting the drag probably isn't the best way of doing it,
	// but it's easy, and it doesn't interfere with the physics processing.
	rigidbody.drag = rigidbody.velocity.magnitude / 150;

	// Compute the engine RPM based on the average RPM of the two wheels, then call the shift gear function
	EngineRPM = (FrontLeftWheel.rpm + FrontRightWheel.rpm)/2 * GearRatio[CurrentGear];
	ShiftGears();

	// set the audio pitch to the percentage of RPM to the maximum RPM plus one, this makes the sound play
	// up to twice it's pitch, where it will suddenly drop when it switches gears.
	//audio.pitch = Mathf.Abs(EngineRPM / MaxEngineRPM) + 1.0 ;
	audio.pitch= Mathf.Clamp01(3*rigidbody.velocity.magnitude/342.0)+1;
	//audio.pitch = Mathf.Abs(EngineRPM / MaxEngineRPM) + 1.0 ;
	// this line is just to ensure that the pitch does not reach a value higher than is desired.
	if ( audio.pitch > 2.0 ) {
		audio.pitch = 2.0;
	}
}
function handleBreak(vertical:float, velocity:float)
{
	var result:float = vertical;
	if(vertical<0.0f && velocity<=0.5f)
			result=0.0f;
	return result;
}
function FixedUpdate()
{
	// finally, apply the values to the wheels.	The torque applied is divided by the current gear, and
	// multiplied by the user input variable.
	var vertical_axis:float = handleBreak(Input.GetAxis("Vertical"),rigidbody.velocity.magnitude)/4;
	var horizontal_axis: float =Input.GetAxis("Horizontal"); 
	
	/*var accelerator_axis:float=Input.GetAxis("Accelerator_Pedal");
	var break_axis:float=Input.GetAxis("Break_Pedal");*/
	
	//if(DEBUG && INPUT_DEBUG) print("Horizontal: "+horizontal_axis+"\tVertical: "+vertical_axis);
	//var break_axis: float = Input.GetAxis("Break");
	
	FrontLeftWheel.motorTorque = EngineTorque / GearRatio[CurrentGear] * vertical_axis;
	FrontRightWheel.motorTorque = EngineTorque / GearRatio[CurrentGear] * vertical_axis;
		
	// the steer angle is an arbitrary value multiplied by the user input.
	SteeringWheel.localRotation=Quaternion.identity;
	
	
	
	var velocity : int = rigidbody.velocity.magnitude*3.6;
	var adjust_steer: float=15f;
	/*if(velocity>50)
		adjust_steer=30f;
	else if (velocity>150)
		adjust_steer=0.001f;*/	
	FrontLeftWheel.steerAngle =adjust_steer * horizontal_axis;
	FrontRightWheel.steerAngle =adjust_steer * horizontal_axis;
	//SteeringWheel.Rotate(0.0,0.0,-1.0*FrontRightWheel.steerAngle);
	SteeringWheel.Rotate(10.5,90.0,horizontal_axis*-90.0);
	
	
	if(Mathf.Abs(velocity)<10)
		Velocimeter_display.text="00"+velocity;
	else if (Mathf.Abs(velocity)<100)
		Velocimeter_display.text="0"+velocity;
	else
		Velocimeter_display.text=velocity.ToString();
	
}

function ShiftGears() {
	// this funciton shifts the gears of the vehcile, it loops through all the gears, checking which will make
	// the engine RPM fall within the desired range. The gear is then set to this "appropriate" value.
	if ( EngineRPM >= MaxEngineRPM ) {
		var AppropriateGear : int = CurrentGear;
		
		for ( var i = 0; i < GearRatio.length; i ++ ) {
			if ( FrontLeftWheel.rpm * GearRatio[i] < MaxEngineRPM ) {
				AppropriateGear = i;
				break;
			}
		}
		
		CurrentGear = AppropriateGear;
	}
	
	if ( EngineRPM <= MinEngineRPM ) {
		AppropriateGear = CurrentGear;
		
		for ( var j = GearRatio.length-1; j >= 0; j -- ) {
			if ( FrontLeftWheel.rpm * GearRatio[j] > MinEngineRPM ) {
				AppropriateGear = j;
				break;
			}
		}
		
		CurrentGear = AppropriateGear;
	}
	//RPMPointer.rotation=Quaternion.identity;
	
	//RPMPointer.RotateAroundLocal(Vector3 (-0.394,1,27.8),90+0.1*FrontLeftWheel.rpm/360.0);
	//RPMPointer.Rotate(0.394*FrontLeftWheel.rpm/360.0,90+0.1*FrontLeftWheel.rpm/360.0,-27.8*FrontLeftWheel.rpm/360.0);
	
	//RPMPointer.Rotate(FrontLeftWheel.rpm,0,0);
}
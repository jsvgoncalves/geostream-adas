using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public float x;
	public float z;
	public float y;
	public int zone;
	public bool useGPS;
	public double initialFakeLat;
	public double initialFakeLon;
	public double fakeLat;
	public double fakeLon;
	public float fakeSpeed = 0;
	public float fakeSpeedIncrement;
	public float fakeSpeedDecrement;
	public float maxFakeSpeed;
	public float fakeRotation;
    public LocalWeather localWeather;
	public GeoUTMConverter playerConverter;
	// Use this for initialization
	public void Start () 
	{
		Input.compass.enabled = true;
		playerConverter = new GeoUTMConverter();
		transform.position = new Vector3(0,20,0);
		if(Settings.useGPS)
		{
			Input.location.Start();
		}
		else
		{
			if(Settings.startLat != 0 && Settings.startLon != 0)
			{
				initialFakeLat = Settings.startLat;
				initialFakeLon = Settings.startLon;
			}
			playerConverter.ToUTM(initialFakeLat,initialFakeLon);
			x = (float)(playerConverter.X / GeoUTMConverter.Precision);
			z = (float)(playerConverter.Y / GeoUTMConverter.Precision);
			zone = (int)playerConverter.Zone;

		}
	}
	
	// Update is called once per frame
	public void Update () 
	{
		if(useGPS)
		{
			LocationInfo li = Input.location.lastData;
			playerConverter.ToUTM((double)li.latitude,(double)li.longitude);
			x = (float)(playerConverter.X / GeoUTMConverter.Precision);
			z = (float)(playerConverter.Y / GeoUTMConverter.Precision);
			zone = (int)playerConverter.Zone;
			transform.position = new Vector3(0,20,0);
		}
		else
		{


			if(Application.platform == RuntimePlatform.Android)
			{
				Input.compensateSensors = true;
				if(Input.GetTouch(0).phase == TouchPhase.Stationary || Input.GetTouch(0).phase == TouchPhase.Moved)
				{
					if(fakeSpeed < -this.maxFakeSpeed)
					{
						fakeSpeed = -maxFakeSpeed;
					}
					else
					{
						fakeSpeed+=  -fakeSpeedIncrement;
					}

					rigidbody.velocity = fakeSpeed*transform.forward;
				}

				transform.localEulerAngles = new Vector3(Input.acceleration.z + transform.localEulerAngles.x,Input.acceleration.x + transform.localEulerAngles.y,Input.acceleration.x*30);

			}

			if(Input.GetKey(KeyCode.A))
			{
				transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y - fakeRotation, transform.rotation.eulerAngles.z);
				
			}
			if(Input.GetKey(KeyCode.D))
			{

				transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + fakeRotation, transform.rotation.eulerAngles.z);
			}
			if(Input.GetKey(KeyCode.W))
			{
				if(fakeSpeed < -this.maxFakeSpeed)
				{
					fakeSpeed = -maxFakeSpeed;
				}
				else
				{
					fakeSpeed+=  -fakeSpeedIncrement;
				}
			
				rigidbody.velocity = fakeSpeed*transform.forward;
				//transform.position = transform.position - fakeSpeed*transform.forward;

			}

			if(Input.GetKey(KeyCode.S))
			{
				if(fakeSpeed > this.maxFakeSpeed)
				{
					fakeSpeed = maxFakeSpeed;
				}
				else
				{
					fakeSpeed+=  +fakeSpeedDecrement;
				}
				rigidbody.velocity = fakeSpeed*transform.forward;
			}
			playerConverter.ToLatLon(transform.position.x + x, transform.position.z + z, (int)playerConverter.Zone, playerConverter.Hemi);
			fakeLat = playerConverter.Latitude;
			fakeLon = playerConverter.Longitude;
		}
		
		
			   

	}
}

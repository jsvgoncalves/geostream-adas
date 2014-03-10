using UnityEngine;
using System.Collections;

public class GyroCamera : MonoBehaviour
{
	Gyroscope gyro;
	
	// Use this for initialization
	void Start ()
	{
		gyro = Input.gyro;
		gyro.enabled = true;
	}
	
	void OnGUI()
	{
		GUI.Label(new Rect(10,10,100,20), gyro.attitude.z.ToString ());
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		transform.rotation = ConvertRotation(gyro.attitude);
	}
	
	private static Quaternion ConvertRotation(Quaternion q)
	{
		return new Quaternion(q.x, q.y, -q.z, -q.w);
	}
}
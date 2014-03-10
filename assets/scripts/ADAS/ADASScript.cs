using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;

public class ADASScript : MonoBehaviour {
	// PUBLIC
	// The update cooldown in ms.
	public int cooldown = 1000;
	// The Android device configs.
	public String Host = "172.30.41.184";
	public Int32 Port = 5173;

	// PRIVATE
	// Socket stuff
	internal Boolean socketReady = false;
	TcpClient mySocket;
	NetworkStream theStream;
	StreamWriter theWriter;
	StreamReader theReader;
	// Timer for location updates.
	long lastUpdate = 0;

	
	// Use this for initialization
	void Start () {
		setupSocket();
	}
	
	// Update is called once per frame
	void Update () {
		// Check to see if the cooldown has passed.
		long current = (long)((TimeSpan)(DateTime.Now-new DateTime(1970,1,1))).TotalMilliseconds;
		if(current >= lastUpdate + cooldown) {
			// Reset the cooldown counter
			lastUpdate = current;

			// Get the DemoPlayerScript component to get the <lat,lon>
			DemoPlayerScript ps = (DemoPlayerScript)(this.GetComponent<DemoPlayerScript>());

			// Write it to the socket.
			//TODO: Get the actual speed limit.
			writeSocket("{\"latitude\": " + ps.getLatitude() + 
			            ",\"longitude\": " + ps.getLongitude() + 
			            ",\"speed-limit\": " + 50 +
			            "}");
//			print(current + " | Current position <" + ps.getLatitude() + ", " + ps.getLongitude() + ">");
		}
	}
	
	public void setupSocket() {
		try {
			mySocket = new TcpClient(Host, Port);
			theStream = mySocket.GetStream();
			theWriter = new StreamWriter(theStream);
			theReader = new StreamReader(theStream);
			socketReady = true;
		}
		catch (Exception e) {
			Debug.Log("Socket error: " + e);
		}
	}
	
	public void writeSocket(string theLine) {
		if (!socketReady)
			return;
		String foo = theLine + "\r\n";
		theWriter.Write(foo);
		theWriter.Flush();
	}
	
	public String readSocket() {
		if (!socketReady)
			return "";
		if (theStream.DataAvailable)
			return theReader.ReadLine();
		return "";
	}
	
	public void closeSocket() {
		if (!socketReady)
			return;
		theWriter.Close();
		theReader.Close();
		mySocket.Close();
		socketReady = false;
	}
}

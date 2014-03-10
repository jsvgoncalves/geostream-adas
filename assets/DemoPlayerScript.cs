using UnityEngine;
using System.Collections;

public class DemoPlayerScript : Player {

    float initialX;
    float initialZ;
	int localzone;

	// Use this for initialization
	void Start () {

        GeoUTMConverter gc = new GeoUTMConverter();
        gc.ToUTM(initialFakeLat, initialFakeLon);
        initialX = (float)gc.X;
        initialZ = (float)gc.Y;
        RaycastHit hit;
		// Store initial gc.zone
		localzone = (int) gc.Zone;
        Vector3 rayPosition = new Vector3(0, 10000, 0);
        if (Physics.Raycast(rayPosition, -Vector3.up, out hit))
        {
            transform.position = new Vector3(0, hit.point.y + 3, 0);
        }
	
	}
	
	// Update is called once per frame
	void Update () {

        RaycastHit hit;
        Vector3 rayPosition = new Vector3(transform.position.x, 10000, transform.position.z);
        if (Physics.Raycast(rayPosition, -Vector3.up, out hit)) {
            transform.position = new Vector3(transform.position.x, hit.point.y+1f, transform.position.z);
        }

        GeoUTMConverter gc = new GeoUTMConverter();
		gc.ToLatLon (initialX + x, initialZ + z, localzone, GeoUTMConverter.Hemisphere.Northern);
		fakeLat = gc.Latitude;
		fakeLon = gc.Longitude;

        if (Input.GetKey(KeyCode.W)) {
            rigidbody.AddForce(100 * transform.forward);
        }
		if (Input.GetKey(KeyCode.S)) {
			rigidbody.AddForce(10 * -transform.forward);
		}
		if (Input.GetKey(KeyCode.A)) {
			transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y-1f, transform.eulerAngles.z);
		}
		if (Input.GetKey(KeyCode.D)) {
			transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y+1f, transform.eulerAngles.z);
		}



        x = transform.position.x;
        z = transform.position.z;

	}

	public double getLatitude() {
			return this.fakeLat;
	}

	public double getLongitude() {
		return this.fakeLon;
	}
}

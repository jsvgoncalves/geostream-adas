using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;


public class GoogleElevationTest : MonoBehaviour {

    public List<float> heights;
	// Use this for initialization
	void Start () {

        GoogleElevation ge = new GoogleElevation();
        ge.coordinates.Add(41.177988f);
        ge.coordinates.Add(-8.598049f);
        ge.coordinates.Add(41.177774f);
        ge.coordinates.Add(-8.596877f);
		ge.SyncGetHeights();
		heights = ge.heights;
	}

    void OnGUI()
    {
        for (int i = 0; i < heights.Count; i++)
        {
            GUI.Label(new Rect(100, 50 * i + 100, 100, 100), heights[i].ToString());
        }
    }
    // Update is called once per frame
    void Update()
    {
	
	}
}

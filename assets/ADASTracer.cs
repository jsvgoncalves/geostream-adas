using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;


public class ADASTracer : MonoBehaviour {
	
	public TextAsset gpsLog;
    private bool done = false;
    GameObject go;
	// Use this for initialization
	void Start () {

//		pl.SetOffset(offsetPositionX, offsetPositionZ);

        Invoke("loadXML", 20);
	}
	
	// Update is called once per frame
	void Update () {

       
	}

	void loadXML(){
       // DestroyObject(go);
        //if (done || !GetComponent<MapChunkManager>().isReady)
        //   return;
		PolyLine pl = new PolyLine("TESTINGTRACER");
		pl.heigth = 1;
		pl.width = 3;
		pl.material = Resources.Load("Materials/blinker") as Material;
		float offsetPositionX = GetComponent<MapChunkManager>().offsetX;
		float offsetPositionZ = GetComponent<MapChunkManager>().offsetZ;
		pl.SetOffset(offsetPositionX, offsetPositionZ);

		XmlDocument XMLFile = new XmlDocument();
		XMLFile.LoadXml(gpsLog.text);
		XmlNodeList coords = XMLFile.GetElementsByTagName("trkpt");
		List<Node> nodes = new List<Node>();
		GeoUTMConverter convertor;
		foreach (XmlNode coord in coords) {
			// coord.Attributes["lat"].Value
			// coord.Attributes["lon"].Value
			Node n = new Node();
			n.lat = double.Parse(coord.Attributes["lat"].Value);
			n.lon = double.Parse(coord.Attributes["lon"].Value);
			convertor = new GeoUTMConverter();
			convertor.ToUTM(n.lat,n.lon);
			n.northing = convertor.Y;
			n.easthing = convertor.X;
			nodes.Add(n);
		}

		for (int a = 0; a < nodes.Count -1; a++) {
			Node n = nodes[a];
			
			Vector3 position = new Vector3((float)(n.easthing - offsetPositionX), 99999, (float)(n.northing - offsetPositionZ));
			float baseHeight = 0;
			RaycastHit hit;
			
			if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity)) {
				baseHeight = hit.point.y;
			}
			n.height = baseHeight + 1f;
			pl.Add(n);

//            Debug.Log("Node is " + (float)(n.easthing - offsetPositionX) + "|" + (float)(n.northing - offsetPositionZ) + "|" + n.height);
//            Color random = new Color(((float)n.easthing % 255) / 255f, ((float)n.northing % 255) / 255f, ((float)n.easthing % 255) / 255f);
//            Debug.DrawLine(new Vector3((float)nodes[a].easthing - offsetPositionX, 200, (float)nodes[a].northing - offsetPositionZ), new Vector3((float)nodes[a + 1].easthing - offsetPositionX, 200, (float)nodes[a + 1].northing - offsetPositionZ), random,300);
		}
		
//		foreach (Node node in nodes) {
//			pl.Add (node);
//		}
		go = pl.Close (transform);
        done = true;

	}
}

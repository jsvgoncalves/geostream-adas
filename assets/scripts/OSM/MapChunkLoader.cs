using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Net;
using System.Threading;


public class MapChunkLoader : MonoBehaviour {


	public static double precision = 1;
	private MapManager mm;
	public bool isLoaded = false;
	public float offsetPositionX;
	public float offsetPositionZ;
	public float minimumLat = 41.1767f;
	public float maximumLat = 41.1923f;
	public float minimumLon = -8.6f;
	public float maximumLon = -8.55f;
	public Material groundMaterial;
	public Material buildingMaterial;
	public Material roadMaterial;
	public int layerMask;
	public bool toUnload = true;
	public List<long> wayList;
    public int numberOfDivisions;
	public MapChunkManager mapManager;
    public bool exportObjs;
	private List<GameObject> buildings;
    public Transform treePrefab;
    private bool workerThreadCompleted = false;
    public Dictionary<string, string> structures = new Dictionary<string, string>();

	// Use this for initialization
	IEnumerator Start () {
        Loom.Current.GetComponent<Loom>();
		layerMask = 1 << 8;
		wayList = new List<long>();
		//StartCoroutine(LoadChunk(-8.6f,41.1767f,-8.55f,41.1923f));
		MapChunkLoader.precision = GeoUTMConverter.Precision;
		GeoUTMConverter latlon2Utm = new GeoUTMConverter();
		latlon2Utm.ToUTM((minimumLat+maximumLat)/2f,(minimumLon+maximumLon)/2f);

        transform.position = new Vector3(((float)latlon2Utm.X - offsetPositionX), -0.1f, ((float)latlon2Utm.Y - offsetPositionZ));

		GameObject floor = new GameObject();
		floor.name = "Ground";
		floor.isStatic = true;
		
        CreateGround cg = new CreateGround();
        cg.maxLat = maximumLat + 0.01f * (maximumLat - minimumLat); //0.0001f;
        cg.maxLon = maximumLon + 0.01f * (maximumLat - minimumLat);
        cg.minLat = minimumLat - 0.01f * (maximumLat - minimumLat);
        cg.minLon = minimumLon - 0.01f * (maximumLat - minimumLat);
        cg.numberOfDivisions = numberOfDivisions;
        
        MeshFilter mf = floor.AddComponent<MeshFilter>();

        MeshRenderer mr = floor.AddComponent<MeshRenderer>();
        mr.material = groundMaterial;
        floor.transform.position = transform.position;
        floor.transform.parent = transform;
        floor.layer = LayerMask.NameToLayer("RayCast");

        string floorPath = Application.persistentDataPath + "Assets/Resources/Objs/" + cg.maxLat + "I" + cg.maxLon + ".obj";

        if (!File.Exists(floorPath)) // If the file isn't cached we calculate everything and then we cache it
        {
            mf.sharedMesh = cg.GetGroundMesh();
            if (exportObjs)
            {
                ObjExporter oe = new ObjExporter();
                oe.MeshToFile(mf, floorPath);
            }
        }
        else
        {
            ObjImporter oi = new ObjImporter();
            StartCoroutine(oi.FileToMesh("file://" + floorPath));

            while (oi._myMesh == null)
            {
                yield return null;
            }

            mf.sharedMesh = oi._myMesh;
            Debug.LogWarning("Loaded Ground Chunk from cache");
        }

        //Texture2D t = new Texture2D(1024, 1024);
        
        MapTexture mt = new MapTexture();
        mt.getTexture(cg.minLon.ToString(), cg.minLat.ToString(), cg.maxLon.ToString(), cg.maxLat.ToString(),Application.persistentDataPath,mr.material);
        while (mt.texture == null)
        {
            yield return null;
        }

       
        
        //t.LoadImage(mt.ReadFully(mt.mq_dataStream));
        //mr.material.SetTexture("_MainTex", t);
        
        MeshCollider m = floor.AddComponent<MeshCollider>();
        Loom l = Loom.Current;
        LoadChunk(minimumLon, minimumLat, maximumLon, maximumLat);
        
		//StartCoroutine();
   	}

	public bool Contains(float lat, float lon)
	{
		if(lat > minimumLat && lat < maximumLat && lon > minimumLon && lon < maximumLon)
			return true;
		else return false;
	}

	

    private IEnumerator LoadChunkElements()
    {
        for (int i = 0; i < mm.ways.Count; i++)
        {
            if (!mapManager.mapHash.Contains(mm.ways[i].id))//!mapManager.wayList.Contains(mm.ways[i]))
            {
                wayList.Add(mm.ways[i].id);
                mapManager.mapHash.Add(mm.ways[i].id);
               // TODO : Complete with dictionary!
                string classType = "";
                if (structures.TryGetValue(mm.ways[i].structureType, out classType))
                {
                    System.Type t = System.Type.GetType(classType + ",Assembly-CSharp");
                    //Debug.Log(t);
                    Structure currentStructure = (Structure)System.Activator.CreateInstance(t);
                    
                    StartCoroutine(currentStructure.Build(mm.ways[i], this));
                    yield return null;
                }
            }
            else
            {
                mapManager.mapHash.Add(mm.ways[i].id);
                wayList.Add(mm.ways[i].id);
            }
        }
        for (int i = 0; i < mm.nodes.Count; i++)
        {
            Node currentNode = mm.nodes[i];
            //TODO : COmplete with Dictionary!
            string classType = "";
            if (structures.TryGetValue(mm.nodes[i].structureType, out classType))
            {
                System.Type t = System.Type.GetType(classType + ",Assembly-CSharp");
                //Debug.Log(t);
                Structure currentStructure = (Structure)System.Activator.CreateInstance(t);
                StartCoroutine(currentStructure.Build(mm.nodes[i], this));
                yield return null;
            }
        }
        isLoaded = true;
        yield return null;

    }

   

    public void LoadChunkThread(float minLon, float minLat, float maxLon, float maxLat, string persistentpath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(persistentpath + "Assets/Resources/OSM/"));
        string cacheFile = persistentpath + "Assets/Resources/OSM/" + minLon + "," + minLat + "," + maxLon + "," + maxLat + ".xml";
        string xmlString = "";
        if (!File.Exists(cacheFile)) // if we haven't cache it we download it and save it.
        {
            string url = "http://www.overpass-api.de/api/xapi?map?bbox=" + minLon + "," + minLat + "," + maxLon + "," + maxLat;
            Debug.Log(url);
            WebResponse webResponse = null;
            HttpWebRequest r = (HttpWebRequest)WebRequest.Create(url);
            webResponse = r.GetResponse();
            Stream dataStream = webResponse.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            xmlString = reader.ReadToEnd();
            Debug.Log("Response was : " + xmlString);
            System.IO.File.WriteAllText(cacheFile, xmlString);
        }
        else // we have it cached, lets read it locally
        {
            xmlString = System.IO.File.ReadAllText(cacheFile);
        }
        
        XmlDocument XMLFile = new XmlDocument();
        
        XMLFile.LoadXml(xmlString);
        mm = new MapManager(XMLFile);
        Loom.QueueOnMainThread(() =>
        {
            StartCoroutine(LoadChunkElements());
        });
        
       
    }
	
	private void LoadChunk(float minLon,float minLat,float maxLon,float maxLat)
    {
        string path = Application.persistentDataPath;
        Thread workerThread = new Thread(() => LoadChunkThread(minLon, minLat, maxLon, maxLat, path));
        workerThread.Start();
    }

    void Update()
    {
      
    }

}



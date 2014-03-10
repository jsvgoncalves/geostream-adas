using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


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
	private int layerMask;
	public bool toUnload = true;
	public List<long> wayList;
    public int numberOfDivisions;
	public MapChunkManager mapManager;
	private List<GameObject> buildings;
    public Transform treePrefab;
	// Use this for initialization
	IEnumerator Start () {

		layerMask = 1 << 8;
		wayList = new List<long>();
		//StartCoroutine(LoadChunk(-8.6f,41.1767f,-8.55f,41.1923f));
		MapChunkLoader.precision = GeoUTMConverter.Precision;
		GeoUTMConverter latlon2Utm = new GeoUTMConverter();
		latlon2Utm.ToUTM((minimumLat+maximumLat)/2f,(minimumLon+maximumLon)/2f);

        transform.position = new Vector3(((float)latlon2Utm.X - offsetPositionX), -0.1f, ((float)latlon2Utm.Y - offsetPositionZ));

		//GeoUTMConverter bottomLeft = new GeoUTMConverter();
		//GeoUTMConverter topRight = new GeoUTMConverter();
		//bottomLeft.ToUTM(minimumLat,minimumLon);
		//topRight.ToUTM(maximumLat,maximumLon);
		GameObject floor = new GameObject();
		floor.name = "Ground";
		floor.isStatic = true;
		/*GoogleElevation ge = new GoogleElevation();

		ge.coordinates.Add(minimumLat);
		ge.coordinates.Add(minimumLon);

		ge.coordinates.Add(minimumLat);
		ge.coordinates.Add(maximumLon);

		ge.coordinates.Add(maximumLat);
		ge.coordinates.Add(maximumLon);

		ge.coordinates.Add(maximumLat);
		ge.coordinates.Add(minimumLon);

        */
		//ge.SyncGetHeights();
        //yield return StartCoroutine(ge.GetHeights());

		//Mesh msh = CreateGroundWithHeights((float)(topRight.X-bottomLeft.X)/1.9f,(float)(topRight.Y-bottomLeft.Y)/1.9f,ge.heights);
        CreateGround cg = new CreateGround();
        cg.maxLat = maximumLat + 0.001f;
        cg.maxLon = maximumLon + 0.001f;
        cg.minLat = minimumLat - 0.001f;
        cg.minLon = minimumLon - 0.001f;
        cg.numberOfDivisions = numberOfDivisions;
        
        MeshFilter mf = floor.AddComponent<MeshFilter>();
        mf.mesh = cg.GetGroundMesh() ;
		MeshRenderer mr = floor.AddComponent<MeshRenderer>();
		mr.material = groundMaterial;
		floor.transform.position = transform.position;
		floor.transform.parent = transform;
        floor.layer = LayerMask.NameToLayer("RayCast");

		MeshCollider m = floor.AddComponent<MeshCollider>();

		yield return StartCoroutine(LoadChunk(minimumLon,minimumLat,maximumLon,maximumLat));



	}

	public bool Contains(float lat, float lon)
	{
		if(lat > minimumLat && lat < maximumLat && lon > minimumLon && lon < maximumLon)
			return true;
		else return false;
	}

	Mesh CreateGroundWithHeights(float width, float height, List<float> heights)
	{
		Mesh m = new Mesh();
		m.name = "ScriptedMesh";
		m.vertices = new Vector3[] {
			new Vector3(-width, heights[0], -height),
			new Vector3(width, heights[1], -height),
			new Vector3(width, heights[2], height),
			new Vector3(-width, heights[3], height)
		};
		m.uv = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2(1, 1),
			new Vector2 (0, 1)
		};
		m.triangles = new int[] { 0, 2, 1, 0, 3, 2};
		m.RecalculateNormals();
		
		return m;
	}

	Mesh CreateGround(float width, float height)
	{
		Mesh m = new Mesh();
		m.name = "ScriptedMesh";
		m.vertices = new Vector3[] {
			new Vector3(-width, 0, -height),
			new Vector3(width, 0, -height),
			new Vector3(width, 0, height),
			new Vector3(-width, 0, height)
		};
		m.uv = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2(1, 1),
			new Vector2 (0, 1)
		};
		m.triangles = new int[] { 0, 2, 1, 0, 3, 2};
		m.RecalculateNormals();
		
		return m;
	}
	
	// Update is called once per frame
	void Update () {
	}

    private void CreateBuilding(Way w)
    {
        Mesh roofm = new Mesh();
        Mesh wallsm = new Mesh();
        Loom.RunAsync(() =>
        {
            Vector3[] nodes = new Vector3[w.nodes.Count];
            Vector2[] xz = new Vector2[w.nodes.Count];

            float height = (float)w.nodes[0].northing % 10 + 2.0f;

            if (w.height != 0)
                height = w.height;

            Vector3 centroid = new Vector3();
			Loom.QueueOnMainThread(()=>{
                Vector3 position;
                RaycastHit hit;
	            for (int a = 0; a < w.nodes.Count; a++)
	            {
	                Node n = w.nodes[a];
					position = new Vector3((float)((n.easthing - offsetPositionX) / precision), 5000, (float)((n.northing - offsetPositionZ) / precision));
					float castH = 0;
					
					if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, layerMask))
					{
						castH = hit.point.y;
					}
					nodes[a] = new Vector3(position.x, castH + height, position.z);
					xz[a] = new Vector2(position.x, position.z);
	                centroid += nodes[a];
	            }
	            centroid /= w.nodes.Count;
	            centroid.y += 1;
			
           

            Vector2[] xzRoof = new Vector2[w.nodes.Count - 1];

            for (int a = 0; a < xzRoof.Length; a++)
            {
                xzRoof[a] = xz[a];
            }

            Triangulator tr = new Triangulator(xzRoof);

            int[] indices = tr.Triangulate();
		
            // Create the mesh
            
            

            Vector2[] uvs = new Vector2[nodes.Length];
            for (int a = 0; a < nodes.Length; a++)
            {
                if (a < nodes.Length - 1)
                {
                    uvs[a] = new Vector2(Mathf.Abs(nodes[a].x - nodes[a + 1].x), Mathf.Abs(nodes[a].z - nodes[a + 1].z));
                }
                else
                {
                    uvs[a] = new Vector2(1, 1);
                }
            }
            

           



            // Set up game object with mesh;

           
				
                centroid = new Vector3(centroid.x, centroid.y, centroid.z);
				BuildingCountourMesh(nodes, wallsm);
				roofm.vertices = nodes;
				roofm.triangles = indices;
                roofm.uv = uvs;
                roofm.RecalculateNormals();
                roofm.RecalculateBounds();

                GameObject build = new GameObject();
                build.name = w.id.ToString();

                build.transform.parent = this.transform;
                GameObject roof = new GameObject();
				//roof.transform.position = new Vector3(0,baseHeight,0);
                roof.name = (2 * w.id).ToString();
                mapManager.mapHash.Add(2 * w.id);
                wayList.Add(2 * w.id);

                GameObject walls = new GameObject();
                walls.name = (3 * w.id).ToString();
				//walls.transform.position = new Vector3(0,baseHeight,0);
                mapManager.mapHash.Add(3 * w.id);
                wayList.Add(3 * w.id);


                if (w.name != null)
                {
                    GameObject label = new GameObject();
                    FloatingLabel lb = label.AddComponent<FloatingLabel>();
                    lb.transform.parent = roof.transform;
                    lb.text = w.name;
                    lb.target = GameObject.FindGameObjectWithTag("Player").transform;
                    lb.transform.position = centroid;
                }
                walls.transform.parent = build.transform;
                roof.transform.parent = build.transform;
                MeshCollider wallmc = walls.AddComponent<MeshCollider>();
                MeshCollider roofmc = roof.AddComponent<MeshCollider>();
           
                walls.AddComponent(typeof(MeshRenderer));
                MeshFilter filter = walls.AddComponent(typeof(MeshFilter)) as MeshFilter;
                filter.mesh = wallsm;
                walls.GetComponent<MeshRenderer>().material = buildingMaterial;
                if (w.height != 0)
                    walls.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Real Height Material") as Material;


                //walls.transform.parent = transform;

                wallmc.sharedMesh = wallsm;

                roof.AddComponent(typeof(MeshRenderer));
                roof.AddComponent(typeof(MeshCollider));
                MeshFilter filter2 = roof.AddComponent(typeof(MeshFilter)) as MeshFilter;
                filter2.mesh = roofm;

                roofmc.sharedMesh = roofm;
                roof.GetComponent<MeshRenderer>().material = buildingMaterial;
                if (w.height != 0)
                    roof.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Real Height Material") as Material;
                roof.transform.parent = transform;
            });
        });
    }

    private void CreateGroundArea(Way w)
    {
        Vector3[] nodes = new Vector3[w.nodes.Count];
        Vector2[] xz = new Vector2[w.nodes.Count];

        float height = 0;

        if (w.height != 0)
            height = w.height;

        Vector3 centroid = new Vector3();
        for (int a = 0; a < w.nodes.Count; a++)
        {
            RaycastHit hit;
            Node n = w.nodes[a];
            nodes[a] = new Vector3((float)((n.easthing - offsetPositionX) / precision), 5000, (float)((n.northing - offsetPositionZ) / precision));
            if (Physics.Raycast(nodes[a], -Vector3.up, out hit, Mathf.Infinity, layerMask))
            {
                nodes[a].y = hit.point.y + height + 0.5f;
            }
            else
            {
                nodes[a].y = 1;
            }
            xz[a] = new Vector2((float)((n.easthing - offsetPositionX) / precision), (float)((n.northing - offsetPositionZ) / precision));
            centroid += nodes[a];
        }
        centroid /= w.nodes.Count;
        centroid.y += 1;

      //  Vector3 position = new Vector3(centroid.x, 5000, centroid.z);
        float baseHeight = 0;



        /*RaycastHit hit;
        if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, layerMask))
        {
            baseHeight = hit.point.y;
        }*/
        //centroid = new Vector3(centroid.x, centroid.y + baseHeight, centroid.z);
        GameObject build = new GameObject();
        //build.transform.position = new Vector3(0, centroid.y, 0);
        build.name = w.id.ToString();
        mapManager.mapHash.Add(w.id);
        wayList.Add(w.id);
        build.isStatic = true;
        build.transform.parent = this.transform;
        GameObject roof = new GameObject();
       // roof.transform.position = new Vector3(0, centroid.y, 0);
        roof.name = (2 * w.id).ToString();
        mapManager.mapHash.Add(2 * w.id);
        wayList.Add(2 * w.id);
        roof.isStatic = true;
        if (w.name != null)
        {
            GameObject label = new GameObject();
            FloatingLabel lb = label.AddComponent<FloatingLabel>();
            lb.text = w.name; 
            lb.transform.position = centroid;
            lb.target = GameObject.FindGameObjectWithTag("Player").transform;
            label.transform.parent = build.transform;
        }
        roof.transform.parent = build.transform;



        Vector2[] xzRoof = new Vector2[w.nodes.Count - 1];

        for (int a = 0; a < xzRoof.Length; a++)
        {
            xzRoof[a] = xz[a];
        }

        Triangulator tr = new Triangulator(xzRoof);
       
        int[] indices = tr.Triangulate();
        // Create the mesh
        Mesh roofm = new Mesh();
        roofm.vertices = nodes;
        roofm.triangles = indices;

        Vector2[] uvs = new Vector2[nodes.Length];
        for (int a = 0; a < nodes.Length; a++)
        {
            if (a < nodes.Length - 1)
            {
                uvs[a] = new Vector2(Mathf.Abs(nodes[a].x) / nodes[nodes.Length - 1].x, Mathf.Abs(nodes[a].z) / nodes[nodes.Length - 1].x);
            }
            else
            {
                uvs[a] = new Vector2(1, 1);
            }
        }

        roofm.uv = uvs;
        roofm.RecalculateNormals();
        roofm.RecalculateBounds();


        roof.AddComponent(typeof(MeshRenderer));
        MeshFilter filter2 = roof.AddComponent(typeof(MeshFilter)) as MeshFilter;
        roof.AddComponent<MeshCollider>();
        filter2.mesh = roofm;
        if (w.type == WayType.Parking)
            roof.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Parking Material") as Material;
        if (w.type == WayType.Park)
            roof.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Park Material") as Material;
        if (w.type == WayType.RiverBank)
            roof.GetComponent<MeshRenderer>().material = Resources.Load("Materials/River Material") as Material;
        
        
        roof.transform.parent = transform;
    }

    private void CreateRoad(Way w)
    {
        //GameObject go = new GameObject();
        //go.name = "Road";
        PolyLine pl = new PolyLine("Way - " + w.id.ToString());
        pl.SetOffset(offsetPositionX, offsetPositionZ);
        pl.heigth = w.height;

        if (w.type == WayType.Footway)
        {
            pl.material = Resources.Load("Materials/Footway Material") as Material;
            pl.width = 1;
        }
        if (w.type == WayType.Motorway)
        {
            pl.material = Resources.Load("Materials/Road Material") as Material;
            pl.width = 4;
            pl.lanes = 2;
        }
        if (w.type == WayType.Residential)
        {
            pl.material = Resources.Load("Materials/Road Material") as Material;
            pl.width = 2;
        }
        if (w.type == WayType.River)
        {
            pl.material = Resources.Load("Materials/River Material") as Material;
            pl.width = 8;
        }
        //Road road = go.AddComponent<Road>();
        //road.groundOffset = 0.01f;
        //road.points.Clear();
        //road.roadWidth = 0.1f;// (float)(road.roadWidth/precision);
        //road.mat = (Material)Resources.LoadAssetAtPath("Assets/RoadTool/Example/Texture/Materials/Road.mat", typeof(Material));
        for (int a = 0; a < w.nodes.Count; a++)
        {
            Node n = w.nodes[a];

            Vector3 position = new Vector3((float)(n.easthing - offsetPositionX), 5000, (float)(n.northing - offsetPositionZ));
			float baseHeight = 0;
			RaycastHit hit;

            if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, layerMask))
			{
				baseHeight = hit.point.y;
			}
			n.height = baseHeight;
			//Vector3 v = newn. Vector3((float)(((float)n.easthing-offsetPositionX)/precision),(float)0.0,(float)(((float)n.northing-offsetPositionZ)/precision));
            //road.points.Insert(0, v);
            pl.Add(n);

        }
        pl.Close(transform);
        //go.isStatic = true;
        //road.Refresh();
        //go.transform.parent = transform;
    }

    private void CreateTree(Node n)
    {
        Vector3 position = new Vector3((float)(n.easthing - offsetPositionX), 15000.0f, (float)(n.northing - offsetPositionZ));
        RaycastHit hit;
        if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, layerMask))
        {
            position.y = hit.point.y;
        }


        Instantiate(mapManager.treePrefab, position, Quaternion.identity);

    }

	private Mesh CombineMeshes(List<Mesh> meshes)
	{
		CombineInstance[] combine = new CombineInstance[meshes.Count];
		int i = 0;
		while (i < meshes.Count) {
			combine[i].mesh = meshes[i];
			//combine[i].transform = meshes[i].transform.localToWorldMatrix;
			i++;
		}
		Mesh m = new Mesh();
		m.CombineMeshes(combine,false);
		return m;
	}
	
	private Mesh BuildingCountourMesh(Vector3[] nodes, Mesh mesh)
	{
		
		//Mesh mesh = new Mesh();
        Loom.RunAsync(() =>
        {
            Vector3[] bottomvertices = new Vector3[nodes.Length];
            float ccwChecker = 0;
            for (int i = 0; i < bottomvertices.Length; i++)
            {
                if (i < bottomvertices.Length - 1)
                    ccwChecker += (nodes[i + 1].x - nodes[i].x) * (nodes[i + 1].z + nodes[i].z);
                bottomvertices[i] = new Vector3(nodes[i].x, 0, nodes[i].z);
            }

            Vector3[] allVertices = new Vector3[2 * nodes.Length];

            Vector2[] uvs = new Vector2[allVertices.Length];

            for (int i = 0; i < bottomvertices.Length; i++)
            {
                allVertices[i] = bottomvertices[i];
            }
            for (int i = bottomvertices.Length; i < 2 * bottomvertices.Length; i++)
            {
                allVertices[i] = nodes[i - bottomvertices.Length];
            }

            for (int i = 0; i < allVertices.Length; i++)
            {
                if (i < allVertices.Length - 1)
                {
                    uvs[i] = new Vector2(allVertices[i].x - allVertices[i + 1].x, allVertices[i].z - allVertices[i + 1].z);
                }
                else
                {
                    uvs[i] = new Vector2(1, 1);
                }
            }




            int numberOfTrisVert = 3 * (allVertices.Length);

            if (numberOfTrisVert <= 0)
            {
                Debug.LogError(numberOfTrisVert);
            }

            if (numberOfTrisVert > 0)
            {
                int[] tris = new int[numberOfTrisVert];

                int C1 = nodes.Length;

                int C2 = nodes.Length + 1;

                int C3 = 0;

                int C4 = 1;

                for (int x = 0; x < numberOfTrisVert; x += 6)
                {

                    if (C2 >= allVertices.Length)
                        C2 = allVertices.Length - 1;

                    if (C1 >= allVertices.Length)
                        C1 = allVertices.Length - 1;


                    if (ccwChecker < 0)
                    {
                        tris[x] = C1;

                        tris[x + 1] = C2;

                        tris[x + 2] = C3;

                        tris[x + 3] = C3;

                        tris[x + 4] = C2;

                        tris[x + 5] = C4;
                    }
                    else
                    {
                        tris[x] = C1;

                        tris[x + 1] = C3;

                        tris[x + 2] = C2;

                        tris[x + 3] = C3;

                        tris[x + 4] = C4;

                        tris[x + 5] = C2;
                    }

                    C1++;

                    C2++;

                    C3++;

                    C4++;

                }
                Loom.QueueOnMainThread(() =>
                {
                    mesh.vertices = allVertices;
                    mesh.uv = uvs;
                    mesh.triangles = tris;
                    mesh.Optimize();
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                });
            }
        });
		return mesh;
	}
	
	private IEnumerator LoadChunk(float minLon,float minLat,float maxLon,float maxLat)	
	{
		Debug.Log("Started Download");
		string url = "http://www.overpass-api.de/api/xapi?map?bbox="+minLon+","+minLat+","+maxLon+","+maxLat;
		Debug.Log(url);
		WWW request = new WWW(url);
		yield return request;



		Debug.Log("Done");
		Debug.Log(request.text);
		if (request.error == null && request.isDone && !isLoaded)
		{
			XmlDocument XMLFile = new XmlDocument();
			XMLFile.LoadXml(request.text);

			mm = new MapManager(XMLFile);

            for (int i = 0; i < mm.trees.Count; i++)
            {
                Node currentNode = mm.nodes[i];
                if (currentNode.type == NodeType.Tree)
                {
                    CreateTree(currentNode);
                }
                
            }


			for(int i = 0; i < mm.ways.Count; i++)
			{
				if(!mapManager.mapHash.Contains(mm.ways[i].id))//!mapManager.wayList.Contains(mm.ways[i]))
				{
					wayList.Add(mm.ways[i].id);
					mapManager.mapHash.Add(mm.ways[i].id);
					if(mm.ways[i].type == WayType.Building)
					{

                        CreateBuilding(mm.ways[i]);

					}
					else if (mm.ways[i].type == WayType.Parking || mm.ways[i].type == WayType.Park || mm.ways[i].type == WayType.RiverBank)
					{
                        CreateGroundArea(mm.ways[i]);

					}
					else
					{
						if( mm.ways[i].nodes.Count > 1)
						{
                            CreateRoad(mm.ways[i]);
						}
					}
				}
				else
				{
					mapManager.mapHash.Add(mm.ways[i].id);
					wayList.Add(mm.ways[i].id);
				}
			}

			isLoaded = true;
			
		}
		else
		{
			Debug.Log("Error");
		}
	
	}	

}



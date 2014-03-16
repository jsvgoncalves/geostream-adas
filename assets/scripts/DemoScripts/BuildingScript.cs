using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

public class BuildingScript : Structure
{
    public static float referenceBaseHeight = 10;
    public static float referenceVariationHeight = 30;
    public override IEnumerator Build(Way w, MapChunkLoader mcl)
    {
        //CreateBuilding(Way w)
    
        Mesh roofm = new Mesh();
        Mesh wallsm = new Mesh();

        GameObject build = new GameObject();
        build.name = "Building - " +  w.id.ToString();

        build.transform.parent = mcl.transform;
        GameObject roof = new GameObject();
        //roof.transform.position = new Vector3(0,baseHeight,0);
        roof.name = "Roof - " +(2 * w.id).ToString();
        mcl.mapManager.mapHash.Add(2 * w.id);
        mcl.wayList.Add(2 * w.id);

        GameObject walls = new GameObject();
        walls.name = "Walls - " + (3 * w.id).ToString();
        //walls.transform.position = new Vector3(0,baseHeight,0);
        mcl.mapManager.mapHash.Add(3 * w.id);
        mcl.wayList.Add(3 * w.id);

        walls.AddComponent<MeshCollider>();
        walls.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = walls.AddComponent(typeof(MeshFilter)) as MeshFilter;

        roof.AddComponent<MeshCollider>();
        roof.AddComponent(typeof(MeshRenderer));
        MeshFilter filter2 = roof.AddComponent(typeof(MeshFilter)) as MeshFilter;


              
               


        string buildPath = Application.persistentDataPath + "Assets/Resources/Objs/" + build.name + ".obj";

        string roofPath = Application.persistentDataPath+"Assets/Resources/Objs/" + roof.name + ".obj";

        if (!File.Exists(buildPath) && !File.Exists(roofPath)) // If the file isn't cached we calculate everything and then we cache it
        {
            Vector3[] nodes = new Vector3[w.nodes.Count];
            Vector2[] xz = new Vector2[w.nodes.Count];

            float height = (float)w.nodes[0].northing % referenceVariationHeight + referenceBaseHeight;

            if (w.height != 0)
            {
                height = w.height;
                referenceVariationHeight = Mathf.Abs(referenceVariationHeight - height);
                referenceBaseHeight = (referenceBaseHeight + height) / 2;
            }
            Vector3 centroid = new Vector3();
			
                Vector3 position;
                RaycastHit hit;
	            for (int a = 0; a < w.nodes.Count; a++)
	            {
                    yield return null;
	                Node n = w.nodes[a];
                    position = new Vector3((float)((n.easthing - mcl.offsetPositionX) / MapChunkLoader.precision), 5000, (float)((n.northing - mcl.offsetPositionZ) / MapChunkLoader.precision));
					float castH = 0;
					
					if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, mcl.layerMask))
					{
						castH = hit.point.y;
					}
					nodes[a] = new Vector3(position.x, castH + height, position.z);
					xz[a] = new Vector2(position.x, position.z);
	                centroid += nodes[a];
	            }
	            centroid /= w.nodes.Count;
	            centroid.y += 1;
			
           

            Vector2[] xzRoof = new Vector2[w.nodes.Count -1];

            for (int a = 0; a < xzRoof.Length; a++)
            {
                xzRoof[a] = xz[a];
            }
            int[] indices;
            Vector3[] roofNodes;
            if (nodes.Length != 5 && nodes.Length != 6 && nodes.Length != 7 && nodes.Length != 8)
             {
                 Triangulator tr = new Triangulator(xzRoof);

                 int[] tempIndices = tr.Triangulate();
                 indices = tempIndices;
                 roofNodes = nodes;
                 
             }
             else
             {
                 int[] tempIndices = new int[(nodes.Length-1)*3];
                 Vector3 midpoint = new Vector3();
                
                 for (int i = 0; i < nodes.Length-1; i++)
                 {
                     midpoint.x += nodes[i].x;
                     midpoint.y += nodes[i].y;
                     midpoint.z += nodes[i].z;
                 }

                 roofNodes = new Vector3[nodes.Length];
                 midpoint.x = midpoint.x / (nodes.Length-1);
                 midpoint.y = midpoint.y / (nodes.Length - 1) + height/5;
                 midpoint.z = midpoint.z / (nodes.Length - 1);

                 

                 for (int i = 0; i < roofNodes.Length - 1; i++)
                 {
                     roofNodes[i] = nodes[i];
                 }

                 roofNodes[roofNodes.Length - 1] = midpoint;
               
                Triangle test = new Triangle();
                 test.a = roofNodes[0];
                 test.b = roofNodes[1];
                 test.c = roofNodes[4];
                 Vector3 testVector = test.Normal;

                 int u = 0;
                 for (int i = 0; i < roofNodes.Length - 2; i += 1)
                 {
                     if (testVector.y > 0)
                     {
                         tempIndices[u] = i;
                         tempIndices[u + 1] = i + 1;
                         tempIndices[u + 2] = roofNodes.Length - 1;
                     }
                     else
                     {
                         tempIndices[u + 1] = i;
                         tempIndices[u] = i + 1;
                         tempIndices[u + 2] = roofNodes.Length - 1;
                     }
                     u += 3;
                     if (u >= tempIndices.Length - 3)
                     {
                         i += 1;
                         if (testVector.y > 0)
                         {
                             tempIndices[u] = i;
                             tempIndices[u + 1] = 0;
                             tempIndices[u + 2] = roofNodes.Length - 1;
                         }
                         else
                         {
                             tempIndices[u + 1] = i;
                             tempIndices[u] = 0;
                             tempIndices[u + 2] = roofNodes.Length - 1;
                         }
                     }
                 }

               

             

                 indices = tempIndices;
               

             }
            // Create the mesh


            Vector2[] uvs = new Vector2[roofNodes.Length];
            for (int a = 0; a < roofNodes.Length; a++)
            {
                if (a < roofNodes.Length - 1)
                {
                    uvs[a] = new Vector2(a,0);
                }
                else
                {
                    uvs[a] = new Vector2(a/2, 1);
                }
            }

            roofm.vertices = roofNodes;
            roofm.triangles = indices;
            roofm.uv = uvs;
            roofm.RecalculateNormals();
            roofm.RecalculateBounds();
           
            
            // Set up game object with mesh;
                centroid = new Vector3(centroid.x, centroid.y, centroid.z);
				wallsm = BuildingCountourMesh(nodes, wallsm, height);
				

                

                if (w.name != null)
                {
                    GameObject label = new GameObject();
                    FloatingLabel lb = label.AddComponent<FloatingLabel>();
                    lb.transform.parent = roof.transform;
                    lb.text = w.name;
                    lb.target = GameObject.FindGameObjectWithTag("Player").transform;
                    lb.transform.position = centroid;
                }
                build.transform.parent = mcl.transform;
                walls.transform.parent = build.transform;
                roof.transform.parent = build.transform;
                
              
           
              //Wall
                filter.sharedMesh = wallsm;
               // wallmc.sharedMesh = wallsm;
                
              //Roof  
                filter2.sharedMesh = roofm;
                //roofmc.sharedMesh = roofm;
                

                if (mcl.exportObjs)
                {
                    ObjExporter oe1 = new ObjExporter();
                    ObjExporter oe2 = new ObjExporter();
                    oe1.MeshToFile(filter, buildPath);
                    oe2.MeshToFile(filter2, roofPath);
                }
            }
            else
            {
                ObjImporter oi = new ObjImporter();
                mcl.StartCoroutine(oi.FileToMesh("file://" + buildPath));

                while (oi._myMesh == null)
                {
                    yield return null;
                }

                filter.sharedMesh = oi._myMesh;
                Debug.LogWarning("Loaded Walls from cache " + buildPath);
               
            
                ObjImporter oi2 = new ObjImporter();
                mcl.StartCoroutine(oi2.FileToMesh("file://" + roofPath));

                while (oi2._myMesh == null)
                {
                    yield return null;
                }

                filter2.sharedMesh = oi2._myMesh;
                Debug.LogWarning("Loaded Roof from cache " + roofPath);
            }



        /*if (w.height != 0)
        {
            walls.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Real Height Material") as Material;
        }
        else*/
        {
            int textureIndex = UnityEngine.Random.Range(1,4);

            walls.GetComponent<MeshRenderer>().material = Resources.Load("Materials/BuildingMaterial" + textureIndex) as Material;
        }
        /*if (w.height != 0)
        {
            roof.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Real Height Material") as Material;
        }
        else*/
        {
            int textureIndex = UnityEngine.Random.Range(1, 3);
            roof.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Roof" + textureIndex) as Material;
        }
          
    
    }
    
    public override IEnumerator Build(Node n, MapChunkLoader mcl)
    {
        return null;
    }

    private Mesh BuildingCountourMesh(Vector3[] nodes, Mesh mesh, float heigth)
    {

        //Mesh mesh = new Mesh();

        Vector3[] bottomvertices = new Vector3[nodes.Length];
        float ccwChecker = 0;
        for (int i = 0; i < bottomvertices.Length; i++)
        {
            if (i < bottomvertices.Length - 1)
                ccwChecker += (nodes[i + 1].x - nodes[i].x) * (nodes[i + 1].z + nodes[i].z);
            bottomvertices[i] = new Vector3(nodes[i].x, nodes[i].y- heigth, nodes[i].z);
        }

        Vector3[] allVertices = new Vector3[2 * nodes.Length];

        Vector2[] uvs = new Vector2[allVertices.Length];


        float cumulativeDist = 0;
        for (int i = 0; i < bottomvertices.Length; i++)
        {
            float tempDist = 0;
            if(i < bottomvertices.Length-1)
            {
                tempDist = Vector3.Distance(bottomvertices[i],bottomvertices[i+1]);
            }
            allVertices[i] = bottomvertices[i];
            //uvs[i] = new Vector2(i, 0);
            uvs[i] = new Vector2(cumulativeDist, 0);
            cumulativeDist += tempDist;
        }
        for (int i = bottomvertices.Length; i < 2 * bottomvertices.Length; i++)
        {
            allVertices[i] = nodes[i - bottomvertices.Length];
            uvs[i] = new Vector2(uvs[i - bottomvertices.Length].x, heigth/5);
        }

        for (int i = 0; i < allVertices.Length; i++)
        {
           // uvs[i] = new Vector2(i, 0);
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

            mesh.vertices = allVertices;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.Optimize();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

        }

        return mesh;
    }

    class Triangle
    {
        public Vector3 a, b, c;
        public Vector3 Normal
        {
            get
            {
                var dir = Vector3.Cross(b - a, c - a);
                var norm = Vector3.Normalize(dir);
                return norm;
            }
        }
    }
}
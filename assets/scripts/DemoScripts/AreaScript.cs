using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

class AreaScript : Structure
{
    public override IEnumerator Build(Way w, MapChunkLoader mcl)
    {
        Vector3[] nodes = new Vector3[w.nodes.Count];
        Vector2[] xz = new Vector2[w.nodes.Count];

        float height = 0;


        mcl.mapManager.mapHash.Add(w.id);
        mcl.wayList.Add(w.id);
        GameObject roof = new GameObject();
        // roof.transform.position = new Vector3(0, centroid.y, 0);
        roof.name = "Area - " + (2 * w.id).ToString();
        mcl.mapManager.mapHash.Add(2 * w.id);
        mcl.wayList.Add(2 * w.id);
        roof.isStatic = true;
        if (w.name != null)
        {
            GameObject label = new GameObject();
            FloatingLabel lb = label.AddComponent<FloatingLabel>();
            lb.text = w.name;
            //lb.transform.position = centroid;
            lb.target = GameObject.FindGameObjectWithTag("Player").transform;
            label.transform.parent = roof.transform;
        }
        roof.transform.parent = mcl.transform;
        Mesh roofm = new Mesh();
        roof.AddComponent<MeshRenderer>();
        MeshFilter filter2 = roof.AddComponent<MeshFilter>();
        roof.AddComponent<MeshCollider>();

        string areaPath = Application.persistentDataPath + "Assets/Resources/Objs/" + roof.name + ".obj";

        if (!File.Exists(areaPath)) // If the file isn't cached we calculate everything and then we cache it
        {

            if (w.height != 0)
                height = w.height;

            Vector3 centroid = new Vector3();
            for (int a = 0; a < w.nodes.Count; a++)
            {
                yield return null;
                RaycastHit hit;
                Node n = w.nodes[a];
                nodes[a] = new Vector3((float)((n.easthing - mcl.offsetPositionX) / MapChunkLoader.precision), 5000, (float)((n.northing - mcl.offsetPositionZ) / MapChunkLoader.precision));
                if (Physics.Raycast(nodes[a], -Vector3.up, out hit, Mathf.Infinity, mcl.layerMask))
                {
                    nodes[a].y = hit.point.y + height + 0.5f;
                }
                else
                {
                    nodes[a].y = 1;
                }
                xz[a] = new Vector2((float)((n.easthing - mcl.offsetPositionX) / MapChunkLoader.precision), (float)((n.northing - mcl.offsetPositionZ) / MapChunkLoader.precision));
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




            Vector2[] xzRoof = new Vector2[w.nodes.Count - 1];

            for (int a = 0; a < xzRoof.Length; a++)
            {
                xzRoof[a] = xz[a];
            }

            Triangulator tr = new Triangulator(xzRoof);

            int[] indices = tr.Triangulate();
            // Create the mesh

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
            filter2.sharedMesh = roofm;





            if (mcl.exportObjs)
            {
                ObjExporter oe = new ObjExporter();
                oe.MeshToFile(filter2, areaPath);
            }
        }
        else
        {
            ObjImporter oi = new ObjImporter();
            mcl.StartCoroutine(oi.FileToMesh("file://" + areaPath));
            while (oi._myMesh == null)
            {
                yield return null;
            }

            filter2.sharedMesh = oi._myMesh;
            Debug.LogWarning("Loaded Area from cache " + areaPath);
        }

        if (w.type == WayType.Parking)
            roof.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Parking Material") as Material;
        if (w.type == WayType.Park)
            roof.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Park Material") as Material;
        if (w.type == WayType.RiverBank)
            roof.GetComponent<MeshRenderer>().material = Resources.Load("Materials/River Material") as Material;
    }

    public override System.Collections.IEnumerator Build(Node n, MapChunkLoader mcl)
    {
        throw new NotImplementedException();
    }
}


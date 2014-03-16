using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

class RoadScript : Structure
{
    public override IEnumerator Build(Way w, MapChunkLoader mcl)
    { //GameObject go = new GameObject();
        //go.name = "Road";

        GameObject road = new GameObject();
        road.name = "Road - " + w.id.ToString();
        road.isStatic = true;
        road.transform.parent = mcl.transform;
        string roadPath = Application.persistentDataPath + "Assets/Resources/Objs/" + road.name + ".obj";

        if (!File.Exists(roadPath)) // If the file isn't cached we calculate everything and then we cache it
        {
            Debug.Log("STARTED CREATING ROAD");
            PolyLine pl = new PolyLine(w.id.ToString());
            pl.SetOffset(mcl.offsetPositionX, mcl.offsetPositionZ);
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

            for (int a = 0; a < w.nodes.Count; a++)
            {
                Node n = w.nodes[a];

                Vector3 position = new Vector3((float)(n.easthing - mcl.offsetPositionX), 5000, (float)(n.northing - mcl.offsetPositionZ));
                float baseHeight = 0;
                RaycastHit hit;

                if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, mcl.layerMask))
                {
                    baseHeight = hit.point.y;
                }
                n.height = baseHeight;
                pl.Add(n);

            }
            //Closed road;
            mcl.StartCoroutine(pl.Close(road));

            if (mcl.exportObjs)
            {
                while (road.GetComponent<MeshFilter>() == null)
                {
                    yield return null;
                }
                MeshFilter mf = road.GetComponent<MeshFilter>();
                ObjExporter oe = new ObjExporter();
                oe.MeshToFile(mf, roadPath);

            }
        }
        else
        {
            ObjImporter oi = new ObjImporter();
            mcl.StartCoroutine(oi.FileToMesh("file://" + roadPath));
            while (oi._myMesh == null)
            {
                yield return null;

            }
            MeshFilter mf = road.AddComponent<MeshFilter>();
            MeshRenderer mr = road.AddComponent<MeshRenderer>();
            mf.sharedMesh = oi._myMesh;
            Debug.LogWarning("Loaded Road from cache " + roadPath);
            if (w.type == WayType.Footway)
            {
                mr.material = Resources.Load("Materials/Footway Material") as Material;

            }
            if (w.type == WayType.Motorway)
            {
                mr.material = Resources.Load("Materials/Road Material") as Material;

            }
            if (w.type == WayType.Residential)
            {
                mr.material = Resources.Load("Materials/Road Material") as Material;

            }
            if (w.type == WayType.River)
            {
                mr.material = Resources.Load("Materials/River Material") as Material;

            }
        }
    }

    public override System.Collections.IEnumerator Build(Node n, MapChunkLoader mcl)
    {
        throw new NotImplementedException();
    }
}


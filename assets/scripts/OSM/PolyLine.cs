using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PolyLine {

	public float offsetX;
	public float offsetY;
	public float width = 2;
	public int lanes = 1;
	public Material material;
	public float heigth;
    public bool exportObjs = true;
	public List<Node> nodes = new List<Node>();
	private string tag = "";
	// Use this for initialization
	void Start () {

	}

	public PolyLine(string i)
	{
		tag = i;
	}

	public void SetOffset(float oX, float oY)
	{
		offsetX = oX;
		offsetY = oY;
	}

	public void Add(Node n)
	{
		nodes.Add(n);
	}

    public IEnumerator Close(GameObject go)
    {


        Mesh m = new Mesh();

        List<Vector3> vertexes = new List<Vector3>();


        List<Vector2> uvs = new List<Vector2>();
        //Loom.RunAsync(() =>
        //{
            if (this.nodes.Count == 2)
            {
                Node mid1 = new Node();
                mid1.northing = (nodes[0].northing * 1.0f + nodes[1].northing * 2.0f) / 3.0f;
                mid1.easthing = (nodes[0].easthing * 1.0f + nodes[1].easthing * 2.0f) / 3.0f;
                mid1.height = (nodes[0].height * 1.0f + nodes[1].height * 2.0f) / 3.0f + 20f;


                Node mid2 = new Node();
                mid2.northing = (nodes[0].northing * 2.0f + nodes[1].northing * 1.0f) / 3.0f;
                mid2.easthing = (nodes[0].easthing * 2.0f + nodes[1].easthing * 1.0f) / 3.0f;
                mid2.height = (nodes[0].height * 2.0f + nodes[1].height * 1.0f) / 3.0f + 20f;


                this.nodes.Insert(1, mid1);
                this.nodes.Insert(1, mid2);

            }
            List<int> triangles = new List<int>();
            //Loom.RunAsync(()=>{
            Vector2 previousP2 = new Vector2(0, 0);
            Vector2 previousP3 = new Vector2(0, 0);
            for (int i = 0; i < this.nodes.Count - 2; i++) // here we create the vertexes
            {
                yield return null;
                Vector2 BA = new Vector2((float)(nodes[i].easthing - nodes[1 + i].easthing), (float)(nodes[i].northing - nodes[1 + i].northing));
                Vector2 AB = -BA;
                BA.Normalize();
                Vector2 BC = new Vector2((float)(nodes[2 + i].easthing - nodes[1 + i].easthing), (float)(nodes[2 + i].northing - nodes[1 + i].northing));
                BC.Normalize();
                Vector2 dir = BA + BC;

                float dot = 1;
                Vector3 cross = Vector3.Cross(new Vector3(AB.x, 0, AB.y), new Vector3(BC.x, 0, BC.y));

                if (cross.y < 0)
                    dot = -dot;
                //Vector2 enDdir = new Vector2((float)(nodes[i+1].easthing - nodes[i].easthing),(float)(nodes[i+1].northing - nodes[i].northing));



                Vector2 perpDirR = new Vector2(dir.x, dir.y);
                Vector2 perpDirL = new Vector2(-dir.x, -dir.y);

                perpDirR = new Vector2(-perpDirR.x, -perpDirR.y);
                perpDirL = new Vector2(-perpDirL.x, -perpDirL.y);


                perpDirR.Normalize();
                perpDirL.Normalize();
                perpDirR *= width;
                perpDirL *= width;

                //Vector3 p0 = new Vector3((float)(nodes[i].easthing - offsetX)+perpDirL.x,0.1f + heigth, (float)(nodes[i].northing - offsetY)+perpDirL.y);
                //Vector3 p1 = new Vector3((float)(nodes[i].easthing - offsetX)+perpDirR.x,0.1f + heigth, (float)(nodes[i].northing - offsetY)+perpDirR.y);
                Vector3 p2 = new Vector3((float)(nodes[i + 1].easthing - offsetX) + perpDirL.x, 0.1f + nodes[i + 1].height, (float)(nodes[i + 1].northing - offsetY) + perpDirL.y);
                Vector3 p3 = new Vector3((float)(nodes[i + 1].easthing - offsetX) + perpDirR.x, 0.1f + nodes[i + 1].height, (float)(nodes[i + 1].northing - offsetY) + perpDirR.y);
                //Vector3 p4 = new Vector3((float)(nodes[i + 2].easthing - offsetX) + perpDirL.x, 0.1f + heigth, (float)(nodes[i + 2].northing - offsetY) + perpDirL.y);
                //Vector3 p5 = new Vector3((float)(nodes[i + 2].easthing - offsetX) + perpDirR.x, 0.1f + heigth, (float)(nodes[i + 2].northing - offsetY) + perpDirR.y);



                if (i == 0)
                {
                    Vector3 p0 = new Vector3((float)(nodes[i].easthing - offsetX) + perpDirL.x, 0.1f + nodes[i + 1].height, (float)(nodes[i].northing - offsetY) + perpDirL.y);
                    Vector3 p1 = new Vector3((float)(nodes[i].easthing - offsetX) + perpDirR.x, 0.1f + nodes[i + 1].height, (float)(nodes[i].northing - offsetY) + perpDirR.y);
                    previousP2 = p0;
                    previousP3 = p1;
                    if (dot < 0)
                    {
                        vertexes.Add(p0);
                        uvs.Add(new Vector2(lanes, 0));
                        vertexes.Add(p1);
                        uvs.Add(new Vector2(0, 0));
                    }
                    else
                    {
                        vertexes.Add(p1);
                        uvs.Add(new Vector2(0, 0));
                        vertexes.Add(p0);
                        uvs.Add(new Vector2(lanes, 0));
                    }
                }
                float ccwChecker = 0;
                ccwChecker += (previousP3.x - previousP2.x) * (previousP3.y + previousP2.y);
                ccwChecker += (p2.x - previousP3.x) * (p2.y + previousP3.y);
                ccwChecker += (p3.x - p2.x) * (p3.y + p2.y);

                if (dot < 0)
                {
                    vertexes.Add(p2);
                    uvs.Add(new Vector2(0, Vector3.Distance(p3, previousP3)));
                    vertexes.Add(p3);
                    uvs.Add(new Vector2(lanes, Vector3.Distance(p3, previousP3)));
                }
                else
                {
                    vertexes.Add(p3);
                    uvs.Add(new Vector2(0, Vector3.Distance(p3, previousP3)));
                    vertexes.Add(p2);
                    uvs.Add(new Vector2(lanes, Vector3.Distance(p3, previousP3)));
                }
                previousP2 = p2;
                previousP3 = p3;

                if (i == nodes.Count - 3)
                {

                    Vector3 p4 = new Vector3((float)(nodes[i + 2].easthing - offsetX) + perpDirL.x, 0.1f + nodes[i + 1].height, (float)(nodes[i + 2].northing - offsetY) + perpDirL.y);
                    Vector3 p5 = new Vector3((float)(nodes[i + 2].easthing - offsetX) + perpDirR.x, 0.1f + nodes[i + 1].height, (float)(nodes[i + 2].northing - offsetY) + perpDirR.y);
                    ccwChecker += (p4.x - p3.x) * (p4.y + p3.y);
                    ccwChecker += (p5.x - p4.x) * (p5.y + p4.y);
                    if (dot < 0)
                    {
                        vertexes.Add(p4);
                        uvs.Add(new Vector2(0, Vector3.Distance(p4, p2)));
                        vertexes.Add(p5);
                        uvs.Add(new Vector2(lanes, Vector3.Distance(p4, p2)));
                    }
                    else
                    {
                        vertexes.Add(p5);
                        uvs.Add(new Vector2(0, Vector3.Distance(p4, p2)));
                        vertexes.Add(p4);
                        uvs.Add(new Vector2(lanes, Vector3.Distance(p4, p2)));

                    }
                }
            }



            for (int i = 0; i < vertexes.Count; i += 2)
            {
                int v0 = i;
                int v1 = i + 1;
                int v2 = i + 2;
                int v3 = i + 3;

                if (i + 2 < vertexes.Count)
                {
                    triangles.Add(v0);
                    triangles.Add(v2);
                    triangles.Add(v1);

                    triangles.Add(v2);
                    triangles.Add(v3);
                    triangles.Add(v1);
                }

            }

            //    Loom.QueueOnMainThread(() =>
            //    {
            m.vertices = vertexes.ToArray();
            m.uv = uvs.ToArray();
            m.triangles = triangles.ToArray();
            m.Optimize();
            m.RecalculateBounds();
            m.RecalculateNormals();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = m;

            go.AddComponent<MeshRenderer>().material = material;
            MeshCollider mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = m;

            //   });


            //});
        //}
        yield return 0;




    }
	
	// Update is called once per frame
	void Update () {
	
	}
}

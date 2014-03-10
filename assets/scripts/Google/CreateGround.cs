using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateGround{

    public int numberOfDivisions;
    public float maxLat;
    public float maxLon;
    public float minLon;
    public float minLat;
    public float offsetLat;
    public float offsetLon;
    private List<float> heigths;
    private List<float> coordinates;
    private List<Vector3> vertexes; 
	// Use this for initialization
	public Mesh GetGroundMesh () {
        CreateDivisions();
        GoogleElevation ge = new GoogleElevation();
        ge.coordinates = coordinates;
        ge.SyncGetHeights();
        heigths = ge.heights;
        CreateVertexes();
        return CreateGroundGeometry();
	}

    void CreateVertexes()
    {
        GeoUTMConverter off = new GeoUTMConverter();
        
        vertexes = new List<Vector3>();
        for(int i = 0; i < coordinates.Count; i+=2)
        {
            GeoUTMConverter conv = new GeoUTMConverter();
            conv.ToUTM(coordinates[i], coordinates[i + 1]);
            vertexes.Add(new Vector3((float)(conv.X ), heigths[i / 2], (float)(conv.Y )));
        }
    }

    void CreateGroundGameObject()
    {
        GameObject go = new GameObject();
        go.AddComponent<MeshCollider>();
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        Mesh m = CreateGroundGeometry();
        mf.mesh = m;
    }

    Mesh CreateGroundGeometry()
    {
        Mesh m = new Mesh();
        
            m = new Mesh();
            m.name = "Ground";

            int hCount2 = numberOfDivisions + 1;
            int vCount2 = numberOfDivisions + 1;
            int numTriangles = numberOfDivisions * numberOfDivisions * 6;
            int numVertices = hCount2 * vCount2;

            Vector3[] vertices = new Vector3[numVertices];
            Vector2[] uvs = new Vector2[numVertices];
            int[] triangles = new int[numTriangles];

            int index = 0;
            float uvFactorX = 1.0f / numberOfDivisions;
            float uvFactorY = 1.0f / numberOfDivisions;
			float width = (vertexes[vertexes.Count - 1].x - vertexes[0].x );
			float length = (vertexes[vertexes.Count - 1].z - vertexes[0].z );
			float scaleX = width / numberOfDivisions;
			float scaleY = length / numberOfDivisions;
          
            for (float y = 0.0f; y < vCount2; y++)
            {
                for (float x = 0.0f; x < hCount2; x++)
                {
                    vertices[index] = new Vector3(x * scaleX - width / 2f, heigths[(int)(y + x * hCount2)], y * scaleY - length / 2f);
                    uvs[index++] = new Vector2(x * uvFactorX, y * uvFactorY);
                }
            }

            index = 0;
            for (int y = 0; y < numberOfDivisions; y++)
            {
                for (int x = 0; x < numberOfDivisions; x++)
                {
                    triangles[index] = (y * hCount2) + x;
                    triangles[index + 1] = ((y + 1) * hCount2) + x;
                    triangles[index + 2] = (y * hCount2) + x + 1;

                    triangles[index + 3] = ((y + 1) * hCount2) + x;
                    triangles[index + 4] = ((y + 1) * hCount2) + x + 1;
                    triangles[index + 5] = (y * hCount2) + x + 1;
                    index += 6;
                }
            }

            m.vertices = vertices;
            m.uv = uvs;
            m.triangles = triangles;
            m.RecalculateNormals();

           // AssetDatabase.CreateAsset(m, "Assets/Editor/" + planeAssetName);
           // AssetDatabase.SaveAssets();
        
        return m;
    }

    void CreateDivisions()
    {
        coordinates = new List<float>();
        //coordinates.Add(minLat);
        //coordinates.Add(minLon);
        for (int a = 0; a <= numberOfDivisions; a++)
        {
            for (int i = 0; i <= numberOfDivisions; i++)
            {
                float midLat = minLat + (maxLat-minLat) * (i) / numberOfDivisions;
                float midLon = minLon + (maxLon - minLon) * (a) / numberOfDivisions;
                coordinates.Add(midLat);
                coordinates.Add(midLon);
            }
        }

    }

    // Update is called once per frame
  
}

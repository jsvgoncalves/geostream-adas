using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

public class ObjExporter
{

    public string MeshToString(string filename, string meshName, Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] triangles)
    {
       
        StringBuilder sb = new StringBuilder();


        sb.Append("g ").Append(meshName).Append("\n");
        foreach (Vector3 v in vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector2 v in uvs)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }

        sb.Append("\n");
        sb.Append("usemtl ").Append("default\n");
        sb.Append("usemap ").Append("default\n");//mats[material].name).Append("\n");

        for (int i = 0; i < triangles.Length; i += 3)
        {
            sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
        }
        
        return sb.ToString();
    }

    public string MeshToString(MeshFilter mf)
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.renderer.sharedMaterials;

        StringBuilder sb = new StringBuilder();

            
        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 v in m.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }

        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append("default\n");
            sb.Append("usemap ").Append("default\n");//mats[material].name).Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
            }
        }
        return sb.ToString();
    }

    public void MeshToFileThread(string filename, string meshName, Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] triangles)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            Debug.Log("Exported to " + filename);
            sw.Write(MeshToString(filename, "mesh", vertices, normals, uvs, triangles));
        }
    }

    public void MeshToFile(MeshFilter mf, string filename)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filename));
       
        Debug.Log("Exporting to " + filename);
        if (mf.sharedMesh == null || mf.sharedMesh.vertexCount == 0)
           return;

        Vector3[]vertices = mf.sharedMesh.vertices;
        Vector3[]normals = mf.sharedMesh.normals;
        Vector2[]uvs = mf.sharedMesh.uv;
        int[] triangles = mf.sharedMesh.triangles;
        
        Thread workerThread = new Thread(() => MeshToFileThread(filename, "mesh", vertices, normals, uvs, triangles));
        workerThread.Start();
        
            
    }
}
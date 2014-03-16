using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

class TreeScript : Structure
{

    public override System.Collections.IEnumerator Build(Way n, MapChunkLoader mcl)
    {
        throw new NotImplementedException();
    }

    public override System.Collections.IEnumerator Build(Node n, MapChunkLoader mcl)
    {
        Vector3 position = new Vector3((float)(n.easthing - mcl.offsetPositionX), 15000.0f, (float)(n.northing - mcl.offsetPositionZ));
        RaycastHit hit;
        if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, mcl.layerMask))
        {
            position.y = hit.point.y;
            yield return null;
        }

        MapChunkLoader.Instantiate(Resources.Load("/Prefabs/Tree.prefab"), position, Quaternion.identity);
        yield return null;
    }
}


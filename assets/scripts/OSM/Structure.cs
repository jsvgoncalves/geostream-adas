using UnityEngine;
using System.Collections;

abstract public class Structure{

	abstract public IEnumerator Build(Node n, MapChunkLoader mcl);

    abstract public IEnumerator Build(Way w, MapChunkLoader mcl);
	
}

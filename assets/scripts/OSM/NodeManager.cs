using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeManager{

	public Dictionary<long, Node> nodes;

	public NodeManager()
	{
		nodes = new Dictionary<long, Node>();
	}
}

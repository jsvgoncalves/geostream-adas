using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeReferenceHash
{
	private Dictionary<long,int> ways = new Dictionary<long,int>();


	public void Add(long k)
	{
		if(!ways.ContainsKey(k))
			ways.Add(k,1);
		else
			ways[k] = ways[k] +1;
	}

	public int Find(long k)
	{
		if(!ways.ContainsKey(k))
			return -1;
		return ways[k];
	}

	public bool Contains(long k)
	{
		if(!ways.ContainsKey(k))
			return false;
		else
		return true;
	}

	public int Remove(long k)
	{
		if(!ways.ContainsKey(k))
			return -1;
		else if (ways[k] == 1)
		{
			ways.Remove(k);
			return 0;
		}
		else if (ways[k] >1)
		{
			ways[k] = ways[k] -1;
			return ways[k];
		}
		return -1;
	}

}
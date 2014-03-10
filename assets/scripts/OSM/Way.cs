using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WayType { Building, Motorway, Residential, Footway, Park, Parking, River, RiverBank };

public class Way
{

	public long id;
	public int version;
	public string timestamp;
	public long changeset;
	public long uid;
	public string user;
	public WayType type;
	public string name;
	public int height;

	public List<Node> nodes;

	public Way()
	{
		nodes = new List<Node>();
	}
}



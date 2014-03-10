using UnityEngine;
using System.Collections;


public enum NodeType { Tree };

public class Node 
{
	public long id;
	public float height;
	public double lat;
	public double lon;
	public string zone;
	public double northing;
	public double easthing;
	public int version;
	public string timestamp;
	public long changeset;
	public long uid;
    public NodeType type;
	public string user;
}

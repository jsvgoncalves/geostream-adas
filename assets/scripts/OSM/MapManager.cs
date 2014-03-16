using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

public class MapManager {

	private NodeManager nm;
	public List<Node> nodes;
	public List<Node> trees;
	public List<Way> ways;
	private GeoUTMConverter convertor;


	public void DebugDrawRoads()
	{
		for(int i= 0; i < this.ways.Count; i++)
		{
			Color random = new Color( ((float)this.ways[i].nodes[0].easthing%255)/255f,((float)this.ways[i].nodes[0].northing%255)/255f,((float)this.ways[i].nodes[0].easthing%255)/255f);
			for (int a = 0; a < this.ways[i].nodes.Count-1;a++)
			{
				Debug.DrawLine(new Vector3((float)this.ways[i].nodes[a].easthing,0,(float)this.ways[i].nodes[a].northing),new Vector3((float)this.ways[i].nodes[a+1].easthing,0,(float)this.ways[i].nodes[a+1].northing),random);
			}
		}
	}

	public MapManager(XmlDocument XMLFile)
	{
		XmlNode node = XMLFile["osm"];
		Debug.Log(node.InnerText);

		nm = new NodeManager();
		nodes = new List<Node>();
		ways = new List<Way>();
        trees = new List<Node>();
		
		// How to traverse nodes in the file 
		// (this loops over just the declaration and the root, since they are the only
		// two top-level children of the XmlDocument object)
		foreach (XmlElement cnode in node.ChildNodes)
		{

			if(cnode.LocalName == "node")
			{
				Node tempNode = new Node();
				tempNode.id = long.Parse(cnode.GetAttribute("id"));
				
				tempNode.changeset = long.Parse(cnode.GetAttribute("changeset"));
				
				tempNode.lat = double.Parse(cnode.GetAttribute("lat"));
				
				tempNode.lon = double.Parse(cnode.GetAttribute("lon"));
				
				tempNode.timestamp = cnode.GetAttribute("timestamp");
				
				tempNode.uid = long.Parse(cnode.GetAttribute("uid"));
				
				tempNode.user = cnode.GetAttribute("user");
				
				tempNode.version = int.Parse(cnode.GetAttribute("version"));

				convertor = new GeoUTMConverter();

				convertor.ToUTM(tempNode.lat,tempNode.lon);

				tempNode.northing = convertor.Y;

				tempNode.easthing = convertor.X;
                
                XmlNode xmlNode = cnode as XmlNode;

                foreach (XmlElement xmlNodeRef in xmlNode.ChildNodes)
				{
					if(xmlNodeRef.LocalName == "tag")
                    {
                        string key = xmlNodeRef.GetAttribute("k");
						string value = xmlNodeRef.GetAttribute("v");

                        if (key.Contains("natural"))
                        {
                            if (value.Contains("tree"))
                            {
                                tempNode.type = NodeType.Tree;
                                tempNode.structureType = "tree";
								trees.Add(tempNode);
                            }
                        }
                    }
                }

                

				nm.nodes[tempNode.id] = tempNode;

				nodes.Add(tempNode);
			}
			if(cnode.LocalName == "way")
			{
				GeoUTMConverter converter = new GeoUTMConverter();
				XmlNode xmlWay = cnode as XmlNode;
				Way tempWay = new Way();
				tempWay.type = WayType.Residential; 
				tempWay.changeset = long.Parse(cnode.GetAttribute("changeset"));
				tempWay.id = long.Parse(cnode.GetAttribute("id"));
				tempWay.timestamp = cnode.GetAttribute("timestamp");
				tempWay.uid = long.Parse(cnode.GetAttribute("uid"));
				tempWay.user = cnode.GetAttribute("user");
				tempWay.version = int.Parse(cnode.GetAttribute("version"));

				bool toAdd = true;
				foreach (XmlElement xmlNodeRef in xmlWay.ChildNodes)
				{
					if(xmlNodeRef.LocalName == "nd")
					{
						long nodeRef = long.Parse(xmlNodeRef.GetAttribute("ref"));
						Node tempNode = nm.nodes[nodeRef];

						tempWay.nodes.Add(tempNode);
					}
					if(xmlNodeRef.LocalName == "tag")
					{
						string key = xmlNodeRef.GetAttribute("k");
						string value = xmlNodeRef.GetAttribute("v");
						if(key.Contains("building"))
						{
							tempWay.type = WayType.Building;
                            tempWay.structureType = "building";
						}
						if(key.Contains("amenity"))
						{
                            if (value.Contains("parking"))
                            {
                                tempWay.type = WayType.Parking;
                                tempWay.structureType = "parking";
                            }
						}
						if(key.Contains ("landuse"))
						{
							if(value.Contains("grass"))
							{
								tempWay.type = WayType.Park;
                                tempWay.structureType = "grass";
							}
						}
						if(key.Contains("highway"))
						{
							tempWay.type = WayType.Residential;
							if(value.Contains("residential"))
							{
								tempWay.type = WayType.Residential;
                                tempWay.structureType = "residential";
							}
							else if(value.Contains("footway"))
							{
								tempWay.type = WayType.Footway;
                                tempWay.structureType = "footway";
							}
							else if(value.Contains("motorway"))
							{
								tempWay.type = WayType.Motorway;
                                tempWay.structureType = "motorway";
							}
						}

						if(key.Contains("leisure"))
						{
							if(value.Contains("park"))
							{
								tempWay.type = WayType.Park;
                                tempWay.structureType = "park";
							}
						}
						if(key.Contains("waterway"))
						{
                            if (value.Contains("river"))
                            {
                                tempWay.type = WayType.River;
                                tempWay.structureType = "river";
                            }
                            if (value.Contains("riverbank"))
                            {
                                tempWay.type = WayType.RiverBank;
                                tempWay.structureType = "riverbank";
                            }
						}
						if(key.Contains("bridge"))
						{
							tempWay.height += 3;
						}
						if(key.Contains("name"))
						{
							tempWay.name = value;
						}
						if(key.Contains("height"))
						{
							tempWay.height = float.Parse(Regex.Replace(value, "[^-,.0-9]", "")); // Remove everything non-numeric
						}
						if(key.Contains("area"))
						{
							toAdd = false;
						}
						if(key.Contains ("natural"))
						{
                            if (value.Contains("water"))
                            {
                                tempWay.type = WayType.RiverBank;
                                tempWay.structureType = "riverbank";

                            }
						}
					}

				}
				
				if(toAdd)
				ways.Add(tempWay);
			}
		}

	}
}

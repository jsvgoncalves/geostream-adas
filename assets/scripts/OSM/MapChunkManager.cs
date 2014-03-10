using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapChunkManager : MonoBehaviour {

	public GameObject currentPlayer;
	public Material groundMaterial;
	public Material buildingMaterial;
	public Material roadMaterial;
	private Player player;
    private float areaRadius;
	public float offsetX;
	public float offsetZ;
    public int numberOfDivisions;
	private float maxLon;
	private float minLon;
	private float maxLat;
	private float minLat;
	private int unloadRadius;
	public float chunkStep;
	public int numChunks;
    public Transform treePrefab;
	public List<GameObject> mapChunks;
	public NodeReferenceHash mapHash;
    public bool isReady = false;
	// Use this for initialization
	void Start () {
	
		mapHash = new NodeReferenceHash();

	}

	void LoadUnloadChunks()
	{
		player = currentPlayer.GetComponent<Player>();
		GeoUTMConverter conv = new GeoUTMConverter();
		conv.ToUTM(player.initialFakeLat,player.initialFakeLon);
		offsetX = (float)conv.X;
		offsetZ = (float)conv.Y;

		List<GameObject> newMapChunks = new List<GameObject>();

		for(int i = 0; i < mapChunks.Count; i++)
		{
			MapChunkLoader mc = mapChunks[i].GetComponent<MapChunkLoader>();
			mc.toUnload = true;
		}


        int centerLat = (int)((player.fakeLat - player.initialFakeLat) / chunkStep);
        int centerLon = (int)((player.fakeLon - player.initialFakeLon) / chunkStep);
        float newLat = (float)(player.initialFakeLat + (float)(centerLat*chunkStep));
        float newLon = (float)(player.initialFakeLon + (float)(centerLon*chunkStep));

		for (int a = -numChunks; a <= numChunks; a+=2)
		{
			for (int b = -numChunks; b <= numChunks; b+=2)
			{
				bool hasChunk = false;
				for(int i = 0; i < mapChunks.Count; i++)
				{
					MapChunkLoader mc = mapChunks[i].GetComponent<MapChunkLoader>();
                    if (mc.Contains((float)(newLat + (b - 0.5f) * chunkStep), (float)(newLon + (a - 0.5f) * chunkStep)))
					{
						mc.toUnload = false;
						hasChunk = true;
						break;
					}
				}
				if(!hasChunk)
				{
					GameObject go = new GameObject();
					go.name = "World Chunk";
					go.isStatic = true;
					MapChunkLoader mcl = go.AddComponent<MapChunkLoader>();
					go.AddComponent<TaskExecutorScript>();
					mcl.groundMaterial = groundMaterial;
					mcl.buildingMaterial = buildingMaterial;
					mcl.roadMaterial = roadMaterial;
					mcl.mapManager = this;
                    mcl.treePrefab = treePrefab;
					mcl.maximumLat = newLat + (b+1.0f)*chunkStep;
					mcl.maximumLon = newLon + (a+1.0f)*chunkStep;
					mcl.minimumLat = newLat + (b-1.0f)*chunkStep;
					mcl.minimumLon = newLon + (a-1.0f)*chunkStep;
                    mcl.numberOfDivisions = numberOfDivisions;

					mcl.offsetPositionX = offsetX;
					mcl.offsetPositionZ = offsetZ;
					go.transform.parent = this.transform;

					newMapChunks.Add(go);
				}
			}
		}
		for(int i = 0; i < mapChunks.Count; i++)
		{
			MapChunkLoader mc = mapChunks[i].GetComponent<MapChunkLoader>();
			if(mc.toUnload)
			{
                for (int a = 0; a < mc.wayList.Count; a+=0)
                {
                    long item = mc.wayList[a];
                    if (mapHash.Remove(item) <= 0)
                    {
                        GameObject go = GameObject.Find(item.ToString());
                        Destroy(go);

                    }
                    mc.wayList.Remove(item);
                }
                
				if(mapChunks[i].transform.childCount == 1)
				{
                    Destroy(mapChunks[i]);
					mapChunks.Remove(mapChunks[i]);
                    
				}
				
			}
		}

		for(int i = 0; i < newMapChunks.Count; i++)
		{
			mapChunks.Add(newMapChunks[i]);
		}
        
	}
		
	// Update is called once per frame
	void Update () {
         LoadUnloadChunks();
         isReady = true;
		
	}
}

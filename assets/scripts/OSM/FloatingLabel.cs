using UnityEngine;
using System.Collections;

public class FloatingLabel : MonoBehaviour {


	public string text;
	public int size = 2;
	public float offset = 2;
	public Transform target;
	// Use this for initialization
	void Start () {
	
		TextMesh tm = gameObject.AddComponent<TextMesh>();
		MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
		tm.text = text;
		tm.alignment = TextAlignment.Center;
		tm.anchor = TextAnchor.MiddleCenter;
		tm.offsetZ = offset;
        tm.characterSize = size;
		tm.font = Resources.Load("Fonts/arial",typeof(Font)) as Font;
		mr.material = Resources.Load("Fonts/arial",typeof(Material)) as Material;
		mr.material.color = Color.black;
		gameObject.transform.parent = gameObject.transform;

		gameObject.transform.LookAt(target);
		//gameObject.transform.eulerAngles = new Vector3(0, 180,0); 

	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.LookAt(target);
		gameObject.transform.Rotate(0,180,0);
	}
}

using UnityEngine;
using System.Collections;

public class NodeJTalk : MonoBehaviour {
	public Unode unode;

	public string Dictionary;
	public string Voice;
	public int Sampling;
	public string file;
	public string text;


	// Use this for initialization
	void Start () {
		unode = GameObject.Find("Unode").GetComponent<Unode>();
	}
	
	// Update is called once per frame
	void Update () {
		if (unode.sound) {
			gameObject.AddComponent<voice> ().file = unode.file;
			unode.sound = false;
		}
		if(Input.GetMouseButtonDown(0)){
			unode.message(3,"\"dic\":\""+Dictionary+"\","+
			              "\"voice\":\""+Voice     +"\","+
			             "\"sample\":"  +Sampling  +  ","+
			               "\"file\":\""+file      +"\","+
			               "\"text\":\""+text      +"\"");
		}		
	}
}

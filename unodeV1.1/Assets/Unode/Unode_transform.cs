using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using WebSocketSharp;

public class Unode_transform : MonoBehaviour {
	public Unode_v1_1 unode;

	//Websocket
	private WebSocket ws;	

	//Messagepack
	public Dictionary<string,object> Msgpack;
	private string mode;

	void Awake() {
		unode = GameObject.Find ("Unode").GetComponent<Unode_v1_1> ();
	}

	// Use this for initialization
	void Start () {
		ws = new WebSocket (unode.adress);
		
		ws.Connect ();
				
		ws.OnOpen += (sender, e) => {
			Debug.Log ("Unode_transform.OnOpen:");
		};
		
		ws.OnMessage += (sender, e) => {
			var obj = unode.decode (e.RawData);
			Msgpack = obj as Dictionary<string,object>;
			mode = (string)Msgpack ["mode"];
		};
		
		ws.OnError += (object sender, ErrorEventArgs e) => {
			Debug.Log ("OnClosed:" + e.Message);
		};
		
		ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log ("OnClosed:" + e.Reason);
		};	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (transform.hasChanged) {
			transform.hasChanged = false;
			unode.send_transform(ws,gameObject);
		}
		if(mode == "transform"){
			mode = string.Empty;
			unode.recive_transform(Msgpack);
		}	
	}
	
}

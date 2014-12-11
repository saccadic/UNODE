using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using WebSocketSharp;
using MiniMessagePack;

public class Unode_transform : MonoBehaviour {
	public Unode_v1_1 unode;

	//Websocket
	private WebSocket ws;	

	//Messagepack
	MiniMessagePacker pakage;

	private Dictionary<string,object>	Msgpack,
										packed_data,
										localPosition,
										localEulerAngles,
										localScale;
	public string mode;

	void Awake() {
		unode = GameObject.Find ("Unode").GetComponent<Unode_v1_1> ();
		pakage = new MiniMessagePacker ();

		localPosition = new Dictionary<string, object> {
			{ "x", 0.0f},
			{ "y", 0.0f},
			{ "z", 0.0f}
		};
		localEulerAngles = new Dictionary<string, object> {
			{ "x", 0.0f},
			{ "y", 0.0f},
			{ "z", 0.0f}
		};
		localScale = new Dictionary<string, object> {
			{ "x", 0.0f},
			{ "y", 0.0f},
			{ "z", 0.0f}
		};
		packed_data = new Dictionary<string, object> {
			{ "mode", "transform" },
			{ "name", ""},
			{ "localPosition", localPosition},
			{ "localEulerAngles", localEulerAngles},
			{ "localScale", localScale},
		};

	}

	// Use this for initialization
	void Start () {
		ws = new WebSocket (unode.adress);
		
		ws.ConnectAsync ();

		//ws.WaitTime = TimeSpan.FromSeconds(2);
				
		ws.OnOpen += (sender, e) => {
			Debug.Log ("Unode_transform.OnOpen:");
		};
		
		ws.OnMessage += (sender, e) => {
			Msgpack = unode.decode(e.RawData) as Dictionary<string,object>;
			mode = (string)Msgpack ["mode"];
			if(mode == "transform"){
				mode = string.Empty;
				//unode.recive_transform(Msgpack);
			}	
		};
		
		ws.OnError += (object sender, ErrorEventArgs e) => {
			Debug.Log ("OnClosed:" + name + ":" + e.Message);
		};
		
		ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log ("OnClosed:" + e.Reason);
		};	
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.hasChanged) {
			transform.hasChanged = false;
			send_transform(ws,gameObject);
		}
	}


 private void send_transform(WebSocket s,GameObject obj){

		localPosition ["x"] 	= obj.transform.localPosition.x;
		localPosition ["y"] 	= obj.transform.localPosition.y;
		localPosition ["z"] 	= obj.transform.localPosition.z;

		localEulerAngles ["x"] 	= obj.transform.localEulerAngles.x;
		localEulerAngles ["y"] 	= obj.transform.localEulerAngles.y;
		localEulerAngles ["z"] 	= obj.transform.localEulerAngles.z;

		localScale ["x"] 		= obj.transform.localScale.x;
		localScale ["y"] 		= obj.transform.localScale.y;
		localScale ["z"] 		= obj.transform.localScale.z;

		packed_data ["name"] 			= obj.name;
		packed_data ["localPosition"] 	= localPosition;
		packed_data ["localEulerAngles"]= localEulerAngles;
		packed_data ["localScale"] 		= localScale;
	
		byte [] data = pakage.Pack (packed_data);

		s.Send(data);
	}
}

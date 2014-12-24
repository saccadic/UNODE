using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//using WebSocketSharp;

public class NodeJTalk : MonoBehaviour {
	public Unode_v1_3 unode;

	//Websocket
	//private WebSocket ws;

	//Messagepack
	public Dictionary<string,object> Msgpack;

	public string Dictionary;
	public string Voice;
	public int Sampling;
	public string file;
	[Multiline(10)]public string text;

	public string mode;

	void Awake() {
		unode = GameObject.Find ("Unode_v1_3").GetComponent<Unode_v1_3> ();

	}

	// Use this for initialization
	void Start () {
		Debug.Log ("NodeJTalk.");
		/*
		ws = new WebSocket (unode.adress);
		ws.Connect ();
		unode.RegistNodeModule(ws, "openjtalk", "NodeJTalk.js");

		ws.OnOpen += (sender, e) => {
			Debug.Log ("NodeJTalk.OnOpen:");
		};

		ws.OnMessage += (sender, e) => {
			var obj = unode.MessagePackDecode (e.RawData);
			Msgpack = obj as Dictionary<string,object>;
			mode    = (string)Msgpack ["mode"];

		};

		ws.OnError += (object sender, ErrorEventArgs e) => {
			Debug.Log ("OnError[NodeJTalk]:" + e.Message);
		};

		ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log ("OnClosed[NodeJTalk]:" + e.Reason);
		};
		*/
	}
	
	// Update is called once per frame
	void Update () {
		if(unode.mode == "talking"){
			unode.mode = string.Empty;
			if (gameObject.GetComponent<voice> () == null)
				gameObject.AddComponent<voice> ().file = file;
		}

		if(Input.GetMouseButtonDown(0)){
			if(gameObject.GetComponent<voice> () == true)
				Destroy(gameObject.GetComponent<voice> ());
			talk(text);
		}		
		if(Input.GetMouseButtonDown(1)){
			talk("現在時刻は" + DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分ss秒") + "です。");
		}
	}

	void OnApplicationQuit() {
		//if(ws != null)
		//	ws.Close();
	}

	void talk(string str){
		str = str.Replace ("\n"," ");
		var option = new Dictionary<string, object> {
			{ "name" , "openjtalk" },
			{ "mode" , "child" },
			{ "dic"  , Dictionary},
			{ "voice", Voice},
			{ "sample", Sampling},
			{ "file" , file},
			{ "text" , str}
		};
		unode.SendToNodeModule(unode.ws,"openjtalk",option);
	}
}

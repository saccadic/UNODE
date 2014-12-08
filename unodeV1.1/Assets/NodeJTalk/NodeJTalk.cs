using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using WebSocketSharp;

public class NodeJTalk : MonoBehaviour {
	public Unode_v1_1 unode;

	//Websocket
	private WebSocket ws;

	//Messagepack
	public Dictionary<string,object> Msgpack;

	public string Dictionary;
	public string Voice;
	public int Sampling;
	public string file;
	[Multiline(10)]public string text;

	private string mode;

	void Awake() {
		unode = GameObject.Find ("Unode").GetComponent<Unode_v1_1> ();
	}

	// Use this for initialization
	void Start () {
		Debug.Log ("NodeJTalk.");
		ws = new WebSocket (unode.adress);

		ws.Connect ();
		unode.regist_js (ws, "openjtalk", "NodeJTalk.js");

		ws.OnOpen += (sender, e) => {
			Debug.Log ("NodeJTalk.OnOpen:");
		};

		ws.OnMessage += (sender, e) => {
			var obj = unode.decode (e.RawData);
			Msgpack = obj as Dictionary<string,object>;
			mode    = (string)Msgpack ["mode"];
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
		if(mode == "talking"){
			mode = string.Empty;
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
		unode.run_js(ws,"openjtalk",option);
	}
}

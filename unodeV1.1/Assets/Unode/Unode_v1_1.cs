using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using WebSocketSharp;
using MiniMessagePack;

using Debug = UnityEngine.Debug;

public class Unode_v1_1 : MonoBehaviour {
	
	//Websocket
	public string adress;
	public WebSocket ws;
	public bool run;

	//Messagepack
	public Dictionary<string,object> Msgpack;
	private object obj;
	public string mode;

	//Node.exe
	public string program_name;
	public bool windowtype_hidden = false;
	public bool x86;

	[HideInInspector]
	public string nodejs_path;
	public string Script_path;

	public bool connected = false;
	private Process nodejs_process;
	private ProcessStartInfo info;
	public bool cmd = true;
	public string command;
	public string js;
	
	void Awake() {
		if(run = open_nodejs(x86))
			ws = new WebSocket(adress);
	}

	void Start () {
		if(run){
			ws.Connect ();
			//ws.WaitTime = TimeSpan.FromSeconds(10);
			var packed_data = new Dictionary<string, object> {
				{ "mode", "connect" },
			};
			send(ws,packed_data);
		}

		ws.OnOpen += (sender, e) => {
			Debug.Log ("ws.OnOpen:");
		};
		
		ws.OnMessage += (sender, e) => {
			obj = decode(e.RawData);
			Msgpack = obj as Dictionary<string,object>;
			mode = (string)Msgpack["mode"];
			switch(mode){
				case "connected":
					Debug.Log ((string)Msgpack["ver"]);
					connected = true;
					break;
				case "echo":
					Debug.Log ("echo:"+(string)Msgpack["text"]);
					break;
			}
		};

		ws.OnError += (object sender, ErrorEventArgs e) => {
			Debug.Log ("OnClosed:" + e.Message);
		};

		ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log ("OnClosed:" + e.Reason);
		};
	}
	
	//メインスレッド
	void FixedUpdate () {
		if (run && ws.IsAlive && ws != null) {
			Debug.Log("Ping:"+ws.Ping());
		} else {
			Debug.Log("Error:Not Run Node.js");
		}
	}

	void OnApplicationQuit() {
		if (ws != null) {
			var packed_data = new Dictionary<string, object> {
				{ "mode", "exit" }
			};
			send (ws,packed_data);
			ws.Close ();
		}
		kill_nodejs ();
	}
	
	private bool open_nodejs(bool arch){
		if (arch == true) {
			nodejs_path = Application.streamingAssetsPath + "/.node/x86/";
		} else {
			nodejs_path = Application.streamingAssetsPath + "/.node/x64/";
		}
			Script_path = Application.streamingAssetsPath + "/.node/src/";

		info = new System.Diagnostics.ProcessStartInfo();
		info.FileName = nodejs_path + program_name;
		info.WorkingDirectory = Script_path;

		if(cmd){
			info.Arguments = command;
		}else{
			info.Arguments = Script_path + js;
		}

		if(windowtype_hidden){
			info.WindowStyle = ProcessWindowStyle.Hidden;
		}

		try{
			nodejs_process = Process.Start(info);
		}catch(System.ComponentModel.Win32Exception w){
			Debug.Log("Not Found." + w);
			return false;
		}
		return true;
	}

	private void kill_nodejs(){
		if (!nodejs_process.HasExited) {
			nodejs_process.Kill ();
		}
	}

	public byte[] encode(Dictionary<string,object> data){
		var pakage = new MiniMessagePacker ();
		byte[] msgpakage = pakage.Pack(data);

		return msgpakage;
	}

	public object decode(byte[] data){
		var pakage = new MiniMessagePacker ();
		object unpakage = pakage.Unpack(data);
		
		return unpakage;
	}

	public void send(WebSocket s,Dictionary<string,object> data){
		byte[] msgpakage = encode(data);
		s.Send(msgpakage);
	}

	public void regist_js(WebSocket s,string name,string js){
		var packed_data = new Dictionary<string, object> {
			{ "mode", "child" },
			{ "regist", true},
			{ "name", name},
			{ "js", js}
		};
		send(s,packed_data);		
	}

	public void run_js(WebSocket s,string name,Dictionary<string,object> option){
		var packed_data = new Dictionary<string, object> {
			{ "mode", "child" },
			{ "name", name},
			{ "options", option}
		};
		send(s,packed_data);		
	}
}

﻿using UnityEngine;
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
	private bool run;

	//Messagepack
	public Dictionary<string,object> dict;
	private object obj;
	//public int mode;
	public string mode;

	//Node.exe
	public string program_name;
	public bool   x86;

	[HideInInspector]
	public string nodejs_path;
	public string Script_path;


	private Process nodejs_process;
	private ProcessStartInfo info;
	public bool cmd = true;
	public string command;
	public string js;
	public bool windowtype_hidden = false;

	//外部出力
	public string[] js_codes;

	[HideInInspector]
	public byte[] data;

	public object[] obj_data;
	
	void Awake() {
		if(run = open_nodejs(x86))
			ws = new WebSocket(adress);
	}

	void Start () {
		if(run){
			ws.Connect ();
			var packed_data = new Dictionary<string, object> {
				{ "mode", "connect" },
			};
			if(js_codes.Length > 0){
				packed_data.Add("js_codes",js_codes);
			}
			send(packed_data);
		}

		ws.OnOpen += (sender, e) => {
			Debug.Log ("ws.OnOpen:");
		};
		
		ws.OnMessage += (sender, e) => {
			obj = decode(e.RawData);
			dict = obj as Dictionary<string,object>;
			mode = (string)dict ["mode"];
			
			switch(mode){
			case "connected":
				Debug.Log ((string)dict ["ver"]);
				break;
			case "hello":
				Debug.Log ((string)dict ["text"]);
				break;
			}
		};
		
		ws.OnClose += (sender, e) => {
			Debug.Log ("OnClosed:" + e.Reason);
		};
	}
	
	//メインスレッド
	void Update () {
		if (run && ws.IsAlive && ws != null) {


			Debug.Log ("Ping:"+ws.Ping());
			//ws.WaitTime = TimeSpan.FromSeconds(10);

			if(Input.GetMouseButtonDown(0)){
				var packed_data = new Dictionary<string, object> {
					{ "mode", "hello" },
					{ "text", "hello,world" },
				};
				send(packed_data);
			}

		} else {
			Debug.Log("Error:Not Run Node.js");
		}
	}

	void OnApplicationQuit() {
		if (ws != null) {
			var packed_data = new Dictionary<string, object> {
				{ "mode", "exit" }
			};
			send (packed_data);
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

	public void send(Dictionary<string,object> data){
		byte[] msgpakage = encode(data);
		ws.Send(msgpakage);
	}

	public void regist_js(string name,string js){
		var packed_data = new Dictionary<string, object> {
			{ "mode", "child" },
			{ "regist", true},
			{ "name", name},
			{ "js", js}
		};
		send(packed_data);		
	}

	public void run_js(string name,Dictionary<string, object> option){
		var packed_data = new Dictionary<string, object> {
			{ "mode", "child" },
			{ "name", name},
			{ "options", option}
		};
		send(packed_data);		
	}
}

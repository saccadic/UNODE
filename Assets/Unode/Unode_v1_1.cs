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
	public string msg;
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
	public string nodejs_path;
	public string Script_path;
	private Process nodejs_process;
	private ProcessStartInfo info;
	public bool cmd = true;
	public string command;
	public string js;
	public bool windowtype_hidden = false;

	//外部出力
	public bool sound;
	public string file;

	[HideInInspector]
	public byte[] data;

	public object[] obj_data;

	public bool triger,tmp=true;

	void Awake() {
		if(run = open_nodejs(x86))
			ws = new WebSocket(adress);
			
	}

	void Start () {
		if(run){
			ws.Connect ();
			var unpacked_data = new Dictionary<string, object> {
				{ "mode", "connect" },
			};
			send(unpacked_data);
		}
	}
	
	//メインスレッド
	void Update () {
		if (run && ws.IsAlive && ws != null) {

			/*
			if(Input.GetKeyDown(KeyCode.B)){
				var unpacked_data = new Dictionary<string, object> {
					{ "mode", 255 },
					{ "data", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") }
				};
				send(unpacked_data);
			}

			if(Input.GetKeyDown(KeyCode.V)){
				var unpacked_data = new Dictionary<string, object> {
					{ "mode", 1 },
				};
				send(unpacked_data);
			}

			*/

			ws.OnOpen += (sender, e) => {
					Debug.Log ("ws.OnOpen:");
			};

			ws.OnMessage += (sender, e) => {
				triger = true;
				msg = string.Empty;

				//Debug.Log(e.RawData.Length +":"+e.GetType());
				if(e.RawData.Length>0){
					obj = decode(e.RawData);
					dict = obj as Dictionary<string,object>;
					mode = (string)dict ["mode"];

					/*
					try {
						mode = (int)(long)dict ["mode"];
						switch (mode) {
							case 1:
									Debug.Log ("Node.js: " + (string)dict ["ver"]);
									break;

							case 255:
									Debug.Log ("date: "    +(string) dict ["date"]);
									break;

							default:
									Debug.Log ("Error:" + (long)dict ["mode"]);
									break;

						}
					} catch {
							Debug.Log ("Error:parse");
					}
					*/
				}
			};

			ws.OnClose += (sender, e) => {
				Debug.Log ("OnClosed:" + e.Reason);
			};

			triger = false;
			Debug.Log ("Ping:"+ws.Ping());
			ws.WaitTime = TimeSpan.FromSeconds (10);
		} else {
			Debug.Log("Error:Not Run Node.js");
		}
	}

	void OnApplicationQuit() {
		if(ws != null)
			ws.Close();
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
		nodejs_process.Kill();
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



}

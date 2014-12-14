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
	public float wait=1.0f;

	//Messagepack
	MiniMessagePacker pakage;

	private Dictionary<string,object>	Msgpack,
										packed_data,
										localPosition,
										localEulerAngles,
										localScale;
	private Dictionary<string,object>[] array;
	public string obj_name;
	public string mode;

	private object data;

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

		array = new Dictionary<string, object>[100];
		for(int i=0;i<array.Length;i++){
			array[i] = new Dictionary<string, object>{
				{ "localPosition", localPosition},
				{ "localEulerAngles", localEulerAngles},
				{ "localScale", localScale}
			};
		}

		packed_data = new Dictionary<string, object> {
			{ "mode", "transform" },
			{ "name", string.Empty},
			{ "objects" , array}
		};

		/*
		packed_data = new Dictionary<string, object> {
			{ "mode", "transform" },
			{ "name", ""},
			{ "localPosition", localPosition},
			{ "localEulerAngles", localEulerAngles},
			{ "localScale", localScale},
			{ "objects" , array}
		};
		*/

	}

	// Use this for initialization
	void Start () {
		obj_name = name;
		setup_websocket (ws,unode.adress);	
		StartCoroutine(transformToNode());
	}
	
	// Update is called once per frame
	void Update () {

	}

	IEnumerator transformToNode(){
		if (transform.hasChanged) {
			transform.hasChanged = false;

			localPosition ["x"] = transform.localPosition.x;
			localPosition ["y"] = transform.localPosition.y;
			localPosition ["z"] = transform.localPosition.z;
			
			localEulerAngles ["x"] = transform.localEulerAngles.x;
			localEulerAngles ["y"] = transform.localEulerAngles.y;
			localEulerAngles ["z"] = transform.localEulerAngles.z;
			
			localScale ["x"] = transform.localScale.x;
			localScale ["y"] = transform.localScale.y;
			localScale ["z"] = transform.localScale.z;

			/*
			packed_data ["name"] = obj_name;
			packed_data ["localPosition"] = localPosition;
			packed_data ["localEulerAngles"] = localEulerAngles;
			packed_data ["localScale"] = localScale;
			*/

			for(int i=0;i<array.Length;i++){
				array[i]["localPosition"] = localPosition;
				array[i]["localEulerAngles"] = localEulerAngles;
				array[i]["localScale"] = localScale;
			}
			packed_data ["objects"] = array;


			try{
				byte [] data = pakage.Pack (packed_data);
				Debug.Log(sizeof(byte)*data.Length);
				ws.Send (data);
			}catch{
				Debug.Log("error"+"["+obj_name+"]"+":send");
			}
		} else {
			if(mode.Length>0)
				mode = string.Empty;
		}

		//yield return null;
		yield return new WaitForSeconds (wait);

		StartCoroutine(transformToNode());
	}

	void OnApplicationQuit() {
		if(ws != null)
			ws.Close();
	}

	void setup_websocket(WebSocket socket,string adress){
		ws = new WebSocket (adress);
		ws.Connect();

		ws.OnOpen += (sender, e) => {
			Debug.Log ("Unode_transform.OnOpen:");
		};
		
		ws.OnMessage += (sender, e) => {
			switch(e.Type){
				case Opcode.Binary:
					try{
						Msgpack = unode.decode(e.RawData) as Dictionary<string,object>;
						if(Msgpack.TryGetValue("mode",out data)){
							//Debug.Log("["+obj_name+"]byte::"+e.RawData.Length);
							mode = (string)data;
							if(mode == "transform"){
								//unode.recive_transform(Msgpack);
							}	
						}else{
							Debug.Log("error"+"["+obj_name+"]"+":mode::"+e.RawData.Length);
						}
					}catch{
						Debug.Log("error:"+"["+obj_name+"]"+"Msgpack");
					}
					break;
				case Opcode.Text:
					Debug.Log("text:"+e.Data);
					break;
			}
		};
		
		ws.OnError += (object sender, ErrorEventArgs e) => {
			Debug.Log ("OnError:" + obj_name + ":" + e.Message);
		};
		
		ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log ("OnClosed"+"[transform]:" + e.Reason);
			//setup_websocket (ws,adress);	
		};	
	}
}

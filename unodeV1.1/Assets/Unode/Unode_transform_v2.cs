using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using WebSocketSharp;
using MiniMessagePack;

public class Unode_transform_v2 : MonoBehaviour {
	public Unode_v1_3 unode;

	//Websocket
	private WebSocket ws = null;	
	public float wait=1.0f;

	//Messagepack
	private Dictionary<string,object>	Msgpack,
										packed_data,
										localPosition,
										localEulerAngles,
										localScale;
	private Dictionary<string,object>[] array;

	//Program Options
	public string ObjectName;
	public string mode;
	private object data;
	private GameObject[] objects;
	public bool SendMode = false;

	void Awake() {
		unode = GameObject.Find ("Unode_v1_3").GetComponent<Unode_v1_3> ();

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
			{ "name", name},
			{ "objects" , array}
		};
	}

	// Use this for initialization
	void Start () {
		if (unode.IsNodeJS) {
			ObjectName = name;
			ws = new WebSocket (unode.adress);
			SetupTransform (ws, unode.adress);
			StartCoroutine (transformToNodeJS ());
		}
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnApplicationQuit() {
		if(ws != null)
			ws.Close();
	}

	public void recive_transform(Dictionary<string,object> transformData){
		var l_pos         = (Dictionary<string,object>)transformData["localPosition"];
		var l_EulerAngles = (Dictionary<string,object>)transformData["localPosition"];
		var l_Scale       = (Dictionary<string,object>)transformData["localPosition"];
		
		transform.localPosition    = new Vector3((long)l_pos["x"],(long)l_pos["y"],(long)l_pos["z"]);
		transform.localEulerAngles = new Vector3((long)l_EulerAngles["x"],(long)l_EulerAngles["y"],(long)l_EulerAngles["z"]);
		transform.localScale       = new Vector3((long)l_Scale["x"],(long)l_Scale["y"],(long)l_Scale["z"]);
	}
	
	IEnumerator transformToNodeJS(){
		while(true){
			objects = GameObject.FindGameObjectsWithTag("TransformToNodeJS");

			if(objects.Length > 0){
				array = new Dictionary<string, object>[objects.Length];

				for(int i=0;i<array.Length;i++){
					try{
						if(objects[i].transform.hasChanged){
							objects[i].transform.hasChanged = false;
							SendMode = true;

							localPosition ["x"] = objects[i].transform.localPosition.x;
							localPosition ["y"] = objects[i].transform.localPosition.y;
							localPosition ["z"] = objects[i].transform.localPosition.z;
							
							localEulerAngles ["x"] = objects[i].transform.localEulerAngles.x;
							localEulerAngles ["y"] = objects[i].transform.localEulerAngles.y;
							localEulerAngles ["z"] = objects[i].transform.localEulerAngles.z;
							
							localScale ["x"] = objects[i].transform.localScale.x;
							localScale ["y"] = objects[i].transform.localScale.y;
							localScale ["z"] = objects[i].transform.localScale.z;
							
							array[i] = new Dictionary<string, object>{
								{ "name", objects[i].name},
								{ "localPosition", localPosition},
								{ "localEulerAngles", localEulerAngles},
								{ "localScale", localScale}
							};
							//Debug.Log("Update"+"["+objects[i].name+"]");
						}else {
							if(mode.Length>0)
								mode = string.Empty;
						}
					}catch{
						Debug.Log("error"+"["+objects[i].name+"]"+":Dictionary");
					}
				}

				packed_data["objects"] = array;

				if(SendMode){
					SendMode = false;
					try{
						unode.send(ws,packed_data);
					}catch{
						Debug.Log("error"+"["+ObjectName+"]"+":send");
					}
				}
			}

			yield return new WaitForSeconds (wait);
			//StartCoroutine(transformToNodeJS());
		}
	}

	void SetupTransform(WebSocket ws,string adress){

		ws.Connect();

		ws.OnOpen += (sender, e) => {
			Debug.Log ("Unode_transform.OnOpen:");
		};
		
		ws.OnMessage += (sender, e) => {
			switch(e.Type){
				case Opcode.Binary:
					try{
						Msgpack = unode.MessagePackDecode(e.RawData) as Dictionary<string,object>;
						if(Msgpack.TryGetValue("mode",out data)){
							//Debug.Log("["+ObjectName+"]byte::"+e.RawData.Length);
							mode = (string)data;
							if(mode == "transform"){
								//unode.recive_transform(Msgpack);
							}	
						}else{
							Debug.Log("error"+"["+ObjectName+"]"+":mode::"+e.RawData.Length);
						}
					}catch{
						Debug.Log("error:"+"["+ObjectName+"]"+"Msgpack");
					}
					break;
				case Opcode.Text:
					Debug.Log("text:"+e.Data);
					break;
			}
		};
		
		ws.OnError += (object sender, ErrorEventArgs e) => {
			Debug.Log ("OnError:" + ObjectName + ":" + e.Message);
		};
		
		ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log ("OnClosed"+"[transform]:" + e.Reason);
			//setup_websocket (ws,adress);	
		};	
	}
}

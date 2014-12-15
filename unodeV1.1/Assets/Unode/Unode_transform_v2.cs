using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using WebSocketSharp;
using MiniMessagePack;

public class Unode_transform_v2 : MonoBehaviour {
	public Unode_v1_3 unode;

	//Websocket
	private WebSocket ws = null;	
	public float wait=1.0f;
	public bool connected = false;

	//Messagepack
	private Dictionary<string,object>	Msgpack,
										packed_data,
										localPosition,
										localEulerAngles,
										localScale;
	private List<object> list;

	//Program Options
	public string ObjectName;
	public string mode;
	private object data;
	private GameObject[] objects;
	private Dictionary<string,object> object_dic;
	public bool SendMode = false;
	public bool ReciveMode=false;
	private float time = 0.0f;
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
			{ "size", 0},
			{ "objects" , 0}
		};
	}

	// Use this for initialization
	void Start () {
		if (unode.IsNodeJS) {
			ObjectName = name;
			ws = new WebSocket (unode.adress);
			SetupTransform (ws, unode.adress);
			//StartCoroutine (transformToNodeJS ());
		}
	}

	void Update () {
		time = time + Time.deltaTime;
		if(time >= wait){
			time = 0;
			objects = GameObject.FindGameObjectsWithTag("TransformToNodeJS");
			object_dic = objects.ToDictionary (n => n.name,n => (object)n);
			
			ReciveTransform (Msgpack,object_dic);
			transformToNodeJS(objects);
		}
	}
	
	void OnApplicationQuit() {
		if(ws != null)
			ws.Close();
	}

	private void ReciveTransform(Dictionary<string,object> dic,Dictionary<string,object> GameObjs){
		if(ReciveMode){
			try{
				ReciveMode = false;
				SendMode = false;
				var objects = dic["objects"] as List<object>;
				Debug.Log (objects.Count);

				for(int i=0;i<objects.Count;i++){
					var data = objects[i] as Dictionary<string,object>;
					GameObject obj = GameObjs[(string)data["name"]] as GameObject;


					//Debug.Log((string)data["name"]);

					var l_pos         = (Dictionary<string,object>)data["localPosition"];
					var l_EulerAngles = (Dictionary<string,object>)data["localEulerAngles"];
					var l_Scale       = (Dictionary<string,object>)data["localScale"];

					obj.transform.localPosition    = new Vector3((long)l_pos["x"],(long)l_pos["y"],(long)l_pos["z"]);
					obj.transform.localEulerAngles = new Vector3((long)l_EulerAngles["x"],(long)l_EulerAngles["y"],(long)l_EulerAngles["z"]);
					obj.transform.localScale       = new Vector3((long)l_Scale["x"],(long)l_Scale["y"],(long)l_Scale["z"]);

				}
				
			}catch{
				Debug.Log("error"+"["+ObjectName+"]"+":ReciveTransform");
			}
		}
	}
	
	//IEnumerator transformToNodeJS(){
	private void transformToNodeJS(GameObject[] objects){
		if(objects.Length > 0){

			//array = new Dictionary<string, object>();
			list = new List<object>();

			for(int i=0;i<objects.Length;i++){
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
						
						var data = new Dictionary<string, object>{
							{ "name", objects[i].name},
							{ "localPosition", localPosition},
							{ "localEulerAngles", localEulerAngles},
							{ "localScale", localScale}
						};

						list.Add(data);
					}else {
						if(mode.Length>0)
							mode = string.Empty;
					}
				}catch{
					Debug.Log("error"+"["+objects[i].name+"]"+":Dictionary");
				}
			}

			packed_data["size"] = list.Count;
			packed_data["objects"] = list;

			if(SendMode){
				SendMode = false;
				try{
					unode.send(ws,packed_data);
				}catch{
					Debug.Log("error"+"["+ObjectName+"]"+":send");
				}
			}
		}
	}

	void SetupTransform(WebSocket ws,string adress){

		ws.Connect();

		ws.OnOpen += (sender, e) => {
			Debug.Log ("Unode_transform.OnOpen:");
		};

		var packed_data = new Dictionary<string, object> {
			{ "mode", "transform" },
			{ "regist", true },
		};
		unode.send(ws,packed_data);

		ws.OnMessage += (sender, e) => {
			switch(e.Type){
				case Opcode.Binary:
					//try{
						Msgpack = unode.MessagePackDecode(e.RawData) as Dictionary<string,object>;
						if(Msgpack.TryGetValue("mode",out data)){
							mode = (string)data;
							switch(mode){
								case "connected":
									connected = true;
									break;
								case "transform":
									if(connected)
										ReciveMode = true;
									break;
							}
						}else{
							Debug.Log("error"+"["+ObjectName+"]"+"::mode::"+e.RawData.Length);
						}
					//}catch{
					//	Debug.Log("error:"+"["+ObjectName+"]"+"Msgpack");
					//}
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

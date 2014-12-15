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
		SendMode = false;
		if(time >= wait){
			time = 0;
			objects = GameObject.FindGameObjectsWithTag("TransformToNodeJS");
			object_dic = objects.ToDictionary (n => n.name,n => (object)n);
			if(ReciveMode){

			}else{
				//transformToNodeJS(objects);
			}
		}

		ReciveMode = false;
		ReciveTransform (Msgpack,object_dic);
	}
	
	void OnApplicationQuit() {
		if(ws != null)
			ws.Close();
	}

	private void ReciveTransform(Dictionary<string,object> dic,Dictionary<string,object> GameObjs){
		try{
			var objects = dic["objects"] as List<object>;
			Debug.Log (objects.Count);

			for(int i=0;i<objects.Count;i++){
				var data = objects[i] as Dictionary<string,object>;
				GameObject obj = GameObjs[(string)data["name"]] as GameObject;


				//Debug.Log((string)data["name"]);

				var l_pos         = (Dictionary<string,object>)data["localPosition"];
				var l_EulerAngles = (Dictionary<string,object>)data["localEulerAngles"];
				var l_Scale       = (Dictionary<string,object>)data["localScale"];
				//Debug.Log((float)(double)l_pos["x"]);
				obj.transform.localPosition    = new Vector3((float)(double)l_pos["x"],(float)(double)l_pos["y"],(float)(double)l_pos["z"]);
				obj.transform.localEulerAngles = new Vector3((float)(double)l_EulerAngles["x"],(float)(double)l_EulerAngles["y"],(float)(double)l_EulerAngles["z"]);
				obj.transform.localScale       = new Vector3((float)(double)l_Scale["x"],(float)(double)l_Scale["y"],(float)(double)l_Scale["z"]);
				//Debug.Log(obj.transform.localPosition);
				//Debug.Log(obj.transform.localEulerAngles);
				//Debug.Log(obj.transform.localScale);

			}
			
		}catch{
			Debug.Log("error"+"["+ObjectName+"]"+":ReciveTransform");
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

						var tmp = new Vector3();

						tmp = objects[i].transform.localPosition;

						localPosition ["x"] = tmp.x;
						localPosition ["y"] = tmp.y;
						localPosition ["z"] = tmp.z;

						tmp = objects[i].transform.localEulerAngles;

						localEulerAngles ["x"] = tmp.x;
						localEulerAngles ["y"] = tmp.y;
						localEulerAngles ["z"] = tmp.z;

						tmp = objects[i].transform.localScale;

						localScale ["x"] = tmp.x;
						localScale ["y"] = tmp.y;
						localScale ["z"] = tmp.z;
						
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

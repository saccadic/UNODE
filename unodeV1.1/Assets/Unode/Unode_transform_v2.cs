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
	public List<GameObject> TransformObject;
	private Dictionary<string,object> tmp_data;
	public bool SendMode = false;
	public bool ReciveMode=false;
	private float time = 0.0f;
	private Vector3 tmp;

	private Dictionary<string,object> l_pos, l_EulerAngles, l_Scale,TransformData;
	private List<object> TransformObjects;
	private GameObject obj;
	private int i,t;
	private float[] vec;
	void Awake() {
		unode = GameObject.Find ("Unode_v1_3").GetComponent<Unode_v1_3> ();

		TransformObject = new List<GameObject>();
		l_pos = new Dictionary<string, object>();
		l_EulerAngles = new Dictionary<string, object>();
		l_Scale = new Dictionary<string, object>();
		TransformData = new Dictionary<string, object> ();

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
			list = new List<object>();
			SetupTransform (ws, unode.adress);
		}
	}

	void Update () {
		if(TransformObject.Count()>0){
			time = time + Time.deltaTime;
			if(time >= wait){
				time = 0;
				if (ReciveMode) {
					ReciveMode = false;
					ReciveTransform (Msgpack);			
				} else {
					transformToNodeJS(TransformObject);	
				}
			}
		}
	}
	
	void OnApplicationQuit() {
		if(ws != null)
			ws.Close();
	}


	private void ReciveTransform(Dictionary<string,object> dic){
		try{
			TransformObjects = dic["objects"] as List<object>;
			//Debug.Log ("size:"+TransformObjects.Count);

			for(i=0;i<TransformObjects.Count;i++){
				TransformData = TransformObjects[i] as Dictionary<string,object>;
				obj = GameObject.Find((string)TransformData["name"]);//GameObjs[(string)TransformData["name"]] as GameObject; //
	
				//Debug.Log("name:"+obj);

				l_pos         = (Dictionary<string,object>)TransformData["localPosition"];
				l_EulerAngles = (Dictionary<string,object>)TransformData["localEulerAngles"];
				l_Scale       = (Dictionary<string,object>)TransformData["localScale"];

				//Debug.Log(l_EulerAngles["x"].GetType());

				vec = new float[3];
				if(l_pos["x"].GetType() == typeof(double)){
					vec[0] = (float)(double)l_pos["x"];
				}else{
					vec[0] = (float)(Int64)l_pos["x"];
				}
				if(l_pos["y"].GetType() == typeof(double)){
					vec[1] = (float)(double)l_pos["y"];
				}else{
					vec[1] = (float)(Int64)l_pos["y"];
				}
				if(l_pos["z"].GetType() == typeof(double)){
					vec[2] = (float)(double)l_pos["z"];
				}else{
					vec[2] = (float)(Int64)l_pos["z"];
				}
				obj.transform.localPosition = new Vector3(vec[0],vec[1],vec[2]);

				if(l_EulerAngles["x"].GetType() == typeof(double)){
					vec[0] = (float)(double)l_EulerAngles["x"];
				}else{
					vec[0] = (float)(Int64)l_EulerAngles["x"];
				}
				if(l_EulerAngles["y"].GetType() == typeof(double)){
					vec[1] = (float)(double)l_EulerAngles["y"];
				}else{
					vec[1] = (float)(Int64)l_EulerAngles["y"];
				}
				if(l_EulerAngles["z"].GetType() == typeof(double)){
					vec[2] = (float)(double)l_EulerAngles["z"];
				}else{
					vec[2] = (float)(Int64)l_EulerAngles["z"];
				}
				obj.transform.localEulerAngles = new Vector3(vec[0],vec[1],vec[2]);

				if(l_Scale["x"].GetType() == typeof(double)){
					vec[0] = (float)(double)l_Scale["x"];
				}else{
					vec[0] = (float)(Int64)l_Scale["x"];
				}
				if(l_Scale["y"].GetType() == typeof(double)){
					vec[1] = (float)(double)l_Scale["y"];
				}else{
					vec[1] = (float)(Int64)l_Scale["y"];
				}
				if(l_Scale["z"].GetType() == typeof(double)){
					vec[2] = (float)(double)l_Scale["z"];
				}else{
					vec[2] = (float)(Int64)l_Scale["z"];
				}
				obj.transform.localScale = new Vector3(vec[0],vec[1],vec[2]);

				obj.transform.hasChanged = false;
			}
			
		}catch{
			Debug.Log("error"+"["+ObjectName+"]"+":ReciveTransform");
		}
	}
	
	//IEnumerator transformToNodeJS(){
	private void transformToNodeJS(List<GameObject> data){
		list.Clear();
		foreach (GameObject objects in data){
			try{
				if(objects.transform.hasChanged){
					objects.transform.hasChanged = false;
					SendMode = true;
					//Debug.Log("change");
					tmp = objects.transform.localPosition;
					localPosition = new Dictionary<string, object> {
						{ "x", Math.Round(tmp.x, 3, MidpointRounding.AwayFromZero)},
						{ "y", Math.Round(tmp.y, 3, MidpointRounding.AwayFromZero)},
						{ "z", Math.Round(tmp.z, 3, MidpointRounding.AwayFromZero)}
					};

					tmp = objects.transform.localEulerAngles;
					localEulerAngles = new Dictionary<string, object> {
						{ "x", Math.Round(tmp.x, 3, MidpointRounding.AwayFromZero)},
						{ "y", Math.Round(tmp.y, 3, MidpointRounding.AwayFromZero)},
						{ "z", Math.Round(tmp.z, 3, MidpointRounding.AwayFromZero)}
					};

					tmp = objects.transform.localScale;
					localScale = new Dictionary<string, object> {
						{ "x", Math.Round(tmp.x, 3, MidpointRounding.AwayFromZero)},
						{ "y", Math.Round(tmp.y, 3, MidpointRounding.AwayFromZero)},
						{ "z", Math.Round(tmp.z, 3, MidpointRounding.AwayFromZero)}
					};

					tmp_data = new Dictionary<string, object>{
						{ "name", objects.name},
						{ "localPosition", localPosition},
						{ "localEulerAngles", localEulerAngles},
						{ "localScale", localScale}
					};

					list.Add(tmp_data);
				}else {
					if(mode.Length>0)
						mode = string.Empty;
				}
			}catch{
				Debug.Log("error"+"["+objects.name+"]"+":Dictionary");
			}
		}

		if(SendMode){
			try{
				//Debug.Log(list.Count);
				packed_data["size"] = list.Count;
				packed_data["objects"] = list;
				unode.send(ws,packed_data);
				SendMode = false;
			}catch{
				Debug.Log("error"+"["+ObjectName+"]"+":send");
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
			//ws.Connect();	
		};	
	}
}

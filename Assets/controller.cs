using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class controller : MonoBehaviour {

	public Unode_v1_1 unode;
	public string name;
	public int x, y;
	private Texture2D texture;

	private byte[] byte_data;
	private object[] obj_data;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		/*
		if(unode.mode == 10){
			Debug.Log("File:"+((string)unode.dict["str"]));
			byte_data = new byte[((List<object>)unode.dict["data"]).Count];
			obj_data  = ((List<object>)unode.dict["data"]).ToArray();
			for(int i=0;i<obj_data.Length;i++){
				byte_data[i] = (byte)(long)obj_data[i];
			}
			gameObject.renderer.material.mainTexture = loadTexture(byte_data,x,y);
			unode.mode = 0;

			var unpacked_data = new Dictionary<string, object> {
				{ "mode", 0   },
			};
			
			unode.send(unpacked_data);
		}
		*/


		if(Input.GetKeyDown(KeyCode.A)){
			Debug.Log("send:mode-1");
			//unode.ws.Connect ();
			unode.triger = true;
			var unpacked_data = new Dictionary<string, object> {
				{ "mode", 10   },
				{ "name", name }
			};
			gameObject.renderer.material.mainTexture = null;

			unode.send(unpacked_data);
		}
	}

	private Texture loadTexture(byte[] data, int width, int height){
		texture = new Texture2D(width, height);
		texture.LoadImage(data);
		
		return texture;
	}
}

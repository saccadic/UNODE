using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vnc : MonoBehaviour {
	public Unode_v1_1 unode;
	// Use this for initialization

	public string host;
	public int    port;
	public string password;

	public string vncserver_name;
	public long width,height;
	public bool IsReady;
	public long x, y, r_w, r_h;
	private Texture2D img;

	private byte[]   byte_data;

	public float m_x,m_y,tmp_m_x,tmp_m_y;
	void Start () {
		IsReady = false;
	}
	
	// Update is called once per frame
	void Update () {
		switch(unode.mode){
			case "init" :
				vncserver_name 	= (string)unode.dict["title"];
				width  			= (long)unode.dict["width"];
				height 			= (long)unode.dict["height"];
				IsReady 		= true;
				
				Debug.Log("Successfully connected and authorised");
				Debug.Log("remote screen name: " + vncserver_name + " width:" + width + " height: " + height);
				break;
			case "fream" :
				Debug.Log("Update fream");
				x 		= (long)unode.dict["x"];
				x 		= (long)unode.dict["x"];
				r_w  	= (long)unode.dict["width"];
				r_h 	= (long)unode.dict["height"];
				img 	= loadtexture((int)r_w,(int)r_h);

				gameObject.renderer.material.mainTexture = img;

				break;
		}

		if(Input.GetKeyDown(KeyCode.Space)){
			var config = new Dictionary<string, object> {
				{ "host", host },
				{ "port", port},
				{ "password", password}
			};

			var unpacked_data = new Dictionary<string, object> {
				{ "mode", "init" },
				{ "config",config}
			};

			unode.send(unpacked_data);
		}

		if(m_x != tmp_m_x || m_y != tmp_m_y){
			tmp_m_x = m_x;
			tmp_m_y = m_y;

			var mouse = new Dictionary<string, object> {
				{ "mode", "mouse" },
				{ "x", m_x },
				{ "y", m_y},
				{ "button", 0}
			};
			unode.send(mouse);
		}
	}
	
	
	private Texture2D loadtexture(int width,int height){
		object[] obj_data;
		Texture2D texture;
		
		byte_data = new byte[((List<object>)unode.dict["data"]).Count];
		obj_data  = ((List<object>)unode.dict["data"]).ToArray();
		for(int i=0;i<obj_data.Length;i++){
			byte_data[i] = (byte)(long)obj_data[i];
		}
		texture = new Texture2D(width, height);
		texture.LoadImage(byte_data);
		
		return texture;
	}
}

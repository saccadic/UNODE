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
	public float width,height;
	public bool IsReady;
	public int x, y, rect_w, rect_h;
	public Texture2D img;

	private byte[,]   byte_data;

	public int m_x,m_y,tmp_m_x,tmp_m_y;

	public string text;

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
				x 		= (int)(long)unode.dict["x"];
				y 		= (int)(long)unode.dict["y"];
				rect_w  = (int)(long)unode.dict["width"];
				rect_h 	= (int)(long)unode.dict["height"];
				img 	= loadtexture( ((List<object>)unode.dict["image"]).ToArray(), (int)rect_w,(int)rect_h);

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

		if(Input.GetKeyDown(KeyCode.Return)){
			var unpacked_data = new Dictionary<string, object> {
				{ "mode", "keyboard" },
				{ "str",text}
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
	
	
	private Texture2D loadtexture(object[] image,int width,int height){
		//object[] r,g,b,data;
		//Texture2D texture;
		//Dictionary<string,object> dict;


		/*
		dict = image as Dictionary<string,object>;

		byte_data = new   byte[3,((List<object>)dict["red"]).Count];

		r  = ((List<object>)dict["red"])  .ToArray();
		g  = ((List<object>)dict["green"]).ToArray();
		b  = ((List<object>)dict["blue"]) .ToArray();




		texture = new Texture2D(width, height, TextureFormat.ARGB32, true);

		int offset = 0;
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				Color col = new Color((byte)(long)r[x], (byte)(long)g[x], (byte)(long)b[x],1.0f);
				texture.SetPixel(x, y, col);
			}
		}

		texture = new Texture2D(width, height, TextureFormat.ARGB32, true);
		int offset = 0;
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				Color col = new Color((long)image[x], (long)image[x], (long)image[x],1.0f);
				texture.SetPixel(x, y, col);
			}
		}
		
		texture.Apply();
*/
		byte[] data;
		data = new byte[image.Length];

		for(int i=0;i<data.Length;i++){
			data[i] = (byte)(long)image[i];
		}

		Texture2D texture = new Texture2D(width, height);
		texture.LoadImage(data);

		return texture;
	}
}

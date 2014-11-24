using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using MiniJSON;
using MiniMessagePack;

public class json : MonoBehaviour {
	public Unode unode;
	
	// Update is called once per frame
	void Update () {
		/*
		if(unode.msg.Length > 0){
			var dict = Json.Deserialize(unode.msg) as Dictionary<string,object>;
			if(dict != null){
				var mode = (int)(long)dict ["mode"];
				if(mode == 20){
					Debug.Log (unode.msg);
					byte[] decodedBytes = Convert.FromBase64String (((List<object>) dict["data"]).ToString());
					string decodedText = Encoding.UTF8.GetString (decodedBytes);
					Debug.Log (decodedText);

					//byte[] data = ((List<object>) dict["data"]).;
				}
			}
		}
		*/

		if (unode.msg.Length > 0) {
			var packer = new MiniMessagePacker ();
			var unpacked_data = packer.Unpack (unode.data) as Dictionary<string,object>;
			Debug.Log((int)unpacked_data["mode"]);
		}

		if(Input.GetKeyDown(KeyCode.A)){
			Debug.Log("send:mode-1");
			//unode.ws.Connect ();
			unode.ws.Send("{\"mode\":10}");
		}
	}

}

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Text.RegularExpressions;

public class NodeJTalk : MonoBehaviour {
	public Unode unode;

	public string Dictionary;
	public string Voice;
	public int Sampling;
	public string file;
	[Multiline(10)]public string text;


	// Use this for initialization
	void Start () {
		unode = GameObject.Find("Unode").GetComponent<Unode>();
	}
	
	// Update is called once per frame
	void Update () {
		if (unode.sound) {
			gameObject.AddComponent<voice> ().file = unode.file;
			unode.sound = false;
		}
		if(Input.GetMouseButtonDown(0)){
			send(text);
		}		
		if(Input.GetMouseButtonDown(1)){
			send("現在時刻は" + DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分ss秒") + "です。");
		}	
	}

	void send(string str){
		str = str.Replace ("\n"," ");
		unode.message(3,"\"dic\":\""+Dictionary+"\","+
		              "\"voice\":\""+Voice     +"\","+
		              "\"sample\":" +Sampling  +  ","+
		              "\"file\":\"" +file      +"\","+
		              "\"text\":\"" +str       +"\"");
	}


}

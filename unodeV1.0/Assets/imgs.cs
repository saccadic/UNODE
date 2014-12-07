using UnityEngine;
using System.Collections;

public class imgs : MonoBehaviour {

	public vnc v;

	// Use this for initialization
	void Start () {
		v = GameObject.Find ("vnc").gameObject.GetComponent<vnc>();
	}
	
	// Update is called once per frame
	void Update () {
		if (v.img != null) {
				gameObject.renderer.material.mainTexture = v.img;
		}
	}
}

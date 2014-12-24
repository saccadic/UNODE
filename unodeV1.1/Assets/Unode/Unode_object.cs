using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unode_object : MonoBehaviour {

	private Unode_transform_v2 UnodeTransform;

	void Awake() {
		UnodeTransform = GameObject.Find ("Unode_transform").GetComponent<Unode_transform_v2> ();
	}

	// Use this for initialization
	void Start () {
		UnodeTransform.AddTransformObject(gameObject);
	}
	
	// Update is called once per frame
	void OnDestroy () {
		UnodeTransform.RemoveTransformObject(gameObject);
	}
}

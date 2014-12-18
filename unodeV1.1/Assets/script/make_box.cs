using UnityEngine;
using System.Collections;

public class make_box : MonoBehaviour {

	public GameObject obj;
	public Vector3 v;

	public float offset = 0;
	// Use this for initialization
	void Start () {
		int n=0;
		for(int z=0;z<v.z;z++){
			for(int y=0;y<v.y;y++){
					for(int x=0;x<v.x;x++){
					GameObject clone = (GameObject)Instantiate(obj);
					clone.transform.parent = transform;
					clone.transform.position = new Vector3(x,y,z) + transform.position + new Vector3(offset,offset,offset);
					clone.gameObject.name = n+"";
					clone.gameObject.AddComponent<Unode_object>();
					n++;
				}
			}
		}
	}
}

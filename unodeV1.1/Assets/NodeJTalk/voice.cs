using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class voice : MonoBehaviour {

	public  string file;
	private string path;
	
	private AudioSource audiosource;
	public AudioClip clip;

	// Use this for initialization
	IEnumerator Start () {
		audiosource = GetComponent<AudioSource>();
		path = "file://"+Application.streamingAssetsPath + "/.node/data/" + file;
		WWW www = new WWW(path);
		yield return www;
		//clip = new AudioClip ();
		clip = www.audioClip;
		//while (!clip.isReadyToPlay)
		audiosource.clip =  clip;
		audiosource.Play();
	}

	bool tmp = false;
	void Update(){
		if(audiosource.isPlaying == true && tmp == false){
			tmp = true;
		}
		if(audiosource.isPlaying == false && tmp) {
			Destroy(this);		
		}
	}

}

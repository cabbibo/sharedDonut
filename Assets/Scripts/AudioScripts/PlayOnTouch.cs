using UnityEngine;
using System.Collections;

public class PlayOnTouch : MonoBehaviour {

  public AudioSource source;
  public AudioClip   clip;
  public string      name;
  public GameObject  title;

	// Use this for initialization
	void Start () {  

    title.GetComponent<TextMesh>().text = name;


	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  void OnTriggerEnter(){

    print("BAL");
    source.clip = clip;
    source.Play();

  }


}

using UnityEngine;
using System.Collections;

public class Prism : MonoBehaviour {

  // Defining some of render values
  public float _NoiseSize;
  public Vector3 _Color1;
  public Vector3 _Color2;

  public GameObject puppy;
  

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  public void select(){
    puppy.GetComponent<Dough>().select();
  }

  public void deselect(){
    puppy.GetComponent<Dough>().deselect();
  }


}

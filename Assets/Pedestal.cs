using UnityEngine;
using System.Collections;

public class Pedestal : MonoBehaviour {



  public GameObject[] prisms;

  public float attractionRadius = .5f;


  public float closestLength;
  public GameObject closestPrism;
  public GameObject attractingPrism;
  public GameObject selectedPrism;

  public bool prismSelected = false;
  public bool prismAttracting = false;

  private Vector3 v1;

  private RayOfLight rayOfLight;
  private Light light;


	// Use this for initialization
	void Start () {

    rayOfLight = GetComponent<RayOfLight>();
    light  = GetComponent<Light>();
	
	}
	
	// Update is called once per frame
	void Update () {

    closestLength = 1000.0f;


    for( int i = 0;  i < prisms.Length; i++ ){
      v1 = prisms[i].transform.position -transform.position;

      float l = v1.magnitude;

      if( l < closestLength ){
        closestLength = l;
        closestPrism = prisms[i];
      }

    }


    if( closestLength < attractionRadius ){
      if( prismAttracting == false ){
        startAttracting( closestPrism );
      }
    }else{

      if( prismAttracting == true ){
        stopAttracting( attractingPrism );
      }

    }

    if( prismAttracting == true ){
      updatePrismAttracting( attractingPrism );
    }


    if( closestLength < 0.01 && closestPrism == attractingPrism ){
      selectPrism( closestPrism );
    }
	
	}


  void startAttracting( GameObject prism ){

    // play attraction noise

    prismAttracting = true;

    attractingPrism = prism;

    rayOfLight.startAttracting( prism );


  }

  void stopAttracting( GameObject prism ){

    prismAttracting = false;
    rayOfLight.stopAttracting( prism );

    if( prismSelected == true ){
      selectedPrism.GetComponent<Prism>().deselect();
      prismSelected = false;
      selectedPrism = null;
      attractingPrism = null;
      light.intensity = 1;

    }



  }

  void updatePrismAttracting( GameObject prism ){

    v1 = prism.transform.position - transform.position;

    v1 = -10 * v1;
    //v1.Normalize();

    prism.GetComponent<Rigidbody>().AddForce( v1 );

  }


  void selectPrism( GameObject prism ){

    prismSelected = true;

    if( selectedPrism != prism ){
      selectedPrism = prism;
       light.intensity = .3f;
      prism.GetComponent<Prism>().select();
    }
    
  }

  void OnTriggerEnter(Collider c){

    if( c.gameObject.tag == "Prism"){
     // print("YESSSS");
    }


  }

}

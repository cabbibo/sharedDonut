using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlugInterface : MonoBehaviour {


  // TODO:
  // Figure out articulate ways to define 'META INTERFACE'
  // Which bundles certain values of interface together to make
  // much more power with single slider

  // be able to define shape of curve as part of uniform
  // be able to set to osscilate in certain ways?


  // TODO: Be able to create 'presets'
  // Be able to save presets from inside VR
  // Be able to reload these presets, and add them to a a 'presets interface'
  // Be able to select audio

  // To Think about: Be able to choose a 'render mode ', and have interface for that mode dynamically update

  public struct FloatUniform{
    public float value;
    public float low;
    public float high;
    public float og;
  }



  //public Dictionary Sliders<string,GameObject>;

  public Hashtable ComputeSliders;
  public Hashtable RenderSliders;

  public GameObject SliderPrefab;
  public GameObject SongPlayPrefab;
  
  public GameObject ComputeInterfaceBody;
  public GameObject RenderInterfaceBody;
  public GameObject SongInterfaceBody;

  public class ComputeUniforms{

    public FloatUniform _LengthOfConnectionSprings;
    public FloatUniform _ConnectionSpringStrength;
    public FloatUniform _MaxVel;
    public FloatUniform _MaxForce;
    public FloatUniform _ForceMultiplier;
    
    public FloatUniform _Dampening;
    public FloatUniform _HandRepelRadius          ;
    public FloatUniform _HandRepelStrength        ;

    public FloatUniform _ReturnSpringStrength     ;

    public FloatUniform _DistanceToAudioHit       ;
    public FloatUniform _AudioHitMultiplier       ;

  };


  public class RenderUniforms{

    public FloatUniform _NormalizedVelocityValue;
    public FloatUniform _NormalizedVertexValue;
    public FloatUniform _NormalizedOriginalValue;

    public FloatUniform _AudioValue;
    public FloatUniform _AudioSampleSize;
    public FloatUniform _ReflectionValue;
    public FloatUniform _NormalMapSize;
    public FloatUniform _NormalMapDepth;

  };


  private ComputeUniforms computeUniforms;
  private RenderUniforms renderUniforms;




  void Start(){

    //Sliders = new Dictionary<string, GameObject>();
    ComputeSliders = new Hashtable(); 
    RenderSliders  = new Hashtable(); 

    computeUniforms = new ComputeUniforms();

    computeUniforms._LengthOfConnectionSprings.value  = 1.1f ;
    computeUniforms._LengthOfConnectionSprings.og     = 1.1f ;
    computeUniforms._LengthOfConnectionSprings.low    = 0f   ;
    computeUniforms._LengthOfConnectionSprings.high   = 100.5f  ;

    computeUniforms._ConnectionSpringStrength.value   = 10.9f  ;
    computeUniforms._ConnectionSpringStrength.og      = 10.9f  ;
    computeUniforms._ConnectionSpringStrength.low     = 1.9f    ;
    computeUniforms._ConnectionSpringStrength.high    = 20.9f ;

    computeUniforms._MaxVel.value                     = 30.5f  ;
    computeUniforms._MaxVel.value                     = 30.5f  ;
    computeUniforms._MaxVel.low                       =   .1f  ;
    computeUniforms._MaxVel.high                      = 100.5f ;

    computeUniforms._MaxForce.value                   = 30.2f  ;
    computeUniforms._MaxForce.value                   = 30.2f  ;
    computeUniforms._MaxForce.low                     =   .2f  ;
    computeUniforms._MaxForce.high                    = 60.2f  ;


    computeUniforms._ForceMultiplier.value            = .01f   ;
    computeUniforms._ForceMultiplier.og               = .01f   ;
    computeUniforms._ForceMultiplier.low              = .0001f  ;
    computeUniforms._ForceMultiplier.high             = .03f    ;


    computeUniforms._Dampening.value                  = .98f   ;
    computeUniforms._Dampening.og                     = .98f   ;
    computeUniforms._Dampening.low                    = .4f    ;
    computeUniforms._Dampening.high                   = .99f   ;

    computeUniforms._HandRepelRadius.value            = 1.0f   ;
    computeUniforms._HandRepelRadius.og               = 1.0f   ;
    computeUniforms._HandRepelRadius.low              = .01f   ;
    computeUniforms._HandRepelRadius.high             = 10.0f   ;


    computeUniforms._HandRepelStrength.value          = .05f   ;
    computeUniforms._HandRepelStrength.og             = .05f   ;
    computeUniforms._HandRepelStrength.low            = .001f  ;
    computeUniforms._HandRepelStrength.high           = 1.0f   ;


    computeUniforms._ReturnSpringStrength.value       = 4.1f   ;
    computeUniforms._ReturnSpringStrength.og          = 4.1f   ;
    computeUniforms._ReturnSpringStrength.low         = .01f   ;
    computeUniforms._ReturnSpringStrength.high        = 10.1f  ;


    computeUniforms._DistanceToAudioHit.value         = 5.8f   ;
    computeUniforms._DistanceToAudioHit.og            = 5.8f   ;
    computeUniforms._DistanceToAudioHit.low           = .3f    ;
    computeUniforms._DistanceToAudioHit.high          = 50.8f  ;

    computeUniforms._AudioHitMultiplier.value         = .01f   ;
    computeUniforms._AudioHitMultiplier.og            = .01f   ;
    computeUniforms._AudioHitMultiplier.low           = .001f  ;
    computeUniforms._AudioHitMultiplier.high          = .06f   ;

    CreateComputeInterface();
    SetComputeInterface();



    renderUniforms = new RenderUniforms();

    renderUniforms._NormalizedOriginalValue.value  = 1f ;
    renderUniforms._NormalizedOriginalValue.og     = 1f ;
    renderUniforms._NormalizedOriginalValue.low    = 0f ;
    renderUniforms._NormalizedOriginalValue.high   = 1f ;

    renderUniforms._NormalizedVertexValue.value  = 1f ;
    renderUniforms._NormalizedVertexValue.og     = 1f ;
    renderUniforms._NormalizedVertexValue.low    = 0f ;
    renderUniforms._NormalizedVertexValue.high   = 1f ;

    renderUniforms._NormalizedVelocityValue.value  = 1f ;
    renderUniforms._NormalizedVelocityValue.og     = 1f ;
    renderUniforms._NormalizedVelocityValue.low    = 0f ;
    renderUniforms._NormalizedVelocityValue.high   = 1f ;

    renderUniforms._AudioValue.value   = 0f ;
    renderUniforms._AudioValue.og      = 0f ;
    renderUniforms._AudioValue.low     = 0f ;
    renderUniforms._AudioValue.high    = 1f ;

    renderUniforms._AudioSampleSize.value   = 0.1f ;
    renderUniforms._AudioSampleSize.og      = 0.1f ;
    renderUniforms._AudioSampleSize.low     = 0.1f ;
    renderUniforms._AudioSampleSize.high    = 1f ;

    renderUniforms._ReflectionValue.value   = 0f  ;
    renderUniforms._ReflectionValue.value   = 0f  ;
    renderUniforms._ReflectionValue.low     = 0f  ;
    renderUniforms._ReflectionValue.high    = 1f ;

    renderUniforms._NormalMapSize.value   = 2f ;
    renderUniforms._NormalMapSize.og      = 2f ;
    renderUniforms._NormalMapSize.low     = 0f ;
    renderUniforms._NormalMapSize.high    = 10f ;

    renderUniforms._NormalMapDepth.value   = 0f  ;
    renderUniforms._NormalMapDepth.value   = 0f  ;
    renderUniforms._NormalMapDepth.low     = 0f  ;
    renderUniforms._NormalMapDepth.high    = 20f ;



    CreateRenderInterface();
    SetRenderInterface();

    setUpSongs();

  }

  public void setUpSongs(){

    int i = 0;


    Vector3 p = SongInterfaceBody.transform.position;
    p += new Vector3( 0 , .03f * i + .1f , 0 );

    setSong( "Philip Glass - Knee 1 (Nosaj Thing Remix)" ,"Audio/1-03 Knee 1 Remix" , p );

    i++;



    p = SongInterfaceBody.transform.position;
    p += new Vector3( 0 , .03f * i + .1f , 0 );

    setSong( "Burial - Come Down To Us" ,"Audio/03 Come Down To Us" , p );

    i++;

      p = SongInterfaceBody.transform.position;
    p += new Vector3( 0 , .03f * i + .1f , 0 );

    setSong( "LORN - CONDUIT" ,"Audio/LORN - VESSEL - 04 CONDUIT" , p );

    i++;


    p = SongInterfaceBody.transform.position;
    p += new Vector3( 0 , .03f * i + .1f , 0 );

    setSong( "123MRK - Weird" ,"Audio/02 Weird" , p );

    i++;



  

  }

  public void setSong( string title , string clip , Vector3 p ){

    GameObject s = (GameObject) Instantiate( SongPlayPrefab , p , new Quaternion());

    s.GetComponent<PlayOnTouch>().clip = Resources.Load(clip) as AudioClip; 
    s.GetComponent<PlayOnTouch>().name = title;
    s.GetComponent<PlayOnTouch>().source = GameObject.FindWithTag("AudioSource").GetComponent<AudioSource>();
    s.transform.parent = SongInterfaceBody.transform;


  }

  public void SetComputeUniforms( ComputeShader Mat ){

    //print(computeUniforms._Dampening.value);

   // Mat.SetFloat( "_LengthOfConnectionSprings"    , u._LengthOfConnectionSprings);

    foreach (var field in typeof(ComputeUniforms).GetFields()){

      FloatUniform u = (FloatUniform)field.GetValue( computeUniforms );

      Mat.SetFloat( field.Name , u.value );

    }
   
    //Mat.SetFloat( "_MaxVel"                       , computeUniforms._MaxVel.value                    );
    //Mat.SetFloat( "_MaxForce"                     , computeUniforms._MaxForce.value                  );
    //Mat.SetFloat( "_ForceMultiplier"              , computeUniforms._ForceMultiplier.value           );
    //Mat.SetFloat( "_Dampening"                    , computeUniforms._Dampening.value                 );
    //Mat.SetFloat( "_HandRepelRadius"              , computeUniforms._HandRepelRadius.value           );
    //Mat.SetFloat( "_HandRepelStrength"            , computeUniforms._HandRepelStrength.value         );
    //Mat.SetFloat( "_ReturnSpringStrength"         , computeUniforms._ReturnSpringStrength.value      );
    //Mat.SetFloat( "_DistanceToAudioHit"           , computeUniforms._DistanceToAudioHit.value        );
    //Mat.SetFloat( "_AudioHitMultiplier"           , computeUniforms._AudioHitMultiplier.value        );

  }

  public void SetRenderUniforms( Material Mat ){

    foreach (var field in typeof(RenderUniforms).GetFields()){

      FloatUniform u = (FloatUniform)field.GetValue( renderUniforms );

      Mat.SetFloat( field.Name , u.value );

    }
   
  }


  public void resetRenderUniforms(){


    foreach (var field in typeof(RenderUniforms).GetFields()){

      FloatUniform u = (FloatUniform)field.GetValue( renderUniforms );
      u.value = u.og;

      field.SetValue( renderUniforms , u );

    }

  }

  public void resetComputeUniforms(){


    foreach (var field in typeof(ComputeUniforms).GetFields()){

      FloatUniform u = (FloatUniform)field.GetValue( computeUniforms );
      u.value = u.og;

      field.SetValue( renderUniforms , u );

    }
   

    //computeUniforms._LengthOfConnectionSprings  = 0.001f;
    //computeUniforms._LengthOfConnectionSprings.value  = computeUniforms._LengthOfConnectionSprings.og ;
    //computeUniforms._ConnectionSpringStrength.value   = computeUniforms._ConnectionSpringStrength.og  ;
    //computeUniforms._MaxVel.value                     = computeUniforms._MaxVel.og                    ;
    //computeUniforms._MaxForce.value                   = computeUniforms._MaxForce.og                  ;
    //computeUniforms._ForceMultiplier.value            = computeUniforms._ForceMultiplier.og           ;
    //computeUniforms._Dampening.value                  = computeUniforms._Dampening.og                 ;
    //computeUniforms._HandRepelRadius.value            = computeUniforms._HandRepelRadius.og           ;
    //computeUniforms._HandRepelStrength.value          = computeUniforms._HandRepelStrength.og         ;
    //computeUniforms._ReturnSpringStrength.value       = computeUniforms._ReturnSpringStrength.og      ;
    //computeUniforms._DistanceToAudioHit.value         = computeUniforms._DistanceToAudioHit.og        ;
    //computeUniforms._AudioHitMultiplier.value         = computeUniforms._AudioHitMultiplier.og        ;

  }

  public void updateInterfaceValues(){



    foreach (var field in typeof(ComputeUniforms).GetFields()){

     
      if(ComputeSliders[field.Name] != null ){

        FloatUniform u = (FloatUniform)field.GetValue( computeUniforms );
        GameObject slider = (GameObject)ComputeSliders[field.Name];

        float v = slider.GetComponent<Slider>().Value;
        float final = v * (u.high - u.low) + u.low;

        u.value = final;


        field.SetValue( computeUniforms , u );

      }else{

      }

    }

    foreach (var field in typeof(RenderUniforms).GetFields()){

     
      if(RenderSliders[field.Name] != null ){

        FloatUniform u = (FloatUniform)field.GetValue( renderUniforms );
        GameObject slider = (GameObject)RenderSliders[field.Name];

        float v = slider.GetComponent<Slider>().Value;
        float final = v * (u.high - u.low) + u.low;

        u.value = final;


        field.SetValue( renderUniforms , u );

      }else{

      }

    }





  }




  public void CreateComputeInterface(){

    int i = 0;

    foreach (var field in typeof(ComputeUniforms).GetFields()){

      Vector3 p = ComputeInterfaceBody.transform.position;
      p += new Vector3( 0 , .03f * i + .1f , 0 );


      if( 
          field.Name != "_MaxVel" &&
          field.Name != "_MaxForce" &&
          field.Name != "_ConnectionSpringStrength" &&
          field.Name != "_LengthOfConnectionSprings"
        ){

        GameObject s = (GameObject) Instantiate( SliderPrefab , p , new Quaternion());
        ComputeSliders[field.Name] = s;
        s.GetComponent<Slider>().SetSliderName(field.Name);
        s.transform.parent = ComputeInterfaceBody.transform;
        i++;
      }
    }

  }

  public void CreateRenderInterface(){

    int i = 0;

    foreach (var field in typeof(RenderUniforms).GetFields()){

      Vector3 p = RenderInterfaceBody.transform.position;
      p += new Vector3( 0 , .03f * i + .1f , 0 );

      GameObject s = (GameObject) Instantiate( SliderPrefab , p , new Quaternion());
      RenderSliders[field.Name] = s;
      s.GetComponent<Slider>().SetSliderName(field.Name);
      s.transform.parent = RenderInterfaceBody.transform;
      i++;

    }

  }

  public void SetComputeInterface(){

    foreach (var field in typeof(ComputeUniforms).GetFields()){

     
      if(ComputeSliders[field.Name] != null ){

        FloatUniform u = (FloatUniform)field.GetValue( computeUniforms );
        GameObject slider = (GameObject)ComputeSliders[field.Name];

        float final = (u.value - u.low) / (u.high - u.low);// + u.low;

        //Wfinal = 1.0f;
        slider.GetComponent<Slider>().SetValue( final );

      }else{

        print( "NO SLIDERSSS" );
      
      }

    }

  }


  public void SetRenderInterface(){

    foreach (var field in typeof(RenderUniforms).GetFields()){

     
      if(RenderSliders[field.Name] != null ){

        FloatUniform u = (FloatUniform)field.GetValue( renderUniforms );
        GameObject slider = (GameObject)RenderSliders[field.Name];

        float final = (u.value - u.low) / (u.high - u.low);// + u.low;

        //Wfinal = 1.0f;
        slider.GetComponent<Slider>().SetValue( final );

      }else{

        print( "NO SLIDERSSS" );
      
      }

    }


  }

	// Update is called once per frame
	void Update () {

    updateInterfaceValues();
	
	}
}

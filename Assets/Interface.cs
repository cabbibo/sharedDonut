using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interface : MonoBehaviour {

  public struct ComputeUniforms{

    public float _LengthOfConnectionSpring;
    public float _ConnectionSpringStrength;
    public float _MaxVel;
    public float _MaxForce;
    public float _ForceMultiplier;
    
    public float _Dampening;
    public float _HandRepelRadius          ;
    public float _HandRepelStrength        ;

    public float _ReturnSpringStrength     ;

    public float _DistanceToAudioHit       ;
    public float _AudioHitMultiplier       ;

  };



  public static void SetComputeUniforms( ComputeUniforms u , ComputeShader Mat ){

//    Mat.SetFloat( "_LengthOfConnectionSprings"    , computeUniforms._LengthOfConnectionSprings);
    Mat.SetFloat( "_ConnectionSpringStrength "    , u._ConnectionSpringStrength );
    Mat.SetFloat( "_MaxVel"                       , u._MaxVel                   );
    Mat.SetFloat( "_MaxForce"                     , u._MaxForce                 );
    Mat.SetFloat( "_ForceMultiplier"              , u._ForceMultiplier          );
    Mat.SetFloat( "_Dampening"                    , u._Dampening                );
    Mat.SetFloat( "_HandRepelRadius"              , u._HandRepelRadius          );
    Mat.SetFloat( "_HandRepelStrength"            , u._HandRepelStrength        );
    Mat.SetFloat( "_ReturnSpringStrength"         , u._ReturnSpringStrength     );
    Mat.SetFloat( "_DistanceToAudioHit"           , u._DistanceToAudioHit       );
    Mat.SetFloat( "_AudioHitMultiplier"           , u._AudioHitMultiplier       );

  }



  public static void SetOriginalComputeUniforms( ComputeShader Mat){

    ComputeUniforms computeUniforms = new ComputeUniforms();
//    computeUniforms._LengthOfConnectionSprings  = 0.0001f;
    computeUniforms._ConnectionSpringStrength   = .3f    ;
    computeUniforms._MaxVel                     = 30.5f  ;
    computeUniforms._MaxForce                   = 30.2f  ;
    computeUniforms._ForceMultiplier            = .01f   ;
    computeUniforms._Dampening                  = .98f   ;
    computeUniforms._HandRepelRadius            = 1.0f   ;
    computeUniforms._HandRepelStrength          = .05f   ;
    computeUniforms._ReturnSpringStrength       = 1.1f   ;
    computeUniforms._DistanceToAudioHit         = 2.0f   ;
    computeUniforms._AudioHitMultiplier         = .003f  ;

    SetComputeUniforms( computeUniforms ,  Mat );

  }

	// Use this for initialization
	void Start () {

    //computeUniforms = new ComputeUniforms();
    //SetOriginalComputeUniforms();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

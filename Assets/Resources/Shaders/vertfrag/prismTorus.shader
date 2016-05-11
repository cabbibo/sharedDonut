Shader "Custom/prismTorus" {



    SubShader{
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Cull off
        Pass{

            Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
 
            CGPROGRAM
            #pragma target 5.0
 
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 

            #include "Chunks/VertC4Struct.cginc"
            struct Pos {
                float3 pos;
            };

            StructuredBuffer<VertC4> buf_Points;
            StructuredBuffer<Pos> og_Points;

            uniform float4x4 worldMat;
            uniform float4x4 invWorldMat;
            uniform float4x4 miniMat;

            uniform float3 _PrismPosition;


            uniform sampler2D _AudioMap;

 
            //A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float  inOut : TEXCOORD3;
                int id : TEXCOORD2;

            };

            //#include "Chunks/getTorusIDs.cginc"
            #include "Chunks/getGridDiscard.cginc"
            #include "Chunks/semLookup.cginc"
            #include "Chunks/uvNormalMap.cginc"
            #include "Chunks/noise.cginc"

						// From http://www.iquilezles.org/www/articles/palettes/palettes.htm
						// cosine based palette, 4 vec3 params
						float3 palette( in float t, in float3 a, in float3 b, in float3 c, in float3 d ){
						  return a + b*cos( 6.28318*(c*t+d) );
						}
           



            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

                varyings o;

                //Get if close or far 
                float origin = id % 2;
                o.inOut = origin;

                o.id = id;
                
                uint fID = id * 20 + 10;
                VertC4 v = buf_Points[fID ];
                Pos og = og_Points[fID ];


                if( origin == 0 ){

                 	

	                o.worldPos = (v.pos - _PrismPosition ) * .6 + _PrismPosition;

             	 	}else{

                  // Center inside of smallSphere
                  //o.worldPos = mul( invWorldMat , float4( v.pos , 1.) ).xyz;
                  //o.worldPos = mul( miniMat , float4( o.worldPos , 1.) ).xyz;

             	 		o.worldPos = _PrismPosition;

                 //nor;// * .5 + .5;//float3(float(fID)/32768., v.uv.x , v.uv.y);
             	 	}
             	 	o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

                return o;
            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (varyings i) : COLOR {


            		float3 dir = i.worldPos - _PrismPosition;
                float dist = length( dir );

            	//	float n = noise( (i.worldPos * float3( 1. , 10. , 1. )) + 200. * _Time.y * normalize(dir) );
            		float rayNoise = noise( i.worldPos * float3( .4 , 1. , 2. ) + ( 1. * _Time.y * normalize(dir) ));// +// 20. * _Time.y * dir );

            		float idNoise = hash( floor( i.id / 2. ) );
         
              	float3 fCol = float3( .3 , 0.9, 0.7  );
              	float3 turq = float3( .3 , 0.9, 0.7  );
              	float3 purple = float3( .4 , .2 , .9 );


              	//fCol *= n * n * n * 1.;



              	fCol = lerp(  turq , purple , idNoise );
              	fCol *= tex2D( _AudioMap , float2( rayNoise  * .1, 0. ) ).xyz;

                return float4( fCol , length( fCol ) * i.inOut );//.1 * float4(1,0.5f,0.0f,1.0) / ( i.dToPoint * i.dToPoint  * i.dToPoint );
            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
  
}

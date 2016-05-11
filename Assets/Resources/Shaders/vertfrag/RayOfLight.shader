Shader "Custom/RayOfLight" {



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
 

            #include "Chunks/VertStruct.cginc"
            #include "Chunks/noise.cginc"
            struct Pos {
                float3 pos;
            };

            StructuredBuffer<Vert> buf_Points;
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
                float3 nor : TEXCOORD3;
                int id : TEXCOORD2;

            };

            // From http://www.iquilezles.org/www/articles/palettes/palettes.htm
            // cosine based palette, 4 vec3 params
            float3 palette( in float t, in float3 a, in float3 b, in float3 c, in float3 d ){
              return a + b*cos( 6.28318*(c*t+d) );
            }
           



            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

              varyings o;



              o.id = id;
              
              uint fID = floor( id / 18 );
              uint triIndex = id % 3;
              uint whichTri = floor((id % 18) / 3 );

              Vert v = buf_Points[fID ];
              Pos og = og_Points[fID ];

              o.worldPos = v.pos + float3( hash( fID ) , hash( id + 2 ) , hash( id + 10 ) ) * 10.;

              o.nor = normalize(v.vel);

              float hexRadius = .005;
              float3 z = normalize(v.pos);
              float3 x = normalize(cross( z , float3( 0 , 1 , 0 ) ));
              float3 y = normalize(cross( x , z ));
              //x = normalize( cross( y , z ));

              float angle = ((float)whichTri / 6 ) * 2. * 3.14159;
              float upAngle = (((float)whichTri +1) / 6 ) * 2. * 3.14159;

             // angle = hash( id +2 );
              //upAngle = hash( id +5);

              if( triIndex == 0 ){
                o.worldPos = v.pos;
              }else if( triIndex == 1 ){
                o.worldPos = v.pos + hexRadius * cos( angle ) * x  + hexRadius * sin( angle ) * y;
                //o.worldPos = v.pos + hexRadius * hash( id + 2 ) * x   + hexRadius * hash( id + 5 )  * y;
              }else{
                //o.worldPos = v.pos + hexRadius *hash( id + 6 ) * x  + hexRadius *hash( id + 2 )  * y;
                o.worldPos = v.pos + hexRadius * cos( upAngle ) * x  + hexRadius * sin( upAngle ) * y;
              }

              o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

              return o;
            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (varyings i) : COLOR {

              float3 fCol = ( i.nor * .5 + .5 );

              return float4( fCol , 1. );


            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
  
}

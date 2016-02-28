Shader "Custom/goldBug" {



    SubShader{
//        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
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


            uniform float _NormalizedVelocityValue;
            uniform float _NormalizedVertexValue;
            uniform float _NormalizedOriginalValue;

            uniform float _AudioValue;
            uniform float _AudioSampleSize;
            uniform float _ReflectionValue;
            uniform float _NormalMapSize;
            uniform float _NormalMapDepth;




            uniform float3 _HandL;
            uniform float3 _HandR;
            uniform int _RibbonWidth;
            uniform int _RibbonLength;
            uniform int _TotalVerts;
            uniform sampler2D _NormalMap;
            uniform sampler2D _SEMMap;
            uniform sampler2D _TextureMap;
            uniform sampler2D _BumpMap;
            uniform sampler2D _AudioMap;
            uniform samplerCUBE _CubeMap;
 
            //A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 nor      : TEXCOORD0;
                float3 eye      : TEXCOORD2;
                float3 debug    : TEXCOORD3;
                float2 uv       : TEXCOORD4;
            };

            #include "Chunks/getTorusIDs.cginc"
            //#include "Chunks/getTubeNormalIDs.cginc"
            #include "Chunks/getGridDiscard.cginc"
            #include "Chunks/semLookup.cginc"
            #include "Chunks/uvNormalMap.cginc"

            //#include "Assets/Shaders/Chunks/Resources/uvNormalMap.cginc"

           


            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

                varyings o;

                // from getRibbonID 
                uint fID = getID( id );
                VertC4 v = buf_Points[fID];
                Pos og = og_Points[fID];

                float3 dif =  mul( worldMat , float4( og.pos , 1.) ).xyz - v.pos;

                o.worldPos = v.pos;

                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));





                float3 fDif = normalize( dif ) * _NormalizedOriginalValue;
                float3 fNor = v.nor * _NormalizedVertexValue;
                float3 fVel = normalize( v.vel) * _NormalizedVelocityValue;

                o.debug = normalize(fDif + fNor + fVel);// * nor;//o.worldPos - og.pos;

                o.eye = _WorldSpaceCameraPos - o.worldPos;
                o.uv = v.uv;

               // o.debug = v.vel;

                o.nor = v.nor; //nor;// * .5 + .5;//float3(float(fID)/32768., v.uv.x , v.uv.y);
                return o;
            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (varyings i) : COLOR {

                float3 difL = _HandL - i.worldPos;
                float3 difR = _HandR - i.worldPos;

                difL = normalize( difL );
                difR = normalize( difR );


                float3 fNorm = uvNormalMap( _NormalMap , i.pos ,  i.uv  * float2( 1. , .2), i.nor , _NormalMapSize ,_NormalMapDepth);
                //fNorm = i.debug;

                float3 reflL = reflect( difL , fNorm );
                float matchL = max( 0. , -dot( reflL , normalize(i.eye) )); 
                float3 reflR = reflect( difR , fNorm );
                float matchR = max( 0. , -dot( reflR , normalize(i.eye) ));

                float3 col = (fNorm * .5 +.5) + (difL * .5 +.5) + (difR * .5 +.5);


                float3 fRefl = reflect( -i.eye , fNorm );
                float3 cubeCol = texCUBE(_CubeMap, normalize( fRefl ) ).rgb;



                float fr = dot( fNorm , -i.eye );
                float3 aCol = tex2D( _AudioMap , float2(fr * _AudioSampleSize , 0.)).xyz;

                float3 fCol = i.debug * .5 + .5;
                fCol = lerp( fCol , fCol * cubeCol , _ReflectionValue );

                fCol = lerp( fCol , 2 * aCol * fCol , _AudioValue ); //tex2D( _AudioMap , float2(i.uv.y, 0. ));

                //fCol = aCol;
                //fCol = fNorm * .5 + .5;

                //fCol = i.debug * .5 + .5;

                //fCol = float3( fr , fr , fr );

               //fCol =  (normalize(i.nor + normalize(i.debug ))* .5 ) + .5;
               //fCol = cubeCol;
               //fCol = aCol;
               //fCol = normalize( i.debug ) * .5 + .5;
               //fCol =  length( i.debug ) * length( i.debug ) * (i.nor * .5 + .5) * cubeCol ;

               //fCol = i.debug * .5 + .5;
                return float4( fCol , 1.);//.1 * float4(1,0.5f,0.0f,1.0) / ( i.dToPoint * i.dToPoint  * i.dToPoint );
            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
	
}

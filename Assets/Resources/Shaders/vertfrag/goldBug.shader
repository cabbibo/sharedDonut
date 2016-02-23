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

            //#include "Chunks/getRibbonID.cginc"
            #include "Chunks/getTubeNormalIDs.cginc"
            #include "Chunks/getGridDiscard.cginc"
            #include "Chunks/semLookup.cginc"
            #include "Chunks/uvNormalMap.cginc"

            //#include "Assets/Shaders/Chunks/Resources/uvNormalMap.cginc"

           
            uint3 getID( uint id  ){

  uint base = floor( id / 6 );
  uint tri  = id % 6;
  uint row = floor( base / _RibbonWidth );
  uint col = (base) % _RibbonWidth;

  uint rowU = (row + 1) % _RibbonLength;
  uint colU = (col + 1) % _RibbonWidth;

  uint rDoID = row * _RibbonWidth;
  uint rUpID = rowU * _RibbonWidth;
  uint cDoID = col;
  uint cUpID = colU;


  uint fID = 0;
  uint tri1 = 0;
  uint tri2 = 0;


  if( tri == 0 ){
    fID = rDoID + cDoID;
    tri1 = rUpID + cDoID;
    tri2 = rUpID + cUpID;
  }else if( tri == 1 ){
    fID = rUpID + cDoID;
    tri1 = rUpID + cUpID;
    tri2 = rDoID + cDoID;
  }else if( tri == 2 ){
    fID = rUpID + cUpID;
    tri1 = rDoID + cDoID;
    tri2 = rUpID + cDoID;
  }else if( tri == 3 ){
    fID = rDoID + cDoID;
    tri1 = rUpID + cUpID;
    tri2 = rDoID + cUpID;
  }else if( tri == 4 ){
    fID = rUpID + cUpID;
    tri1 = rDoID + cUpID;
    tri2 = rDoID + cDoID;
  }else if( tri == 5 ){
    fID = rDoID + cUpID;
    tri1 = rDoID + cDoID;
    tri2 = rUpID + cUpID;
  }else{
    fID = 0;
  }


    if( fID  >= _TotalVerts ){ fID  -= _TotalVerts; }
    if( tri1 >= _TotalVerts ){ tri1 -= _TotalVerts; }
    if( tri2 >= _TotalVerts ){ tri2 -= _TotalVerts; }
    return uint3( fID , tri1 , tri2 );

}
            

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

                varyings o;

                // from getRibbonID 
                uint3 fID = getID( id );
                VertC4 v = buf_Points[fID.x];
                Pos og = og_Points[fID.x];
                float3 tri1 = buf_Points[fID.y].pos;
                float3 tri2 = buf_Points[fID.z].pos;

                float3 nor = normalize(cross( normalize(v.pos - tri1) , normalize(v.pos - tri2)));

                uint4 forNorm = getTubeNormalIDs( id );

                float3 dif = og.pos - v.pos;


                float3 l = buf_Points[forNorm.x].pos;
                float3 r = buf_Points[forNorm.y].pos;
                float3 u = buf_Points[forNorm.z].pos;
                float3 d = buf_Points[forNorm.w].pos;

                /*
                
                float3 l = buf_Points[v.lID].pos;
                float3 r = buf_Points[v.rID].pos;
                float3 u = buf_Points[v.uID].pos;
                float3 d = buf_Points[v.dID].pos;
                
                */

                nor = -normalize( cross( normalize(l - r) , normalize( u - d ) ));

                o.worldPos = v.pos;

                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

                o.debug = v.debug;//normalize( dif );// * nor;//o.worldPos - og.pos;

                o.eye = _WorldSpaceCameraPos - o.worldPos;
                o.uv = v.uv;

                o.nor = v.nor; //nor;// * .5 + .5;//float3(float(fID)/32768., v.uv.x , v.uv.y);
                return o;
            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (varyings i) : COLOR {

                float3 difL = _HandL - i.worldPos;
                float3 difR = _HandR - i.worldPos;

                difL = normalize( difL );
                difR = normalize( difR );


                float3 fNorm = uvNormalMap( _NormalMap , i.pos ,  i.uv  * float2( 1. , .2), i.nor , 20.1 ,.2);

                float3 reflL = reflect( difL , fNorm );
                float matchL = max( 0. , -dot( reflL , normalize(i.eye) )); 
                float3 reflR = reflect( difR , fNorm );
                float matchR = max( 0. , -dot( reflR , normalize(i.eye) ));

                float3 col = (fNorm * .5 +.5) + (difL * .5 +.5) + (difR * .5 +.5);


                float3 semCol = tex2D( _SEMMap , semLookup( i.eye , fNorm) );
                float3 fRefl = reflect( -i.eye , fNorm );
                float3 cubeCol = float3( 1.0 ,1.0 , 1.0 ) - texCUBE(_CubeMap,fRefl ).rgb;


                float3 aCol = tex2D( _AudioMap , float2( length( i.debug) * 4. , 0.)).xyz;

                float fr = dot( fNorm , -i.eye );
               
                col = normalize( col );

                col = float3( matchL , matchL , matchL );

                col  = pow( matchR , 11 ) * (reflR * .5 + .5 );
                col += pow( matchL , 11 ) * (reflL * .5 + .5 );
                //col += (fNorm * .5 + .5);
                //col = (fNorm * .5 + .5);


                //float3 c = getGridDiscard( i.uv , cubeCol );

                float skyboxVal = length( cubeCol ) / 1.;

                float3 fCol = pow( cubeCol  , 1. ) * 1.;

                fCol += aCol; //tex2D( _AudioMap , float2(i.uv.y, 0. ));

                //fCol = i.nor + ;

               fCol =  i.nor * .5 + .5;
               //fCol *= aCol * 2.;
               //fCol =  length( i.debug ) * length( i.debug ) * (i.nor * .5 + .5) * cubeCol ;

                return float4( fCol , 1.);//.1 * float4(1,0.5f,0.0f,1.0) / ( i.dToPoint * i.dToPoint  * i.dToPoint );
            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
	
}

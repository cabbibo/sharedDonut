// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/prism" {
 Properties {
  

    _NumberSteps( "Number Steps", Int ) = 20
    _MaxTraceDistance( "Max Trace Distance" , Float ) = 10.0
    _IntersectionPrecision( "Intersection Precision" , Float ) = 0.0001
    _NumberTexture( "NumberTexture" , 2D ) = "white" {}


  }
  
  SubShader {
   // Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

    Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
    LOD 200

    Pass {
      //Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
      //ZTest Off
      //ZWrite On

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0
      #pragma multi_compile_fog
      #pragma multi_compile_fwdbase
      // Use shader model 3.0 target, to get nicer looking lighting
      //#pragma target 3.0

      #include "UnityCG.cginc"
      #include "Chunks/noise.cginc"
      #include "UnityShaderVariables.cginc"

      #define UNITY_PASS_FORWARDBASE
      #define IF(a, b, c) lerp(b, c, step((fixed) (a), 0));

      
      uniform int   _NumberSteps;
      uniform float _IntersectionPrecision;
      uniform float _MaxTraceDistance;

			#include "Chunks/VertexIn.cginc"
			#include "Chunks/VertexOut.cginc"

      struct FragOut
        {
            half4 color : SV_Target ;
            //float depth : SV_Depth ;
        };
        

     	 	#define PI 3.14159265
				#define TAU (2*PI)
				#define PHI (1.618033988749895)

				// Shortcut for 45-degrees rotation
				void pR45(inout float2 p) {
				p = (p + float2(p.y, -p.x))*sqrt(0.5);
				}


				// "Generalized Distance Functions" by Akleman and Chen.
				// see the Paper at https://www.viz.tamu.edu/faculty/ergun/research/implicitmodeling/papers/sm99.pdf
				// Macro based version for GLSL 1.2 / ES 2.0 by Tom

				#define GDFVector0 float3(1, 0, 0)
				#define GDFVector1 float3(0, 1, 0)
				#define GDFVector2 float3(0, 0, 1)

				#define GDFVector3 normalize(float3(1, 1, 1 ))
				#define GDFVector4 normalize(float3(-1, 1, 1))
				#define GDFVector5 normalize(float3(1, -1, 1))
				#define GDFVector6 normalize(float3(1, 1, -1))

				#define GDFVector7 normalize(float3(0, 1, PHI+1.))
				#define GDFVector8 normalize(float3(0, -1, PHI+1.))
				#define GDFVector9 normalize(float3(PHI+1., 0, 1))
				#define GDFVector10 normalize(float3(-PHI-1., 0, 1))
				#define GDFVector11 normalize(float3(1, PHI+1., 0))
				#define GDFVector12 normalize(float3(-1, PHI+1., 0))

				#define GDFVector13 normalize(float3(0, PHI, 1))
				#define GDFVector14 normalize(float3(0, -PHI, 1))
				#define GDFVector15 normalize(float3(1, 0, PHI))
				#define GDFVector16 normalize(float3(-1, 0, PHI))
				#define GDFVector17 normalize(float3(PHI, 1, 0))
				#define GDFVector18 normalize(float3(-PHI, 1, 0))

				#define fGDFBegin float d = 0.;

				// Version with variable exponent.
				// This is slow and does not produce correct distances, but allows for bulging of objects.
				#define fGDFExp(v) d += pow(abs(dot(p, v)), e);

				// Version with without exponent, creates objects with sharp edges and flat faces
				#define fGDF(v) d = max(d, abs(dot(p, v)));

				#define fGDFExpEnd return pow(d, 1./e) - r;
				#define fGDFEnd return d - r;

				float fOctahedron(float3 p, float r) {
					fGDFBegin
					fGDF(GDFVector3) fGDF(GDFVector4) fGDF(GDFVector5) fGDF(GDFVector6)
					fGDFEnd
				}


      float sdBox( float3 p, float3 b ){

        float3 d = abs(p) - b;

        return min(max(d.x,max(d.y,d.z)),0.0) +
               length(max(d,0.0));

      }

      float sdSphere( float3 p, float s ){
        return length(p)-s;
      }

      float sdCapsule( float3 p, float3 a, float3 b, float r )
      {
          float3 pa = p - a, ba = b - a;
          float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
          return length( pa - ba*h ) - r;
      }

      float2 smoothU( float2 d1, float2 d2, float k)
      {
          float a = d1.x;
          float b = d2.x;
          if( k == 0 ){ k = 0.0000001; }
          float h = clamp(0.5+0.5*(b-a)/k, 0.0, 1.0);
          return float2( lerp(b, a, h) - k*h*(1.0-h), lerp(d2.y, d1.y, pow(h, 2.0)));
      }

      
      float3 modit(float3 x, float3 m) {
          float3 r = x%m;
          return r<0 ? r+m : r;
      }

			float opS( float d1, float d2 )
			{
			    return max(-d1,d2);
			}

      // From Inigo Quilez
			float sdTriPrism( float3 p, float2 h ){
			    float3 q = abs(p);
			    return max(q.z-h.y,max(q.x*0.866025+p.y*0.5,-p.y)-h.x*0.5);
			}

			float sdPlane( float3 p, float4 n ){
			  // n must be normalized
			  return dot(p,n.xyz) + n.w;
			}


    
      float2 map( in float3 pos ){
        
        float2 res;

        res = float2( fOctahedron( pos - float3( 0. , -.5 , 0. ) , .55 ), 0.6 );
        res.x = opS( sdPlane( pos , float4( 0 , 1 , 0 , .5 ) ), res.x );

        float n = noise( pos * (10. +sin( _Time.x * 20.) ) + float3( _SinTime.x , _SinTime.y , _SinTime.z ) );

        res.x += n * .3;
        
        return res; 
     
      }

      #include "Chunks/calcNormal.cginc"
      #include "Chunks/noise.cginc"
   
         

      float2 calcIntersection( in float3 ro , in float3 rd ){     
            
               
        float h =  _IntersectionPrecision * 2;
        float t = 0.0;
        float res = -1.0;
        float id = -1.0;
        
        [unroll(20)] for( int i=0; i< 20; i++ ){
            
            if( h < _IntersectionPrecision || t > _MaxTraceDistance ) break;
    
            float3 pos = ro + rd*t;
            float2 m = map( pos );
            
            h = m.x;
            t += h;
            id = m.y;
            
        }
    
    
        if( t <  _MaxTraceDistance ){ res = t; }
        if( t >  _MaxTraceDistance ){ id = -1.0; }
        
        return float2( res , id );
          
      
      }
            
    

      VertexOut vert(VertexIn v) {
        
        VertexOut o;

        o.normal = v.normal;
        
        o.uv = v.texcoord;
  
        // Getting the position for actual position
        o.pos = mul( UNITY_MATRIX_MVP , v.position );
     
        float3 mPos = mul( unity_ObjectToWorld , v.position );

        o.ro = v.position;
        o.camPos = mul( unity_WorldToObject , float4( _WorldSpaceCameraPos  , 1. )); 

        return o;

      }


     // Fragment Shader
      FragOut frag(VertexOut i) {

        FragOut o;

        float3 ro = i.ro;
        float3 rd = normalize(ro - i.camPos);

        float3 col = float3( 0.0 , 0.0 , 0.0 );
        float2 res = calcIntersection( ro , rd );
        
        col= float3( 0. , 0. , 0. );

        float depth = 1.;

        if( res.y > -0.5 ){

          float3 pos = ro + rd * res.x;
          float3 nor = calcNormal( pos );
          
          
          nor = mul(  nor, (float3x3)unity_WorldToObject ); 
          nor = normalize( nor );
          col = nor * .5 + .5;

          col = dot( nor ,rd );//float3( .1 , .1 , .1 );

          //float n = noise( pos * 10 );
          //if( n < 0.5 ){ discard; }

          float4 wPos = mul( unity_ObjectToWorld , float4( pos.x , pos.y , pos.z , 1. ) );

          //depth = (log(c * wPos.z + 1) / log(c * far + 1) * wPos.w);

         // float depthWithOffset=wPos.w-.9;

          //convert position to clip space, read depth
          float4 clip_pos = mul(UNITY_MATRIX_VP, float4(wPos.xyz, 1.0));
          float depthWithOffset = clip_pos.z / clip_pos.w;

          depth = depthWithOffset +.4;

          if( res.y >= 1. ){
            col *= res.y - 1;
          }
          
        }else{
          col = float3( 0,0,0);//discard;
        }

        fixed4 color;
        color = fixed4(col, 1. );

        o.color = color;
       // o.depth = depth;
        return o;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}
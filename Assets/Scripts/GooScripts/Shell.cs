using UnityEngine;
 
//This game object invokes PlaneComputeShader (when attached via drag'n drop in the editor) using the PlaneBufferShader (also attached in the editor)
//to display a grid of points moving back and forth along the z axis.
public class Shell : MonoBehaviour
{
    public Shader shader;
    public ComputeShader computeShader;

    public GameObject[] Hands; 
    private int numberHands{ get { return Hands.Length; } }

    public SlugInterface uniformInterface;

    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _ogBuffer;
    private ComputeBuffer _transBuffer;
    private ComputeBuffer _disformBuffer;
    private ComputeBuffer _handBuffer; 

    public const int threadX = 8;
    public const int threadY = 8;
    public const int threadZ = 8;

    public const int strideX = 8 ;
    public const int strideY = 8 ;
    public const int strideZ = 8 ;

    public const int numDisformers = 20;

    public float tubeRadius = .6f;
    public float shellRadius = .8f;

    public int ribbonWidth = 256;


    public GameObject handL;
    public GameObject handR;
    public GameObject audioObj;
    public GameObject handPosL;
    public GameObject handPosR;
    public GameObject DisformerPrefab;
    public GameObject Mini;

    public Texture2D normalMap;
    public Cubemap cubeMap;

    /*
        
        float3 pos 
        float3 vel
        float3 nor
        float2 uv
        float  ribbonID
        float  life 
        float3 debug

    */




    private int gridX { get { return threadX * strideX; } }
    private int gridY { get { return threadY * strideY; } }
    private int gridZ { get { return threadZ * strideZ; } }

    private int vertexCount { get { return gridX * gridY * gridZ; } }



    private int ribbonLength { get { return (int)Mathf.Floor( (float)vertexCount / ribbonWidth ); } }

    private int _kernel;
    private Material material;

    private Vector3 p1;
    private Vector3 p2;

    private Texture2D audioTexture;

    private float[] transValues = new float[32];
    private float[] disformValues = new float[3 * numDisformers];
    private float[] handValues;

    private GameObject[] Disformers; 




 
    //We initialize the buffers and the material used to draw.
    void Start (){

      handValues = new float[numberHands * AssignStructs.HandStructSize];

      uniformInterface = GetComponent<SlugInterface>();

      createDisformers();
      createBuffers();
      createMaterial();

      _kernel = computeShader.FindKernel("CSMain");

      PostRenderEvent.PostRender += Render;

    }
 
    //When this GameObject is disabled we must release the buffers or else Unity complains.
    private void OnDisable(){
      ReleaseBuffer();
    }
 
    //After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
    //this just draws the "mesh" as a set of points
    private void Render() {
     
      Dispatch();

      int numVertsTotal = ribbonWidth * 3 * 2 * (ribbonLength);
      
      material.SetPass(0);

      material.SetBuffer("buf_Points", _vertBuffer);
      material.SetBuffer("og_Points", _ogBuffer);
      material.SetInt("_RibbonWidth" , ribbonWidth);
      material.SetInt("_RibbonLength" , ribbonLength);
      material.SetInt("_TotalVerts" ,vertexCount);
      material.SetTexture("_AudioMap", audioTexture);
      material.SetVector( "_HandL",handPosL.transform.position);      
      material.SetVector( "_HandR",handPosR.transform.position); 
      material.SetTexture( "_NormalMap" , normalMap );
      material.SetTexture("_CubeMap" , cubeMap ); 
      material.SetInt( "_Mini" , 0 );
      material.SetMatrix("worldMat", transform.localToWorldMatrix);
      material.SetMatrix("invWorldMat", transform.worldToLocalMatrix);
      material.SetMatrix("miniMat", Mini.transform.localToWorldMatrix);

      uniformInterface.SetRenderUniforms( material );

      Graphics.DrawProcedural(MeshTopology.Triangles, numVertsTotal);

      material.SetPass(0);

      material.SetBuffer("buf_Points", _vertBuffer);
      material.SetBuffer("og_Points", _ogBuffer);
      material.SetInt("_RibbonWidth" , ribbonWidth);
      material.SetInt("_RibbonLength" , ribbonLength);
      material.SetInt("_TotalVerts" ,vertexCount);
      material.SetTexture("_AudioMap", audioTexture);
      material.SetVector( "_HandL",handPosL.transform.position);      
      material.SetVector( "_HandR",handPosR.transform.position); 
      material.SetTexture( "_NormalMap" , normalMap );
      material.SetTexture("_CubeMap" , cubeMap ); 
      material.SetInt( "_Mini" , 1 );
      material.SetMatrix("worldMat", transform.localToWorldMatrix);
      material.SetMatrix("invWorldMat", transform.worldToLocalMatrix);
      material.SetMatrix("miniMat", Mini.transform.localToWorldMatrix);


      uniformInterface.SetRenderUniforms( material );

  
      Graphics.DrawProcedural(MeshTopology.Triangles, numVertsTotal);





    }

    private Vector3 getVertPosition( float uvX , float uvY  ){

     float u = uvY * 2.0f * Mathf.PI;
     float v = uvX * 2.0f * Mathf.PI;

     float largeMovement = Mathf.Sin( uvY * 10.0f ) * .3f;
     float smallMovement = Mathf.Sin( uvY * 100.0f )  * ( uvY * uvY * .03f);
     float tubeRad = .2f; //tubeRadius * Mathf.Pow( uvY - .01f , .3f)  * ( 1.0f + largeMovement + smallMovement ) ;
     float slideRad = .8f;//shellRadius / 2.0f + uvY;

     float xV = (slideRad + tubeRad * Mathf.Cos(v)) * Mathf.Cos(u) ;
     float zV = (slideRad + tubeRad * Mathf.Cos(v)) * Mathf.Sin(u) ;
     float yV = (tubeRad) * Mathf.Sin(v) + tubeRad;

     //print( xV );
     return new Vector3( xV , yV , zV );

   }

   /*

  Vector3 cubicCurve( float t , Vector3  c0 , Vector3 c1 , Vector3 c2 , Vector3 c3 ){
  
    float s  = 1.0f - t; 

    Vector3 v1 = c0 * ( s * s * s );
    Vector3 v2 = 3.0f * c1 * ( s * s ) * t;
    Vector3 v3 = 3.0f * c2 * s * ( t * t );
    Vector3 v4 = c3 * ( t * t * t );

    Vector3 value = v1 + v2 + v3 + v4;

    return value;

  }



Vector3 cubicFromValue( float val , out Vector3 upPos , out Vector3 doPos ){

  //float3 upPos;
  //float3 doPos;


  Vector3 p0 = new Vector3( 0. , 0. , 0. );
  Vector3 v0 = new Vector3( 0. , 0. , 0. );
  Vector3 p1 = new Vector3( 0. , 0. , 0. );
  Vector3 v1 = new Vector3( 0. , 0. , 0. );

  Vector3 p2 = new Vector3( 0. , 0. , 0. );



  float base = val * (float(pointsLength)-1.);
  float baseUp   = floor( base );
  float baseDown = ceil( base );
  float amount = base - baseUp;

  if( baseUp == 0. ){

      p0 = pointsBuffer[ int( baseUp )        ];
      p1 = pointsBuffer[ int( baseDown )      ];
      p2 = pointsBuffer[ int( baseDown + 1. ) ];


      v1 = .5 * ( p2 - p0 );

  }else if( baseDown == float(_PointsLength-1) ){

      p0 = pointsBuffer[ int( baseUp )      ];
      p1 = pointsBuffer[ int( baseDown )    ];
      p2 = pointsBuffer[ int( baseUp - 1. ) ];

      v0 = .5 * ( p1 - p2 );

  }else{

      p0 = pointsBuffer[ int( baseUp )   ];
      p1 = pointsBuffer[ int( baseDown ) ];


      Vector3 pMinus;

      pMinus = pointsBuffer[ int( baseUp - 1. )   ];
      p2 =     pointsBuffer[ int( baseDown + 1. ) ];

      v1 = .5 * ( p2 - p0 );
      v0 = .5 * ( p1 - pMinus );

  }


  Vector3 c0 = p0;
  Vector3 c1 = p0 + v0/3.;
  Vector3 c2 = p1 - v1/3.;
  Vector3 c3 = p1;




  Vector3 pos = cubicCurve( amount , c0 , c1 , c2 , c3 );

  upPos = cubicCurve( amount  + .01 , c0 , c1 , c2 , c3 );
  doPos = cubicCurve( amount  - .01 , c0 , c1 , c2 , c3 );

  return pos;


}*/


    /*private Vector3 getVertPosition( float uvX , float uvY  ){

      float u = uvY * 2.0f * Mathf.PI;
      float v = uvX * 2.0f * Mathf.PI;

      float largeMovement = Mathf.Sin( uvY * 10.0f ) * .3f;
      float smallMovement = Mathf.Sin( uvY * 100.0f )  * ( uvY * uvY * .03f);
      float tubeRad = tubeRadius *  .2f ;
      float slideRad = shellRadius / 2.0f ;

      //float xV = (slideRad + tubeRad * Mathf.Cos(v)) * Mathf.Cos(u);
      //float zV = (slideRad + tubeRad * Mathf.Cos(v)) * Mathf.Sin(u);
      //float yV = (tubeRad) * Mathf.Sin(v) + tubeRad + uvY + .5f;//+ ( .2f * u );

      float rad = 2.5f *(.5f - Mathf.Abs(uvY - .5f));
      float yV = Mathf.Sin( v ) * rad + rad ;
      float xV = Mathf.Cos( v ) * rad + Mathf.Pow(Mathf.Abs( uvY - .5f ) , 2.0f)* 10.0f;
      float zV = uvY * 3.0f - 1.5f;

      //print( xV );
      return new Vector3( xV , yV , zV );

    }*/


    // Creates the objects that we will use to disform the body of the tube
    private void createDisformers(){

      Disformers = new GameObject[numDisformers];

      _disformBuffer = new ComputeBuffer( numDisformers ,  3 * sizeof(float));
      float[] disValues = new float[ 3 * numDisformers];
      for( int i = 0; i < numDisformers; i++ ){

        float x = Random.Range( 0.01f , .99f );
        float y = Random.Range( 0.01f , .99f );

        Vector3 fPos = getVertPosition( x , y );
        fPos *= 1.0f;

   
        Vector3 pos = transform.TransformPoint( fPos );
        Disformers[i] = (GameObject) Instantiate( DisformerPrefab, pos , new Quaternion());

        Disformers[i].GetComponent<MeshRenderer>().material.SetFloat("_audioID" , (float)i/numDisformers);
        Disformers[i].GetComponent<MeshRenderer>().material.SetFloat("_NumDisformers" , numDisformers);
        Disformers[i].GetComponent<setAudioSourceTexture>().sourceObj = audioObj;
        Disformers[i].transform.parent = transform;


      }



    }




    private void createBuffers() {

      _vertBuffer = new ComputeBuffer( vertexCount ,  AssignStructs.VertC4StructSize * sizeof(float));
      _ogBuffer = new ComputeBuffer( vertexCount ,  3 * sizeof(float));
      _transBuffer = new ComputeBuffer( 32 ,  sizeof(float));
      _handBuffer = new ComputeBuffer( numberHands , AssignStructs.HandStructSize * sizeof(float));
      

      float[] inValues = new float[ AssignStructs.VertC4StructSize * vertexCount];
      float[] ogValues = new float[ 3         * vertexCount];

      int index = 0;
      int indexOG = 0;


      for (int z = 0; z < gridZ; z++) {
        for (int y = 0; y < gridY; y++) {
          for (int x = 0; x < gridX; x++) {

            int id = x + y * gridX + z * gridX * gridY; 
            
            float col = (float)(id % ribbonWidth );
            float row = Mathf.Floor( ((float)id+.01f) / ribbonWidth);


            float uvX = col / ribbonWidth;
            float uvY = row / ribbonLength;

            Vector3 fVec = getVertPosition( uvX , uvY );


            //pos
            ogValues[indexOG++] = fVec.x;
            ogValues[indexOG++] = fVec.y;
            ogValues[indexOG++] = fVec.z;

            AssignStructs.VertC4 vert = new AssignStructs.VertC4();


            vert.pos = fVec * .9f;
            vert.vel = new Vector3( 0 , 0 , 0 );
            vert.nor = new Vector3( 0 , 1 , 0 );
            vert.uv  = new Vector2( uvX , uvY );
            vert.ribbonID = 0;
            vert.life = -1;
            vert.debug = new Vector3( 0 , 1 , 0 );
            vert.row   = row; 
            vert.col   = col; 

            vert.lID = convertToID( col - 1 , row + 0 );
            vert.rID = convertToID( col + 1 , row + 0 );
            vert.uID = convertToID( col + 0 , row + 1 );
            vert.dID = convertToID( col + 0 , row - 1 );

            AssignStructs.AssignVertC4Struct( inValues , index , out index , vert );

          }
        }
      }

      _vertBuffer.SetData(inValues);
      _ogBuffer.SetData(ogValues);

    }

    private float convertToID( float col , float row ){

      float id;

      if( col >= ribbonWidth ){ col -= ribbonWidth; }
      if( col < 0 ){ col += ribbonWidth; }

      if( row >= ribbonLength ){ row -= ribbonLength; }
      if( row < 0 ){ row += ribbonLength; }


      id = row * ribbonWidth + col;

      return id;

    }

 
    //For some reason I made this method to create a material from the attached shader.
    private void createMaterial(){

      material = new Material( shader );

    }
 
    //Remember to release buffers and destroy the material when play has been stopped.
    void ReleaseBuffer(){

      _vertBuffer.Release(); 
      _ogBuffer.Release(); 
      _transBuffer.Release(); 
      DestroyImmediate( material );

    }


    private void Dispatch() {

      AssignStructs.AssignTransBuffer( transform , transValues , _transBuffer );
      AssignStructs.AssignDisformerBuffer( Disformers , disformValues , _disformBuffer );
      AssignStructs.AssignHandBuffer( Hands , handValues , _handBuffer );

      computeShader.SetInt( "_NumDisformers", numDisformers );
      computeShader.SetInt( "_NumberHands", Hands.Length );

      computeShader.SetFloat( "_DeltaTime"    , Time.deltaTime );
      computeShader.SetFloat( "_Time"         , Time.time      );
      computeShader.SetInt( "_RibbonWidth"  , ribbonWidth    );
      computeShader.SetInt( "_RibbonLength"  , ribbonLength    );

      uniformInterface.SetComputeUniforms( computeShader );

      audioTexture = audioObj.GetComponent<audioSourceTexture>().AudioTexture;

      computeShader.SetTexture(_kernel,"_Audio", audioTexture);

      computeShader.SetBuffer( _kernel , "transBuffer"  , _transBuffer    );
      computeShader.SetBuffer( _kernel , "vertBuffer"   , _vertBuffer     );
      computeShader.SetBuffer( _kernel , "ogBuffer"     , _ogBuffer       );
      computeShader.SetBuffer( _kernel , "disformBuffer", _disformBuffer  );
      computeShader.SetBuffer( _kernel , "handBuffer"   , _handBuffer     );

      computeShader.Dispatch(_kernel, strideX , strideY , strideZ );



    }

}
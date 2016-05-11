using UnityEngine;
 
//This game object invokes PlaneComputeShader (when attached via drag'n drop in the editor) using the PlaneBufferShader (also attached in the editor)
//to display a grid of points moving back and forth along the z axis.
public class Dough : MonoBehaviour
{
    


    public GameObject prism;
    public GameObject pedestal;



    public GameObject handBufferInfo; 

    public Shader shader;
    public Shader prismShader;

    private Material material;
    private Material prismMaterial;


    public ComputeShader computeShader;

    private bool activated = false;
    private float activationTime = 0.0f;
    private float activeVal = 0; // 0 -> 1  for fade in / fade out ! 
    private bool deactivating = false;
    private bool activating = false;
    private float deactivationTime = 0;

    private float activatingTime = 5;
    private float deactivatingTime = 5;

    public GameObject DisformerPrefab;

    private DonutInterface uniformInterface;

    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _ogBuffer;
    private ComputeBuffer _transBuffer;
    private ComputeBuffer _disformBuffer;

    private DonutInfo info;

    private AudioSource audio;

    private Transform miniTransform;


    

    private const int threadX = 6;
    private const int threadY = 6;
    private const int threadZ = 6;

    private const int strideX = 6 ;
    private const int strideY = 6 ;
    private const int strideZ = 6 ;

    public const int numDisformers = 20;

    public float tubeRadius = .6f;
    public float shellRadius = .8f;

    public int ribbonWidth = 256;





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


    private Vector3 p1;
    private Vector3 p2;

    private Texture2D audioTexture;

    private float[] transValues = new float[32];
    private float[] disformValues = new float[3 * numDisformers];
    private float[] handValues;

    private GameObject[] Disformers; 


  

 
    //We initialize the buffers and the material used to draw.
    void Start (){


      prism.GetComponent<Prism>().puppy = gameObject;

      uniformInterface =       GetComponent<DonutInterface>();
      info             = prism.GetComponent<DonutInfo>();

      audio = pedestal.GetComponent<AudioSource>();

      GameObject o = new GameObject();

      miniTransform = o.transform; //new Transform();
      miniTransform.position = prism.transform.position;
      miniTransform.rotation = prism.transform.rotation;
      miniTransform.localScale = prism.transform.localScale;
      miniTransform.localScale *= .2f;

      miniTransform.parent = prism.transform;

      //uniformInterface.SetDonutInfo( gameObject );

      createDisformers();
      createBuffers();
      createMaterial();

      _kernel = computeShader.FindKernel("CSMain");

      PostRenderEvent.PostRender += Render;

      // Hit it w/ a dispatch first time
      Dispatch();


    }

    public void select(){

      
      activated = true;
      activationTime = Time.time;
      activating = true;

      uniformInterface.SetDonutInfo( prism );
      
      audio.volume = 0;
      audio.clip = info.clip;
      audio.Play();

    }

    public void deselect(){
      deactivating = true;
      deactivationTime = Time.time;
    }
 
    //When this GameObject is disabled we must release the buffers or else Unity complains.
    private void OnDisable(){
      ReleaseBuffer();
    }


    private void FixedUpdate(){

      if( activated == true ){

        

        if( activating == true ){
          activeVal = (Time.time - activationTime)  / activatingTime;
          audio.volume = activeVal;
          print( activeVal );

          if( activeVal > 1 ){
            activating  = false;
            activeVal = 1;
            audio.volume = 1;
          }
        }

        if( deactivating == true ){
          activeVal = 1.0f - ((Time.time - deactivationTime)  / deactivatingTime);
          audio.volume = activeVal;
          print( activeVal );

          if( activeVal < 0 ){
            deactivating  = false;
            activeVal = 0;
            audio.volume = 0;
            audio.Stop();
            activated = false;
          }
        }

        Dispatch();
      }

    }
 
    //After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
    //this just draws the "mesh" as a set of points
    private void Render() {


      int numVertsTotal = ribbonWidth * 3 * 2 * (ribbonLength);
      

      // Only computes physics and does big render
      // if the object is activated!!!
      if( activated == true ){
     
        
        
        
        material.SetPass(0);

        material.SetBuffer("buf_Points", _vertBuffer);
        material.SetBuffer("og_Points", _ogBuffer);

        material.SetInt("_Mini"   , 1);


        material.SetInt("_RibbonWidth" , ribbonWidth);
        material.SetInt("_RibbonLength" , ribbonLength);
        material.SetInt("_TotalVerts" ,vertexCount);
        material.SetTexture("_AudioMap", audioTexture);
        material.SetTexture( "_NormalMap" , info._NormalMap );
        material.SetTexture("_CubeMap" , info._CubeMap ); 

        material.SetMatrix("worldMat", transform.localToWorldMatrix);
        material.SetMatrix("invWorldMat", transform.worldToLocalMatrix);
        prismMaterial.SetMatrix("miniMat", miniTransform.localToWorldMatrix );

        uniformInterface.SetRenderUniforms( material );



        Graphics.DrawProcedural(MeshTopology.Triangles, numVertsTotal);



        // Draws prism lines only when activated
        prismMaterial.SetPass(0);

        prismMaterial.SetBuffer("buf_Points", _vertBuffer);
        prismMaterial.SetBuffer("og_Points", _ogBuffer);

        prismMaterial.SetInt("_RibbonWidth" , ribbonWidth);
        prismMaterial.SetInt("_RibbonLength" , ribbonLength);
        prismMaterial.SetInt("_TotalVerts"   , vertexCount);

        prismMaterial.SetTexture("_AudioMap", audioTexture);


        prismMaterial.SetMatrix("worldMat", transform.localToWorldMatrix);
        prismMaterial.SetMatrix("invWorldMat", transform.worldToLocalMatrix);
        prismMaterial.SetMatrix("miniMat", miniTransform.localToWorldMatrix );

        prismMaterial.SetVector( "_PrismPosition",prism.transform.position);

        prismMaterial.SetFloat( "_Active" , activeVal );       
 
        prismMaterial.SetFloat( "_NoiseSize" , prism.GetComponent<Prism>()._NoiseSize );


        Graphics.DrawProcedural( MeshTopology.Lines, numVertsTotal / 42 );


      
        // Draws minature version all the time
        material.SetPass(0);

        material.SetBuffer("buf_Points", _vertBuffer);
        material.SetBuffer("og_Points", _ogBuffer);

        material.SetInt("_Mini"   , 0);

        material.SetInt("_RibbonWidth" , ribbonWidth);
        material.SetInt("_RibbonLength" , ribbonLength);
        material.SetInt("_TotalVerts"   , vertexCount);
        
        material.SetTexture("_AudioMap", audioTexture);
        material.SetTexture( "_NormalMap" , info._NormalMap );
        material.SetTexture("_CubeMap" , info._CubeMap ); 

        material.SetMatrix("worldMat", transform.localToWorldMatrix);
        material.SetMatrix("invWorldMat", transform.worldToLocalMatrix);
        material.SetMatrix("miniMat", miniTransform.localToWorldMatrix );


        uniformInterface.SetRenderUniforms(  material );

        Graphics.DrawProcedural( MeshTopology.Triangles, numVertsTotal / 256 );
        
    }





      



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
       
        Disformers[i].transform.parent = transform;

     
        Disformers[i].GetComponent<MeshRenderer>().enabled = false;

      }



    }




    private void createBuffers() {

      _vertBuffer = new ComputeBuffer( vertexCount ,  AssignStructs.VertC4StructSize * sizeof(float));
      _ogBuffer = new ComputeBuffer( vertexCount ,  3 * sizeof(float));
      _transBuffer = new ComputeBuffer( 32 ,  sizeof(float));
      

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
      prismMaterial = new Material( prismShader );

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

      computeShader.SetInt( "_NumDisformers", numDisformers );
      computeShader.SetInt( "_NumberHands", handBufferInfo.GetComponent<HandBuffer>().numberHands );

      computeShader.SetFloat( "_DeltaTime"    , Time.deltaTime );
      computeShader.SetFloat( "_Time"         , Time.time      );
      computeShader.SetInt( "_RibbonWidth"  , ribbonWidth    );
      computeShader.SetInt( "_RibbonLength"  , ribbonLength    );

      uniformInterface.SetComputeUniforms( computeShader );

      audioTexture = pedestal.GetComponent<audioSourceTexture>().AudioTexture;

      computeShader.SetTexture(_kernel,"_Audio", audioTexture);

      computeShader.SetBuffer( _kernel , "transBuffer"  , _transBuffer    );
      computeShader.SetBuffer( _kernel , "vertBuffer"   , _vertBuffer     );
      computeShader.SetBuffer( _kernel , "ogBuffer"     , _ogBuffer       );
      computeShader.SetBuffer( _kernel , "disformBuffer", _disformBuffer  );
      computeShader.SetBuffer( _kernel , "handBuffer"   , handBufferInfo.GetComponent<HandBuffer>()._handBuffer );

      computeShader.Dispatch(_kernel, strideX , strideY , strideZ );



    }

}
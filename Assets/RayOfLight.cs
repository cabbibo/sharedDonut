using UnityEngine;
using System.Collections;

public class RayOfLight : MonoBehaviour {



  public GameObject handBufferInfo; 

  public Shader shader;
  public ComputeShader computeShader;


  private Material material;

  private Pedestal pedestal;

  private const int threadX = 4;
  private const int threadY = 4;
  private const int threadZ = 4;

  private const int strideX = 4;
  private const int strideY = 4;
  private const int strideZ = 4;

  private int gridX { get { return threadX * strideX; } }
  private int gridY { get { return threadY * strideY; } }
  private int gridZ { get { return threadZ * strideZ; } }

  private int vertexCount { get { return gridX * gridY * gridZ; } }

  private int _kernel;

  private float[] transValues = new float[32];


  private ComputeBuffer _vertBuffer;
  private ComputeBuffer _ogBuffer;
  private ComputeBuffer _transBuffer;

  private AudioSource audio;
  private Texture2D audioTexture;


	// Use this for initialization
	void Start () {

    pedestal = GetComponent<Pedestal>();

    createBuffers();
    createMaterial();

    _kernel = computeShader.FindKernel("CSMain");

    PostRenderEvent.PostRender += Render;
	
	}
	
	// Update is called once per frame
	void Update () {


	}

  private void FixedUpdate(){

    Dispatch();

  }

  private void createBuffers() {

      _vertBuffer = new ComputeBuffer( vertexCount ,  AssignStructs.VertStructSize * sizeof(float));
      _ogBuffer = new ComputeBuffer( vertexCount ,  3 * sizeof(float));
      _transBuffer = new ComputeBuffer( 32 ,  sizeof(float));
      

      float[] inValues = new float[ AssignStructs.VertStructSize * vertexCount];
      float[] ogValues = new float[ 3         * vertexCount];

      int index = 0;
      int indexOG = 0;


      for (int z = 0; z < gridZ; z++) {
        for (int y = 0; y < gridY; y++) {
          for (int x = 0; x < gridX; x++) {

            int id = x + y * gridX + z * gridX * gridY; 

            Vector3 fVec = new Vector3( Random.value , Random.value , Random.value );

            //pos
            ogValues[indexOG++] = fVec.x;
            ogValues[indexOG++] = fVec.y;
            ogValues[indexOG++] = fVec.z;

            AssignStructs.Vert vert = new AssignStructs.Vert();


            vert.pos = fVec;
            vert.vel = new Vector3( 0 , 0 , 0 );
            vert.nor = new Vector3( 0 , 1 , 0 );
            vert.uv  = new Vector2( 0 , 0 );
            vert.ribbonID = 0;
            vert.life = Random.value;
            vert.debug = new Vector3( 0 , 1 , 0 );


            AssignStructs.AssignVertStruct( inValues , index , out index , vert );

          }
        }
      }

      _vertBuffer.SetData(inValues);
      _ogBuffer.SetData(ogValues);

    }

  //For some reason I made this method to create a material from the attached shader.
  private void createMaterial(){

    material = new Material( shader );

  }
 
  //When this GameObject is disabled we must release the buffers or else Unity complains.
  private void OnDisable(){
    ReleaseBuffer();
  }

  //Remember to release buffers and destroy the material when play has been stopped.
  void ReleaseBuffer(){

    _vertBuffer.Release(); 
    _ogBuffer.Release(); 
    _transBuffer.Release(); 
    DestroyImmediate( material );

  }


  private void Render(){


    material.SetPass(0);

    material.SetBuffer("buf_Points", _vertBuffer);
    material.SetBuffer("og_Points", _ogBuffer);

    material.SetMatrix("worldMat", transform.localToWorldMatrix);
    material.SetMatrix("invWorldMat", transform.worldToLocalMatrix);

    

    Graphics.DrawProcedural(MeshTopology.Triangles, vertexCount * 18 );


  }

  private void Dispatch(){
    
    AssignStructs.AssignTransBuffer( transform , transValues , _transBuffer );

    computeShader.SetInt( "_NumberHands", handBufferInfo.GetComponent<HandBuffer>().numberHands );

    computeShader.SetFloat( "_DeltaTime"    , Time.deltaTime );
    computeShader.SetFloat( "_Time"         , Time.time      );

    computeShader.SetVector( "_Position" , transform.position );

    audioTexture = pedestal.GetComponent<audioSourceTexture>().AudioTexture;

    computeShader.SetTexture(_kernel,"_Audio", audioTexture);

    computeShader.SetBuffer( _kernel , "transBuffer"  , _transBuffer    );
    computeShader.SetBuffer( _kernel , "vertBuffer"   , _vertBuffer     );
    computeShader.SetBuffer( _kernel , "ogBuffer"     , _ogBuffer       );
    computeShader.SetBuffer( _kernel , "handBuffer"   , handBufferInfo.GetComponent<HandBuffer>()._handBuffer );

    computeShader.Dispatch(_kernel, strideX , strideY , strideZ );

  }

  public void startAttracting( GameObject prism ){}
  public void stopAttracting( GameObject prism ){}
}

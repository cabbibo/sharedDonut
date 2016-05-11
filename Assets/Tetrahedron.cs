using UnityEngine;
using System.Collections;

// from https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/
public class Tetrahedron : MonoBehaviour {

  public int[] newTriangles;




  void Start() {



      Mesh mesh = new Mesh();

      //Vector3 p0 = new Vector3(0,0,1);
      //Vector3 p1 = new Vector3(.943f,0,-.333f);
      //Vector3 p2 = new Vector3(-.471f,0.816f,-.333f);
      //Vector3 p3 = new Vector3(-.471f,-0.816f,-.333f);
      //Vector3 p2 = new Vector3(0.0f,0,Mathf.Sqrt(0.75f) /2.0);
      //Vector3 p3 = new Vector3(0.0f,Mathf.Sqrt(0.75f),Mathf.Sqrt(0.75f)/3);
      //  x= 0.000, y= 0.000, z= 1.000 1, x= 0.943, y= 0.000, z=-0.333 2, x=-0.471, y= 0.816, z=-0.333 3, x=-0.471, y=-0.816, z=-0.333

      Vector3 p0 = new Vector3( 0 , 0 , 1 );
      Vector3 p1 = new Vector3( 1 , 0 , 0 );
      Vector3 p2 = new Vector3( 0 , 0 , -1 );
      Vector3 p3 = new Vector3( -1 , 0 , 0);
      Vector3 p4 = new Vector3( 0 , 1 , 0 );

      Vector3 m = new Vector3( 0 , .5f , 0 );

      p0 -= m;
      p1 -= m;
      p2 -= m;
      p3 -= m;
      p4 -= m;

      mesh.Clear();
       
      mesh.vertices = new Vector3[]{
          p0,p1,p4,
          p1,p2,p4,
          p2,p3,p4,
          p3,p0,p4,
          p0,p2,p1,
          p0,p3,p2
      };

      mesh.triangles = new int[]{
          0,1,2,
          3,4,5,
          6,7,8,
          9,10,11,
          12,13,14,
          15,16,17
      };
       
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
      mesh.Optimize();


      GetComponent<MeshFilter>().mesh = mesh;
      GetComponent<MeshCollider>().sharedMesh = mesh;
  }

}

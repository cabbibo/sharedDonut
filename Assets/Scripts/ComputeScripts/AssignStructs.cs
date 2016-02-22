﻿using UnityEngine;
using System.Collections;

public class AssignStructs : MonoBehaviour {

  public struct Vert{
    public Vector3 pos; 
    public Vector3 vel;
    public Vector3 nor;
    public Vector2 uv;
    public float  ribbonID;
    public float  life; 
    public Vector3 debug;
  };



  public struct Hand{
    public float active;
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 aVel;
    public float  triggerVal;
    public float  thumbVal;
    public float  sideVal;
    public Vector2 thumbPos;
  };


  public static int VertStructSize = 16;
  public static int HandStructSize = 15;

  public static void test(){
    print("Assign Structs working");
  }


  public static void AssignVertStruct( float[] inValues , int id , out int index , Vert i ){

    index = id;
    //pos
    // need to be slightly different to not get infinte forces
    inValues[index++] = i.pos.x * .99f;
    inValues[index++] = i.pos.y * .99f;
    inValues[index++] = i.pos.z * .99f;
   
    //vel
    inValues[index++] = i.vel.x; //Random.Range(-.00f , .00f );
    inValues[index++] = i.vel.y; //Random.Range(-.00f , .00f );
    inValues[index++] = i.vel.z; //Random.Range(-.00f , .00f );

    //nor
    inValues[index++] = i.nor.x;
    inValues[index++] = i.nor.y;
    inValues[index++] = i.nor.z;

    //uv
    inValues[index++] = i.uv.x;
    inValues[index++] = i.uv.y;

    //ribbon id
    inValues[index++] = i.ribbonID;

    //life
    inValues[index++] = i.life;

    //debug
    inValues[index++] = i.debug.x;
    inValues[index++] = i.debug.y;
    inValues[index++] = i.debug.z;

  }


  public static void AssignHandStruct( float[] inValues , int id , out int index , Hand i ){

    index = id;


    inValues[index++] = i.active;


    //pos
    inValues[index++] = i.pos.x;
    inValues[index++] = i.pos.y;
    inValues[index++] = i.pos.z;
   
    //vel
    inValues[index++] = i.vel.x;
    inValues[index++] = i.vel.y;
    inValues[index++] = i.vel.z;

    //nor
    inValues[index++] = i.aVel.x;
    inValues[index++] = i.aVel.y;
    inValues[index++] = i.aVel.z;

    //vals
    inValues[index++] = i.triggerVal;
    inValues[index++] = i.thumbVal;
    inValues[index++] = i.sideVal;

    //thumb pos
    inValues[index++] = i.thumbPos.x;
    inValues[index++] = i.thumbPos.y;

  }


  public static void AssignNullHandStruct( float[] inValues , int id , out int index ){

    index = id;


    inValues[index++] = 0;


    //pos
    inValues[index++] = 0;
    inValues[index++] = 0;
    inValues[index++] = 0;
   
    //vel
    inValues[index++] = 0;
    inValues[index++] = 0;
    inValues[index++] = 0;

    //nor
    inValues[index++] = 0;
    inValues[index++] = 0;
    inValues[index++] = 0;

    //vals
    inValues[index++] = 0;
    inValues[index++] = 0;
    inValues[index++] = 0;

    //thumb pos
    inValues[index++] = 0;
    inValues[index++] = 0;

  }

  public static void AssignTransBuffer(Transform t , float[] transValues , ComputeBuffer _transBuffer ){

    Matrix4x4 m = t.localToWorldMatrix;

    for( int i = 0; i < 16; i++ ){
      int x = i % 4;
      int y = (int) Mathf.Floor(i / 4);
      transValues[i] = m[x,y];
    }

    m = t.worldToLocalMatrix;

    for( int i = 0; i < 16; i++ ){
      int x = i % 4;
      int y = (int) Mathf.Floor(i / 4);
      transValues[i+16] = m[x,y];
    }

    _transBuffer.SetData(transValues);

  }

  public static void AssignDisformerBuffer( GameObject[] Disformers , float[] disformValues , ComputeBuffer _disformBuffer){

    for( int i = 0; i < Disformers.Length; i++ ){
      disformValues[ i * 3 + 0 ]  = Disformers[i].transform.position.x;
      disformValues[ i * 3 + 1 ]  = Disformers[i].transform.position.y;
      disformValues[ i * 3 + 2 ]  = Disformers[i].transform.position.z;
    }

    _disformBuffer.SetData( disformValues );
  
  }


  public static void AssignHandBuffer( GameObject[] Hands , float[] handValues , ComputeBuffer _handBuffer ){

    int index = 0;
    for( int i =0; i < Hands.Length; i++ ){
      if( Hands[i] != null ){
        AssignHandStruct( handValues , index , out index , Hands[i].GetComponent<controllerInfo>().hand );
      }else{
        AssignNullHandStruct( handValues , index , out index );
      }
    }
  
    _handBuffer.SetData( handValues );
  }

}
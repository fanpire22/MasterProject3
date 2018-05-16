// ***********************************************************
//	Copyright 2017 Next Limit Technologies, http://www.nextlimit.com
//	All rights reserved.
//
//	THIS SOFTWARE IS PROVIDED 'AS IS' AND WITHOUT ANY EXPRESS OR
//	IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//
// ***********************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  [System.Serializable]
  public class CarOptimizedGO
  {
    [SerializeField]
    private GameObject go_;
    public GameObject GameObject
    {
      get { return go_; }
    }

    [SerializeField]
    private Transform transform_;
    public Transform Transform
    {
      get { return transform_; }
    }


    [SerializeField]
    private int vertexCount_;
    public int VertexCount
    {
      get { return vertexCount_; }
    }

    [SerializeField]
    private int boneCount_;
    public int BoneCount
    {
      get { return boneCount_; }
    }

    [SerializeField]
    private bool hasRootBone_;
    public bool HasRootBone
    {
      get { return hasRootBone_; }
    }

    public CarOptimizedGO(GameObject go, int boneCount, bool hasRootBone)
    {
      go_ = go;
      transform_ = go.transform;

      Mesh mesh = go.GetMesh();
      if (mesh != null)
      {
        vertexCount_ = mesh.vertexCount;
        boneCount_   = boneCount;
        hasRootBone_ = hasRootBone;
      }
    }
  }
}

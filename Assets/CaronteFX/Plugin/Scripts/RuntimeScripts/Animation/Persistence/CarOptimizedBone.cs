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
  public class CarOptimizedBone
  {
    [SerializeField]
    private Transform optimizedTransform_;

    [SerializeField]
    private int optimizedGOIdx_;

    [SerializeField]
    private int boneIdx_;

    [SerializeField]
    private string originalPath_;

    [SerializeField]
    private Vector3 originalLocalPosition_;

    [SerializeField]
    private Quaternion originalLocalRotation_;

    [SerializeField]
    private Vector3 originalLocalScale_;

    public CarOptimizedBone( Transform optimizedTransform, int optimizedGOIdx, int boneIdx, string originalPath, 
                             Vector3 originalLocalPosition, Quaternion originalLocalRotation, Vector3 originalLocalScale)
    {
      optimizedTransform_ = optimizedTransform;
      optimizedGOIdx_     = optimizedGOIdx;

      boneIdx_ = boneIdx;

      originalPath_          = originalPath;
      originalLocalPosition_ = originalLocalPosition;
      originalLocalRotation_ = originalLocalRotation;
      originalLocalScale_    = originalLocalScale;
    }

    public Transform GetOptimizedTransform()
    {
      return optimizedTransform_;
    }

    public int GetOptimizedGOIdx()
    {
      return optimizedGOIdx_;

    }
    public int GetBoneIdx()
    {
      return boneIdx_;
    }

    public string GetOriginalPath()
    {
      return originalPath_;
    }

    public Vector3 GetOriginalLocalPosition()
    {
      return originalLocalPosition_;
    }

    public Quaternion GetOriginalLocalRotation()
    {
      return originalLocalRotation_;
    }

    public Vector3 GetOriginalLocalScale()
    {
      return originalLocalScale_;
    }
  }

}


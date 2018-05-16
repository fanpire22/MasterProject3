using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CaronteFX
{
  public class CarAnimatedGO
  {
    public Transform tr_;

    public int vertexCount_;
    public int bytesOffset_;
    public int boneIdxBegin_;
    public int boneIdxEnd_;
    public int boneCount_;
    public int optimizedBoneIdx_;

    public CarAnimatedGO(Transform tr, int vertexCount, int bytesOffset )
      : this(tr, vertexCount, bytesOffset, -1, 0, 0 )
    {
    }

    public CarAnimatedGO(Transform tr, int vertexCount, int bytesOffset, int optimizedBoneIdx )
      : this(tr, vertexCount, bytesOffset, optimizedBoneIdx, 0, 0 )
    {
    }

    public CarAnimatedGO(Transform tr, int vertexCount, int bytesOffset, int optimizedBoneIdx, int boneIdxBegin, int boneIdxEnd )
    {
      tr_ = tr;

      vertexCount_  = vertexCount;
      bytesOffset_  = bytesOffset;
      optimizedBoneIdx_ = optimizedBoneIdx;

      boneIdxBegin_ = boneIdxBegin;
      boneIdxEnd_   = boneIdxEnd;
      boneCount_    = boneIdxEnd_ - boneIdxBegin_;
    }
  }
}

using System;
using System.Runtime.InteropServices;
using UnityEngine;


namespace CaronteFX
{
  public class CarSkinnedAnimatedGPUBuffers : CarGPUBuffers
  {
    int nBone_;
    public int BoneCount
    {
      get { return nBone_; }
    }

    bool hasRootBone_;
    public bool HasRootBone
    {
      get { return hasRootBone_; }
    }

    ComputeBuffer[] arrBuf_rootBoneMatrix_;
    ComputeBuffer[] arrBuf_boneMatrix_;
    ComputeBuffer buf_bindposeMatrix_;
    ComputeBuffer buf_identityRootMatrix_;
    ComputeBuffer buf_initialRootMatrix_;

    ComputeBuffer buf_Position_;
    ComputeBuffer buf_Normal_;
    ComputeBuffer buf_BoneWeight_;

    ComputeBuffer buf_actualBoneMatrix_;
    ComputeBuffer buf_actualNormalMatrix_;

    ComputeBuffer buf_visibility_;

    Matrix4x4[] arrCache_BindPose_;
    Matrix4x4[] arrCache_Bone_;

    Matrix4x4[] arrInitialRootMatrix_;
    Matrix4x4[] arrRootBoneMatrix_;

    Vector2[] arrVisibilityInterval_;
    
    public CarSkinnedAnimatedGPUBuffers(int bufferSize, int nVertex, int nBones, bool hasRootBone)
      : base(bufferSize, nVertex)
    {
      nBone_ = nBones;
      hasRootBone_ = hasRootBone;

      CreateComputeBuffers();
    }

    public ComputeBuffer GetInitialRootMatrixBuffer()
    {
      return buf_initialRootMatrix_;
    }

    public ComputeBuffer GetRootBoneMatrixBuffer(int bufferIdx)
    {
      if (hasRootBone_)
      {
        return arrBuf_rootBoneMatrix_[bufferIdx];
      }
      else
      {
        return buf_identityRootMatrix_; 
      }
    }

    public ComputeBuffer GetBonesBuffer(int bufferIdx)
    {
      return arrBuf_boneMatrix_[bufferIdx];
    }

    public ComputeBuffer GetBindposesBuffer()
    {
      return buf_bindposeMatrix_;
    }

    public ComputeBuffer GetPositionsBuffer()
    {
      return buf_Position_;
    }

    public ComputeBuffer GetNormalsBuffer()
    {
      return buf_Normal_;
    }

    public ComputeBuffer GetBoneWeightsBuffer()
    {
      return buf_BoneWeight_;
    }

    public ComputeBuffer GetActualBoneMatrixBuffer()
    {
      return buf_actualBoneMatrix_;
    }

    public ComputeBuffer GetActualBoneNormalMatrixBuffer()
    {
      return buf_actualNormalMatrix_;
    }

    public ComputeBuffer GetVisibilityBuffer()
    {
      return buf_visibility_;
    }

    public void SetInitialRootMatrix(Matrix4x4 initialRootMatrix)
    {
      arrInitialRootMatrix_[0] = initialRootMatrix;
      buf_initialRootMatrix_.SetData(arrInitialRootMatrix_);
    }

    public void SetRootBoneMatrixInCache(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
      arrRootBoneMatrix_[0] = Matrix4x4.TRS(localPosition, localRotation, localScale);
    }

    public void SetBoneMatrixInCache(int boneIdx, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
      arrCache_Bone_[boneIdx] = Matrix4x4.TRS(localPosition, localRotation, localScale);
    }

    public void SetRootBoneMatrixFromCache(int bufferIdx)
    {
      if (hasRootBone_)
      {
        arrBuf_rootBoneMatrix_[bufferIdx].SetData(arrRootBoneMatrix_);
      }
    }

    public void SetFrameBonesFromCache(int bufferIdx)
    {
      arrBuf_boneMatrix_[bufferIdx].SetData(arrCache_Bone_);
    }

    public void SetBindPoses(Matrix4x4[] arrBindpose)
    {
      buf_bindposeMatrix_.SetData(arrBindpose);
    }

    public void SetPositions(Vector3[] arrPosition)
    {
      buf_Position_.SetData(arrPosition);
    }

    public void SetNormals(Vector3[] arrNormal)
    {
      buf_Normal_.SetData(arrNormal);
    }

    public void SetBoneWeights(CarBoneWeight[] arrBoneWeight)
    {
      buf_BoneWeight_.SetData(arrBoneWeight);
    }

    public void CreateBonesCaches(Matrix4x4[] arrBindPose)
    {
      arrCache_BindPose_ = new Matrix4x4[arrBindPose.Length];
      Array.Copy(arrBindPose, arrCache_BindPose_, arrBindPose.Length);

      arrCache_Bone_ = new Matrix4x4[nBone_];

      arrInitialRootMatrix_ = new Matrix4x4[1];
      arrRootBoneMatrix_    = new Matrix4x4[1];

      arrVisibilityInterval_ = new Vector2[nBone_];
    }

    private void CreateComputeBuffers()
    {
      if (hasRootBone_)
      {
        arrBuf_rootBoneMatrix_ = new ComputeBuffer[bufferSize_];
        for (int i = 0; i < bufferSize_; i++)
        {
          arrBuf_rootBoneMatrix_[i] = new ComputeBuffer(1, Marshal.SizeOf(typeof(Matrix4x4)));
        }
      }
      else
      {
        buf_identityRootMatrix_ = new ComputeBuffer(1, Marshal.SizeOf(typeof(Matrix4x4)));
        buf_identityRootMatrix_.SetData(new Matrix4x4[1]{ Matrix4x4.identity });
      }

      arrBuf_boneMatrix_ = new ComputeBuffer[bufferSize_];
      for (int i = 0; i < bufferSize_; i++)
      {
        arrBuf_boneMatrix_[i] = new ComputeBuffer(nBone_, Marshal.SizeOf(typeof(Matrix4x4)));
      }

      buf_bindposeMatrix_ = new ComputeBuffer(nBone_, Marshal.SizeOf(typeof(Matrix4x4)));
      buf_initialRootMatrix_  = new ComputeBuffer(1, Marshal.SizeOf(typeof(Matrix4x4)));   

      buf_Position_   = new ComputeBuffer(nVertex_, Marshal.SizeOf(typeof(Vector3)));
      buf_Normal_     = new ComputeBuffer(nVertex_, Marshal.SizeOf(typeof(Vector3)));
      buf_BoneWeight_ = new ComputeBuffer(nVertex_, Marshal.SizeOf(typeof(BoneWeight)));

      buf_actualBoneMatrix_ = new ComputeBuffer(nBone_, Marshal.SizeOf(typeof(Matrix4x4)));
      buf_actualNormalMatrix_ = new ComputeBuffer(nBone_, sizeof(float) * 9);

      buf_visibility_ = new ComputeBuffer(nBone_, Marshal.SizeOf(typeof(Vector2)));
    }

    public void Clear()
    {
      if (hasRootBone_)
      {
        for (int i = 0; i < bufferSize_; i++)
        {
          arrBuf_rootBoneMatrix_[i].Release();
        }
      }
      else
      {
        buf_identityRootMatrix_.Release();
      }

      for (int i = 0; i < bufferSize_; i++)
      {
        arrBuf_boneMatrix_[i].Release();
      }

      buf_bindposeMatrix_.Release();
      buf_initialRootMatrix_.Release();

      buf_Position_  .Release();
      buf_Normal_    .Release();
      buf_BoneWeight_.Release();

      buf_actualBoneMatrix_.Release();
      buf_actualNormalMatrix_.Release();

      buf_visibility_.Release();
    }

    public void SetBoneVisibilityInterval(int boneIdx, Vector2 visibilityInterval)
    {
      if (boneIdx == -1)
      {
        for (int i = 0; i < nBone_; i++)
        {
          arrVisibilityInterval_[i] = visibilityInterval;
        }
      }
      else
      {
        arrVisibilityInterval_[boneIdx] = visibilityInterval;
      }
    }

    public void SetVisibilityData()
    {
      buf_visibility_.SetData(arrVisibilityInterval_);
    }

    public void BufferFrameBoneMatrices(int boneIdxInSkinned, int boneIdxBegin, int boneIdxEnd, int[] arrIdxBoneInSkinned, byte[] binaryAnim, ref int cursor)
    {
      Vector3 r;
      r.x = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
      r.y = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
      r.z = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);

      Quaternion q;
      q.x = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
      q.y = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
      q.z = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
      q.w = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);

      if (boneIdxInSkinned == -1)
      {
        SetRootBoneMatrixInCache(r, q, Vector3.one);

        for (int i = boneIdxBegin; i < boneIdxEnd; i++)
        {
          boneIdxInSkinned = arrIdxBoneInSkinned[i];

          if (boneIdxInSkinned != -1)
          {
            Vector3 boneR;
            boneR.x = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
            boneR.y = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
            boneR.z = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);

            Quaternion boneQ;
            boneQ.x = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
            boneQ.y = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
            boneQ.z = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
            boneQ.w = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);

            Vector3 boneS;
            boneS.x = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
            boneS.y = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);
            boneS.z = CarBinaryReader.ReadSingleFromArrByte(binaryAnim, ref cursor);

            SetBoneMatrixInCache(boneIdxInSkinned, boneR, boneQ, boneS);
          }
        }
      }
      else
      {
        SetBoneMatrixInCache(boneIdxInSkinned, r, q, Vector3.one);
      }
    }

  }
 
}

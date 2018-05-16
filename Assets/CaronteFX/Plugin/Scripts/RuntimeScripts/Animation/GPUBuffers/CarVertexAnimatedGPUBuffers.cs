// ***********************************************************
//	Copyright 2016 Next Limit Technologies, http://www.nextlimit.com
//	All rights reserved.
//
//	THIS SOFTWARE IS PROVIDED 'AS IS' AND WITHOUT ANY EXPRESS OR
//	IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//
// ***********************************************************

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace CaronteFX
{
  public class CarVertexAnimatedGPUBuffers : CarGPUBuffers
  {
    int nFiber_;

    ComputeBuffer[] arrPositionBuffer_;
    ComputeBuffer[] arrNormalBuffer_;

    ComputeBuffer definitionBuffer_;
    ComputeBuffer vertexDataBuffer_;

    private int DefinitionStride
    {
      get { return nFiber_ * definition_.GetStride(); }
    }

    private int CompressedPoseStride
    {
      get { return nFiber_ * compressedPose_.GetStride();  }
    }

    Vector3[] arrPositionCache_;
    Vector3[] arrNormalCache_;

    CarPositionCompressed[] arrPositionCompressedCache_;
    CarNormalCompressed[]   arrNormalCompressedCache_;

    UInt32[] arrCompressedFiberCache_;

    CarDefinition      definition_;
    CarCompressedPose  compressedPose_;

    CarVertexDataCache vertexDataCache_; //for normals-tangents re computation (for now only normals supported)

    public CarVertexAnimatedGPUBuffers(int bufferSize, int nVertex, bool isCompressed, bool isBoxCompression,
                                       bool isFiberCompression, CarDefinition definition, CarCompressedPose compressedPose,
                                       bool isVertexLocalSystems, CarVertexDataCache vertexDataCache)
      : base(bufferSize, nVertex)
    {
      CreateComputeBuffers(isCompressed, isBoxCompression, isFiberCompression, definition, 
                           compressedPose, isVertexLocalSystems, vertexDataCache);
    }

    private void CreateComputeBuffers(bool isCompressed, bool isBoxCompression, bool isFiberCompression, 
                                      CarDefinition definition, CarCompressedPose compressedPose,
                                      bool isVertexLocalSystems, CarVertexDataCache vertexDataCache)
    {
      if (isCompressed)
      {
        if (isBoxCompression)
        {
          CreateComputeBuffersBoxCompression();
        }
        else if (isFiberCompression)
        {
          CreateComputeBuffersFiberCompression(definition, compressedPose);
        }
      }
      else
      {
        CreateComputeBuffersUncompressed();
      }

      if (isVertexLocalSystems)
      {
        CreateComputeBuffersVertexLocalSystems(vertexDataCache);
      }
    }

    private void CreateComputeBuffersBoxCompression()
    {
      //TODO: remove the two extra frames and interpolate from the output textures with another compute buffer;
      arrPositionCompressedCache_ = new CarPositionCompressed[nVertex_];
      arrNormalCompressedCache_ = new CarNormalCompressed[nVertex_];

      arrPositionBuffer_ = new ComputeBuffer[bufferSize_];
      arrNormalBuffer_   = new ComputeBuffer[bufferSize_];

      for (int i = 0; i < bufferSize_; i++ )
      {
        arrPositionBuffer_[i] = new ComputeBuffer(nVertex_, sizeof(UInt16) * 4);
        arrNormalBuffer_[i]   = new ComputeBuffer(nVertex_, sizeof(Byte) * 4);
      }
    }

    private void CreateComputeBuffersFiberCompression(CarDefinition definition, CarCompressedPose compressedPose)
    {
      definition_     = definition;
      compressedPose_ = compressedPose;

      nFiber_ = definition.GetNumberOfFibers();

      definitionBuffer_ = new ComputeBuffer(DefinitionStride, sizeof(UInt32));
      definitionBuffer_.SetData(definition.GPUCache);

      arrCompressedFiberCache_ = new UInt32[CompressedPoseStride];
      compressedPose_.GPUCache = arrCompressedFiberCache_;

      arrPositionBuffer_ = new ComputeBuffer[bufferSize_];
      for (int i = 0; i < bufferSize_; i++)
      {
        arrPositionBuffer_[i] = new ComputeBuffer(CompressedPoseStride, sizeof(UInt32));
      }
    }

    private void CreateComputeBuffersUncompressed()
    {
      //two extra frames available for second frame for interpolation and to avoid compute shader out of range problems.
      //TODO: remove the two extra frames and interpolate from the output textures with another compute buffer;
      arrPositionCache_ = new Vector3[nVertex_];
      arrNormalCache_ = new Vector3[nVertex_];

      arrPositionBuffer_ = new ComputeBuffer[bufferSize_];
      arrNormalBuffer_   = new ComputeBuffer[bufferSize_];

      for (int i = 0; i < bufferSize_; i++)
      {
        arrPositionBuffer_[i] = new ComputeBuffer(nVertex_, sizeof(float) * 3);
        arrNormalBuffer_[i] = new ComputeBuffer(nVertex_, sizeof(float) * 3);
      }
    }

    private void CreateComputeBuffersVertexLocalSystems(CarVertexDataCache vertexDataCache)
    {
      vertexDataCache_ = vertexDataCache;
      int stride = Marshal.SizeOf(typeof(CarVertexData));
      vertexDataBuffer_ = new ComputeBuffer(nVertex_, stride);
      vertexDataBuffer_.SetData(vertexDataCache_.Cache);
    }

    public ComputeBuffer GetPositionBuffer(int bufferFrame)
    {
      //TODO: bufferLogic
      int idx = bufferFrame;
      return arrPositionBuffer_[idx];
    }

    public ComputeBuffer GetNormalBuffer(int bufferFrame)
    {
      //TODO: bufferLogic
      int idx = bufferFrame;
      return arrNormalBuffer_[idx];
    }

    public ComputeBuffer GetDefinitionBuffer()
    {
      return definitionBuffer_;
    }

    public ComputeBuffer GetVertexDataBuffer()
    {
      return vertexDataBuffer_;
    }

    public void Clear()
    {
      if (arrPositionBuffer_ != null)
      {
        for (int i = 0; i < bufferSize_; i++)
        {
          arrPositionBuffer_[i].Release();
        }
      }

      if (arrNormalBuffer_ != null)
      {
        for (int i = 0; i < bufferSize_; i++)
        {
          arrNormalBuffer_[i].Release();
        }
      }

      if (definitionBuffer_ != null)
      {
        definitionBuffer_.Release();
      }

      if (vertexDataBuffer_ != null)
      {
        vertexDataBuffer_.Release();
      }
    }

    public void BufferFrameMesh(int bufferFrame, bool vertexCompression, bool boxCompression, bool fiberCompression, bool hasTangents, byte[] animCache, ref int cursor)
    { 
      if (vertexCompression)
      {
        if (boxCompression)
        {
          BufferFrameMeshBox(bufferFrame, hasTangents, animCache, ref cursor);
        }
        else if (fiberCompression)
        {
          BufferFrameMeshFiber(bufferFrame, animCache, ref cursor);
        }
      }
      else
      {
        BufferFrameMeshUncompressed(bufferFrame, hasTangents, animCache, ref cursor);
      }
    }

    private void BufferFrameMeshUncompressed(int bufferFrame, bool hasTangents, byte[] animCache, ref int cursor)
    {
      //Skip buffering of bounding boxes. They will be uploaded in each frame.
      AdvanceCursor(24, ref cursor);
      {
        CarBinaryReader.ReadArrByteToArrVector3(animCache, ref cursor, arrPositionCache_, 0, nVertex_);
        ComputeBuffer positionBuffer = arrPositionBuffer_[bufferFrame];
        positionBuffer.SetData(arrPositionCache_);
      }


      {
        CarBinaryReader.ReadArrByteToArrVector3(animCache, ref cursor, arrNormalCache_, 0, nVertex_);
        ComputeBuffer normalBuffer = arrNormalBuffer_[bufferFrame];
        normalBuffer.SetData(arrNormalCache_);
      }

      if (hasTangents)
      {
        //TODO fill tangents cache
        AdvanceCursor(32 * nVertex_, ref cursor);
      }
    }

    private void BufferFrameMeshBox(int bufferFrame, bool hasTangents, byte[] animCache, ref int cursor)
    {
      //Skip buffering of bounding boxes. They will be uploaded in each frame.
      AdvanceCursor(24, ref cursor);

      {
        CarBinaryReader.ReadArrByteToArrPositionComp(animCache, ref cursor, arrPositionCompressedCache_, 0, nVertex_);
        ComputeBuffer positionBuffer = arrPositionBuffer_[bufferFrame];
        positionBuffer.SetData(arrPositionCompressedCache_);
      }

      {
        CarBinaryReader.ReadArrByteToArrNormalComp(animCache, ref cursor, arrNormalCompressedCache_, 0, nVertex_);
        ComputeBuffer normalBuffer = arrNormalBuffer_[bufferFrame];
        normalBuffer.SetData(arrNormalCompressedCache_);
      }

      if (hasTangents)
      {
        //TODO fill tangents cache
        AdvanceCursor(4 * nVertex_, ref cursor);
      }
    }

    private void BufferFrameMeshFiber(int bufferFrame, byte[] animCache, ref int cursor)
    {
      compressedPose_.LoadGPU(animCache, ref cursor, definition_);
      ComputeBuffer positionBuffer = arrPositionBuffer_[bufferFrame];
      positionBuffer.SetData(arrCompressedFiberCache_);
    }

  }//class CarVertexAnimatedGPUBuffers...
}

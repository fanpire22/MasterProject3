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


namespace CaronteFX
{
  public class CarGPUBufferer
  {
    Dictionary<int, int> dictFrameToGPUFrame_ = new Dictionary<int, int>();
    Queue<Tuple2<int, int>> queueGPUFrames_ = new Queue<Tuple2<int, int>>();

    Tuple2<int, int> lastElement_;
    int frameBufferSize_ = 0;

    public CarGPUBufferer(int gpuFrameBufferSize)
    {
      SetBufferSize(gpuFrameBufferSize);
    }

    private void SetBufferSize(int gpuFrameBufferSize)
    {
      frameBufferSize_ = gpuFrameBufferSize;
      Init();
    }

    public void Init()
    {
      dictFrameToGPUFrame_.Clear();
      queueGPUFrames_.Clear();

      for (int i = 0; i < frameBufferSize_; i++)
      {
        queueGPUFrames_.Enqueue(new Tuple2<int, int>(i, -i));
      }
      lastElement_ = new Tuple2<int, int>(-1, -1);
    }

    public int GetNumberOfFramesBuffered()
    {
      return dictFrameToGPUFrame_.Count;
    }

    public bool IsFrameBuffered(int frame)
    {
      return ( dictFrameToGPUFrame_.ContainsKey(frame) );
    }

    public bool IsFrameRangeBuffered(int frameMin, int frameMax)
    {
      int firstFrameBuffered = queueGPUFrames_.Peek().Second;
      int lastFrameBuffered  = lastElement_.Second;

      return ( (frameMin >= firstFrameBuffered && frameMax <= lastFrameBuffered) );
    }

    public int GetBufferFrame(int frame)
    {
      return ( dictFrameToGPUFrame_[frame] );
    }

    public void AddFrameToBuffer(int frame)
    {
      Tuple2<int,int> tuple_frameGpuFrame = queueGPUFrames_.Dequeue();

      int gpuFrame   = tuple_frameGpuFrame.First;
      int realFrame  = tuple_frameGpuFrame.Second;

      if (realFrame != -1)
      {
        dictFrameToGPUFrame_.Remove(realFrame);
      }

      tuple_frameGpuFrame.Second = frame;
      dictFrameToGPUFrame_[frame] = gpuFrame;

      queueGPUFrames_.Enqueue(tuple_frameGpuFrame);
      lastElement_ = tuple_frameGpuFrame;
    }
  }
}
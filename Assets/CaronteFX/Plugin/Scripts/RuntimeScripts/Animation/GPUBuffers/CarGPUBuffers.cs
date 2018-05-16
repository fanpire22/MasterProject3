using System;
using UnityEngine;

namespace CaronteFX
{
  public abstract class CarGPUBuffers
  {
    protected const int CONST_renderTextureWidth = 256;

    protected int bufferSize_;
    public int BufferSize
    {
      get { return bufferSize_;  }
    }

    protected int nVertex_;
    public int VertexCount
    {
      get { return nVertex_;  }
    }

    protected RenderTexture positionTexture_;
    public RenderTexture PositionTexture
    {
      get { return positionTexture_; }
    }

    protected RenderTexture positionInterpolationTexture1_;
    public RenderTexture PositionInterpolationTexture1
    {
      get { return positionInterpolationTexture1_; }
    }

    protected RenderTexture positionInterpolationTexture2_;
    public RenderTexture PositionInterpolationTexture2
    {
      get { return positionInterpolationTexture2_; }
    }

    protected RenderTexture normalTexture_;
    public RenderTexture NormalTexture
    {
      get { return normalTexture_; }
    }

    protected RenderTexture normalInterpolationTexture1_;
    public RenderTexture NormalInterpolationTexture1
    {
      get { return normalInterpolationTexture1_; }
    }

    protected RenderTexture normalInterpolationTexture2_;
    public RenderTexture NormalInterpolationTexture2
    {
      get { return normalInterpolationTexture2_; }
    }

    public CarGPUBuffers(int bufferSize, int nVertex)
    {
      bufferSize_ = bufferSize;
      nVertex_    = nVertex;

      CreateRenderTextures();
    }

    protected void CalculateBestMatchTextureSize(int vertexCount, out int width, out int height)
    {
      width = CONST_renderTextureWidth;
      height = (vertexCount / width) + 1;
    }

    protected void CreateRenderTextures()
    {
      int width, height;
      CalculateBestMatchTextureSize(nVertex_, out width, out height);

      positionTexture_ = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
      positionTexture_.filterMode = FilterMode.Point;
      positionTexture_.enableRandomWrite = true;
      positionTexture_.Create();

      positionInterpolationTexture1_ = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
      positionInterpolationTexture1_.filterMode = FilterMode.Point;
      positionInterpolationTexture1_.enableRandomWrite = true;
      positionInterpolationTexture1_.Create();

      positionInterpolationTexture2_ = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
      positionInterpolationTexture2_.filterMode = FilterMode.Point;
      positionInterpolationTexture2_.enableRandomWrite = true;
      positionInterpolationTexture2_.Create();

      normalTexture_ = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
      normalTexture_.filterMode = FilterMode.Point;
      normalTexture_.enableRandomWrite = true;
      normalTexture_.Create();

      normalInterpolationTexture1_ = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
      normalInterpolationTexture1_.filterMode = FilterMode.Point;
      normalInterpolationTexture1_.enableRandomWrite = true;
      normalInterpolationTexture1_.Create();

      normalInterpolationTexture2_ = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
      normalInterpolationTexture2_.filterMode = FilterMode.Point;
      normalInterpolationTexture2_.enableRandomWrite = true;
      normalInterpolationTexture2_.Create();
    }

    protected void AdvanceCursor(Int64 bytesOffset, ref int cursor)
    {
      cursor += (int)bytesOffset;
    }

  }
}

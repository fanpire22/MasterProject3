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
using System.Runtime.InteropServices;

namespace CaronteFX
{

  //-----------------------------------------------------------------------------------
  //
  // CarCorpusclesRenderer:
  //
  //-----------------------------------------------------------------------------------
  [AddComponentMenu("")]
  [ExecuteInEditMode]
  public class CarCorpusclesDepthRenderer : CarCorpusclesRenderer
  {
    ///////////////////////////////////////////////////////////////////////////////////
    // 
    // Data Members:
    // 

    //  
    // UI params:  
    //_________________________________________________________________________________
    public RenderTexture depthRenderTarget_ = null;
    public enum EBlurMode
    {
      None,
      Naive,
      Bilateral
    }
    public EBlurMode blurMode_ = EBlurMode.None;


    ///////////////////////////////////////////////////////////////////////////////////
    //  
    // Operations:
    //   
    ///////////////////////////////////////////////////////////////////////////////////
    protected override bool SetRenderShader(out Shader renderShader)
    {
      renderShader = (Shader)Resources.Load("CFX Corpuscles Depth");

      if (!renderShader || !renderShader.isSupported)
      {
        enabled = false;
        return false;
      }

      depthRenderTarget_ = (RenderTexture) Resources.Load("CarDepthRenderTexture");

      return true;
    }
    ///////////////////////////////////////////////////////////////////////////////////
    //  
    // Unity callbacks:
    //   
    void OnEnable()
    {

    }
    //-----------------------------------------------------------------------------------
    void OnRenderObject()
    {
      if (hasBeenInited_)
      {

        RenderTexture current = RenderTexture.active;
        Graphics.SetRenderTarget(depthRenderTarget_);
        GL.Clear(true, true, Color.red, 1.0f);

        renderMaterial_.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Triangles, 6, currentCorpuscles_);
        Graphics.SetRenderTarget(current);
      }
    }
    //-----------------------------------------------------------------------------------
    void OnDestroy()
    {
      DeInit();
    }
    //-----------------------------------------------------------------------------------
  }
}



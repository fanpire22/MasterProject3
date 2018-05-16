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
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace CaronteFX
{

  //-----------------------------------------------------------------------------------
  //
  // CarCorpusclesRenderer:
  //
  //-----------------------------------------------------------------------------------
  [AddComponentMenu("")]
  //[ExecuteInEditMode]
  public class CarFluidRenderer : CarCorpusclesRenderer
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


    //  
    // Private members:  
    //_________________________________________________________________________________
    private Shader compositorShader_ = null;
    private Material compositorMaterial_ = null;

    private Dictionary<Camera, CommandBuffer> dictCameraCommandBuffer_ = new Dictionary<Camera, CommandBuffer>();
    ///////////////////////////////////////////////////////////////////////////////////
    //  
    // Operations:
    //   
    ///////////////////////////////////////////////////////////////////////////////////
    protected override bool SetRenderShader(out Shader renderShader)
    {
      renderShader     = (Shader)Resources.Load("CFX Corpuscles Depth");
      compositorShader_ = (Shader)Resources.Load("CFX Fluid Compositor");

      if (!renderShader || !renderShader.isSupported || !compositorShader_ || !compositorShader_.isSupported)
      {
        enabled = false;
        return false;
      }

      depthRenderTarget_ = (RenderTexture) Resources.Load("CarDepthRenderTexture");

      compositorMaterial_ = new Material(compositorShader_);
			compositorMaterial_.hideFlags = HideFlags.HideAndDontSave;

      return true;
    }
    //-----------------------------------------------------------------------------------
    void SetCommandBuffer()
    {
      Camera camera = Camera.current;
      if ( dictCameraCommandBuffer_.ContainsKey(camera) )
      {
        return;
      }
        
      camera.depthTextureMode = DepthTextureMode.Depth;

      CommandBuffer cbuf = new CommandBuffer();
      cbuf.name = "Render fluid";
        
      dictCameraCommandBuffer_[camera] = cbuf;

      RenderTargetIdentifier cameraRenderTargetId = new RenderTargetIdentifier(camera.targetTexture);
      //RenderTargetIdentifier depthRenderTagetId   = new RenderTargetIdentifier(depthRenderTarget_);

      int fluidColorID        = Shader.PropertyToID("_FluidColor");
      int fluidDepthTextureID = Shader.PropertyToID("_FluidDepthTexture");

      /*
      cbuf.SetRenderTarget( depthRenderTagetId );
      cbuf.ClearRenderTarget( true, true, Color.red, 1.0f );
      cbuf.DrawProcedural(Matrix4x4.identity, renderMaterial_, 0, MeshTopology.Triangles, 6, currentCorpuscles_);
      */
      compositorMaterial_.SetColor(fluidColorID, Color.blue);
      compositorMaterial_.SetTexture(fluidDepthTextureID, depthRenderTarget_);

      int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
		  cbuf.GetTemporaryRT (screenCopyID, -1, -1, 0, FilterMode.Bilinear);
		  cbuf.Blit(cameraRenderTargetId, screenCopyID);

      cbuf.Blit(screenCopyID, cameraRenderTargetId, compositorMaterial_);
      //cbuf.Blit(depthRenderTagetId, cameraRenderTargetId);

      camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, cbuf);
    }
    //-----------------------------------------------------------------------------------
    void CleanUp()
    {
      foreach (var camBuffer in dictCameraCommandBuffer_)
		  {
        Camera cam = camBuffer.Key;
        CommandBuffer buf = camBuffer.Value;

			  if (cam != null)
			  {
				  cam.RemoveCommandBuffer (CameraEvent.BeforeImageEffects, buf);
			  }
		  }
		  dictCameraCommandBuffer_.Clear();
    }
    ///////////////////////////////////////////////////////////////////////////////////
    //  
    // Unity callbacks:
    //   
    void OnEnable()
    {
      CleanUp();
    }
    //-----------------------------------------------------------------------------------
	  public void OnDisable()
	  {
		  CleanUp();
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

        SetCommandBuffer();
      }
    }
    //-----------------------------------------------------------------------------------
    void OnDestroy()
    {
      DeInit();
      CleanUp();

      if (compositorMaterial_)
      {
        Object.DestroyImmediate(compositorMaterial_);
      }
    }
    //-----------------------------------------------------------------------------------
  }
}



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

namespace CaronteFX
{
#if UNITY_5_4_OR_NEWER
  [ImageEffectAllowedInSceneView]
#endif
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("")]
  public class CarFluidCompositor : CarCompositorBase
  {
    ///////////////////////////////////////////////////////////////////////////////////
    // 
    // Data Members:
    // 
    public RenderTexture fluidDepthTexture_;
    public Color fluidColor_ = Color.blue;

    //  
    // Shader Ids:  
    //_________________________________________________________________________________
    private int fluidDepthTextureShaderId_;
    private int fluidColorShaderId_;

    ///////////////////////////////////////////////////////////////////////////////////
    //  
    // Operations:
    //   
    ///////////////////////////////////////////////////////////////////////////////////
    protected override void Init()
    {
      base.Init();

      GetShaderPropertiesIds();
      BindRenderPropertiesToShader();

      Camera camera = gameObject.GetComponent<Camera>();
      camera.depthTextureMode = DepthTextureMode.Depth;
    }
    //-----------------------------------------------------------------------------------
    void GetShaderPropertiesIds()
    {
      fluidDepthTextureShaderId_ = Shader.PropertyToID("_FluidDepthTexture");
      fluidColorShaderId_        = Shader.PropertyToID("_FluidColor");
    }
    //-----------------------------------------------------------------------------------
    void BindRenderPropertiesToShader()
    {
      material_.SetTexture(fluidDepthTextureShaderId_, fluidDepthTexture_);
      material_.SetColor(fluidColorShaderId_, fluidColor_);
    }
    //-----------------------------------------------------------------------------------

    ///////////////////////////////////////////////////////////////////////////////////
    //  
    // Unity callbacks:
    //

    private void Start()
    {
      Init();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
      if (hasBeenInited_)
      {
        Graphics.Blit(source, destination, material_);
      }
    }

    private void OnDestroy()
    {
      Deinit();
    }

  }
}


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
  public class CarCorpusclesDiffuseRenderer : CarCorpusclesRenderer
  {
    ///////////////////////////////////////////////////////////////////////////////////
    // 
    // Data Members:
    // 

    //  
    // UI params:  
    //_________________________________________________________________________________
    [SerializeField]
    private Color corpuscleColor_ = Color.blue;
    public Color CorpusclesColor
    {
      get { return corpuscleColor_; }
      set
      {
        corpuscleColor_ = value;
        if (hasBeenInited_)
        {
          renderMaterial_.SetColor( corpuscleColorShaderId_,  corpuscleColor_ );
        }
      }
    }

    //  
    // Shader Ids:  
    //_________________________________________________________________________________
    private int corpuscleColorShaderId_;

    ///////////////////////////////////////////////////////////////////////////////////
    //  
    // Operations:
    //   
    ///////////////////////////////////////////////////////////////////////////////////
    protected override bool SetRenderShader(out Shader renderShader)
    {
      renderShader = (Shader)Resources.Load("CFX Corpuscles Diffuse");

      if (!renderShader || !renderShader.isSupported)
      {
        enabled = false;
        return false;
      }

      return true;
    }
    //-----------------------------------------------------------------------------------
    protected override void GetShaderPropertiesIds()
    {
      base.GetShaderPropertiesIds();
      corpuscleColorShaderId_  = Shader.PropertyToID("_CorpuscleColor");
    }
    //-----------------------------------------------------------------------------------
    protected override void BindRenderPropertiesToShader()
    {
      base.BindRenderPropertiesToShader();
      renderMaterial_.SetColor( corpuscleColorShaderId_,  corpuscleColor_ );
    }
    ///////////////////////////////////////////////////////////////////////////////////
    //  
    // Unity callbacks:
    //   

    void OnRenderObject()
    {
      if (hasBeenInited_)
      {
        renderMaterial_.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Triangles, 6, currentCorpuscles_);
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



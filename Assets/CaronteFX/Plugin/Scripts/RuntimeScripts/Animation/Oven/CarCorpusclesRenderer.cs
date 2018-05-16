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
  public abstract class CarCorpusclesRenderer : MonoBehaviour
  {
    ///////////////////////////////////////////////////////////////////////////////////
    // 
    // Data Members:
    // 


    //  
    // UI params:  
    //_________________________________________________________________________________
    [SerializeField]
    private float corpuscleScale_ = 1.0f;
    public float CorpusclesScale
    {
      get { return corpuscleScale_; }
      set
      {
        corpuscleScale_ = value;
        if (hasBeenInited_)
        {
          renderMaterial_.SetFloat( corpuscleScaleShaderId_,  corpuscleScale_ );
        }
      }
    }
    //  
    // Internal params:  
    //_________________________________________________________________________________
    protected Material  renderMaterial_;
    protected int currentCorpuscles_;
    protected bool hasBeenInited_ = false;

    private float corpuscleRadius_;
    private int maxCorpuscles_; 

    private ComputeBuffer bufQuadVertex_;
    private ComputeBuffer bufCorpusclePosition_;

    //  
    // Shader Ids:  
    //_________________________________________________________________________________
    private int corpuscleRadiusShaderId_;
    private int corpuscleScaleShaderId_;

    ///////////////////////////////////////////////////////////////////////////////////
    //  
    // Operations:
    //   
    ///////////////////////////////////////////////////////////////////////////////////
    protected abstract bool SetRenderShader(out Shader renderShader);
    //-----------------------------------------------------------------------------------
    public void Init(int maxCorpuscles, float corpuscleRadius)
    {
      Shader renderShader;
      bool hasShaderBeenSet = SetRenderShader(out renderShader);
      if (!hasShaderBeenSet)
      {
        return;
      }

      renderMaterial_ = new Material(renderShader);
      renderMaterial_.hideFlags = HideFlags.DontSave;

      maxCorpuscles_ = maxCorpuscles;
      corpuscleRadius_ = corpuscleRadius;
      currentCorpuscles_ = 0;

      CreateComputeBuffers();
      GetShaderPropertiesIds();
      BindComputeBuffersToShader();
      BindRenderPropertiesToShader();

      hasBeenInited_ = true;
    }
    //-----------------------------------------------------------------------------------
    public void DeInit()
    {
      ReleaseComputeBuffers();
      DestroyImmediate(renderMaterial_);
      hasBeenInited_ = false;
    }
    //-----------------------------------------------------------------------------------
    public bool IsInited()
    {
      return hasBeenInited_;
    }
    //-----------------------------------------------------------------------------------
    private void CreateComputeBuffers()
    {
      bufQuadVertex_ = new ComputeBuffer( 6, Marshal.SizeOf(typeof(Vector4)) );

      Vector4[] arrQuadPosition = new Vector4[6]
      {
         new Vector4(-1.0f,  1.0f),
         new Vector4( 1.0f,  1.0f),
         new Vector4( 1.0f, -1.0f),
         new Vector4( 1.0f, -1.0f),
         new Vector4(-1.0f, -1.0f),
         new Vector4(-1.0f,  1.0f),
      };
      bufQuadVertex_.SetData(arrQuadPosition);

      bufCorpusclePosition_ = new ComputeBuffer( maxCorpuscles_, Marshal.SizeOf(typeof(Vector3) ) ); 
    }
    //-----------------------------------------------------------------------------------
    protected virtual void GetShaderPropertiesIds()
    {
      corpuscleRadiusShaderId_ = Shader.PropertyToID("_CorpuscleRadius");
      corpuscleScaleShaderId_  = Shader.PropertyToID("_CorpuscleScale");
    }
    //-----------------------------------------------------------------------------------
    protected virtual void BindComputeBuffersToShader()
    {
      renderMaterial_.SetBuffer( "_BufQuadVertex",        bufQuadVertex_ );
      renderMaterial_.SetBuffer( "_BufCorpusclePosition", bufCorpusclePosition_ );
    }
    //-----------------------------------------------------------------------------------
    protected virtual void BindRenderPropertiesToShader()
    {
      renderMaterial_.SetFloat( corpuscleRadiusShaderId_, corpuscleRadius_ );
      renderMaterial_.SetFloat( corpuscleScaleShaderId_,  corpuscleScale_ );
    }
    //-----------------------------------------------------------------------------------
    public virtual void RebindShaderAttributes()
    {
      BindComputeBuffersToShader();
      BindRenderPropertiesToShader();
    }
    //-----------------------------------------------------------------------------------
    private void ReleaseComputeBuffers()
    {
      if (bufQuadVertex_ != null)
      {
        bufQuadVertex_.Release();
      }

      if (bufCorpusclePosition_ != null)
      {
        bufCorpusclePosition_.Release();
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetCorpusclesPositions(int corpusclesNumber, Vector3[] arrCorpusclePosition)
    {
      currentCorpuscles_ = corpusclesNumber;
      bufCorpusclePosition_.SetData(arrCorpusclePosition);  
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
      if (hasBeenInited_)
      {
        DeInit();
      }
    }
    //-----------------------------------------------------------------------------------
  }
}



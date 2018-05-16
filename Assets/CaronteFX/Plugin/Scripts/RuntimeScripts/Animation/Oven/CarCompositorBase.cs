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
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("")]
    public abstract class CarCompositorBase : MonoBehaviour
    {
      ///////////////////////////////////////////////////////////////////////////////////
      // 
      // Data Members:
      // 

      public Shader shader;
      protected Material material_;
      protected bool hasBeenInited_ = false;
      
      ///////////////////////////////////////////////////////////////////////////////////
      //  
      // Operations:
      //   
      ///////////////////////////////////////////////////////////////////////////////////
      protected virtual void Init()
      {
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }

        if (!shader || !shader.isSupported)
        {
           enabled = false;
           return;
        }

        material_ = new Material(shader);
        material_.hideFlags = HideFlags.HideAndDontSave;

        hasBeenInited_ = true;
      }
      //-----------------------------------------------------------------------------------
      protected void Deinit()
      {
        if (material_)
        {
          DestroyImmediate(material_);
        }

        hasBeenInited_ = false;
      }
      //-----------------------------------------------------------------------------------
    }
}

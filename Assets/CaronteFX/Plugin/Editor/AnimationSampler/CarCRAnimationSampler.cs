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
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CarCRAnimationSampler : CarBodyAnimationSampler
  {
    CRAnimation crAnimation_;

    public CarCRAnimationSampler(GameObject rootGameObject)
    {
      crAnimation_ = rootGameObject.GetComponent<CRAnimation>();

      CarEditorUtils.GetRenderersFromRoot(rootGameObject, out arrNonSkinnedMeshRenderer_, out arrSkinnedMeshRenderer_);
      AssignBodyIds();
    }

    public void AssignTmpAnimationController(GameObject rootGameObject)
    {
      crAnimation_ = rootGameObject.GetComponent<CRAnimation>();
      crAnimation_.LoadActiveAnimation(true);

      CarEditorUtils.GetRenderersFromRoot(rootGameObject, out arrNonSkinnedMeshRenderer_, out arrSkinnedMeshRenderer_);  
    }

    public void UpdateSimulating(CarAnimationData animData, UnityEngine.Mesh animBakingMesh, double eventTime, double deltaTimeAnimation, double deltaTimeSimulation, double startTime)
    {
      if (crAnimation_ == null)
      {
        return;
      }

      double targetTime = eventTime + deltaTimeAnimation + (deltaTimeSimulation * 0.001);

      crAnimation_.SetTime(0.0f);
      crAnimation_.Update( (float)(targetTime - startTime) );     

      for (int i = 0; i < arrSkinnedMeshRenderer_.Length; i++)
      {
        uint idBody = arrIdBodySkinnedGameObjects_[i];
        SkinnedMeshRenderer smRenderer = arrSkinnedMeshRenderer_[i];

        GameObject gameObject = smRenderer.gameObject;
        smRenderer.BakeMesh(animBakingMesh);

        if (idBody != uint.MaxValue)
        {
          Matrix4x4 m_MODEL_TO_WORLD = gameObject.transform.localToWorldMatrix;
          RigidbodyManager.Rg_addEventTargetArrPos_WORLD(eventTime, targetTime, idBody, ref m_MODEL_TO_WORLD, animBakingMesh.vertices);
        }
      }

      for (int i = 0; i < arrNonSkinnedMeshRenderer_.Length; ++i)
      {
        uint idBody = arrIdBodyNormalGameObjects_[i];
        MeshRenderer renderer = arrNonSkinnedMeshRenderer_[i];
        GameObject gameObject = renderer.gameObject;

        if (idBody != uint.MaxValue)
        {
          Matrix4x4 m_MODEL_TO_WORLD = gameObject.transform.localToWorldMatrix;

          if ( crAnimation_.HasVertexAnimatedObjects(gameObject) )
          {
            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            RigidbodyManager.Rg_addEventTargetArrPos_WORLD(eventTime, targetTime, idBody, ref m_MODEL_TO_WORLD, mesh.vertices);
          }
          else
          {
            RigidbodyManager.Rg_addEventTargetPos_WORLD(eventTime, targetTime, idBody, ref m_MODEL_TO_WORLD);
          }

        }
      }
    }
  }
}


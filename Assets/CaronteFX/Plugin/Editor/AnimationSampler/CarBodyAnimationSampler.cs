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
  public abstract class CarBodyAnimationSampler
  {
    protected MeshRenderer[] arrNonSkinnedMeshRenderer_;
    protected SkinnedMeshRenderer[] arrSkinnedMeshRenderer_;

    protected uint[] arrIdBodyNormalGameObjects_;
    protected uint[] arrIdBodySkinnedGameObjects_;

    protected void AssignBodyIds()
    {
      CarManager cnManager = CarManager.Instance;
      CarEntityManager entityManager = cnManager.EntityManager;

      GameObject[] nonSkinnedObjects = getNonSkinnedObjects();
      arrIdBodyNormalGameObjects_ = new uint[nonSkinnedObjects.Length];

      for (int i = 0; i < nonSkinnedObjects.Length; i++)
      {
        GameObject go = nonSkinnedObjects[i];
        if (entityManager.IsGameObjectAnimated(go))
        {
          uint idBody = entityManager.GetIdBodyFromGo(go);
          arrIdBodyNormalGameObjects_[i] = idBody;
        }
        else
        {
          arrIdBodyNormalGameObjects_[i] = uint.MaxValue;
        }
      }


      GameObject[] skinnedObjects = getSkinnedObjects();
      arrIdBodySkinnedGameObjects_ = new uint[skinnedObjects.Length];

      for (int i = 0; i < skinnedObjects.Length; i++)
      {
        GameObject go = skinnedObjects[i];
        if (entityManager.IsGameObjectAnimated(go))
        {
          uint idBody = entityManager.GetIdBodyFromGo(go);
          arrIdBodySkinnedGameObjects_[i] = idBody;
        }
        else
        {
          arrIdBodySkinnedGameObjects_[i] = uint.MaxValue;
        }
      }

    }

    public GameObject[] getNonSkinnedObjects()
    {
      List<GameObject> listGameObject = new List<GameObject>();
      foreach (MeshRenderer mr in arrNonSkinnedMeshRenderer_)
      {
        listGameObject.Add(mr.gameObject);
      }

      return listGameObject.ToArray();
    }

    public GameObject[] getSkinnedObjects()
    {
      List<GameObject> listGameObject = new List<GameObject>();
      foreach (SkinnedMeshRenderer smr in arrSkinnedMeshRenderer_)
      {
        listGameObject.Add(smr.gameObject);
      }

      return listGameObject.ToArray();
    }

    public void SetModeAnimation(bool active)
    {
      foreach (uint idBody in arrIdBodyNormalGameObjects_)
      {
        if (idBody != uint.MaxValue)
        {
          RigidbodyManager.Rg_setAnimatingMode(idBody, active);
        }
      }

      foreach (uint idBody in arrIdBodySkinnedGameObjects_)
      {
        if (idBody != uint.MaxValue)
        {
          RigidbodyManager.Rg_setAnimatingMode(idBody, active);
        }
      }
    }

    public void AddStopAnimatingEvent(double eventTime)
    {
      foreach (uint idBody in arrIdBodyNormalGameObjects_)
      {
        if (idBody != uint.MaxValue)
        {
          RigidbodyManager.Rg_addEventStopAnimating(eventTime, idBody);
        }
      }

      foreach (uint idBody in arrIdBodySkinnedGameObjects_)
      {
        if (idBody != uint.MaxValue)
        {
          RigidbodyManager.Rg_addEventStopAnimating(eventTime, idBody);
        }
      }
    } 
  }
}



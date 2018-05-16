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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CarAnimationData
  {
    public float timeStart_;
    public float timeLenght_;

    bool absoluteTimeSamplingMode_;
    bool stopAnimatingEventAdded_;

    public bool overrideAnimatorController_;
    public AnimationClip clip_un_;

    public GameObject[] arrRootGameObjects_;

    public CNAnimatedbody.EAnimationType animationType_;

    public List<CarAnimatorSampler>  listCarAnimatorSampler_;
    public List<CarCRAnimationSampler> listCarCRAnimationSampler_;

    public CarAnimationData(CNAnimatedbodyEditor animNodeEditor)
    {
      timeStart_ = animNodeEditor.Data.TimeStart;
      timeLenght_ = animNodeEditor.Data.TimeLength;

      absoluteTimeSamplingMode_ = animNodeEditor.Data.AbsoluteTimeSamplingMode;
      stopAnimatingEventAdded_ = false;

      animationType_ = animNodeEditor.getAnimationType();

      if (animationType_ == CNAnimatedbody.EAnimationType.Animator)
      {
        overrideAnimatorController_ = animNodeEditor.Data.OverrideAnimationController;
        clip_un_ = animNodeEditor.Data.UN_AnimationClip;

        arrRootGameObjects_ = animNodeEditor.GetAnimationGameObjects<Animator>();
        listCarAnimatorSampler_ = new List<CarAnimatorSampler>();
        BuildCRAnimatorInfo();
      }
      else if (animationType_ == CNAnimatedbody.EAnimationType.CaronteFX)
      {
        arrRootGameObjects_ = animNodeEditor.GetAnimationGameObjects<CRAnimation>();
        listCarCRAnimationSampler_ = new List<CarCRAnimationSampler>();
        BuildCRAnimationInfo();
      }    
    }

    private void BuildCRAnimatorInfo()
    {
      listCarAnimatorSampler_.Clear();
      foreach (GameObject rootGameObject in arrRootGameObjects_)
      {
        listCarAnimatorSampler_.Add(new CarAnimatorSampler(rootGameObject));
      }
    }

    private void BuildCRAnimationInfo()
    {
      listCarCRAnimationSampler_.Clear();
      foreach (GameObject rootGameObject in arrRootGameObjects_)
      {
        listCarCRAnimationSampler_.Add(new CarCRAnimationSampler(rootGameObject));
      }
    }

    public void UpdateInfo()
    {
      if (animationType_ == CNAnimatedbody.EAnimationType.Animator)
      {
        for (int i = 0; i < listCarAnimatorSampler_.Count; i++)
        {
          CarAnimatorSampler animatorInfo = listCarAnimatorSampler_[i];
          GameObject rootGameObject = arrRootGameObjects_[i];

          animatorInfo.AssignTmpAnimatorController(rootGameObject, clip_un_, overrideAnimatorController_);
        }
      }
      else if (animationType_ == CNAnimatedbody.EAnimationType.CaronteFX)
      {
        for (int i = 0; i < listCarCRAnimationSampler_.Count; i++)
        {
          CarCRAnimationSampler crAnimationInfo = listCarCRAnimationSampler_[i];
          GameObject rootGameObject = arrRootGameObjects_[i];

          crAnimationInfo.AssignTmpAnimationController(rootGameObject);
        }
      }
    }


    public void UpdateSimulating(UnityEngine.Mesh animBakingMesh, double eventTime, double deltaTimeAnimation, double deltaTimeSimulation, double startTime)
    {
      if (animationType_ == CNAnimatedbody.EAnimationType.Animator)
      {
        foreach (CarAnimatorSampler animatorSampler in listCarAnimatorSampler_)
        {
          animatorSampler.UpdateSimulating(this, absoluteTimeSamplingMode_, animBakingMesh, eventTime, deltaTimeAnimation, deltaTimeSimulation, startTime);
        }
      }
      else if (animationType_ == CNAnimatedbody.EAnimationType.CaronteFX)
      {
        foreach (CarCRAnimationSampler crAnimationSampler in listCarCRAnimationSampler_)
        {
          crAnimationSampler.UpdateSimulating(this, animBakingMesh, eventTime, deltaTimeAnimation, deltaTimeSimulation, startTime);
        }
      }
    }

    public void AddStopAnimatingEventIfNotAdded(double eventTime)
    {
      if (!stopAnimatingEventAdded_)
      {
        if (animationType_ == CNAnimatedbody.EAnimationType.Animator)
        {
          foreach (CarAnimatorSampler animatorSampler in listCarAnimatorSampler_)
          {
            animatorSampler.AddStopAnimatingEvent(eventTime);
          }
        }
        else if (animationType_ == CNAnimatedbody.EAnimationType.CaronteFX)
        {
          foreach (CarCRAnimationSampler crAnimationSampler in listCarCRAnimationSampler_)
          {
            crAnimationSampler.AddStopAnimatingEvent(eventTime);
          }
        }
        stopAnimatingEventAdded_ = true;
      }
    }

    public void Reset()
    {
      if (animationType_ == CNAnimatedbody.EAnimationType.Animator)
      {
        foreach (CarAnimatorSampler animatorSampler in listCarAnimatorSampler_)
        {
          animatorSampler.Reset();
        }
      }
    }

    public void SetModeAnimation(bool active)
    {
      if (animationType_ == CNAnimatedbody.EAnimationType.Animator)
      {
        foreach (CarAnimatorSampler animatorInfo in listCarAnimatorSampler_)
        {
          animatorInfo.SetModeAnimation(active);
        }
      }
      else if (animationType_ == CNAnimatedbody.EAnimationType.CaronteFX)
      {
        foreach (CarCRAnimationSampler crAnimationInfo in listCarCRAnimationSampler_)
        {
          crAnimationInfo.SetModeAnimation(active);
        }
      }
    }
  }
}

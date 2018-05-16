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
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  [AddComponentMenu("")] 
  public class CNAttractor : CNEntity
  {
    [SerializeField]
    public bool isRepulsorVsAttractor_ = false;
    public bool IsRepulsorVsAttractor
    {
      get { return isRepulsorVsAttractor_; }
      set { isRepulsorVsAttractor_ = value; }
    }

    [SerializeField]
    private bool isForceVsAcceleration_ = false;
    public bool IsForceVsAcceleration
    {
      get { return isForceVsAcceleration_; }
      set { isForceVsAcceleration_ = value; }
    }

    [SerializeField]
    float forceOrAcceleration_ = 10.0f;
    public float ForceOrAcceleration
    {
      get { return forceOrAcceleration_; }
      set { forceOrAcceleration_ = value; }
    }

    [SerializeField]
    float radius_ = 10.0f;
    public float Radius
    {
      get { return radius_; }
      set { radius_ = value; }
    }

    [SerializeField]
    float decay_ = 0.5f;
    public float Decay
    {
      get { return decay_; }
      set { decay_ = value; }
    }

    [SerializeField]
    GameObject attractorGameObject_ = null;
    public GameObject AttractorGameObject
    {
      get { return attractorGameObject_; }
      set { attractorGameObject_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.AttractorNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNAttractor originalAtt = (CNAttractor)original;

      isRepulsorVsAttractor_ = originalAtt.isRepulsorVsAttractor_;
      isForceVsAcceleration_ = originalAtt.isForceVsAcceleration_;
      forceOrAcceleration_   = originalAtt.forceOrAcceleration_; 
        
      radius_ = originalAtt.radius_;
      decay_  = originalAtt.decay_;
    }
    //-----------------------------------------------------------------------------------
  }
}

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
  /// <summary>
  /// Holds the data of a speed limiter node.
  /// </summary>
  [AddComponentMenu("")]
  public class CNSpeedLimiter : CNEntity
  {
    [SerializeField]
    private float speed_limit_ = 20f;
    public float SpeedLimit
    {
      get { return speed_limit_; }
      set { speed_limit_ = value; }
    }

    [SerializeField]
    private float falling_speed_limit_ = 50f;
    public float FallingSpeedLimit
    {
      get { return falling_speed_limit_; }
      set { falling_speed_limit_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.SpeedLimiterNode; } }

    //-----------------------------------------------------------------------------------
    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNSpeedLimiter originalSl = (CNSpeedLimiter)original;

      speed_limit_         = originalSl.speed_limit_;
      falling_speed_limit_ = originalSl.falling_speed_limit_;   
    }
    //-----------------------------------------------------------------------------------
  }
}
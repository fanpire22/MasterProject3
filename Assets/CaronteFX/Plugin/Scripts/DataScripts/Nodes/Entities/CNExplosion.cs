// ***********************************************************
//	Copyright 2016 Next Limit Technologies, http://www.nextlimit.com
//	All rights reserved.
//
//	THIS SOFTWARE IS PROVIDED 'AS IS' AND WITHOUT ANY EXPRESS OR
//	IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//
// ***********************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  [AddComponentMenu("")]
  public class CNExplosion : CNEntity
  {

    [SerializeField]
    private Transform explosion_Transform_;
    public Transform Explosion_Transform
    {
      get { return explosion_Transform_; }
      set { explosion_Transform_ = value; }
    }

    [SerializeField]
    private int resolution_ = 1;
    public int Resolution
    {
      get { return resolution_; }
      set { resolution_ = Mathf.Clamp(value, 1, 3); }
    }

    [SerializeField]
    private int renderStepSize_ = 1;
    public int RenderStepSize
    {
      get { return renderStepSize_; }
      set { renderStepSize_ = value;}
    }

    [SerializeField]
    private float wave_front_speed_ = 10f;
    public float Wave_front_speed
    {
      get { return wave_front_speed_; }
      set { wave_front_speed_ = Mathf.Clamp(value, 0f, 500f); }
    }

    [SerializeField]
    private float range_ = 5f;
    public float Range
    {
      get { return range_; }
      set { range_ = Mathf.Clamp(value, 0f, 10000f); }
    }

    [SerializeField]
    private float decay_ = 0.5f;
    public float Decay
    {
      get { return decay_; }
      set { decay_ = Mathf.Clamp01(value); }
    }

    [SerializeField]
    private float momentum_ = 80000;
    public float Momentum
    {
      get { return momentum_; }
      set { momentum_ = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    [SerializeField]
    private float objects_limit_speed_ = 50f;
    public float Objects_limit_speed
    {
      get { return objects_limit_speed_; }
      set { objects_limit_speed_ = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    [SerializeField]
    private bool asymmetry_ = false;
    public bool Asymmetry
    {
      get { return asymmetry_; }
      set { asymmetry_ = value; }
    }

    [SerializeField]
    private int asymmetry_random_seed_ = 0;
    public int Asymmetry_random_seed
    {
      get { return asymmetry_random_seed_; }
      set { asymmetry_random_seed_ = value; }
    }

    [SerializeField]
    private int asymmetry_bump_number_ = 16;
    public int Asymmetry_bump_number
    {
      get { return asymmetry_bump_number_; }
      set { asymmetry_bump_number_ = Mathf.Clamp(value, 0, 1024); }
    }

    [SerializeField]
    private float asymmetry_additional_speed_ratio_ = 0.25f;
    public float Asymmetry_additional_speed_ratio
    {
      get { return asymmetry_additional_speed_ratio_; }
      set { asymmetry_additional_speed_ratio_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }


    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.ExplosionNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);
      CNExplosion originalEx = (CNExplosion)original;

      if (originalEx.Explosion_Transform != null)
      {
        Explosion_Transform =  UnityEngine.Object.Instantiate(originalEx.Explosion_Transform);
      }
      
      Resolution                        =  originalEx.Resolution; 
      renderStepSize_                   =  originalEx.RenderStepSize;                  
      Wave_front_speed                  =  originalEx.Wave_front_speed;
      Range                             =  originalEx.Range;
      Decay                             =  originalEx.Decay;
      Momentum                          =  originalEx.Momentum;
      Timer                             =  originalEx.Timer;
      Objects_limit_speed               =  originalEx.Objects_limit_speed;
      Asymmetry                         =  originalEx.Asymmetry;
      Asymmetry_random_seed             =  originalEx.Asymmetry_random_seed;
      Asymmetry_bump_number             =  originalEx.Asymmetry_bump_number;
      Asymmetry_additional_speed_ratio  =  originalEx.Asymmetry_additional_speed_ratio;
    }

  }
}
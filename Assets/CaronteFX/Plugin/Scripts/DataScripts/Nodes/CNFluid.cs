// ***********************************************************
//	Copyright 2016 Next Limit Technologies, http://www.nextlimit.com
//	All rights reserved.
//
//	THIS SOFTWARE IS PROVIDED 'AS IS' AND WITHOUT ANY EXPRESS OR
//	IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//
// ***********************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  [AddComponentMenu("")]
  public class CNFluid : CNCorpuscles
  {
    [SerializeField]
    private float wall_stiffness_ = 0.5f;
    public float Wall_Stiffness
    {
      get { return wall_stiffness_; }
      set { wall_stiffness_ = value; }
    }

    [SerializeField]
    private float wall_damping_ = 8.0f;
    public float Wall_Damping
    {
      get { return wall_damping_; }
      set { wall_damping_ = value; }
    }

    [SerializeField]
    private float cohesion_acceleration_ = 0.8f;
    public float Cohesion_Acceleration
    {
      get { return cohesion_acceleration_; }
      set { cohesion_acceleration_ = value; } 
    }

    [SerializeField]
    private float viscosity_damping_ = 40.0f;
    public float Viscosity_Damping
    {
      get { return viscosity_damping_; }
      set { viscosity_damping_ = value; }
    }
    //-----------------------------------------------------------------------------------
    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNFluid originalFluid = (CNFluid)original;

      wall_stiffness_        = originalFluid.wall_stiffness_;
      wall_damping_          = originalFluid.wall_damping_;
      cohesion_acceleration_ = originalFluid.cohesion_acceleration_;
      viscosity_damping_     = originalFluid.viscosity_damping_;
    }
    //-----------------------------------------------------------------------------------

  }// class CNCorpuscles

} // namespace CaronteFX...

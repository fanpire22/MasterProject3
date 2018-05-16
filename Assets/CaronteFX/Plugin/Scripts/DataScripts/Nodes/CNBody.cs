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
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  /// <summary>
  /// Holds the data of a node of generic bodies.
  /// </summary>
  [AddComponentMenu("")]
  public abstract class CNBody : CNMonoField
  {   
 
    public override CNField Field
    {
      get
      {
        if (field_ == null)
        {
          field_ = new CNField(true, false);
        }
        return field_;
      }
    }

    [SerializeField]
    protected bool doCollide_ = true;
    public bool DoCollide
    {
      get { return doCollide_; }
      set { doCollide_ = value; }
    }

    [SerializeField]
    protected bool isSolidVsShell_ = true;
    public bool IsSolidVsShell
    {
      get { return isSolidVsShell_; }
      set { isSolidVsShell_ = value; }
    }

    [SerializeField]
    protected float mass_ = -1f;
    public float Mass
    {
      get { return mass_; }
      set { mass_ = value; }
    }

    [SerializeField]
    protected float density_ = 1000f;
    public float Density
    {
      get { return density_; }
      set { density_ = value; }
    }

    [SerializeField, Range(0f, 1f)]
    protected float restitution_in01_ = 0.2f;
    public float Restitution_in01
    {
      get { return restitution_in01_; }
      set { restitution_in01_ = value; }
    }

    [SerializeField, Range(0f, 1f)]
    protected float frictionKinetic_in01_ = 0.3f;
    public float FrictionKinetic_in01
    {
      get { return frictionKinetic_in01_; }
      set { frictionKinetic_in01_ = value; }
    }

    [SerializeField, Range(0f, 1f)]
    protected float frictionStatic_in01_ = 0.0f;
    public float FrictionStatic_in01
    {
      get { return frictionStatic_in01_; }
      set { frictionStatic_in01_ = value; }
    }

    [SerializeField]
    protected bool fromKinetic_ = true;
    public bool FromKinetic
    {
      get { return fromKinetic_; }
      set { fromKinetic_ = value; }
    }

    [SerializeField]
    protected Vector3 gravity_ = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 Gravity
    {
      get { return gravity_; }
      set { gravity_ = value; }
    }

    [SerializeField]
    protected float dampingPerSecond_WORLD_ = 0.01f;
    public float DampingPerSecond_WORLD
    {
      get { return dampingPerSecond_WORLD_; }
      set { dampingPerSecond_WORLD_ = value; }
    }

    [SerializeField]
    protected Vector3 velocityStart_ = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 VelocityStart
    {
      get { return velocityStart_; }
      set { velocityStart_ = value; }
    }

    [SerializeField]
    protected Vector3 omegaStart_inRadSeg_ = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 OmegaStart_inRadSeg
    {
      get { return omegaStart_inRadSeg_; }
      set { omegaStart_inRadSeg_ = value; }
    }

    [SerializeField]
    protected Vector3 omegaStart_inDegSeg_ = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 OmegaStart_inDegSeg
    {
      get { return omegaStart_inDegSeg_; }
      set { omegaStart_inDegSeg_ = value; }
    }

    [SerializeField, Range(0f, 1f)]
    protected float explosionOpacity_ = 0.75f;
    public float ExplosionOpacity
    {
      get { return explosionOpacity_; }
      set { explosionOpacity_ = value; }
    }

    [SerializeField, Range(0f, 1f)]
    protected float explosionResponsiveness_ = 1.0f;
    public float ExplosionResponsiveness
    {
      get { return explosionResponsiveness_; }
      set { explosionResponsiveness_ = value; }
    }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNBody originalBody = (CNBody)original;

      doCollide_               = originalBody.doCollide_;
      isSolidVsShell_          = originalBody.isSolidVsShell_;
      mass_                    = originalBody.mass_;
      density_                 = originalBody.density_;

      restitution_in01_        = originalBody.restitution_in01_;
      frictionKinetic_in01_    = originalBody.frictionKinetic_in01_;
      frictionStatic_in01_     = originalBody.frictionStatic_in01_;
      fromKinetic_             = originalBody.fromKinetic_;

      gravity_                 = originalBody.gravity_;
      dampingPerSecond_WORLD_  = originalBody.dampingPerSecond_WORLD_;

      velocityStart_           = originalBody.velocityStart_;
      omegaStart_inRadSeg_     = originalBody.omegaStart_inRadSeg_;
      omegaStart_inDegSeg_     = originalBody.omegaStart_inDegSeg_;

      explosionOpacity_        = originalBody.explosionOpacity_;
      explosionResponsiveness_ = originalBody.explosionResponsiveness_;
    }
  }
}
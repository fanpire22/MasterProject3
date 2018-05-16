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

namespace CaronteFX
{

  [AddComponentMenu("")]
  public class CNRope : CNBody
  {
    [SerializeField]
    private int sides_ = 6;
    public int Sides
    {
      get { return sides_; }
      set { sides_ = value; }
    }

    [SerializeField]
    private float stretch_ = 500f;
    public float Stretch
    {
      get { return stretch_;} 
      set { stretch_ = value; }
    }

    [SerializeField]
    private float bend_ = 0.1f;
    public float Bend
    {
      get { return bend_;} 
      set { bend_ = value; }
    }

    [SerializeField]
    private float torsion_ = 0.1f;
    public float Torsion
    {
      get { return torsion_;} 
      set { torsion_ = value; }
    }

    [SerializeField]
    private bool autoCollide_ = true;
    public bool AutoCollide
    {
      get { return autoCollide_; }
      set { autoCollide_ = value; }
    }

    [SerializeField]
    bool disableCollisionNearJoints_ = true;
    public bool DisableCollisionNearJoints
    {
      get { return disableCollisionNearJoints_; }
      set { disableCollisionNearJoints_ = value; }
    }
   
    [SerializeField]   
    private float dampingPerSecond_CM_ = 1000f;
    public float DampingPerSecond_CM
    {
      get { return dampingPerSecond_CM_; }
      set { dampingPerSecond_CM_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.RopeBodyNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNRope originalRp = (CNRope)original;

      sides_ = originalRp.sides_;

      stretch_ = originalRp.stretch_;
      bend_    = originalRp.bend_;
      torsion_ = originalRp.torsion_;

      dampingPerSecond_CM_ = originalRp.dampingPerSecond_CM_;
    }


  } // CNRope...

} //namespace Caronte...

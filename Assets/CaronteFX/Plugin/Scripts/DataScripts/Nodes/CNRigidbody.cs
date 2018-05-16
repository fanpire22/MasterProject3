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
using System;
using UnityEngine;

namespace CaronteFX
{
  /// <summary>
  /// Holds the data of a rigid bodies node.
  /// </summary>
  [AddComponentMenu("")]
  public class CNRigidbody : CNBody
  {
    [SerializeField]
    bool useBallTree_ = false;
    public bool UseBallTree
    {
      get { return useBallTree_; }
      set { useBallTree_ = value; }
    }

    [SerializeField]
    int balltreeLOD_ = 1;
    public int BalltreeLOD
    {
      get { return balltreeLOD_; }
      set { balltreeLOD_ = value; }
    }

    [SerializeField]
    protected bool isFiniteMass_ = true;
    public bool IsFiniteMass 
    { 
      get { return isFiniteMass_; }
      set { isFiniteMass_ = value; }
    }

    public override CNFieldContentType FieldContentType
    {
      get
      {
        if (IsFiniteMass)
        {
          return CNFieldContentType.RigidBodyNode;
        }
        else
        {
          return CNFieldContentType.IrresponsiveNode;
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNRigidbody rbOriginal = (CNRigidbody)original;

      useBallTree_  = rbOriginal.useBallTree_;
      balltreeLOD_  = rbOriginal.balltreeLOD_;
      isFiniteMass_ = rbOriginal.isFiniteMass_;
    }
    //-----------------------------------------------------------------------------------
  } //class CNRigidbody

}//namespace CaronteFX...
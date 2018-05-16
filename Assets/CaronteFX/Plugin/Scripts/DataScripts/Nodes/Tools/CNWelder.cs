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
  /// <summary>
  /// Holds the data of a welder node.
  /// </summary>
  [AddComponentMenu("")] 
  public class CNWelder : CNMonoField
  {
    public override CNField Field
    {
      get
      {
        if (field_ == null)
        {
          field_ = new CNField(false, false);
        }
        return field_;
      }
    }

    [SerializeField]
    GameObject weldGameObject_;
    public UnityEngine.GameObject WeldGameObject
    {
      get { return weldGameObject_; }
      set { weldGameObject_ = value; }
    }

    [SerializeField]
    Mesh[] arrWeldedMesh_;
    public Mesh[] ArrWeldedMesh
    {
      get { return arrWeldedMesh_; }
      set { arrWeldedMesh_ = value; }
    }

    [SerializeField]
    GameObject[] arrWeldedGO_;
    public GameObject[] ArrWeldedGO
    {
      get { return arrWeldedGO_; }
      set { arrWeldedGO_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.None; } }


    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNWelder originalWel = (CNWelder)original;

      weldGameObject_ = originalWel.weldGameObject_;

    }

  } //namespace CNWelder
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  [AddComponentMenu("")] 
  public class CNPivotModifier : CNMonoField
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

    public enum EPivotLocationMode
    {
      boxCenter, 
      boxBottomCenter,
    }

    [SerializeField]
    EPivotLocationMode pivotLocationMode_;
    public EPivotLocationMode PivotLocationMode
    {
      get { return pivotLocationMode_; }
      set { pivotLocationMode_ = value; }
    }

    [SerializeField]
    Vector3 localPivotOffset_;
    public Vector3 LocalPivotOffset
    {
      get { return localPivotOffset_; }
      set { localPivotOffset_ = value; }
    }

    [SerializeField]
    Mesh[] arrOriginalMesh_;
    public Mesh[] ArrOriginalMesh
    {
      get { return arrOriginalMesh_; }
      set { arrOriginalMesh_ = value; }
    }

    [SerializeField]
    Mesh[] arrModifiedMesh_;
    public Mesh[] ArrModifiedMesh
    {
      get { return arrModifiedMesh_; }
      set { arrModifiedMesh_ = value; }
    }

    [SerializeField]
    GameObject[] arrModifiedGO_;
    public GameObject[] ArrModifiedGO
    {
      get { return arrModifiedGO_; }
      set { arrModifiedGO_ = value; }
    }

    [SerializeField]
    Vector3[] arrMeshMove_;
    public Vector3[] ArrMeshMove
    {
      get { return arrMeshMove_; }
      set { arrMeshMove_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.None; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNPivotModifier originalPm = (CNPivotModifier)original;

      pivotLocationMode_ = originalPm.pivotLocationMode_;
      localPivotOffset_  = originalPm.localPivotOffset_;
    }


  }

}


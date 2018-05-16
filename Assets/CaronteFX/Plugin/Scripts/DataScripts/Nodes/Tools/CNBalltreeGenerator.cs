using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  /// <summary>
  /// Holds the data of a balltree creator node.
  /// </summary>

  [AddComponentMenu("")] 
  public class CNBalltreeGenerator : CNMonoField
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

    public enum ECreationMode
    {
      USERENDERERS,
      USECOLLLIDERS
    }

    [SerializeField]
    ECreationMode creationMode_;
    public ECreationMode CreationMode
    {
      get { return creationMode_; }
      set { creationMode_ = value; }
    }

    [SerializeField]
    float balltreeLOD_ = 0.5f;
    public float BalltreeLOD
    {
      get { return balltreeLOD_; }
      set { balltreeLOD_ = value; }
    }

    [SerializeField]
    float balltreePrecision_ = 0.5f;
    public float BalltreePrecision
    {
      get { return balltreePrecision_; }
      set { balltreePrecision_ = value; }
    }

    [SerializeField]
    float balltreeHoleCovering_ = 0.0f;
    public float BalltreeHoleCovering
    {
      get { return balltreeHoleCovering_; }
      set { balltreeHoleCovering_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.None; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNBalltreeGenerator originalBtg = (CNBalltreeGenerator)original;

      creationMode_      = originalBtg.creationMode_;
      balltreeLOD_       = originalBtg.balltreeLOD_;
      balltreePrecision_ = originalBtg.balltreePrecision_;
    }
  }
}

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
using UnityEngine;

namespace CaronteFX
{
  /// <summary>
  /// Holds the data of a soft bodies node.
  /// </summary>
  [AddComponentMenu("")]
  public class CNSoftbody : CNBody
  {
    [SerializeField]
    private int resolution_ = 64;
    public int Resolution
    {
      get { return resolution_;} 
      set { resolution_ = value; }
    }

    [SerializeField]
    private bool autoCollide_ = false;
    public bool AutoCollide
    {
      get { return autoCollide_; }
      set { autoCollide_ = value; }
    }

    [SerializeField]
    private float lengthStiffness_ = 0.3f;
    public float LengthStiffness
    {
      get { return lengthStiffness_; }
      set { lengthStiffness_ = value; }
    }

    [SerializeField]
    private float volumeStiffness_ = 0.3f;
    public float VolumeStiffness
    {
      get { return volumeStiffness_; }
      set { volumeStiffness_ = value; }
    }

    [SerializeField]
    private float areaStiffness_ = 0.3f;
    public float AreaStiffness
    {
      get { return areaStiffness_; }
      set { areaStiffness_ = value; }
    }

    [SerializeField]
    private bool plasticityFoldout_ = false;
    public bool PlasticityFoldout
    {
      get { return plasticityFoldout_; }
      set { plasticityFoldout_ = value; }
    }

    [SerializeField]
    private bool plasticity_ = false;
    public bool Plasticity
    {
      get { return plasticity_; }
      set { plasticity_ = value; }
    }

    [SerializeField]
    private float threshold_in01_ = 0.1f;
    public float Threshold_in01
    {
      get { return threshold_in01_; }
      set { threshold_in01_ = value; }
    }
    
    [SerializeField]
    private float acquired_in01_ = 0.8f;
    public float Acquired_in01
    {
      get { return acquired_in01_; }
      set { acquired_in01_ = value; }
    }

    [SerializeField]   
    private float compressionLimit_in01_ = 0.1f; 
    public float CompressionLimit_in01
    {
      get { return compressionLimit_in01_; }
      set { compressionLimit_in01_ = value; }
    }
    
    [SerializeField]   
    private float expansionLimit_in_1_100_ = 1.5f;
    public float ExpansionLimit_in_1_100
    {
      get { return expansionLimit_in_1_100_; }
      set { expansionLimit_in_1_100_ = value; }
    }
      
    [SerializeField]   
    private float dampingPerSecond_CM_ = 0.25f;
    public float DampingPerSecond_CM
    {
      get { return dampingPerSecond_CM_; }
      set { dampingPerSecond_CM_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.SoftBodyNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNSoftbody originalSb = (CNSoftbody)original;

      resolution_      = originalSb.resolution_;
      
      lengthStiffness_ = originalSb.lengthStiffness_;
      volumeStiffness_ = originalSb.volumeStiffness_;
      areaStiffness_   = originalSb.areaStiffness_;
      
      plasticity_ = originalSb.plasticity_;
      
      threshold_in01_          = originalSb.threshold_in01_;
      acquired_in01_           = originalSb.acquired_in01_;
      compressionLimit_in01_   = originalSb.compressionLimit_in01_;
      expansionLimit_in_1_100_ = originalSb.expansionLimit_in_1_100_;
      
      dampingPerSecond_CM_ = originalSb.dampingPerSecond_CM_;
    }


  } // class CNSoftbody...

} //namespace Caronte...
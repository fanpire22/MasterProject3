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
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CaronteFX.AnimationFlags;

namespace CaronteFX
{
  [System.Serializable]
  public class CRAnimationEvent : UnityEvent<CRAnimationEvData>
  {
  }

  [AddComponentMenu("CaronteFX/CaronteFX Animation")]
  public class CRAnimation : MonoBehaviour, IAnimatorExporter
  {
    [System.Serializable]
    public enum RepeatMode
    {
      Loop,
      Clamp,
      PingPong,
    };

    [System.Serializable]
    public enum AnimationFileType
    {
      CRAnimationAsset,
      TextAsset,
    };

    public float            speed               = 1.0f;
    public RepeatMode       repeatMode          = RepeatMode.Loop;
    public CRAnimationEvent animationEvent      = null;
    public bool             animate             = true;
    public bool             interpolate         = false;
    public bool             doRuntimeNullChecks = false;

    [HideInInspector]
    public AnimationFileType animationFileType = AnimationFileType.CRAnimationAsset;
    [HideInInspector]
    public CRAnimationAsset activeAnimation = null;
    [HideInInspector]
    public List<CRAnimationAsset> listAnimations = new List<CRAnimationAsset>();
    [HideInInspector]
    public TextAsset activeAnimationText = null;
    [HideInInspector]
    public List<TextAsset> listAnimationsText = new List<TextAsset>();

    #region Public properties
    public float AnimationTime
    {
      get { return time_; }
    }

    public bool Interpolate
    {
      get { return interpolate; }
      set { interpolate = value; }
    }

    public int FrameCount
    {
      get { return frameCount_; }
    }

    public float FrameTime
    {
      get { return frameTime_; }
    }

    public int Fps
    {
      get { return fps_; }
    }

    public float AnimationLength
    {
      get { return animationLength_; }
    }

    public int LastReadFrame
    {
      get { return lastReadFrame_; }
    }

    public int LastFrame
    {
      get { return lastFrame_; }
    }
   
    public bool PreviewInEditor
    {
      get { return previewInEditor_;  }
      set { previewInEditor_ = value;  }
    }

    public bool IsPreviewing
    {
      get { return isPreviewing_; }
      set { isPreviewing_ = value; }
    }

    public Animator AnimatorSync
    {
      get { return animatorSync_; }
      set { animatorSync_ = value; }
    }

    public float StartTimeOffset
    {
      get { return startTimeOffset_; }
      set { startTimeOffset_ = value; }
    }

    public bool GPUVertexAnimation
    {
      get { return decodeInGPU_;  }
      set { decodeInGPU_ = value; }
    }

    public bool GPUSkinnedAnimation
    {
      get { return skinningInGPU_; }
      set { skinningInGPU_ = value; }
    }

    public bool CanRecomputeNormals
    {
      get { return hasVertexLocalSystems_; }
    }

    public CarAnimationPersistence AnimationPersistence
    {
      get { return animationPersistence_; }
      set { animationPersistence_ = value; }
    }
    #endregion

    #region Private serialized members
    [NonSerialized]
    private EFileHeaderFlags fileHeaderFlags_ = EFileHeaderFlags.NONE;

    [SerializeField, HideInInspector]
    private List<CRGOTmpData> listGOTmpData_ = new List<CRGOTmpData>();
    [SerializeField, HideInInspector]
    private bool previewInEditor_ = false;
    [SerializeField, HideInInspector]
    private bool isPreviewing_ = false;
    [SerializeField, HideInInspector]
    private Animator animatorSync_ = null;
    [SerializeField, HideInInspector]
    private float startTimeOffset_ = 0.0f;

    [SerializeField, HideInInspector]
    private bool decodeInGPU_ = false;
    [SerializeField, HideInInspector]
    private bool skinningInGPU_ = false;
    [SerializeField, HideInInspector]
    private bool bufferAllFrames_ = false;

    [SerializeField, HideInInspector]
    [Range(1, 3)]
    private int gpuFrameBufferSize_ = 1;
    [SerializeField, HideInInspector]
    private bool overrideShaderForVertexAnimation_ = true;
    [SerializeField, HideInInspector]
    private bool useDoubleSidedShader_ = true;
    [SerializeField, HideInInspector]
    private bool recomputeNormals_ = true;

    [SerializeField, HideInInspector]
    private CarAnimationPersistence animationPersistence_;
    #endregion

    #region Private non serialized members
    float time_ = 0.0f;
    int frameCount_ = 0;
    
    float frameTime_ = 0.0f;
    int fps_ = 0;
    
    float animationLength_ = 0.0f;
    int lastFrame_ = 0;
    
    int nGameObjects_;
 
    int      nEmitters_ = 0;
    string[] arrEmitterName_;

    int      nSkinnedGO_ = 0;
    GameObject[] arrSkinnedGO_;
    MeshFilter[] arrSkinnedGOMF_;
  
    CRAnimationAsset animationLastLoaded_;
    TextAsset animationLastLoadedText_;

    CRAnimationAsset animationMinimalLastLoaded_;
    TextAsset animationMinimalLastLoadedText_;
    
    int lastReadFrame_ = -1;
    float lastReadFloatFrame_ = -1;

    Shader vertexAnimShader_ = null;
    
    ComputeShader cShaderPositions_     = null;
    ComputeShader cShaderNormals_       = null;
    ComputeShader cShaderInterpolation_ = null;

    ComputeShader cShaderSkinning_      = null;

    ComputeShader cShaderComputeBones_             = null;
    ComputeShader cShaderComputeBonesInter_ = null;
    
    CarGPUBufferer gpuBufferer_;
    CarVertexAnimatedGPUBuffers[]  arrVertexAnimatedGPUBuffers_;
    CarSkinnedAnimatedGPUBuffers[] arrSkinnedAnimatedGPUBuffers_;
    
    byte[] binaryAnim_;
    
    int bCursor1_ = 0;
    int bCursor2_ = 0;
    
    double timeInternal_ = 0.0;
    
    CarAnimatedGO[] arrAnimatedGO_;
    Transform[]     arrBoneTr_;
    int[]           arrBoneOptimizedIdx_;
    
    BitArray  arrSkipObject_;
    BitArray  arrIsBoneAnimatedObject_;
    Vector2[] arrVisibilityInterval_;
    Mesh[]    arrMesh_;
    
    CarDefinition[]     arrDefinition_;
    CarCompressedPose[] arrCompressedPose_;
    
    CarVertexDataCache[] arrVertexDataCache_;
    
    long[]      arrFrameOffsets_;
    int[]       arrCacheIndex_;
    Vector3[][] arrVertex3Cache1_;
    Vector3[][] arrVertex3Cache2_;
    Vector4[][] arrVertex4Cache_;
    bool        interpolationModeActive_;
    
    int  binaryVersion_;
    bool vertexCompression_;
    bool fiberCompression_;
    bool boxCompression_;
    bool hasVertexLocalSystems_;
    bool hasTangentsData_;
    bool hasSkinnedBoxes_;
    bool isAlignedData_;
    
    bool internalPaused_;
    bool internalDOGPUVertexAnim_;
    bool internalDoGPUSkinnedAnim_;
    int  internalGPUBufferSize_;
    int  gpuframeIterator_;
 
    Dictionary<int, int> dictVertexCountCacheIdx_ = new Dictionary<int, int>();
    List<int>            listVertexCacheCount_    = new List<int>();
    HashSet<GameObject>  setVertexAnimatedGO_     = new HashSet<GameObject>();

    List<Transform> listBonesTransformAux_ = new List<Transform>();
    List<int> listBonesOptimizedIdxAux_ = new List<int>();
    
    CRContactEvData ceData = new CRContactEvData();

    delegate void ReadFrameDel(float frame);
    ReadFrameDel readFrameDel_ = new ReadFrameDel( (frame) => {} );

    const int textureWidth = 256;

    const float vecQuantz = 1.0f / 127.0f;
    const float posQuantz = 1.0f / 65535.0f;
    
    const int nLocationComponents = 3;
    const int nRotationComponents = 4;
    const int nScaleComponents    = 3;
    
    const int nBoxComponents = 6;
    const int nPositionComponents = 3;
    const int nNormalComponents   = 3;
    const int nTangentComponents  = 4;

    const int nBoxBytes = nBoxComponents * nFloatBytes;
    const int rqBytes   = (nLocationComponents + nRotationComponents) * nFloatBytes;
    const int boneBytes = (nLocationComponents + nRotationComponents + nScaleComponents) * nFloatBytes;
    
    const int nFloatBytes  = sizeof(float);
    const int nUInt16Bytes = sizeof(UInt16);
    const int nSByteBytes   = sizeof(sbyte);

    #endregion

    #region Unity callbacks
    void Awake()
    {
      LoadActiveAnimation(false);
    }

    void Start()
    {
      ReadFrameAtCurrentTime();
    }

    void Update()
    {
      CheckLoadAnimationChanged(false);

      if (animate && !internalPaused_ && binaryAnim_ != null)
      {
        timeInternal_ += Time.deltaTime  * speed;

        if (animatorSync_ != null)
        {
          AnimatorStateInfo animState = animatorSync_.GetCurrentAnimatorStateInfo(0);
          timeInternal_ = (animState.normalizedTime % 1.0f) * animState.length;
        }

        switch (repeatMode)
        {
          case RepeatMode.Loop:
            time_ = Mathf.Repeat((float)timeInternal_, animationLength_);
            break;
          case RepeatMode.PingPong:
            time_ = Mathf.PingPong((float)timeInternal_, animationLength_);
            break;
          case RepeatMode.Clamp:
            time_ = Mathf.Clamp((float)timeInternal_, 0.0f, animationLength_);
            break;
        }

        ReadFrameAtCurrentTime();
      }
    }

    void OnApplicationPause(bool pauseStatus) 
    {
      internalPaused_ = pauseStatus;
    }

    void OnDestroy()
    {
      CloseActiveAnimation();
    }
    #endregion

    #region Public methods

    #region Animation tracks
    public void AddAnimationAndSetActive(CRAnimationAsset animationAsset)
    {
      AddAnimation(animationAsset);

      activeAnimation = animationAsset;
      animationFileType = AnimationFileType.CRAnimationAsset;
    }

    public void AddAnimation(CRAnimationAsset animationAsset)
    {
      if ( !listAnimations.Contains(animationAsset) )
      {
        listAnimations.Add(animationAsset);
      }
    }

    public void RemoveAnimation(CRAnimationAsset animationAsset)
    {
      listAnimations.Remove(animationAsset);
    }

    public void AddAnimationAndSetActive(TextAsset textAsset)
    {
      AddAnimation(textAsset);

      activeAnimationText = textAsset;
      animationFileType = AnimationFileType.TextAsset;
    }
   
    public void AddAnimation(TextAsset textAsset)
    {
      if (!listAnimationsText.Contains(textAsset))
      {
        listAnimationsText.Add(textAsset);
      }
    }

    public void RemoveAnimation(TextAsset textAsset)
    {
      listAnimationsText.Remove(textAsset);
    }

    public void ChangeToAnimationTrack(int trackIdx)
    {
      if (trackIdx < 0)
      {
        return;
      }
  
      bool isCRAnimationAsset = animationFileType == AnimationFileType.CRAnimationAsset;
      bool isTextAsset        = animationFileType == AnimationFileType.TextAsset;

      if ( isCRAnimationAsset )
      {
        if ( trackIdx < listAnimations.Count)
        {
          activeAnimation = listAnimations[trackIdx];
        }
        else
        {
          activeAnimation = null;
        }      
      }
      else if ( isTextAsset )
      {
        if ( trackIdx < listAnimationsText.Count )
        {
          activeAnimationText = listAnimationsText[trackIdx];
        }
        else
        {
          activeAnimationText = null;
        }     
      }
    }

    public int GetAnimationTrackCount()
    {
      bool isCRAnimationAsset = animationFileType == AnimationFileType.CRAnimationAsset;

      if ( isCRAnimationAsset )
      {
        return listAnimations.Count;
      }
      else
      {
        return listAnimationsText.Count;
      }
    }
    
    public string GetActiveAnimationName()
    {
      string name = string.Empty;
      if (animationFileType == AnimationFileType.CRAnimationAsset && activeAnimation != null)
      {
        name = activeAnimation.name;
      }
      else if (animationFileType == AnimationFileType.TextAsset && activeAnimationText != null)
      {
        name = activeAnimationText.name;
      }

      return name;
    }

    public int GetActiveAnimationTrackIdx()
    {
      bool isCRAnimationAsset = animationFileType == AnimationFileType.CRAnimationAsset;
      bool isTextAsset        = animationFileType == AnimationFileType.TextAsset;

      int trackIdx = -1;
      if ( isCRAnimationAsset && activeAnimation != null )
      {
        trackIdx = listAnimations.FindIndex( (animationasset) => { return animationasset == activeAnimation; } );
      }
      else if ( isTextAsset && activeAnimationText != null )
      {
        trackIdx = listAnimationsText.FindIndex( (animationassetText) => { return animationassetText = activeAnimationText; } );
      }

      return trackIdx;
    }
    #endregion

    public void LoadActiveAnimation(bool fromEditor)
    {
      //first close the current animation
      CloseActiveAnimation();

      byte[] animBytes; 
      bool hasAnimationBytes = GetActiveAnimationBytes(out animBytes);

      if (!hasAnimationBytes)
      {
        return;
      }

      using (MemoryStream ms = new MemoryStream(animBytes, false) )
      {
        if (ms != null)
        {
          using (BinaryReader br = new BinaryReader(ms) )
          {
            if (br != null)
            {
              LoadAnimationCommon(br, fromEditor);

              if (binaryVersion_ < 5 )
              {
                LoadAnimationV0(br, fromEditor);
              }
              if (binaryVersion_ == 5)
              {
                LoadAnimationV5(br, fromEditor);
              }
              else if (binaryVersion_ == 6)
              {
                LoadAnimationV6(br, fromEditor);
              }
              else if (binaryVersion_ >= 7)
              {
                LoadAnimationCurrent(br, fromEditor);
              }

              binaryAnim_         = animBytes;
              lastReadFrame_      = -1;
              lastReadFloatFrame_ = -1;

              if (animationFileType == AnimationFileType.CRAnimationAsset)
              {
                animationLastLoaded_ = activeAnimation;
                animationMinimalLastLoaded_ = activeAnimation;
              }
              else if (animationFileType == AnimationFileType.TextAsset)
              {
                animationLastLoadedText_ = activeAnimationText;
                animationMinimalLastLoadedText_ = activeAnimationText;
              }

              timeInternal_ = startTimeOffset_;
            }

          } // BinaryReader

        } //MemoryStream
      }
    }

    public void LoadActiveAnimationInfoMinimal()
    {

      bool minimalLoaded = (animationFileType == AnimationFileType.CRAnimationAsset) && 
                              (activeAnimation == animationMinimalLastLoaded_);

      bool minimalTextLoaded = (animationFileType == AnimationFileType.TextAsset) &&
                                (activeAnimationText == animationMinimalLastLoadedText_);

      if (minimalLoaded || minimalTextLoaded)
      {
        return;
      }

      byte[] animBytes; 
      bool hasAnimationBytes = GetActiveAnimationBytes(out animBytes);

      if (!hasAnimationBytes)
      {
        return;
      }

      using (MemoryStream ms = new MemoryStream(animBytes, false) )
      {
        if (ms != null)
        {
          using (BinaryReader br = new BinaryReader(ms) )
          {
            LoadAnimationCommon(br, true);

            if (animationFileType == AnimationFileType.CRAnimationAsset)
            {
              animationMinimalLastLoaded_ = activeAnimation;
            }
            else if (animationFileType == AnimationFileType.TextAsset)
            {
              animationMinimalLastLoadedText_ = activeAnimationText;
            }
          }
        }
      }

    }

    public void CloseActiveAnimation()
    {
      binaryAnim_      = null;

      time_            = 0.0f;
      frameCount_      = 0;
      frameTime_       = 0.0f;
      fps_             = 0;
      animationLength_ = 0.0f;
      lastFrame_       = 0;

      nGameObjects_ = 0;
      nSkinnedGO_ = 0;

      boxCompression_   = false;
      fiberCompression_ = false;
      isAlignedData_    = false;
      hasSkinnedBoxes_  = false;

      animationLastLoaded_ = null;
      animationMinimalLastLoaded_ = null;

      animationLastLoadedText_ = null;
      animationMinimalLastLoadedText_ = null;

      ClearGOTmpData();
      ClearGPUBuffers();
    }
     
    public void SetFrame( float frame )
    {
      float time = frame / (float)fps_;
      SetTime( time );
    }
 
    public void SetTime( float time )
    {
      timeInternal_ = time;
      time_         = time;

      ReadFrameAtCurrentTime();
    }

    public void Update(float time)
    {
      if (animate)
      {
        CheckLoadAnimationChanged(isPreviewing_);

        timeInternal_ += time * speed;

        switch (repeatMode)
        {
          case RepeatMode.Loop:
            time_ = Mathf.Repeat( (float)timeInternal_, animationLength_ );
            break;
          case RepeatMode.PingPong:
            time_ = Mathf.PingPong( (float)timeInternal_, animationLength_ );
            break;
          case RepeatMode.Clamp:
            time_ = Mathf.Clamp( (float)timeInternal_, 0.0f, animationLength_ );
            break;
        }

        ReadFrameAtCurrentTime();
      }
    }

    public bool IsBoxCompression()
    {
      return boxCompression_;
    }

    public bool IsFiberCompression()
    {
      return fiberCompression_;
    }

    public bool HasVertexAnimatedObjects(GameObject go)
    {
      return ( setVertexAnimatedGO_.Contains(go) );
    }

    #endregion

    #region Private Methods
    private void ClearGOTmpData()
    {
      foreach (CRGOTmpData goTmpData in listGOTmpData_)
      {
        GameObject go = goTmpData.gameObject_;

        if (go != null)
        {
          go.SetActive(goTmpData.isVisible_);

          go.transform.localPosition = goTmpData.localPosition_;
          go.transform.localRotation = goTmpData.localRotation_;
          go.transform.localScale    = goTmpData.localScale_;

          Mesh mesh = goTmpData.mesh_;
          if (mesh != null)
          {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null)
            {
              mf.sharedMesh = mesh;
            }
          }
        }

        Mesh tmpMesh = goTmpData.tmp_Mesh_;
        if (tmpMesh != null)
        {
          UnityEngine.Object.DestroyImmediate(tmpMesh);
        }
      }
      listGOTmpData_.Clear();
    }

    private void ClearGPUBuffers()
    {
      if (arrVertexAnimatedGPUBuffers_ != null)
      {
        int nVertexAnimatedGPUBuffer = arrVertexAnimatedGPUBuffers_.Length;
        for (int i = 0; i < nVertexAnimatedGPUBuffer; i++)
        {
          CarVertexAnimatedGPUBuffers crGPUBuffer = arrVertexAnimatedGPUBuffers_[i];
          if (crGPUBuffer != null)
          {
            crGPUBuffer.Clear();
          }
        }
      }
      if (arrSkinnedAnimatedGPUBuffers_ != null)
      {
        int nSkinnedAnimatedGPUBuffer = arrSkinnedAnimatedGPUBuffers_.Length;
        for (int i = 0; i < nSkinnedAnimatedGPUBuffer; i++)
        {
          CarSkinnedAnimatedGPUBuffers saGPUBuffers = arrSkinnedAnimatedGPUBuffers_[i];
          if (saGPUBuffers != null)
          {
            saGPUBuffers.Clear();
          }
        }
      }
    }

    private bool GetActiveAnimationBytes(out byte[] animBytes)
    {
      bool isCRAnimationAsset = animationFileType == AnimationFileType.CRAnimationAsset;
      bool isTextAsset        = animationFileType == AnimationFileType.TextAsset;

      if ( isCRAnimationAsset && activeAnimation     == null ||
           isTextAsset        && activeAnimationText == null )
      {
        animBytes = null;
        return false;
      }
  
      animBytes = null;
      if (isCRAnimationAsset)
      {
        animBytes = activeAnimation.Bytes;
      }
      else if (isTextAsset)
      {
        CRAnimationsManager animationsManager = CRAnimationsManager.Instance;
        animBytes = animationsManager.GetBytesFromAnimation(activeAnimationText);
      }

      return true;
    }

    private void LoadAnimationCommon(BinaryReader br, bool fromEditor)
    {
      binaryVersion_ = br.ReadInt32();

      interpolationModeActive_ = interpolate;

      vertexCompression_ = br.ReadBoolean();
      hasTangentsData_   = br.ReadBoolean();
      frameCount_        = br.ReadInt32();
      fps_               = br.ReadInt32();
      nGameObjects_      = br.ReadInt32();

      if (binaryVersion_ >= 8)
      {
        fileHeaderFlags_ = (EFileHeaderFlags)br.ReadUInt32();
      }
      else
      {
        fileHeaderFlags_ = EFileHeaderFlags.NONE;
        if (vertexCompression_)
        {
          fileHeaderFlags_ |= EFileHeaderFlags.BOXCOMPRESSION;
        }
      }

      isAlignedData_         = fileHeaderFlags_.IsFlagSet(EFileHeaderFlags.ALIGNEDDATA);
      boxCompression_        = fileHeaderFlags_.IsFlagSet(EFileHeaderFlags.BOXCOMPRESSION);
      fiberCompression_      = fileHeaderFlags_.IsFlagSet(EFileHeaderFlags.FIBERCOMPRESSION);
      hasVertexLocalSystems_ = fileHeaderFlags_.IsFlagSet(EFileHeaderFlags.VERTEXLOCALSYSTEMS);
      hasSkinnedBoxes_       = fileHeaderFlags_.IsFlagSet(EFileHeaderFlags.SKINNEDBOXES);

      lastFrame_ = Mathf.Max(frameCount_ - 1, 0);

      animationLength_ = (float)lastFrame_ / (float)fps_;
      frameTime_       = 1.0f / (float)fps_;

      arrAnimatedGO_           = new CarAnimatedGO[nGameObjects_];
      arrSkipObject_           = new BitArray(nGameObjects_, false);
      arrIsBoneAnimatedObject_ = new BitArray(nGameObjects_, false);
      arrVisibilityInterval_   = new Vector2[nGameObjects_];
      arrMesh_                 = new Mesh[nGameObjects_];

      if (fiberCompression_)
      {
        arrDefinition_      = new CarDefinition[nGameObjects_];
        arrCompressedPose_  = new CarCompressedPose[nGameObjects_];
      }

      if (hasVertexLocalSystems_)
      {
        arrVertexDataCache_ = new CarVertexDataCache[nGameObjects_];
      }

      arrCacheIndex_ = new int[nGameObjects_];

      dictVertexCountCacheIdx_.Clear();
      listVertexCacheCount_   .Clear();

      internalGPUBufferSize_ = 0;
      gpuframeIterator_ = 0;

      internalDOGPUVertexAnim_  = false;
      internalDoGPUSkinnedAnim_ = false;

      bool GPUModeRequested = GPUVertexAnimation || GPUSkinnedAnimation;

      if ( GPUModeRequested &&
           SystemInfo.supportsComputeShaders &&
           !fromEditor )
      {
        LoadGPUMode();
        AssignComputeShaders();
      }

      AssignReadFrameDelegate();
    }

    private void LoadGPUMode()
    {
      if (GPUVertexAnimation)
      {
        internalDOGPUVertexAnim_ = true;
        arrVertexAnimatedGPUBuffers_ = new CarVertexAnimatedGPUBuffers[nGameObjects_];
      }

      if (GPUSkinnedAnimation && AnimationPersistence != null && AnimationPersistence.HasOptimizedGameObjects() )
      {
        internalDoGPUSkinnedAnim_ = true;

        int numberOfOptimizedObjects = AnimationPersistence.GetOptimizedGameObjectCount();
        arrSkinnedAnimatedGPUBuffers_ = new CarSkinnedAnimatedGPUBuffers[numberOfOptimizedObjects];
      }

      if (bufferAllFrames_)
      {
        internalGPUBufferSize_ = frameCount_;
      }
      else
      {
        internalGPUBufferSize_ = gpuFrameBufferSize_ + 2;
      }

      gpuBufferer_ = new CarGPUBufferer(internalGPUBufferSize_);

      if (useDoubleSidedShader_)
      {
        vertexAnimShader_ = (Shader)Resources.Load("CFX Standard VA (double sided)");
      }
      else
      {
        vertexAnimShader_ = (Shader)Resources.Load("CFX Standard VA");
      }
    }

    private void LoadAnimationV0(BinaryReader br, bool fromEditor)
    {
      for (int i = 0; i < nGameObjects_; i++)
      {
        string relativePath = br.ReadString();
        int vertexCount     = br.ReadInt32();
        int boneCount       = 0;
 
        CreateCacheIdx(i, vertexCount);
        int offsetBytesSize = CalculateStreamOffsetSize( vertexCount, boneCount, 0 );

        Transform tr = transform.Find(relativePath); 
        arrAnimatedGO_[i] = new CarAnimatedGO(tr, vertexCount, offsetBytesSize);
        
        if ( tr == null || 
           ( tr != null && ( vertexCount > 0 && !tr.gameObject.HasMesh() ) ) )
        {
          arrSkipObject_[i] = true;
          continue;
        }
  
        LoadAnimatedGameObject(fromEditor, ref arrAnimatedGO_[i], ref arrMesh_[i]);
        
      } //for GameObjects...

      CreateCaches();

      // read frame offsets:
      //_________________________________________________________________________________
      LoadFrameOffsets(br);
    }

    private void LoadAnimationV5(BinaryReader br, bool fromEditor)
    {
      for (int i = 0; i < nGameObjects_; i++)
      {
        string relativePath = br.ReadString();
        int vertexCount     = br.ReadInt32();
          
        CreateCacheIdx(i, vertexCount);
        int offsetBytesSize = CalculateStreamOffsetSize( vertexCount, 0, 0 );

        Transform tr  = transform.Find(relativePath);
        arrAnimatedGO_[i] = new CarAnimatedGO(tr, vertexCount, offsetBytesSize);

        if ( tr == null || 
           ( tr != null && ( vertexCount > 0 && !tr.gameObject.HasMesh() ) ) )
        {
          arrSkipObject_[i] = true;
          continue;
        }

        arrIsBoneAnimatedObject_[i] = ( vertexCount == 0 && !tr.gameObject.HasMesh() );
        arrVisibilityInterval_[i] = Vector2.zero;


        LoadAnimatedGameObject(fromEditor, ref arrAnimatedGO_[i], ref arrMesh_[i] );
        
      } //for GameObjects...


      // read emmiter names:
      //_________________________________________________________________________________
      LoadEmitterNames(br);

      // create vertex caches:
      //_________________________________________________________________________________
      CreateCaches();

      // read frame offsets:
      //_________________________________________________________________________________
      LoadFrameOffsets(br);
    }

    private void LoadAnimationV6(BinaryReader br, bool fromEditor)
    {
      for (int i = 0; i < nGameObjects_; i++)
      {
        string relativePath = br.ReadString();
        int vertexCount     = br.ReadInt32();
        Vector2 v           = new Vector2( br.ReadSingle(), br.ReadSingle() );
   
        CreateCacheIdx(i, vertexCount);
        int offsetBytesSize = CalculateStreamOffsetSize( vertexCount, 0, 0 );

        Transform tr  = transform.Find(relativePath);
        arrAnimatedGO_[i] = new CarAnimatedGO(tr, vertexCount, offsetBytesSize);

        if ( tr == null || 
           ( tr != null && ( vertexCount > 0 && !tr.gameObject.HasMesh() ) ) )
        {
          arrSkipObject_[i] = true;
          continue;
        }

        arrIsBoneAnimatedObject_[i] = ( vertexCount == 0 && !tr.gameObject.HasMesh() );
        arrVisibilityInterval_[i] = v;

        LoadAnimatedGameObject(fromEditor, ref arrAnimatedGO_[i], ref arrMesh_[i]);
                
      } //for GameObjects...


      // read emmiter names:
      //_________________________________________________________________________________
      LoadEmitterNames(br);

      // create vertex caches:
      //_________________________________________________________________________________
      CreateCaches();

      // read frame offsets:
      //_________________________________________________________________________________
      LoadFrameOffsets(br);
    }

    private void LoadAnimationCurrent(BinaryReader br, bool fromEditor)
    {
      int currentBoneIndex = 0;

      listBonesTransformAux_   .Clear();
      listBonesOptimizedIdxAux_.Clear();

      for (int i = 0; i < nGameObjects_; i++)
      {
        string relativePath = br.ReadString();
        int vertexCount     = br.ReadInt32();
        int boneCount       = br.ReadInt32();

        Vector2 visibilityInterval = new Vector2( br.ReadSingle(), br.ReadSingle() );
        arrVisibilityInterval_[i] = visibilityInterval;

        CreateCacheIdx(i, vertexCount);

        CarDefinition      definition      = null;
        CarCompressedPose  compressedPose  = null;
        CarVertexDataCache vertexDataCache = null;
        int compressedPoseBytesOffset = 0;
        CreateVertexCompressionStructures(br, vertexCount, i, ref definition, ref compressedPose, ref vertexDataCache, ref compressedPoseBytesOffset);

        int offsetBytesSize = CalculateStreamOffsetSize(vertexCount, boneCount, compressedPoseBytesOffset);

        Transform tr;
        CreateAnimatedGOData(relativePath, i, boneCount, vertexCount, offsetBytesSize, ref currentBoneIndex, out tr ); 
   
        LoadBones(br, boneCount, listBonesTransformAux_, listBonesOptimizedIdxAux_);
       
        bool invalidObject = (tr == null) || ( vertexCount > 0 && (!tr.HasMesh() || !tr.HasRenderer()) );
        if ( invalidObject )
        {
          arrSkipObject_[i] = true;
          continue;
        }
   
        LoadAnimatedGameObject ( fromEditor, ref arrAnimatedGO_[i], ref arrMesh_[i] );

        if (internalDOGPUVertexAnim_ && (arrMesh_[i] != null) )
        { 
          Renderer rn = tr.GetComponent<Renderer>();
          CreateVertexAnimatedGPUBuffers(vertexCount, definition, compressedPose, vertexDataCache, rn, ref arrVertexAnimatedGPUBuffers_[i]); 
        }
      } //for GameObjects...


      // Build bones arrays. 
      // Create bones transform and bones tmp data if loaded from editor:
      //_________________________________________________________________________________
      arrBoneTr_           = listBonesTransformAux_.ToArray();
      arrBoneOptimizedIdx_ = listBonesOptimizedIdxAux_.ToArray();
      if (fromEditor)
      {
        CreateTmpBonesData();   
      }
      
      if (hasSkinnedBoxes_)
      {
        LoadSkinnedGO(br);
      }

      // read emmiter names:
      //_________________________________________________________________________________
      LoadEmitterNames(br);

      // create vertex caches:
      //_________________________________________________________________________________
      CreateCaches();

      // read frame offsets:
      //_________________________________________________________________________________
      LoadFrameOffsets(br);

      // create skinned gpu buffers:
      //_________________________________________________________________________________  
      if (internalDoGPUSkinnedAnim_)
      {
        CreateGPUSkinnedAnimationBuffers();
      }
    }

    private void CreateVertexCompressionStructures( BinaryReader br, int vertexCount, int animatedObjectId,  ref CarDefinition definition, ref CarCompressedPose compressedPose, ref CarVertexDataCache vertexDataCache, ref int compressedPoseBytesOffset )
    {
      if (vertexCount > 0)
      {
        if (fiberCompression_)
        {
          definition = new CarDefinition();
          definition.Init(br, internalDOGPUVertexAnim_);
          arrDefinition_[animatedObjectId] = definition;

          compressedPose = new CarCompressedPose(definition.GetNumberOfFibers(), internalDOGPUVertexAnim_);
          arrCompressedPose_[animatedObjectId] = compressedPose;
          compressedPoseBytesOffset = compressedPose.GetBytesOffset(definition);
        }

        if (hasVertexLocalSystems_)
        {
          vertexDataCache = new CarVertexDataCache(vertexCount);
          vertexDataCache.Load(br);
          arrVertexDataCache_[animatedObjectId] = vertexDataCache;
        }
      }
    }

    private void CreateAnimatedGOData(string relativeBonePath, int animatedObjectId, int boneCount, int vertexCount, int offsetBytesSize, ref int currentBoneIndex, out Transform tr)
    {
      tr = transform.Find(relativeBonePath);

      int optimizedBoneIdx = -1;

      if (internalDoGPUSkinnedAnim_ && tr == null)
      {
        if ( AnimationPersistence.HasOptimizedBone(relativeBonePath) )
        {
          CarOptimizedBone optimizedBone = AnimationPersistence.GetOptimizedBone(relativeBonePath);
          tr = optimizedBone.GetOptimizedTransform();
          optimizedBoneIdx = AnimationPersistence.GetIndexOfOptimizedBone(relativeBonePath);                
        }
      }

      int boneIdxBegin = currentBoneIndex;
      currentBoneIndex += boneCount;
      int boneIdxEnd = currentBoneIndex;

      if (boneCount == 0)
      {
        arrAnimatedGO_[animatedObjectId] = new CarAnimatedGO( tr, vertexCount, offsetBytesSize, optimizedBoneIdx );
      }
      else
      {
        arrAnimatedGO_[animatedObjectId] = new CarAnimatedGO( tr, vertexCount, offsetBytesSize, optimizedBoneIdx, boneIdxBegin, boneIdxEnd );
      }

      arrIsBoneAnimatedObject_[animatedObjectId] = (optimizedBoneIdx != -1) || ( tr != null && vertexCount == 0 && !tr.HasMesh() );
    }

    private void LoadBones(BinaryReader br, int boneCount, List<Transform> listBonesTransform, List<int> listBonesOptimizedIdx )
    {
      for (int j = 0; j < boneCount; j++)
      {
        string boneRelativePath = br.ReadString();
        Transform boneTr  = transform.Find(boneRelativePath);

        int boneIdxInSkinned = -1;
        if (internalDoGPUSkinnedAnim_ && boneTr == null)
        {
          if ( AnimationPersistence.HasOptimizedBone(boneRelativePath) )
          {
            CarOptimizedBone optimizedBone = AnimationPersistence.GetOptimizedBone(boneRelativePath);
            boneTr = optimizedBone.GetOptimizedTransform();
            boneIdxInSkinned = optimizedBone.GetBoneIdx();          
          }
        }
        listBonesTransform.Add(boneTr);
        listBonesOptimizedIdx.Add(boneIdxInSkinned);
      }
    }

    private void CreateTmpBonesData()
    {
      foreach(Transform tr in arrBoneTr_)
      {
        if (tr != null)
        {
          CRGOTmpData boneTmpData = new CRGOTmpData(tr.gameObject);
          listGOTmpData_.Add(boneTmpData);
        }
      }   
    }

    private void LoadSkinnedGO(BinaryReader br)
    {
      nSkinnedGO_ = br.ReadInt32();

      arrSkinnedGO_ = new GameObject[nSkinnedGO_];
      arrSkinnedGOMF_ = new MeshFilter[nSkinnedGO_];

      for (int i = 0; i < nSkinnedGO_; i++)
      {
        string skinnedGOPath = br.ReadString();

        Transform tr = transform.Find(skinnedGOPath);

        if (tr != null)
        {
          arrSkinnedGO_[i]   = tr.gameObject;
          arrSkinnedGOMF_[i] = tr.GetComponent<MeshFilter>();
        }    
      }
    }
    
    private void LoadEmitterNames(BinaryReader br)
    {
      nEmitters_ = br.ReadInt32();
      arrEmitterName_ = new string[nEmitters_];
      for (int i = 0; i < nEmitters_; i++)
      {
        arrEmitterName_[i] = br.ReadString();
      }
    }

    private void LoadFrameOffsets(BinaryReader br)
    {
      arrFrameOffsets_ = new long[frameCount_];
      for (int i = 0; i < frameCount_; i++)
      {
        arrFrameOffsets_[i] = br.ReadInt64();
      }
    }

    private void LoadAnimatedGameObject(bool fromEditor, ref CarAnimatedGO goData, ref Mesh mesh)
    {
      Transform tr = goData.tr_;
      GameObject go = tr.gameObject;

      int vertexCount = goData.vertexCount_;
      if (vertexCount > 0)
      {
        setVertexAnimatedGO_.Add(go);
      }

      if (fromEditor)
      {
        CRGOTmpData goTmpData = new CRGOTmpData(go);
        listGOTmpData_.Add(goTmpData);
      }

      if (vertexCount > 0)
      {
        if (fromEditor)
        {
          CRGOTmpData goTmpData = listGOTmpData_[listGOTmpData_.Count - 1 ];
          Mesh tmpMesh = goTmpData.tmp_Mesh_;

          MeshFilter mf = go.GetComponent<MeshFilter>();
          mf.sharedMesh = tmpMesh;
          mesh = tmpMesh;
        }
        else
        {
          mesh = go.GetMeshInstance();
        }
        tr.localScale = Vector3.one;
      }
    }

    private void CreateVertexAnimatedGPUBuffers(int vertexCount, CarDefinition definition, CarCompressedPose compressedPose, 
                                                CarVertexDataCache vertexDataCache, Renderer rn, ref CarVertexAnimatedGPUBuffers vaGPUBuffers)
    {
      vaGPUBuffers = new CarVertexAnimatedGPUBuffers(internalGPUBufferSize_, vertexCount, vertexCompression_, boxCompression_, fiberCompression_, 
                                                     definition, compressedPose, hasVertexLocalSystems_, vertexDataCache);

      SetShaderOrMaterialPropertyBlockForGPUAnimation(rn, vaGPUBuffers);
    }
    private void CreateGPUSkinnedAnimationBuffers()
    {
      int nOptimizedGO = AnimationPersistence.GetOptimizedGameObjectCount();

      for(int i = 0; i < nOptimizedGO; i++)
      { 
        CarOptimizedGO optimizedGO = AnimationPersistence.GetOptimizedGameObject(i);
        CarSkinnedAnimatedGPUBuffers saGPUBuffers = null;

        CreateSkinnedAnimatedGPUBuffers(optimizedGO, ref saGPUBuffers);
        arrSkinnedAnimatedGPUBuffers_[i] = saGPUBuffers;
      }

      if( internalDoGPUSkinnedAnim_ )
      {
        for (int i = 0; i < nGameObjects_; i++)
        {
          CarAnimatedGO animatedGO = arrAnimatedGO_[i];
          int optimizedBoneIdx = animatedGO.optimizedBoneIdx_;

          if (optimizedBoneIdx != -1)
          {
            Vector2 visibilityInterval = arrVisibilityInterval_[i];

            CarOptimizedBone optimizedBone = animationPersistence_.GetOptimizedBone(optimizedBoneIdx);

            int boneIdx      = optimizedBone.GetBoneIdx();
            int skinnedGOIdx = optimizedBone.GetOptimizedGOIdx();

            CarSkinnedAnimatedGPUBuffers skinnedGPUBuffer = arrSkinnedAnimatedGPUBuffers_[skinnedGOIdx];
            skinnedGPUBuffer.SetBoneVisibilityInterval(boneIdx, visibilityInterval);
          }
        }

        for (int i = 0; i < nOptimizedGO; i++)
        {
          CarSkinnedAnimatedGPUBuffers saGPUBuffers = arrSkinnedAnimatedGPUBuffers_[i];
          saGPUBuffers.SetVisibilityData();   
        }
      }
    }

    private void CreateSkinnedAnimatedGPUBuffers(CarOptimizedGO optimizedGO, ref CarSkinnedAnimatedGPUBuffers saGPUBuffers)
    {
      GameObject go = optimizedGO.GameObject;

      Renderer rn = go.GetComponent<Renderer>();

      if (go != null && rn != null)
      {
        Mesh mesh = go.GetMesh();

        if (mesh != null)
        {
          saGPUBuffers = new CarSkinnedAnimatedGPUBuffers(internalGPUBufferSize_, optimizedGO.VertexCount, optimizedGO.BoneCount, optimizedGO.HasRootBone );

          // create caches:
          //_________________________________________________________________________________
          saGPUBuffers.CreateBonesCaches(mesh.bindposes);

          // set initial Matrices
          //_________________________________________________________________________________
          Transform tr = optimizedGO.Transform;
          Matrix4x4 m_Local = Matrix4x4.TRS(tr.localPosition, tr.localRotation, tr.localScale);
          saGPUBuffers.SetInitialRootMatrix( m_Local.inverse );
          saGPUBuffers.SetRootBoneMatrixInCache( Vector3.zero, Quaternion.identity, Vector3.one );

          // upload static buffers:
          //_________________________________________________________________________________
          saGPUBuffers.SetPositions(mesh.vertices);
          saGPUBuffers.SetNormals(mesh.normals);
          saGPUBuffers.SetBindPoses(mesh.bindposes);

          BoneWeight[] arrBoneWeight = mesh.boneWeights;
          int nVertices = arrBoneWeight.Length;
          CarBoneWeight[] arrCarBoneWeight = new CarBoneWeight[nVertices];
          for (int i = 0; i < nVertices; i++)
          {
            arrCarBoneWeight[i] = new CarBoneWeight(arrBoneWeight[i]);
          }
          saGPUBuffers.SetBoneWeights(arrCarBoneWeight);

          // set shader or material:
          //_________________________________________________________________________________
          SetShaderOrMaterialPropertyBlockForGPUAnimation(rn, saGPUBuffers);
        }
      }    
    }

    private void SetShaderOrMaterialPropertyBlockForGPUAnimation(Renderer rn, CarGPUBuffers gpuBuffers)
    {
      if (overrideShaderForVertexAnimation_)
      {
        Material[] arrMaterial = rn.materials;
        if (arrMaterial != null)
        {
          int nMaterial = arrMaterial.Length;
          for (int i = 0; i < nMaterial; i++)
          {
            Material mat = arrMaterial[i];
            if (mat != null)
            {
              mat.shader = vertexAnimShader_;
              arrMaterial[i].SetTexture("_PositionsTex", gpuBuffers.PositionTexture);
              arrMaterial[i].SetTexture("_NormalsTex", gpuBuffers.NormalTexture);
              arrMaterial[i].SetFloat("_useSampler", 1.0f); 
            }
   
          }
          rn.materials = arrMaterial;
        }
      }
      else
      {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetTexture("_PositionsTex", gpuBuffers.PositionTexture);
        mpb.SetTexture("_NormalsTex", gpuBuffers.NormalTexture);
        mpb.SetFloat("_useSampler", 1.0f);
        rn.SetPropertyBlock(mpb);
      }
    }

    private void AssignReadFrameDelegate()
    {
      if ( binaryVersion_ == 4 )
      {
        readFrameDel_ = ReadFrameV4;
      }
      else if ( binaryVersion_ == 5 )
      {
        readFrameDel_ = ReadFrameV5;     
      }
      else if (binaryVersion_ == 6)
      {
        if (interpolate)
        {
          readFrameDel_ = ReadFrameInterpolatedV6;
        }
        else
        {
          readFrameDel_ = ReadFrameV6;
        }
      }
      else if (binaryVersion_ >= 7)
      {
        if (interpolate)
        {
          readFrameDel_ = ReadFrameInterpolatedCurrent;
        }
        else
        {
          readFrameDel_ = ReadFrameCurrent;
        }
      }
    }

    private void AssignComputeShaders()
    {
      if (internalDOGPUVertexAnim_)
      {
        if (hasVertexLocalSystems_)
        {
          cShaderNormals_ = (ComputeShader)Resources.Load("CarCSVertexNormalsFastUpdater");
        }

        if (vertexCompression_)
        {
          if (boxCompression_)
          {
            cShaderPositions_ = (ComputeShader)Resources.Load("CarCSVertexAnimationBox");
          }
          else if (fiberCompression_)
          {
            cShaderPositions_ = (ComputeShader)Resources.Load("CarCSVertexAnimationFiber");
          }
        }
        else
        {
          cShaderPositions_ = (ComputeShader)Resources.Load("CarCSVertexAnimation");
        }
      }

      if (internalDoGPUSkinnedAnim_)
      {
        cShaderSkinning_ = (ComputeShader)Resources.Load("CarCSGPUSkinning");

        cShaderComputeBones_      = (ComputeShader)Resources.Load("CarCSComputeBones");
        cShaderComputeBonesInter_ = (ComputeShader)Resources.Load("CarCSComputeBonesInter");
      }

      cShaderInterpolation_ = (ComputeShader)Resources.Load("CarCSInterpolation");
    }

    private int CalculateStreamOffsetSize( int vertexCount, int boneCount, int compressedPoseBytesOffset )
    {
      int bytesAdvance = nFloatBytes * (nLocationComponents + nRotationComponents);

      if (boneCount > 0)
      {
        bytesAdvance += boneCount * ( nFloatBytes * (nLocationComponents + nRotationComponents + nScaleComponents) );
      }

      if (vertexCount > 0)
      {
        if ( vertexCompression_ )
        { 
          if (fiberCompression_)
          {
            bytesAdvance += compressedPoseBytesOffset;
          }
          else
          {
            bytesAdvance += nFloatBytes * nBoxComponents + (nUInt16Bytes * nPositionComponents + nSByteBytes * nNormalComponents ) * vertexCount;
            if (hasTangentsData_)
            {
              bytesAdvance += nSByteBytes * nTangentComponents * vertexCount;
            }    
          }
        } 
        else
        {
          bytesAdvance += nFloatBytes * nBoxComponents + ( nFloatBytes * ( nPositionComponents + nNormalComponents ) ) * vertexCount;
          if (hasTangentsData_)
          {
            bytesAdvance += nFloatBytes * nTangentComponents * vertexCount;
          }
        }
      }
 
      return bytesAdvance;
    }

    private void CreateCacheIdx(int gameObjectIdx, int vertexCount)
    {
      if (vertexCount > 0)
      {
        if (!dictVertexCountCacheIdx_.ContainsKey(vertexCount))
        {
          listVertexCacheCount_.Add(vertexCount);
          dictVertexCountCacheIdx_[vertexCount] = listVertexCacheCount_.Count - 1;
        }
        arrCacheIndex_[gameObjectIdx] = dictVertexCountCacheIdx_[vertexCount];
      }
      else
      {
        arrCacheIndex_[gameObjectIdx] = -1;
      }
    }

    private void CreateCaches()
    {
      int nCaches = listVertexCacheCount_.Count;

      arrVertex3Cache1_ = new Vector3[nCaches][];
      arrVertex3Cache2_ = new Vector3[nCaches][];
      arrVertex4Cache_  = new Vector4[nCaches][];
      
      for (int i = 0; i < nCaches; i++)
      {
        arrVertex3Cache1_[i] = new Vector3[listVertexCacheCount_[i]];
        
        if (hasTangentsData_ || internalDOGPUVertexAnim_)
        {
          arrVertex4Cache_[i] = new Vector4[listVertexCacheCount_[i]];
        }

        if (!internalDOGPUVertexAnim_)
        {
          arrVertex3Cache2_[i] = new Vector3[listVertexCacheCount_[i]];
        }
      }
    }

    private void CheckLoadAnimationChanged(bool fromEditor)
    {
      bool isCRAnimationAsset = animationFileType == AnimationFileType.CRAnimationAsset;
      bool isTextAsset = animationFileType == AnimationFileType.TextAsset;

      if ( (isCRAnimationAsset && activeAnimation != animationLastLoaded_)  ||
           (isTextAsset && activeAnimationText != animationLastLoadedText_))
      {
        LoadActiveAnimation(fromEditor);
      }
    }

    private void ReadFrameAtCurrentTime()
    {
      if (interpolate != interpolationModeActive_)
      {
          AssignReadFrameDelegate();
          interpolationModeActive_ = interpolate;
      }

      if (binaryAnim_ != null)
      {
        float floatFrame = Mathf.Clamp(time_ * fps_, 0f, (float)lastFrame_);
        readFrameDel_(floatFrame);
      }
    }

    private void SetObjectVisibility(Transform tr, bool isBone, bool isVisible)
    {
      GameObject go = tr.gameObject;

      if (isBone)
      {
        if ( isVisible && go.activeInHierarchy )
        { 
          tr.localScale = Vector3.one;
        }
        else
        {
          tr.localScale = Vector3.zero;
        }
      }
      else
      {
        go.SetActive(isVisible);      
      }
    }

    private void ReadFrameV4(float frame)
    {
      int nearFrame = (int)Mathf.RoundToInt(frame);

      if ( lastReadFrame_ == nearFrame )
      {
        return;
      }

      SetCursorAt(arrFrameOffsets_[nearFrame], ref bCursor1_);

      for ( int i = 0; i < nGameObjects_; i++ )
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];

        Transform tr        = goInfo.tr_;
        int vertexCount     = goInfo.vertexCount_;
        int offsetBytesSize = goInfo.bytesOffset_;

        EGOKeyFrameFlags flags = (EGOKeyFrameFlags)ReadByte(ref bCursor1_);

        bool isVisible = ( flags & EGOKeyFrameFlags.VISIBLE ) == EGOKeyFrameFlags.VISIBLE;

        if ( tr == null || (vertexCount > 0 && arrMesh_[i] == null) )
        {
          if ( isVisible )
          {
            AdvanceCursor(offsetBytesSize, ref bCursor1_);
          }
          continue;
        }

        tr.gameObject.SetActive( isVisible );
          
        if (isVisible)
        {
          ReadRQ(tr, ref bCursor1_);

          if (vertexCount > 0)
          {  
            Mesh mesh    = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];
            ReadMeshVerticesCPU(mesh, cacheIdx, vertexCount);
          }

        } //isVisible

      } //forGameobjects

      lastReadFrame_ = nearFrame;
    }

    private void ReadFrameV5(float frame)
    {
      int nearFrame = (int)Mathf.RoundToInt(frame);

      if ( lastReadFrame_ == nearFrame )
      {
        return;
      }

      long frameOffset = arrFrameOffsets_[nearFrame];

      SetCursorAt(frameOffset, ref bCursor1_);
      for ( int i = 0; i < nGameObjects_; i++ )
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];

        Transform tr        = goInfo.tr_;
        int vertexCount     = goInfo.vertexCount_;
        int offsetBytesSize = goInfo.bytesOffset_;

        EGOKeyFrameFlags flags = (EGOKeyFrameFlags)ReadByte(ref bCursor1_);

        bool isVisible = ( flags & EGOKeyFrameFlags.VISIBLE ) == EGOKeyFrameFlags.VISIBLE;
        bool skipGameObject = arrSkipObject_[i];

        if ( skipGameObject )
        {
          if (isVisible)
          {
            AdvanceCursor(offsetBytesSize, ref bCursor1_);
          }
          continue;
        }

        if ( doRuntimeNullChecks )
        {
          bool isGONull   = tr == null;
          bool isMeshNull = (vertexCount > 0) && (arrMesh_[i] == null);

          if ( isGONull || isMeshNull ) 
          {
            if (isVisible)
            {
              AdvanceCursor(offsetBytesSize, ref bCursor1_);
            }     
            continue;
          }      
        }

        SetObjectVisibility(tr, arrIsBoneAnimatedObject_[i], isVisible);

        if (isVisible)
        {
          ReadRQ(tr, ref bCursor1_);

          if (vertexCount > 0)
          {  
            Mesh mesh    = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];
            ReadMeshVerticesCPU(mesh, cacheIdx, vertexCount);
          }

        } //isVisible

      } //forGameobjects

      ReadEvents(ref bCursor1_);

      lastReadFrame_ = nearFrame;
    }

    private void ReadFrameV6(float frame)
    {
      int nearFrame = (int)Mathf.RoundToInt(frame);

      if ( lastReadFrame_ == nearFrame )
      {
        return;
      }

      long frameOffset = arrFrameOffsets_[nearFrame];
      SetCursorAt(frameOffset, ref bCursor1_);

      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];

        Transform tr        = goInfo.tr_;
        int vertexCount     = goInfo.vertexCount_;
        int offsetBytesSize = goInfo.bytesOffset_;

        EGOKeyFrameFlags flagsnear = (EGOKeyFrameFlags)ReadByte(ref bCursor1_);

        bool isVisible = (flagsnear & EGOKeyFrameFlags.VISIBLE) == EGOKeyFrameFlags.VISIBLE;
        bool isGhost   = (flagsnear & EGOKeyFrameFlags.GHOST)   == EGOKeyFrameFlags.GHOST;

        bool exists = isVisible || isGhost;
        
        bool skipGameObject = arrSkipObject_[i];
        if (skipGameObject)
        {
          AdvanceCursorIfExists(offsetBytesSize, exists, ref bCursor1_);
          continue;
        }

        if (doRuntimeNullChecks)
        {
          bool isGONull = tr == null;
          bool isMeshNull = (vertexCount > 0) && (arrMesh_[i] == null);

          if (isGONull || isMeshNull)
          {
            AdvanceCursorIfExists(offsetBytesSize, exists, ref bCursor1_);
            continue;
          }
        }
    
        Vector2 visibleTimeInterval = arrVisibilityInterval_[i];

        bool isInsideVisibleTimeInterval = (exists) && 
                                           ( time_ >= visibleTimeInterval.x && time_ < visibleTimeInterval.y );

        SetObjectVisibility(tr, arrIsBoneAnimatedObject_[i], isInsideVisibleTimeInterval);

        if (exists)
        { 
          ReadRQ(tr, ref bCursor1_);

          if (vertexCount > 0)
          {
            Mesh mesh = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];
            ReadMeshVerticesCPU(mesh, cacheIdx, vertexCount); 
          }
        }
      } //forGameobjects

      ReadEvents(ref bCursor1_);
      lastReadFrame_ = nearFrame;
    }

    private void ReadFrameInterpolatedV6(float frame)
    {
      if ( lastReadFloatFrame_ == frame )
      {
        return;
      }

      int prevFrame = (int)frame;
      int nextFrame = Mathf.Min(prevFrame + 1, lastFrame_);

      float t = frame - prevFrame;

      long prevFrameOffset = arrFrameOffsets_[prevFrame];
      long nextFrameOffset = arrFrameOffsets_[nextFrame];

      SetCursorAt(prevFrameOffset, ref bCursor1_);
      SetCursorAt(nextFrameOffset, ref bCursor2_);

      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];

        Transform tr        = goInfo.tr_;
        int vertexCount     = goInfo.vertexCount_;
        int offsetBytesSize = goInfo.bytesOffset_;

        EGOKeyFrameFlags flagsPrev = (EGOKeyFrameFlags)ReadByte(ref bCursor1_);
        EGOKeyFrameFlags flagsNext = (EGOKeyFrameFlags)ReadByte(ref bCursor2_);

        bool visiblePrev = (flagsPrev & EGOKeyFrameFlags.VISIBLE) == EGOKeyFrameFlags.VISIBLE;
        bool ghostPrev   = (flagsPrev & EGOKeyFrameFlags.GHOST)   == EGOKeyFrameFlags.GHOST;

        bool existsPrev = visiblePrev || ghostPrev;

        bool visibleNext = (flagsNext & EGOKeyFrameFlags.VISIBLE) == EGOKeyFrameFlags.VISIBLE;
        bool ghostNext   = (flagsNext & EGOKeyFrameFlags.GHOST)   == EGOKeyFrameFlags.GHOST;
  
        bool existsNext  = visibleNext || ghostNext;

        bool skipGameObject = arrSkipObject_[i];

        if (skipGameObject)
        {
          AdvanceCursorsIfExists(offsetBytesSize, existsPrev, ref bCursor1_, existsNext, ref bCursor2_);
          continue;
        }

        if (doRuntimeNullChecks)
        {
          bool isGONull = tr == null;
          bool isMeshNull = (vertexCount > 0) && (arrMesh_[i] == null);

          if (isGONull || isMeshNull)
          {
            AdvanceCursorsIfExists(offsetBytesSize, existsPrev, ref bCursor1_, existsNext, ref bCursor2_);
            continue;
          }
        }
   
        Vector2 visibleTimeInterval = arrVisibilityInterval_[i];

        bool isInsideVisibleTimeInterval = (existsPrev && existsNext) && 
                                            ( time_ >= visibleTimeInterval.x && time_ < visibleTimeInterval.y );

        SetObjectVisibility(tr, arrIsBoneAnimatedObject_[i], isInsideVisibleTimeInterval);
    
        if (!isInsideVisibleTimeInterval)
        {
          AdvanceCursorsIfExists(offsetBytesSize, existsPrev, ref bCursor1_, existsNext, ref bCursor2_);
        }
        else
        {
          float tAux = t;

          if (ghostPrev && visibleNext)
          { 
            float min = visibleTimeInterval.x;
            float max = nextFrame * frameTime_;
            tAux = (time_ - min) / (max - min);
          }
          else if (ghostNext && visiblePrev)
          {
            float min = prevFrame * frameTime_;
            float max = visibleTimeInterval.y;
            tAux = (time_ - min) / (max - min);
          }
          else if (ghostPrev && ghostNext)
          {
            float min = visibleTimeInterval.x;
            float max = visibleTimeInterval.y;
            tAux = (time_ - min) / (max - min);
          }

          ReadRQ(tAux, tr, ref bCursor1_, ref bCursor2_);
          if (vertexCount > 0)
          {
            Mesh mesh = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];
            ReadMeshVerticesCPU(tAux, mesh, cacheIdx, vertexCount); 
          }
        }

      } //forGameobjects

      if ( t < 0.5f )
      {
        if (lastReadFrame_ != prevFrame)
        {
          ReadEvents(ref bCursor1_);
          lastReadFrame_ = prevFrame;
        }
      }
      else
      {
        if (lastReadFrame_ != nextFrame)
        {
          ReadEvents(ref bCursor2_);
          lastReadFrame_ = nextFrame;
        }   
      }
  
      lastReadFloatFrame_ = frame;
    }

    private void ReadFrameCurrent(float frame)
    {
      int nearFrame = (int)Mathf.RoundToInt(frame);

      if ( lastReadFrame_ == nearFrame )
      {
        return;
      }

      if (internalDOGPUVertexAnim_ ||
          internalDoGPUSkinnedAnim_ )
      {
        BufferGPUFrames(nearFrame, internalGPUBufferSize_);
      }

      long frameOffset = arrFrameOffsets_[nearFrame];
      SetCursorAt(frameOffset, ref bCursor1_);

      ReadFrameCurrent(time_, nearFrame);
      ReadFrameSkinnedBoxesCurrent();
      ReadEvents(ref bCursor1_);

      if (internalDoGPUSkinnedAnim_)
      {
        ApplyGPUSkinning(time_, nearFrame);
      }


      lastReadFrame_ = nearFrame;
    }

    // ReadFrameCurrent (non interpolation mode) reads the current frame
    // Use of functions is minimized to improve playback performance in the editor
    //_________________________________________________________________________________
    private void ReadFrameCurrent(float time, int nearFrame)
    {
      for (int i = 0; i < nGameObjects_; i++)
      {
        // Load animatedGO info
        //_________________________________________________________________________________
        CarAnimatedGO goInfo = arrAnimatedGO_[i];

        Transform tr        = goInfo.tr_;
        int vertexCount     = goInfo.vertexCount_;
        int offsetBytesSize = goInfo.bytesOffset_;

        int boneIdxBegin    = goInfo.boneIdxBegin_;
        int boneIdxEnd      = goInfo.boneIdxEnd_;
        int boneCount       = goInfo.boneCount_;

        int optimizedBoneIdx = goInfo.optimizedBoneIdx_;

        // Load flags info
        //_________________________________________________________________________________
        EGOKeyFrameFlags flagsnear = (EGOKeyFrameFlags)binaryAnim_[bCursor1_];
        bCursor1_ += sizeof(byte);

        bool isVisible = (flagsnear & EGOKeyFrameFlags.VISIBLE) == EGOKeyFrameFlags.VISIBLE;
        bool isGhost   = (flagsnear & EGOKeyFrameFlags.GHOST)   == EGOKeyFrameFlags.GHOST;

        bool exists = isVisible || isGhost;

        // Skip GameObject if marked in arrSkipObject 
        // or if requieres gpu buffering (optimizedBoneIdx != -1)
        //_________________________________________________________________________________
        bool skipGameObject = arrSkipObject_[i] || (optimizedBoneIdx != -1);
        if (skipGameObject)
        {
          AdvanceCursorIfExists(offsetBytesSize, exists, ref bCursor1_);
          continue;
        }

        // Do runtime null checks if requested
        //_________________________________________________________________________________
        if (doRuntimeNullChecks)
        {
          bool isGONull = tr == null;
          bool isMeshNull = (vertexCount > 0) && (arrMesh_[i] == null);

          if (isGONull || isMeshNull)
          {
            AdvanceCursorIfExists(offsetBytesSize, exists, ref bCursor1_);
            continue;
          }
        }

        // Check visibility
        //_________________________________________________________________________________
        Vector2 visibleTimeInterval = arrVisibilityInterval_[i];
        bool isInsideVisibleTimeInterval = (exists) &&
                                           (time >= visibleTimeInterval.x && time < visibleTimeInterval.y);


        // Set visibility
        //_________________________________________________________________________________
        GameObject go = tr.gameObject;
        if (arrIsBoneAnimatedObject_[i])
        {
          if (isInsideVisibleTimeInterval && go.activeInHierarchy)
          {
            tr.localScale = Vector3.one;
          }
          else
          {
            tr.localScale = Vector3.zero;
          }
        }
        else
        {
          go.SetActive(isInsideVisibleTimeInterval);
        }

        // If there is frame data read it
        //_________________________________________________________________________________
        if (exists)
        {
          if (isAlignedData_)
          {
            AdvanceCursorPadding4(ref bCursor1_);
          }

          // Read rq
          //_________________________________________________________________________________
          Vector3 r1;
          Quaternion q1;
         
          r1.x = ReadSingle(ref bCursor1_);
          r1.y = ReadSingle(ref bCursor1_);
          r1.z = ReadSingle(ref bCursor1_);
    
          q1.x = ReadSingle(ref bCursor1_);
          q1.y = ReadSingle(ref bCursor1_);
          q1.z = ReadSingle(ref bCursor1_);
          q1.w = ReadSingle(ref bCursor1_);

          tr.localPosition = r1;
          tr.localRotation = q1;

          // Read vertices
          //_________________________________________________________________________________
          if (vertexCount > 0)
          {
            Mesh mesh = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];

            if (internalDOGPUVertexAnim_)
            {
              CarVertexAnimatedGPUBuffers gpuBuffer = arrVertexAnimatedGPUBuffers_[i];
              int bufferFrame = gpuBufferer_.GetBufferFrame(nearFrame);
              if (fiberCompression_)
              {
                CarDefinition definition = arrDefinition_[i];
                CarCompressedPose compressedPose = arrCompressedPose_[i];
                ReadMeshVerticesFiberGPU(bufferFrame, mesh, definition, compressedPose, gpuBuffer);
              }
              else
              {
                ReadMeshVerticesGPU(bufferFrame, mesh, vertexCount, gpuBuffer);
              }

              if (hasVertexLocalSystems_ && recomputeNormals_)
              {
                RecomputeNormalsGPU(vertexCount, gpuBuffer);
              }
            }
            else
            {
              if (fiberCompression_)
              {
                CarDefinition     definition     = arrDefinition_[i];
                CarCompressedPose compressedPose = arrCompressedPose_[i];
                ReadMeshVerticesFiberCPU(mesh, definition, compressedPose, cacheIdx, vertexCount);
              }
              else
              {
                ReadMeshVerticesCPU(mesh, cacheIdx, vertexCount);
              }

              if (hasVertexLocalSystems_ && recomputeNormals_)
              {
                CarVertexDataCache vertexDataCache = arrVertexDataCache_[i];
                RecomputeNormalsCPU(mesh, cacheIdx, vertexDataCache);
              }
            }
          }
          // Read skinned bones
          //_________________________________________________________________________________
          else if (boneCount > 0)
          {
            CarBinaryReader.ReadArrByteToArrBone(binaryAnim_, ref bCursor1_, arrBoneTr_, boneIdxBegin, boneIdxEnd);
          }
        }
      } //forGameobjects
    }

    private void ReadFrameInterpolatedCurrent(float frame)
    {
      if ( lastReadFloatFrame_ == frame )
      {
        return;
      }

      int prevFrame = (int)frame;
      int nextFrame = Mathf.Min(prevFrame + 1, lastFrame_);

      float t = frame - prevFrame;

      if ( internalDOGPUVertexAnim_ ||
           internalDoGPUSkinnedAnim_  )
      {   
        if (speed >= 0)
        {  
          BufferGPUFrames(prevFrame, internalGPUBufferSize_);  
        }  
        else
        {
          BufferGPUFrames(nextFrame, internalGPUBufferSize_);
        }     
      }

      long prevFrameOffset = arrFrameOffsets_[prevFrame];
      long nextFrameOffset = arrFrameOffsets_[nextFrame];

      SetCursorAt(prevFrameOffset, ref bCursor1_);
      SetCursorAt(nextFrameOffset, ref bCursor2_);
 
      ReadFrameInterpolatedCurrent(time_, t, prevFrame, nextFrame);
      ReadFrameInterpolatedSkinnedBoxesCurrent(t);

      if ( t < 0.5f )
      {
        if (lastReadFrame_ != prevFrame)
        {
          ReadEvents(ref bCursor1_);
          lastReadFrame_ = prevFrame;
        }
      }
      else
      {
        if (lastReadFrame_ != nextFrame)
        {
          ReadEvents(ref bCursor2_);
          lastReadFrame_ = nextFrame;
        }   
      }

      if (internalDoGPUSkinnedAnim_)
      {
        ApplyGPUSkinningInterpolated(time_, t, prevFrame, nextFrame);
      }

      lastReadFloatFrame_ = frame;
    }

    // ReadFrameInterpolatedCurrent (interpolation mode) reads the current frame
    // Use of functions is minimized to improve playback performance in the editor
    //_________________________________________________________________________________
    private void ReadFrameInterpolatedCurrent(float time, float t, int prevFrame, int nextFrame)
    {
      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];

        Transform tr        = goInfo.tr_;
        int vertexCount     = goInfo.vertexCount_;
        int offsetBytesSize = goInfo.bytesOffset_;

        int boneIdxBegin    = goInfo.boneIdxBegin_;
        int boneIdxEnd      = goInfo.boneIdxEnd_;
        int boneCount       = goInfo.boneCount_;

        int optimizedBoneIdx = goInfo.optimizedBoneIdx_;

        EGOKeyFrameFlags flagsPrev = (EGOKeyFrameFlags)binaryAnim_[bCursor1_];
        bCursor1_ += sizeof(byte);

        EGOKeyFrameFlags flagsNext = (EGOKeyFrameFlags)binaryAnim_[bCursor2_];
        bCursor2_ += sizeof(byte);

        bool visiblePrev = (flagsPrev & EGOKeyFrameFlags.VISIBLE) == EGOKeyFrameFlags.VISIBLE;
        bool ghostPrev   = (flagsPrev & EGOKeyFrameFlags.GHOST) == EGOKeyFrameFlags.GHOST;

        bool existsPrev = visiblePrev || ghostPrev;

        bool visibleNext = (flagsNext & EGOKeyFrameFlags.VISIBLE) == EGOKeyFrameFlags.VISIBLE;
        bool ghostNext   = (flagsNext & EGOKeyFrameFlags.GHOST) == EGOKeyFrameFlags.GHOST;

        bool existsNext = visibleNext || ghostNext;

        bool skipGameObject = arrSkipObject_[i] || (optimizedBoneIdx != -1);
        if (skipGameObject)
        {
          AdvanceCursorsIfExists(offsetBytesSize, existsPrev, ref bCursor1_, existsNext, ref bCursor2_);
          continue;
        }

        if (doRuntimeNullChecks)
        {
          bool isGONull = tr == null;
          bool isMeshNull = (vertexCount > 0) && (arrMesh_[i] == null);

          if (isGONull || isMeshNull)
          {
            AdvanceCursorsIfExists(offsetBytesSize, existsPrev, ref bCursor1_, existsNext, ref bCursor2_);
            continue;
          }
        }

        Vector2 visibleTimeInterval = arrVisibilityInterval_[i];

        bool isInsideVisibleTimeInterval = (existsPrev && existsNext) &&
                                           (time >= visibleTimeInterval.x && time < visibleTimeInterval.y);

        GameObject go = tr.gameObject;

        if (arrIsBoneAnimatedObject_[i])
        {
          if (isInsideVisibleTimeInterval && go.activeInHierarchy)
          {
            tr.localScale = Vector3.one;
          }
          else
          {
            tr.localScale = Vector3.zero;
          }
        }
        else
        {
          go.SetActive(isInsideVisibleTimeInterval);
        }

        if (!isInsideVisibleTimeInterval)
        {
          AdvanceCursorsIfExists(offsetBytesSize, existsPrev, ref bCursor1_, existsNext, ref bCursor2_);
        }
        else
        {
          if (isAlignedData_)
          {
            AdvanceCursorPadding4(ref bCursor1_);
            AdvanceCursorPadding4(ref bCursor2_);
          }
   
          Vector3 r1;
          Quaternion q1;

          r1.x = ReadSingle(ref bCursor1_);
          r1.y = ReadSingle(ref bCursor1_);
          r1.z = ReadSingle(ref bCursor1_);
    
          q1.x = ReadSingle(ref bCursor1_);
          q1.y = ReadSingle(ref bCursor1_);
          q1.z = ReadSingle(ref bCursor1_);
          q1.w = ReadSingle(ref bCursor1_);

          Vector3 r2;
          Quaternion q2;

          r2.x = ReadSingle(ref bCursor2_);
          r2.y = ReadSingle(ref bCursor2_);
          r2.z = ReadSingle(ref bCursor2_);

          q2.x = ReadSingle(ref bCursor2_);
          q2.y = ReadSingle(ref bCursor2_);
          q2.z = ReadSingle(ref bCursor2_);
          q2.w = ReadSingle(ref bCursor2_);    

          float tOk = t;

          float iMin = visibleTimeInterval.x;
          float iMax = visibleTimeInterval.y;

          if (ghostPrev && ghostNext)
          {
            float min = iMin;
            float max = iMax;
            tOk = (time - min) / (max - min);
          }
          else if (ghostPrev)
          {
            float min = iMin;
            float max = nextFrame * frameTime_;
            tOk = (time - min) / (max - min);
          }
          else if (ghostNext)
          {
            float min = prevFrame * frameTime_;
            float max = iMax;
            tOk = (time - min) / (max - min);
          }

          Vector3 rInterpolated = Vector3.LerpUnclamped(r1, r2, tOk);
          Vector3 rCorrection = Vector3.LerpUnclamped((r1 - rInterpolated), (r2 - rInterpolated), tOk);

          tr.localPosition = rInterpolated;
          tr.localRotation = Quaternion.SlerpUnclamped(q1, q2, tOk);

          if (vertexCount > 0)
          {
            Mesh mesh = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];

            if (internalDOGPUVertexAnim_)
            {
              CarVertexAnimatedGPUBuffers gpuBuffer = arrVertexAnimatedGPUBuffers_[i];

              int bufferFrame1 = gpuBufferer_.GetBufferFrame(prevFrame);
              int bufferFrame2 = gpuBufferer_.GetBufferFrame(nextFrame);

              if (fiberCompression_)
              {
                CarDefinition definition = arrDefinition_[i];
                CarCompressedPose compressedPose = arrCompressedPose_[i];
                ReadMeshVerticesFiberGPU(tOk, bufferFrame1, bufferFrame2, mesh, definition, compressedPose, gpuBuffer);
              }
              else
              {
                ReadMeshVerticesGPU(tOk, bufferFrame1, bufferFrame2, mesh, vertexCount, gpuBuffer);
              }

              if (hasVertexLocalSystems_ && recomputeNormals_)
              {
                RecomputeNormalsGPU(vertexCount, gpuBuffer);
              }
            }
            else
            {
              if (fiberCompression_)
              {
                CarDefinition     definition     = arrDefinition_[i];
                CarCompressedPose compressedPose = arrCompressedPose_[i];

                ReadMeshVerticesFiberCPU(tOk, mesh, definition, compressedPose, cacheIdx, vertexCount);
              }
              else
              {
                ReadMeshVerticesCPU(tOk, mesh, cacheIdx, vertexCount);
              }

              if (hasVertexLocalSystems_ && recomputeNormals_)
              {
                CarVertexDataCache vertexDataCache = arrVertexDataCache_[i];
                RecomputeNormalsCPU(mesh, cacheIdx, vertexDataCache);
              }
            }
          }
          else if (boneCount > 0)
          {
            CarBinaryReader.ReadArrByteToArrBone(binaryAnim_, ref bCursor1_, ref bCursor2_, tOk, arrBoneTr_, boneIdxBegin, boneIdxEnd, rCorrection);
          }
        }

      } //forGameobjects
    }

    private void ReadFrameSkinnedBoxesCurrent()
    {
      if (hasSkinnedBoxes_)
      {
        if (internalDoGPUSkinnedAnim_)
        {
          ReadSkinnedObjectsBoxes();
        }
        else
        {
          SkipSkinnedObjectsBoxes();
        }
      }
    }

    private void ReadSkinnedObjectsBoxes()
    {
      for (int i = 0; i < nSkinnedGO_; i++)
      {
        Vector3 center;
        center.x = ReadSingle(ref bCursor1_);
        center.y = ReadSingle(ref bCursor1_);
        center.z = ReadSingle(ref bCursor1_);

        Vector3 size;
        size.x = ReadSingle(ref bCursor1_);
        size.y = ReadSingle(ref bCursor1_);
        size.z = ReadSingle(ref bCursor1_);

        MeshFilter mf = arrSkinnedGOMF_[i];
        if (mf != null)
        {
          Mesh mesh = mf.mesh;
          if (mesh != null)
          {
            mesh.bounds = new Bounds(center, size);
          }
        }
      }
    }

    private void SkipSkinnedObjectsBoxes()
    {
      AdvanceCursor(nBoxBytes * nSkinnedGO_, ref bCursor1_);
    }

    private void ReadFrameInterpolatedSkinnedBoxesCurrent(float t)
    {
      if (hasSkinnedBoxes_)
      {
        if (internalDoGPUSkinnedAnim_)
        {
          ReadInterpolatedSkinnedObjectsBoxes(t);
        }
        else
        {
          SkipInterpolatedSkinnedObjectsBoxes();
        }
      }
    }

    private void ReadInterpolatedSkinnedObjectsBoxes(float t)
    {
      for (int i = 0; i < nSkinnedGO_; i++)
      {
        Vector3 center1;
        center1.x = ReadSingle(ref bCursor1_);
        center1.y = ReadSingle(ref bCursor1_);
        center1.z = ReadSingle(ref bCursor1_);

        Vector3 center2;
        center2.x = ReadSingle(ref bCursor2_);
        center2.y = ReadSingle(ref bCursor2_);
        center2.z = ReadSingle(ref bCursor2_);

        Vector3 boxCenter = Vector3.LerpUnclamped(center1, center2, t);

        Vector3 size1;
        size1.x = ReadSingle(ref bCursor1_);
        size1.y = ReadSingle(ref bCursor1_);
        size1.z = ReadSingle(ref bCursor1_);

        Vector3 size2;
        size2.x = ReadSingle(ref bCursor2_);
        size2.y = ReadSingle(ref bCursor2_);
        size2.z = ReadSingle(ref bCursor2_);

        Vector3 boxSize = Vector3.LerpUnclamped(size1, size2, t);

        MeshFilter mf = arrSkinnedGOMF_[i];
        if (mf != null)
        {
          Mesh mesh = mf.mesh;
          if (mesh != null)
          {
            mesh.bounds = new Bounds(boxCenter, boxSize);
          }
        }
      }
    }

    private void SkipInterpolatedSkinnedObjectsBoxes()
    {
      AdvanceCursor(nBoxBytes * nSkinnedGO_, ref bCursor1_);
      AdvanceCursor(nBoxBytes * nSkinnedGO_, ref bCursor2_);
    }

    private void BufferGPUFrames(int frame, int nFramesToBuffer)
    {
      if (gpuBufferer_.GetNumberOfFramesBuffered() >= frameCount_)
      {
        return;
      }

      if (speed >= 0)
      { 
        if (gpuframeIterator_ != 1)
        {
          gpuframeIterator_ = 1;
          gpuBufferer_.Init();
        }    
      }
      else
      {
        if (gpuframeIterator_ != -1)
        {
          gpuframeIterator_ = -1;
          gpuBufferer_.Init();
        }    
      }

      int iFrame = frame;
      int nFramesBuffered = 0;

      while ( nFramesBuffered < nFramesToBuffer &&
              nFramesBuffered < frameCount_ )
      {
        if ( iFrame > lastFrame_ )
        {
          iFrame = 0;
        }

        if ( iFrame < 0 )
        {
          iFrame = lastFrame_;
        }

        if ( !gpuBufferer_.IsFrameBuffered(iFrame) )
        {   
          gpuBufferer_.AddFrameToBuffer(iFrame);   
          BufferGPUFrameCurrent(iFrame); 
        }

        iFrame += gpuframeIterator_;

        nFramesBuffered++;
      }
    }

    private void BufferGPUFrameCurrent(int frame)
    {
      int bufferFrame = gpuBufferer_.GetBufferFrame(frame);

      long frameOffset = arrFrameOffsets_[frame];
      SetCursorAt(frameOffset, ref bCursor1_);

      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];

        Transform tr = goInfo.tr_; 

        int vertexCount      = goInfo.vertexCount_;
        int offsetBytesSize  = goInfo.bytesOffset_;

        int optimizedBoneIdx = goInfo.optimizedBoneIdx_;

        int boneIdxBegin     = goInfo.boneIdxBegin_;
        int boneIdxEnd       = goInfo.boneIdxEnd_;

        EGOKeyFrameFlags flagsnear = (EGOKeyFrameFlags)ReadByte(ref bCursor1_);

        bool isVisible = (flagsnear & EGOKeyFrameFlags.VISIBLE) == EGOKeyFrameFlags.VISIBLE;
        bool isGhost   = (flagsnear & EGOKeyFrameFlags.GHOST)   == EGOKeyFrameFlags.GHOST;

        bool exists = isVisible || isGhost;
        bool skipGameObject = arrSkipObject_[i];
        bool needsGPUBuffering = (vertexCount != 0 || optimizedBoneIdx != -1);

        if (skipGameObject || !needsGPUBuffering)
        {
          AdvanceCursorIfExists(offsetBytesSize, exists, ref bCursor1_);
          continue;
        }

        if (doRuntimeNullChecks)
        {
          bool isGONull = (tr == null && optimizedBoneIdx != -1);
          bool isMeshNull = (vertexCount > 0) && (arrMesh_[i] == null);

          if (isGONull || isMeshNull)
          {
            AdvanceCursorIfExists(offsetBytesSize, exists, ref bCursor1_);
            continue;
          }
        }

        // Buffer frame data if exists
        //_________________________________________________________________________________
        if (exists)
        {
          if (isAlignedData_)
          {
            AdvanceCursorPadding4(ref bCursor1_);
          }

          // buffer gpu data
          //_________________________________________________________________________________
          if (vertexCount == 0 && optimizedBoneIdx != -1)
          {
            CarOptimizedBone optimizedBone = AnimationPersistence.GetOptimizedBone(optimizedBoneIdx);

            int idxOptimizedObject = optimizedBone.GetOptimizedGOIdx();
            CarSkinnedAnimatedGPUBuffers saGPUBuffers = arrSkinnedAnimatedGPUBuffers_[idxOptimizedObject];

            saGPUBuffers.BufferFrameBoneMatrices( optimizedBone.GetBoneIdx(), boneIdxBegin, boneIdxEnd, 
                                                  arrBoneOptimizedIdx_, binaryAnim_, ref bCursor1_ );
          }
          else if (vertexCount > 0 && internalDOGPUVertexAnim_)
          {
            AdvanceCursor(rqBytes, ref bCursor1_);
            CarVertexAnimatedGPUBuffers crGPUBuffer = arrVertexAnimatedGPUBuffers_[i];
            crGPUBuffer.BufferFrameMesh(bufferFrame, vertexCompression_, boxCompression_, fiberCompression_, hasTangentsData_, binaryAnim_, ref bCursor1_);
          }
          else
          {
            AdvanceCursor(offsetBytesSize, ref bCursor1_);
          }
        }
      } //forGameobjects

      if (internalDoGPUSkinnedAnim_)
      {
        UploadBonesToGPU(bufferFrame);
      }

    }// BufferGPUFrameCurrent...

    #region GPUSkinning
    private void UploadBonesToGPU(int bufferFrame)
    { 
      int nSkinnedAnimatedGPUBuffers =  arrSkinnedAnimatedGPUBuffers_.Length; 
      for (int i = 0; i < nSkinnedAnimatedGPUBuffers; i++)
      {
        CarSkinnedAnimatedGPUBuffers saGPUBuffers = arrSkinnedAnimatedGPUBuffers_[i];

        saGPUBuffers.SetRootBoneMatrixFromCache(bufferFrame);
        saGPUBuffers.SetFrameBonesFromCache(bufferFrame);
      }
    }

    private void ApplyGPUSkinning(float time, int nearFrame)
    {
      int nSkinnedAnimatedGPUBuffers =  arrSkinnedAnimatedGPUBuffers_.Length;
      int bufferFrame = gpuBufferer_.GetBufferFrame(nearFrame);

      for(int i = 0; i < nSkinnedAnimatedGPUBuffers; i++)
      { 
        CarSkinnedAnimatedGPUBuffers saGPUBuffers = arrSkinnedAnimatedGPUBuffers_[i];
        int boneCount = saGPUBuffers.BoneCount;

        // calculate bonesMatrices
        //_________________________________________________________________________________
        cShaderComputeBones_.SetInt("boneCount", boneCount);
        cShaderComputeBones_.SetFloat("time", time);

        cShaderComputeBones_.SetBuffer(0, "buf_initialRootMatrix", saGPUBuffers.GetInitialRootMatrixBuffer() );
        cShaderComputeBones_.SetBuffer(0, "buf_bindPose", saGPUBuffers.GetBindposesBuffer() );

        cShaderComputeBones_.SetBuffer(0, "buf_rootBoneMatrix", saGPUBuffers.GetRootBoneMatrixBuffer(bufferFrame));
        cShaderComputeBones_.SetBuffer(0, "buf_boneMatrix", saGPUBuffers.GetBonesBuffer(bufferFrame));

        cShaderComputeBones_.SetBuffer(0, "buf_visibility", saGPUBuffers.GetVisibilityBuffer() );

        cShaderComputeBones_.SetBuffer(0, "buf_actualBoneMatrix", saGPUBuffers.GetActualBoneMatrixBuffer() );
        cShaderComputeBones_.SetBuffer(0, "buf_actualNormalMatrix", saGPUBuffers.GetActualBoneNormalMatrixBuffer() );

        cShaderComputeBones_.Dispatch(0, 1, Mathf.CeilToInt( (float)boneCount / 256.0f), 1 );

        // calculate skinning
        //_________________________________________________________________________________
        cShaderSkinning_.SetInt("vertexCount", saGPUBuffers.VertexCount );

        cShaderSkinning_.SetTexture(0, "tex_position", saGPUBuffers.PositionTexture );
        cShaderSkinning_.SetTexture(0, "tex_normal", saGPUBuffers.NormalTexture);

        cShaderSkinning_.SetBuffer(0, "buf_position", saGPUBuffers.GetPositionsBuffer() );
        cShaderSkinning_.SetBuffer(0, "buf_normal", saGPUBuffers.GetNormalsBuffer() );
          
        cShaderSkinning_.SetBuffer(0, "buf_boneWeight", saGPUBuffers.GetBoneWeightsBuffer() );
        cShaderSkinning_.SetBuffer(0, "buf_boneMatrix", saGPUBuffers.GetActualBoneMatrixBuffer() );
        cShaderSkinning_.SetBuffer(0, "buf_normalMatrix", saGPUBuffers.GetActualBoneNormalMatrixBuffer() );

        cShaderSkinning_.Dispatch(0, 1, saGPUBuffers.PositionTexture.height, 1); 
      }

    }// ApplySkinning...


    private void ApplyGPUSkinningInterpolated(float time, float t, int prevFrame, int nextFrame)
    {
      int nSkinnedAnimatedGPUBuffers = arrSkinnedAnimatedGPUBuffers_.Length;

      int bufferPrevFrame = gpuBufferer_.GetBufferFrame(prevFrame);
      int bufferNextFrame = gpuBufferer_.GetBufferFrame(nextFrame);

      for (int i = 0; i < nSkinnedAnimatedGPUBuffers; i++)
      {
        CarSkinnedAnimatedGPUBuffers saGPUBuffers = arrSkinnedAnimatedGPUBuffers_[i];
        int boneCount = saGPUBuffers.BoneCount;

        // calculate bones Matrix
        //_________________________________________________________________________________
        cShaderComputeBonesInter_.SetInt("boneCount", boneCount);
        cShaderComputeBonesInter_.SetFloat("time", time);
        cShaderComputeBonesInter_.SetFloat("t", t);

        cShaderComputeBonesInter_.SetInt("framePrev", prevFrame);
        cShaderComputeBonesInter_.SetInt("frameNext", nextFrame );
        cShaderComputeBonesInter_.SetFloat("frameTime", frameTime_);

        cShaderComputeBonesInter_.SetBuffer(0, "buf_initialRootMatrix", saGPUBuffers.GetInitialRootMatrixBuffer());
        cShaderComputeBonesInter_.SetBuffer(0, "buf_bindPose", saGPUBuffers.GetBindposesBuffer());

        cShaderComputeBonesInter_.SetBuffer(0, "buf_rootBoneMatrixPrev", saGPUBuffers.GetRootBoneMatrixBuffer(bufferPrevFrame));
        cShaderComputeBonesInter_.SetBuffer(0, "buf_boneMatrixPrev", saGPUBuffers.GetBonesBuffer(bufferPrevFrame));

        cShaderComputeBonesInter_.SetBuffer(0, "buf_rootBoneMatrixNext", saGPUBuffers.GetRootBoneMatrixBuffer(bufferNextFrame));
        cShaderComputeBonesInter_.SetBuffer(0, "buf_boneMatrixNext", saGPUBuffers.GetBonesBuffer(bufferNextFrame));

        cShaderComputeBonesInter_.SetBuffer(0, "buf_visibility", saGPUBuffers.GetVisibilityBuffer());

        cShaderComputeBonesInter_.SetBuffer(0, "buf_actualBoneMatrix", saGPUBuffers.GetActualBoneMatrixBuffer());
        cShaderComputeBonesInter_.SetBuffer(0, "buf_actualNormalMatrix", saGPUBuffers.GetActualBoneNormalMatrixBuffer());

        cShaderComputeBonesInter_.Dispatch(0, 1, Mathf.CeilToInt((float)boneCount / 256.0f), 1);

        // calculate skinning
        //_________________________________________________________________________________
        cShaderSkinning_.SetInt("vertexCount", saGPUBuffers.VertexCount);

        cShaderSkinning_.SetTexture(0, "tex_position", saGPUBuffers.PositionTexture);
        cShaderSkinning_.SetTexture(0, "tex_normal", saGPUBuffers.NormalTexture);

        cShaderSkinning_.SetBuffer(0, "buf_position", saGPUBuffers.GetPositionsBuffer());
        cShaderSkinning_.SetBuffer(0, "buf_normal", saGPUBuffers.GetNormalsBuffer());

        cShaderSkinning_.SetBuffer(0, "buf_boneWeight", saGPUBuffers.GetBoneWeightsBuffer());
        cShaderSkinning_.SetBuffer(0, "buf_boneMatrix", saGPUBuffers.GetActualBoneMatrixBuffer());
        cShaderSkinning_.SetBuffer(0, "buf_normalMatrix", saGPUBuffers.GetActualBoneNormalMatrixBuffer());

        cShaderSkinning_.Dispatch(0, 1, saGPUBuffers.PositionTexture.height, 1);
      }

    }// ApplySkinning...

    #endregion

    #region Read translation rotation

    private void ReadRQ( Transform tr, ref int cursor )
    {
      Vector3 r1;
      Quaternion q1;

      r1.x = ReadSingle(ref cursor);
      r1.y = ReadSingle(ref cursor);
      r1.z = ReadSingle(ref cursor);

      q1.x = ReadSingle(ref cursor);
      q1.y = ReadSingle(ref cursor);
      q1.z = ReadSingle(ref cursor);
      q1.w = ReadSingle(ref cursor);

      tr.localPosition = r1;
      tr.localRotation = q1;
    }

    private void ReadRQ( float t, Transform tr, ref int cursor1, ref int cursor2 )
    {
      Vector3 r1;
      Quaternion q1;

      r1.x = ReadSingle(ref cursor1);
      r1.y = ReadSingle(ref cursor1);
      r1.z = ReadSingle(ref cursor1);

      q1.x = ReadSingle(ref cursor1);
      q1.y = ReadSingle(ref cursor1);
      q1.z = ReadSingle(ref cursor1);
      q1.w = ReadSingle(ref cursor1);

      Vector3    r2;
      Quaternion q2;

      r2.x = ReadSingle(ref cursor2);
      r2.y = ReadSingle(ref cursor2);
      r2.z = ReadSingle(ref cursor2);

      q2.x = ReadSingle(ref cursor2);
      q2.y = ReadSingle(ref cursor2);
      q2.z = ReadSingle(ref cursor2);
      q2.w = ReadSingle(ref cursor2);

      tr.localPosition = Vector3.LerpUnclamped(r1, r2, t);
      tr.localRotation = Quaternion.SlerpUnclamped(q1, q2, t);
    }
    #endregion

    #region Read mesh vertices
    private void ReadMeshVerticesCPU(Mesh mesh, int cacheIdx, int vertexCount)
    {
      Vector3 boundsMin;
      boundsMin.x = ReadSingle(ref bCursor1_);
      boundsMin.y = ReadSingle(ref bCursor1_);
      boundsMin.z = ReadSingle(ref bCursor1_);

      Vector3 boundsMax;
      boundsMax.x = ReadSingle(ref bCursor1_);
      boundsMax.y = ReadSingle(ref bCursor1_);
      boundsMax.z = ReadSingle(ref bCursor1_);

      Vector3[] vector3cache = arrVertex3Cache1_[cacheIdx];
      Vector3 boundSize = (boundsMax - boundsMin) * (posQuantz);
      
      if (boxCompression_)
      {
        CarBinaryReader.ReadArrByteDecompToArrPosition(binaryAnim_, ref bCursor1_, vector3cache, 0, vertexCount, boundsMin, boundSize);
        mesh.vertices = vector3cache;

        CarBinaryReader.ReadArrByteDecompToArrNormal(binaryAnim_, ref bCursor1_, vector3cache, 0, vertexCount);
        mesh.normals  = vector3cache;

        if (hasTangentsData_)
        {
          Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];

          CarBinaryReader.ReadArrByteDecompToArrTangent(binaryAnim_, ref bCursor1_, vector4cache, 0, vertexCount);
          mesh.tangents = vector4cache;
        }
      }
      else
      {
        CarBinaryReader.ReadArrByteToArrVector3(binaryAnim_, ref bCursor1_, vector3cache, 0, vertexCount);
        mesh.vertices = vector3cache;

        CarBinaryReader.ReadArrByteToArrVector3(binaryAnim_, ref bCursor1_, vector3cache, 0, vertexCount);
        mesh.normals = vector3cache;

        if (hasTangentsData_)
        {
          Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];

          CarBinaryReader.ReadArrByteToArrVector4(binaryAnim_, ref bCursor1_, vector4cache, 0, vertexCount);
          mesh.tangents = vector4cache;
        }
      }

      Bounds bounds = mesh.bounds;
      bounds.SetMinMax(boundsMin, boundsMax);
      mesh.bounds = bounds;
    }

    private void ReadMeshVerticesCPU(float t, Mesh mesh, int cacheIdx, int vertexCount)
    {
      Vector3[] vector3cache = arrVertex3Cache1_[cacheIdx];

      Vector3 boundsMin1;
      boundsMin1.x = ReadSingle(ref bCursor1_);
      boundsMin1.y = ReadSingle(ref bCursor1_);
      boundsMin1.z = ReadSingle(ref bCursor1_);

      Vector3 boundsMax1;
      boundsMax1.x = ReadSingle(ref bCursor1_);
      boundsMax1.y = ReadSingle(ref bCursor1_);
      boundsMax1.z = ReadSingle(ref bCursor1_);

      Vector3 boundSize1 = (boundsMax1 - boundsMin1) * (posQuantz);

      Vector3 boundsMin2;
      boundsMin2.x = ReadSingle(ref bCursor2_);
      boundsMin2.y = ReadSingle(ref bCursor2_);
      boundsMin2.z = ReadSingle(ref bCursor2_);

      Vector3 boundsMax2;
      boundsMax2.x = ReadSingle(ref bCursor2_);
      boundsMax2.y = ReadSingle(ref bCursor2_);
      boundsMax2.z = ReadSingle(ref bCursor2_);
 
      Vector3 boundSize2 = (boundsMax2 - boundsMin2) * (posQuantz);

      if (boxCompression_)
      {
        CarBinaryReader.ReadArrByteDecompLerpToArrPosition(binaryAnim_, ref bCursor1_, ref bCursor2_, t, vector3cache, 0, vertexCount,
                                                          boundsMin1, boundSize1, boundsMin2, boundSize2 );
        mesh.vertices = vector3cache;

        CarBinaryReader.ReadArrByteDecompLerpToArrNormal(binaryAnim_, ref bCursor1_, ref bCursor2_, t, vector3cache, 0, vertexCount);
        mesh.normals = vector3cache;

        if (hasTangentsData_)
        {

          Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];

          CarBinaryReader.ReadArrByteDecompLerpToArrTangent(binaryAnim_, ref bCursor1_, ref bCursor2_, t, vector4cache, 0, vertexCount);
          mesh.tangents = vector4cache;
        }
      }
      else
      {
        CarBinaryReader.ReadArrByteLerpToArrVector3(binaryAnim_, ref bCursor1_, ref bCursor2_, t, vector3cache, 0, vertexCount);
        mesh.vertices = vector3cache;

        CarBinaryReader.ReadArrByteLerpToArrVector3(binaryAnim_, ref bCursor1_, ref bCursor2_, t, vector3cache, 0, vertexCount);
        mesh.normals = vector3cache;

        if (hasTangentsData_)
        {
          Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];
          CarBinaryReader.ReadArrByteLerpToArrVector4(binaryAnim_, ref bCursor1_, ref bCursor2_, t, vector4cache, 0, vertexCount);
          mesh.tangents = vector4cache;
        }
      }

      Vector3 v3_1 = Vector3.LerpUnclamped(boundsMin1, boundsMin2, t);
      Vector3 v3_2 = Vector3.LerpUnclamped(boundsMax1, boundsMax2, t);
      Bounds bounds = mesh.bounds;
      bounds.SetMinMax(v3_1, v3_2);
      mesh.bounds = bounds;
    }

    private void ReadMeshVerticesFiberCPU(Mesh mesh, CarDefinition definition, CarCompressedPose compressedPose, int cacheIdx, int vertexCount)
    {
      compressedPose.Load(binaryAnim_, ref bCursor1_, definition);

      Vector3[] vector3cache = arrVertex3Cache1_[cacheIdx];
      compressedPose.DecompressPositions(vector3cache, definition);
      mesh.vertices = vector3cache;

      Bounds bounds = mesh.bounds;
      bounds.SetMinMax(compressedPose.Box.min_, compressedPose.Box.max_);
      mesh.bounds = bounds;
    }

    private void ReadMeshVerticesFiberCPU(float t, Mesh mesh, CarDefinition definition, CarCompressedPose compressedPose, int cacheIdx, int vertexCount)
    {
      Vector3[] vector3cache = arrVertex3Cache1_[cacheIdx];

      compressedPose.Load(binaryAnim_, ref bCursor1_, definition);

      CarBox3 box3Frame1 = compressedPose.Box;
      compressedPose.DecompressPositions(vector3cache, definition);

      compressedPose.Load(binaryAnim_, ref bCursor2_, definition);

      CarBox3 box3Frame2 = compressedPose.Box;
      compressedPose.DecompressInterpolatePositions(t, vector3cache, definition);

      mesh.vertices = vector3cache;

      Vector3 minInterpolated = Vector3.LerpUnclamped(box3Frame1.min_, box3Frame2.min_, t);
      Vector3 maxInterpolated = Vector3.LerpUnclamped(box3Frame1.max_, box3Frame2.max_, t);

      Bounds bounds = mesh.bounds;
      bounds.SetMinMax(minInterpolated, maxInterpolated);
      mesh.bounds = bounds;
    }

    private void ReadMeshVerticesGPU(int bufferFrame, Mesh mesh, int vertexCount, CarVertexAnimatedGPUBuffers gpuBuffer)
    {
      cShaderPositions_.SetInt("vertexCount", vertexCount);

      CarBox3 box3 = CarBox3.CreateLoad(binaryAnim_, ref bCursor1_);

      Bounds bounds = mesh.bounds;
      bounds.SetMinMax(box3.min_, box3.max_);
      mesh.bounds = bounds;

      if (boxCompression_)
      {
        cShaderPositions_.SetVector("boundsMin", new Vector4(box3.min_.x, box3.min_.y, box3.min_.z));
        cShaderPositions_.SetVector("boundsMax", new Vector4(box3.max_.x, box3.max_.y, box3.max_.z));
      }

      cShaderPositions_.SetBuffer(0, "positionBuffer", gpuBuffer.GetPositionBuffer(bufferFrame));
      cShaderPositions_.SetTexture(0, "positionTexture", gpuBuffer.PositionTexture);

      cShaderPositions_.SetBuffer(0, "normalBuffer", gpuBuffer.GetNormalBuffer(bufferFrame));
      cShaderPositions_.SetTexture(0, "normalTexture", gpuBuffer.NormalTexture);

      AdvanceCursorSkipMesh(vertexCount, vertexCompression_, hasTangentsData_, ref bCursor1_);

      cShaderPositions_.Dispatch(0, 1, gpuBuffer.PositionTexture.height, 1);
    }

    private void ReadMeshVerticesGPU(float t, int bufferFrame1, int bufferFrame2, Mesh mesh, int vertexCount, CarVertexAnimatedGPUBuffers gpuBuffer)
    {
      cShaderPositions_.SetInt("vertexCount", vertexCount);
      
      CarBox3 box3Frame1 = new CarBox3();
      CarBox3 box3Frame2 = new CarBox3();

      //pos1
      {
        box3Frame1.Load(binaryAnim_, ref bCursor1_);

        if (boxCompression_)
        {
          cShaderPositions_.SetVector("boundsMin", new Vector4(box3Frame1.min_.x, box3Frame1.min_.y, box3Frame1.min_.z));
          cShaderPositions_.SetVector("boundsMax", new Vector4(box3Frame1.max_.x, box3Frame1.max_.y, box3Frame1.max_.z));
        }

        cShaderPositions_.SetBuffer(0, "positionBuffer", gpuBuffer.GetPositionBuffer(bufferFrame1));
        cShaderPositions_.SetTexture(0, "positionTexture", gpuBuffer.PositionInterpolationTexture1);

        cShaderPositions_.SetBuffer(0, "normalBuffer", gpuBuffer.GetNormalBuffer(bufferFrame1));
        cShaderPositions_.SetTexture(0, "normalTexture", gpuBuffer.NormalInterpolationTexture1);

        AdvanceCursorSkipMesh(vertexCount, vertexCompression_, hasTangentsData_, ref bCursor1_);

        cShaderPositions_.Dispatch(0, 1, gpuBuffer.PositionTexture.height, 1);
      }

      //pos2
      {
        box3Frame2.Load(binaryAnim_, ref bCursor2_);

        if (boxCompression_)
        {
          cShaderPositions_.SetVector("boundsMin", new Vector4(box3Frame2.min_.x, box3Frame2.min_.y, box3Frame2.min_.z));
          cShaderPositions_.SetVector("boundsMax", new Vector4(box3Frame2.max_.x, box3Frame2.max_.y, box3Frame2.max_.z));
        }

        cShaderPositions_.SetBuffer(0, "positionBuffer", gpuBuffer.GetPositionBuffer(bufferFrame2));
        cShaderPositions_.SetTexture(0, "positionTexture", gpuBuffer.PositionInterpolationTexture2);

        cShaderPositions_.SetBuffer(0, "normalBuffer", gpuBuffer.GetNormalBuffer(bufferFrame2));
        cShaderPositions_.SetTexture(0, "normalTexture", gpuBuffer.NormalInterpolationTexture2);

        AdvanceCursorSkipMesh(vertexCount, vertexCompression_, hasTangentsData_, ref bCursor2_);

        cShaderPositions_.Dispatch(0, 1, gpuBuffer.PositionTexture.height, 1);
      }

      //interpolate
      {
        Vector3 minInterpolated = Vector3.LerpUnclamped(box3Frame1.min_, box3Frame2.min_, t);
        Vector3 maxInterpolated = Vector3.LerpUnclamped(box3Frame1.max_, box3Frame2.max_, t);

        Bounds bounds = mesh.bounds;
        bounds.SetMinMax(minInterpolated, maxInterpolated);
        mesh.bounds = bounds;

        cShaderInterpolation_.SetFloat("t", t);

        cShaderInterpolation_.SetTexture(0, "texture1", gpuBuffer.PositionInterpolationTexture1);
        cShaderInterpolation_.SetTexture(0, "texture2", gpuBuffer.PositionInterpolationTexture2);
        cShaderInterpolation_.SetTexture(0, "textureOutput", gpuBuffer.PositionTexture);

        cShaderInterpolation_.Dispatch(0, 1, gpuBuffer.PositionTexture.height, 1);

        cShaderInterpolation_.SetTexture(0, "texture1", gpuBuffer.NormalInterpolationTexture1);
        cShaderInterpolation_.SetTexture(0, "texture2", gpuBuffer.NormalInterpolationTexture2);
        cShaderInterpolation_.SetTexture(0, "textureOutput", gpuBuffer.NormalTexture);

        cShaderInterpolation_.Dispatch(0, 1, gpuBuffer.NormalTexture.height, 1);
      }

    }

    private void ReadMeshVerticesFiberGPU(int bufferFrame, Mesh mesh, CarDefinition definition, CarCompressedPose compressedPose, 
                                          CarVertexAnimatedGPUBuffers gpuBuffer)
    {
      int nFibers = definition.GetNumberOfFibers();
      cShaderPositions_.SetInt("nFibers", nFibers);

      CarBox3 box3 = CarBox3.CreateLoad(binaryAnim_, ref bCursor1_);

      Bounds bounds = mesh.bounds;
      bounds.SetMinMax(box3.min_, box3.max_);
      mesh.bounds = bounds;

      cShaderPositions_.SetVector("meshBox_min", new Vector4(box3.min_.x, box3.min_.y, box3.min_.z));
      cShaderPositions_.SetVector("meshBox_max", new Vector4(box3.max_.x, box3.max_.y, box3.max_.z));

      cShaderPositions_.SetBuffer(0, "fibersDefinition", gpuBuffer.GetDefinitionBuffer());
      cShaderPositions_.SetBuffer(0, "fibers", gpuBuffer.GetPositionBuffer(bufferFrame));

      cShaderPositions_.SetTexture(0, "positions", gpuBuffer.PositionTexture);

      compressedPose.AdvanceCursorSkipPose(ref bCursor1_, definition);

      int threadGroupSize = Mathf.CeilToInt((float)nFibers / 32.0f);
      cShaderPositions_.Dispatch(0, threadGroupSize, 1, 1);
    }

    private void ReadMeshVerticesFiberGPU(float t, int bufferFrame1, int bufferFrame2, Mesh mesh, CarDefinition definition, CarCompressedPose compressedPose, 
                                          CarVertexAnimatedGPUBuffers gpuBuffer)
    {
      int nFibers = definition.GetNumberOfFibers();
      cShaderPositions_.SetInt("nFibers", nFibers);
      cShaderPositions_.SetBuffer(0, "fibersDefinition", gpuBuffer.GetDefinitionBuffer());

      CarBox3 box3Frame1 = new CarBox3();
      CarBox3 box3Frame2 = new CarBox3();

      //positions1
      {
        box3Frame1.Load(binaryAnim_, ref bCursor1_);

        cShaderPositions_.SetVector("meshBox_min", new Vector4(box3Frame1.min_.x, box3Frame1.min_.y, box3Frame1.min_.z));
        cShaderPositions_.SetVector("meshBox_max", new Vector4(box3Frame1.max_.x, box3Frame1.max_.y, box3Frame1.max_.z));

        cShaderPositions_.SetBuffer(0, "fibers", gpuBuffer.GetPositionBuffer(bufferFrame1));
        cShaderPositions_.SetTexture(0, "positions", gpuBuffer.PositionInterpolationTexture1);

        compressedPose.AdvanceCursorSkipPose(ref bCursor1_, definition);

        int threadGroupSize = Mathf.CeilToInt((float)nFibers / 32.0f);
        cShaderPositions_.Dispatch(0, threadGroupSize, 1, 1);
      }

      //positions2
      {
        box3Frame2.Load(binaryAnim_, ref bCursor2_);

        cShaderPositions_.SetVector("meshBox_min", new Vector4(box3Frame2.min_.x, box3Frame2.min_.y, box3Frame2.min_.z));
        cShaderPositions_.SetVector("meshBox_max", new Vector4(box3Frame2.max_.x, box3Frame2.max_.y, box3Frame2.max_.z));

        cShaderPositions_.SetBuffer(0, "fibers", gpuBuffer.GetPositionBuffer(bufferFrame2));
        cShaderPositions_.SetTexture(0, "positions", gpuBuffer.PositionInterpolationTexture2);

        compressedPose.AdvanceCursorSkipPose(ref bCursor2_, definition);

        int threadGroupSize = Mathf.CeilToInt((float)nFibers / 32.0f);
        cShaderPositions_.Dispatch(0, threadGroupSize, 1, 1);
      }

      //interpolate
      {
        Vector3 minInterpolated = Vector3.LerpUnclamped(box3Frame1.min_, box3Frame2.min_, t);
        Vector3 maxInterpolated = Vector3.LerpUnclamped(box3Frame1.max_, box3Frame2.max_, t);

        Bounds bounds = mesh.bounds;
        bounds.SetMinMax(minInterpolated, maxInterpolated);
        mesh.bounds = bounds;

        cShaderInterpolation_.SetFloat("t", t);
        cShaderInterpolation_.SetTexture(0, "texture1", gpuBuffer.PositionInterpolationTexture1);
        cShaderInterpolation_.SetTexture(0, "texture2", gpuBuffer.PositionInterpolationTexture2);
        cShaderInterpolation_.SetTexture(0, "textureOutput", gpuBuffer.PositionTexture);

        cShaderInterpolation_.Dispatch(0, 1, gpuBuffer.PositionTexture.height, 1);
      }
    }
    #endregion

    #region Normals recomputation (keeping original normals)
    private void RecomputeNormalsCPU(Mesh mesh, int cacheIdx, CarVertexDataCache vertexDataCache)
    {
      Vector3[] arrVertexCache = arrVertex3Cache1_[cacheIdx];
      Vector3[] arrNormalCache = arrVertex3Cache2_[cacheIdx];
      CarNormalCalculator.CalculateNormals(arrVertexCache, vertexDataCache.Cache, arrNormalCache);
      mesh.normals = arrNormalCache;
    }

    private void RecomputeNormalsGPU(int vertexCount, CarVertexAnimatedGPUBuffers gpuBuffer)
    {
      cShaderNormals_.SetInt("vertexCount", vertexCount);
      cShaderNormals_.SetTexture(0, "positions", gpuBuffer.PositionTexture);
      cShaderNormals_.SetTexture(0, "normals", gpuBuffer.NormalTexture);

      cShaderNormals_.SetBuffer(0, "arrVertexData", gpuBuffer.GetVertexDataBuffer() );

      int threadGroupSize = Mathf.CeilToInt((float)vertexCount / 32.0f);
      cShaderNormals_.Dispatch(0, threadGroupSize, 1, 1);
    }
    #endregion

    #region Read animation events
    private void ReadEvents(ref int cursor)
    {
      if (animationEvent != null && (animationEvent.GetPersistentEventCount() > 0) )
      {
        if (binaryVersion_ < 9)
        {
          ReadEventsV1(ref cursor);
        }
        else
        {
          ReadEventsCurrent(ref cursor);
        }
      }
    }

    private void ReadEventsV1(ref int cursor)
    {
      int nEvents = ReadInt32(ref cursor);
      for (int i = 0; i < nEvents; i++)
      {
        int idEmitter = ReadInt32(ref cursor);
        ceData.emitterName_ = arrEmitterName_[idEmitter];

        int idBodyA = ReadInt32(ref cursor);
        int idBodyB = ReadInt32(ref cursor);

        Transform trA = arrAnimatedGO_[idBodyA].tr_;
        Transform trB = arrAnimatedGO_[idBodyB].tr_;

        if (trA != null)
        {
          ceData.GameObjectA = trA.gameObject;
        }
        else
        {
          ceData.GameObjectA = null;
        }

        if (trB != null)
        {
          ceData.GameObjectB = trB.gameObject;
        }
        else
        {
          ceData.GameObjectB = null;
        }

        Matrix4x4 m_LOCAL_to_WORLD = transform.localToWorldMatrix;

        ceData.position_.x = ReadSingle(ref cursor);
        ceData.position_.y = ReadSingle(ref cursor);
        ceData.position_.z = ReadSingle(ref cursor);

        ceData.position_ = m_LOCAL_to_WORLD.MultiplyPoint3x4(ceData.position_);

        ceData.velocityA_.x = ReadSingle(ref cursor);
        ceData.velocityA_.y = ReadSingle(ref cursor);
        ceData.velocityA_.x = ReadSingle(ref cursor);     

        ceData.velocityA_ = m_LOCAL_to_WORLD.MultiplyVector(ceData.velocityA_);

        ceData.velocityB_.x = ReadSingle(ref cursor);
        ceData.velocityB_.y = ReadSingle(ref cursor);
        ceData.velocityB_.x = ReadSingle(ref cursor);

        ceData.velocityB_ = m_LOCAL_to_WORLD.MultiplyVector(ceData.velocityB_);

        ceData.relativeSpeed_N_ = ReadSingle(ref cursor);
        ceData.relativeSpeed_T_ = ReadSingle(ref cursor);

        ceData.relativeP_N_ = ReadSingle(ref cursor);
        ceData.relativeP_T_ = ReadSingle(ref cursor);

        animationEvent.Invoke(ceData);
      }
    }

    private void ReadEventsCurrent(ref int cursor)
    {
      int nEvents = ReadInt32(ref cursor);
      for (int i = 0; i < nEvents; i++)
      {
        int idEmitter = ReadInt32(ref cursor);
        ceData.emitterName_ = arrEmitterName_[idEmitter];

        ceData.type_ = (CRAnimationEvData.EEventDataType)ReadInt32(ref cursor);

        int idBodyA = ReadInt32(ref cursor);
        int idBodyB = ReadInt32(ref cursor);

        Transform trA = arrAnimatedGO_[idBodyA].tr_;
        Transform trB = arrAnimatedGO_[idBodyB].tr_;

        if (trA != null)
        {
          ceData.GameObjectA = trA.gameObject;
        }
        else
        {
          ceData.GameObjectA = null;
        }

        if (trB != null)
        {
          ceData.GameObjectB = trB.gameObject;
        }
        else
        {
          ceData.GameObjectB = null;
        }

        Matrix4x4 m_LOCAL_to_WORLD = transform.localToWorldMatrix;

        ceData.position_.x = ReadSingle(ref cursor);
        ceData.position_.y = ReadSingle(ref cursor);
        ceData.position_.z = ReadSingle(ref cursor);

        ceData.position_ = m_LOCAL_to_WORLD.MultiplyPoint3x4(ceData.position_);

        ceData.velocityA_.x = ReadSingle(ref cursor);
        ceData.velocityA_.y = ReadSingle(ref cursor);
        ceData.velocityA_.x = ReadSingle(ref cursor);     

        ceData.velocityA_ = m_LOCAL_to_WORLD.MultiplyVector(ceData.velocityA_);

        ceData.velocityB_.x = ReadSingle(ref cursor);
        ceData.velocityB_.y = ReadSingle(ref cursor);
        ceData.velocityB_.x = ReadSingle(ref cursor);

        ceData.velocityB_ = m_LOCAL_to_WORLD.MultiplyVector(ceData.velocityB_);

        ceData.relativeSpeed_N_ = ReadSingle(ref cursor);
        ceData.relativeSpeed_T_ = ReadSingle(ref cursor);

        ceData.relativeP_N_ = ReadSingle(ref cursor);
        ceData.relativeP_T_ = ReadSingle(ref cursor);

        animationEvent.Invoke(ceData);
      }
    }

    #endregion

    #region Binary data read methods

    private bool ReadBoolean(ref int cursor)
    {
      int offset = cursor;
      cursor += sizeof(bool);
      return BitConverter.ToBoolean(binaryAnim_, offset);
    }

    private string ReadString(ref int cursor)
    {
      string str = BitConverter.ToString(binaryAnim_, cursor);
      cursor += str.Length * (sizeof(char) + 1);
      return str;
    }

    private byte ReadByte(ref int cursor)
    {
      return CarBinaryReader.ReadByteFromArrByte(binaryAnim_, ref cursor);
    }

    private SByte ReadSByte(ref int cursor)
    {
      return CarBinaryReader.ReadSByteFromArrByte(binaryAnim_, ref cursor);
    }

    private UInt16 ReadUInt16(ref int cursor)
    {
      return (CarBinaryReader.ReadUInt16FromArrByte(binaryAnim_, ref cursor) );
    }  

    private Int32 ReadInt32(ref int cursor)
    {
      return (CarBinaryReader.ReadInt32FromArrByte(binaryAnim_, ref cursor) );
    }  

    private Int64 ReadInt64(ref int cursor)
    {
      return (CarBinaryReader.ReadInt64FromArrByte(binaryAnim_, ref cursor) );
    }

    private float ReadSingle(ref int cursor)
    {
      return CarBinaryReader.ReadSingleFromArrByte(binaryAnim_, ref cursor);
    }

    private void SetCursorAt(Int64 bytesOffset, ref int cursor)
    {
      cursor = (int)bytesOffset;
    }

    private void AdvanceCursor(Int64 bytesOffset, ref int cursor)
    {
      cursor += (int)bytesOffset;
    }

    private void AdvanceCursorIfExists(long offsetBytesSize, bool exists, ref int cursor)
    {
      if (exists)
      {
        if (isAlignedData_)
        {
          AdvanceCursorPadding4(ref cursor);
        }
        AdvanceCursor(offsetBytesSize, ref cursor);
      }
    }

    private void AdvanceCursorSkipMesh(int vertexCount, bool isCompressed, bool hasTangents, ref int cursor)
    {
      if (isCompressed)
      {
        AdvanceCursor(vertexCount * sizeof(UInt16) * 3, ref cursor);
        AdvanceCursor(vertexCount * sizeof(SByte) * 3, ref cursor);

        if (hasTangents)
        {
          AdvanceCursor(vertexCount * sizeof(SByte) * 4, ref cursor);
        }
      }
      else
      {
        AdvanceCursor(vertexCount * sizeof(float) * 3, ref cursor);
        AdvanceCursor(vertexCount * sizeof(float) * 3, ref cursor);
        if (hasTangents)
        {
          AdvanceCursor(vertexCount * sizeof(float) * 4, ref cursor);
        }
      }
    }

    private void AdvanceCursorsIfExists(long offsetBytesSize, bool existPrev, ref int cursorPrev, bool existNext, ref int cursorNext )
    {
      if (existPrev)
      {
        if (isAlignedData_)
        {
          AdvanceCursorPadding4(ref cursorPrev);
        }
        AdvanceCursor(offsetBytesSize, ref cursorPrev);
      }

      if (existNext)
      {
        if (isAlignedData_)
        {
          AdvanceCursorPadding4(ref cursorNext);
        }
        AdvanceCursor(offsetBytesSize, ref cursorNext);
      }
    }

    private void AdvanceCursorPadding4(ref int cursor)
    {
      int padding = cursor % 4;      
      if (padding != 0)
      {
        AdvanceCursor(4 - padding, ref cursor);
      }
    }

    #endregion

    #endregion

    #region IAnimatorExporter interface

    public void InitAnimationBaking(out int nFrames, out int fps, out float deltaTimeFrame, out float animationLength)
    {
      LoadActiveAnimation(true);

      nFrames         = frameCount_;
      fps             = fps_;
      deltaTimeFrame  = frameTime_;
      animationLength = animationLength_;
    }

    public void FinishAnimationBaking()
    {
      CloseActiveAnimation();
    }

    public void GetGOHeaderData(List<CarGOHeaderData> listGOHeaderData)
    {
      listGOHeaderData.Clear();

      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];
           
        Transform tr        = goInfo.tr_;
        if (tr == null)
        {
          continue;
        }

        int vertexCount     = goInfo.vertexCount_;
        int boneIdxBegin    = goInfo.boneIdxBegin_;
        int boneIdxEnd      = goInfo.boneIdxEnd_;
        int boneCount       = goInfo.boneCount_;

        GameObject go = tr.gameObject;
        string goRelativePath = go.GetRelativePathTo(this.gameObject);

        List<string> listBoneRelativePath = new List<string>();
        for (int j = boneIdxBegin; j < boneIdxEnd; j++)
        {
          Transform boneTr = arrBoneTr_[j];
          if (boneTr != null)
          {
            GameObject boneGO = boneTr.gameObject;
            string boneRelativePath = boneGO.GetRelativePathTo(this.gameObject);
            listBoneRelativePath.Add(boneRelativePath);
          }
          else
          {
            listBoneRelativePath.Add("CRNotFoundBonePath");
          }
        }  
        
        listGOHeaderData.Add(new CarGOHeaderData(goRelativePath, vertexCount, boneCount, listBoneRelativePath));   
      }
    }

    public void GetSkinnedGOHeaderData(List<CarSkinnedGOHeaderData> listSkinnedGOHeaderData)
    {
      listSkinnedGOHeaderData.Clear();
      for (int i = 0; i < nSkinnedGO_; i++)
      {
        GameObject skinnedGO = arrSkinnedGO_[i];
        if (skinnedGO != null)
        {
          string relativePath = skinnedGO.GetRelativePathTo(gameObject);
          listSkinnedGOHeaderData.Add( new CarSkinnedGOHeaderData(relativePath) );
        }
      }
    }
 
    public void GetGOVisibilityData(List<CarGOVisibilityData> listVisibilityData)
    {
      listVisibilityData.Clear();

      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];

        Transform tr = goInfo.tr_;
        if (tr != null)
        {
          Vector2 visibilityInterval = arrVisibilityInterval_[i];
          listVisibilityData.Add( new CarGOVisibilityData(tr, visibilityInterval) );
        }
      }
    }

    public void GetGOFrameData(int frame, List<CarGOFrameData> listFrameData)
    {
      List<EGOKeyFrameFlags> listGOKeyFrameFlags = new List<EGOKeyFrameFlags>();
      GetGOKeyFrameFlags(frame, listGOKeyFrameFlags);

      listFrameData.Clear();
      SetFrame(frame);

      List<Transform>  listBoneTransform   = new List<Transform>();
      List<Vector3>    listBoneTranslation = new List<Vector3>();
      List<Quaternion> listBoneRotation    = new List<Quaternion>();
      List<Vector3>    listBoneScale       = new List<Vector3>();

      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];    
        Transform tr = goInfo.tr_;

        bool skipGameObject = arrSkipObject_[i];
        if (tr == null || skipGameObject)
        {
          continue;
        }

        int vertexCount  = goInfo.vertexCount_;
        int boneIdxBegin = goInfo.boneIdxBegin_;
        int boneIdxEnd   = goInfo.boneIdxEnd_;
        int boneCount    = goInfo.boneCount_;

        CarGOKeyframe goKeyframe = new CarGOKeyframe(vertexCount, boneCount, hasTangentsData_);
        listFrameData.Add( new CarGOFrameData(tr, goKeyframe) );

        GameObject go = tr.gameObject;

        EGOKeyFrameFlags flags = listGOKeyFrameFlags[i];

        bool isVisible = flags.IsFlagSet(EGOKeyFrameFlags.VISIBLE);
        bool isGhost   = flags.IsFlagSet(EGOKeyFrameFlags.GHOST);

        goKeyframe.SetBodyKeyframe(isVisible, isGhost, tr.localPosition, tr.localRotation);

        if ( goKeyframe.HasFrameData() )
        {
          if (vertexCount > 0)
          {
            Mesh mesh = go.GetMesh();
            if (mesh != null)
            {
              goKeyframe.SetVertexKeyframe(mesh.vertices, mesh.normals, mesh.tangents);
            }
          }
          else if (boneCount > 0)
          {
            GetFrameBonesData(boneIdxBegin, boneIdxEnd, listBoneTransform, listBoneTranslation, listBoneRotation, listBoneScale );
            goKeyframe.SetBonesTransform(listBoneTransform.ToArray());
            goKeyframe.SetBonesKeyframe(listBoneTranslation.ToArray(), listBoneRotation.ToArray(), listBoneScale.ToArray());
          }
        }
      } //forGameobjects
    }

    public void GetSkinnedGOFrameData(int frame, List<CarSkinnedGOFrameData> listSkinnedGOFrameData)
    {
      listSkinnedGOFrameData.Clear();

      long frameOffset = arrFrameOffsets_[frame];
      SetCursorAt(frameOffset, ref bCursor1_);

      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];
        int offsetBytesSize = goInfo.bytesOffset_;

        EGOKeyFrameFlags flagsGO = (EGOKeyFrameFlags)binaryAnim_[bCursor1_];
        bCursor1_ += sizeof(byte);

        bool isVisible = flagsGO.IsFlagSet(EGOKeyFrameFlags.VISIBLE);
        bool isGhost = flagsGO.IsFlagSet(EGOKeyFrameFlags.GHOST);

        bool exists = isVisible || isGhost;
        AdvanceCursorIfExists(offsetBytesSize, exists, ref bCursor1_);
      }



      for (int i = 0; i < nSkinnedGO_; i++)
      {
        Vector3 center;
        center.x = ReadSingle(ref bCursor1_);
        center.y = ReadSingle(ref bCursor1_);
        center.z = ReadSingle(ref bCursor1_);

        Vector3 size;
        size.x = ReadSingle(ref bCursor1_);
        size.y = ReadSingle(ref bCursor1_);
        size.z = ReadSingle(ref bCursor1_);

        listSkinnedGOFrameData.Add( new CarSkinnedGOFrameData(center, size) );
      }
    }

    private void GetGOKeyFrameFlags(int frame, List<EGOKeyFrameFlags> listGOKeyFrameFlags)
    {
      listGOKeyFrameFlags.Clear();

      long frameOffset = arrFrameOffsets_[frame];
      SetCursorAt(frameOffset, ref bCursor1_);

      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];

        int offsetBytesSize = goInfo.bytesOffset_;

        EGOKeyFrameFlags flagsGO = (EGOKeyFrameFlags)binaryAnim_[bCursor1_];
        bCursor1_ += sizeof(byte);

        listGOKeyFrameFlags.Add(flagsGO);

        bool isVisible = flagsGO.IsFlagSet(EGOKeyFrameFlags.VISIBLE);
        bool isGhost   = flagsGO.IsFlagSet(EGOKeyFrameFlags.GHOST);

        bool exists = isVisible || isGhost;
        AdvanceCursorIfExists(offsetBytesSize, exists, ref bCursor1_);
      }
    }

    public List<string> GetEventEmitterNames()
    {
      List<string> listEventEmitterName = new List<string>();
      listEventEmitterName.AddRange(arrEmitterName_);
      return listEventEmitterName;
    }

    public void GetAnimationEventsData(int frame, List<CRAnimationEvData> listAnimationEventData)
    {
      listAnimationEventData.Clear();

      long frameOffset = arrFrameOffsets_[frame];
      SetCursorAt(frameOffset, ref bCursor1_);

      for (int i = 0; i < nGameObjects_; i++)
      {
        CarAnimatedGO goInfo = arrAnimatedGO_[i];
        int offsetBytesSize = goInfo.bytesOffset_;

        EGOKeyFrameFlags flagsGO = (EGOKeyFrameFlags)binaryAnim_[bCursor1_];
        bCursor1_ += sizeof(byte);

        bool isVisible = flagsGO.IsFlagSet(EGOKeyFrameFlags.VISIBLE);
        bool isGhost   = flagsGO.IsFlagSet(EGOKeyFrameFlags.GHOST);

        bool exists = isVisible || isGhost;
        AdvanceCursorIfExists(offsetBytesSize, exists, ref bCursor1_);
      }

      SkipSkinnedObjectsBoxes();

      if (binaryVersion_ < 9)
      {
        WriteEventsV1(listAnimationEventData);
      }
      else
      {
        WriteEventsCurrent(listAnimationEventData);
      }
    }

    private void WriteEventsV1(List<CRAnimationEvData> listAnimationEventData)
    {
      int nEvents = ReadInt32(ref bCursor1_);
      for (int i = 0; i < nEvents; i++)
      {
        CRContactEvData ceData = new CRContactEvData();

        int idEmitter = ReadInt32(ref bCursor1_);
        ceData.emitterName_ = arrEmitterName_[idEmitter];
        ceData.type_ = CRAnimationEvData.EEventDataType.Contact;

        int idBodyA = ReadInt32(ref bCursor1_);
        int idBodyB = ReadInt32(ref bCursor1_);

        Transform trA = arrAnimatedGO_[idBodyA].tr_;
        Transform trB = arrAnimatedGO_[idBodyB].tr_;

        if (trA != null)
        {
          ceData.GameObjectA = trA.gameObject;
        }
        else
        {
          ceData.GameObjectA = null;
        }

        if (trB != null)
        {
          ceData.GameObjectB = trB.gameObject;
        }
        else
        {
          ceData.GameObjectB = null;
        }

        Matrix4x4 m_LOCAL_to_WORLD = transform.localToWorldMatrix;

        ceData.position_.x = ReadSingle(ref bCursor1_);
        ceData.position_.y = ReadSingle(ref bCursor1_);
        ceData.position_.z = ReadSingle(ref bCursor1_);

        ceData.position_ = m_LOCAL_to_WORLD.MultiplyPoint3x4(ceData.position_);

        ceData.velocityA_.x = ReadSingle(ref bCursor1_);
        ceData.velocityA_.y = ReadSingle(ref bCursor1_);
        ceData.velocityA_.x = ReadSingle(ref bCursor1_);     

        ceData.velocityA_ = m_LOCAL_to_WORLD.MultiplyVector(ceData.velocityA_);

        ceData.velocityB_.x = ReadSingle(ref bCursor1_);
        ceData.velocityB_.y = ReadSingle(ref bCursor1_);
        ceData.velocityB_.x = ReadSingle(ref bCursor1_);

        ceData.velocityB_ = m_LOCAL_to_WORLD.MultiplyVector(ceData.velocityB_);

        ceData.relativeSpeed_N_ = ReadSingle(ref bCursor1_);
        ceData.relativeSpeed_T_ = ReadSingle(ref bCursor1_);

        ceData.relativeP_N_ = ReadSingle(ref bCursor1_);
        ceData.relativeP_T_ = ReadSingle(ref bCursor1_);

        listAnimationEventData.Add(ceData);
      }
    }

    private void WriteEventsCurrent(List<CRAnimationEvData> listAnimationEventData)
    {
      int nEvents = ReadInt32(ref bCursor1_);
      for (int i = 0; i < nEvents; i++)
      {
        CRAnimationEvData evData;
        int idEmitter = ReadInt32(ref bCursor1_);
        string emitterName = arrEmitterName_[idEmitter];

        CRAnimationEvData.EEventDataType type = (CRAnimationEvData.EEventDataType)ReadInt32(ref bCursor1_);

        if (type == CRAnimationEvData.EEventDataType.Contact)
        {
          evData = new CRContactEvData();
          evData.emitterName_ = emitterName;
          evData.type_ = type;

          CRContactEvData ceData = (CRContactEvData)evData;

          int idBodyA = ReadInt32(ref bCursor1_);
          int idBodyB = ReadInt32(ref bCursor1_);

          Transform trA = arrAnimatedGO_[idBodyA].tr_;
          Transform trB = arrAnimatedGO_[idBodyB].tr_;

          if (trA != null)
          {
            ceData.GameObjectA = trA.gameObject;
          }
          else
          {
            ceData.GameObjectA = null;
          }

          if (trB != null)
          {
            ceData.GameObjectB = trB.gameObject;
          }
          else
          {
            ceData.GameObjectB = null;
          }

          Matrix4x4 m_LOCAL_to_WORLD = transform.localToWorldMatrix;

          ceData.position_.x = ReadSingle(ref bCursor1_);
          ceData.position_.y = ReadSingle(ref bCursor1_);
          ceData.position_.z = ReadSingle(ref bCursor1_);

          ceData.position_ = m_LOCAL_to_WORLD.MultiplyPoint3x4(ceData.position_);

          ceData.velocityA_.x = ReadSingle(ref bCursor1_);
          ceData.velocityA_.y = ReadSingle(ref bCursor1_);
          ceData.velocityA_.x = ReadSingle(ref bCursor1_);     

          ceData.velocityA_ = m_LOCAL_to_WORLD.MultiplyVector(ceData.velocityA_);

          ceData.velocityB_.x = ReadSingle(ref bCursor1_);
          ceData.velocityB_.y = ReadSingle(ref bCursor1_);
          ceData.velocityB_.x = ReadSingle(ref bCursor1_);

          ceData.velocityB_ = m_LOCAL_to_WORLD.MultiplyVector(ceData.velocityB_);

          ceData.relativeSpeed_N_ = ReadSingle(ref bCursor1_);
          ceData.relativeSpeed_T_ = ReadSingle(ref bCursor1_);

          ceData.relativeP_N_ = ReadSingle(ref bCursor1_);
          ceData.relativeP_T_ = ReadSingle(ref bCursor1_);

          listAnimationEventData.Add(ceData);
        }
      }
    }

    private void GetFrameBonesData(int boneIdxBegin, int boneIdxEnd, List<Transform> listBoneTransform, List<Vector3> listBoneTranslation, List<Quaternion> listBoneRotation, List<Vector3> listBoneScale)
    {
      listBoneTransform.Clear();
      listBoneTranslation.Clear();
      listBoneRotation.Clear();
      listBoneScale.Clear();

      for (int i = boneIdxBegin; i < boneIdxEnd; i++)
      {
        Transform tr = arrBoneTr_[i];

        listBoneTransform.Add( tr );
        if ( tr != null)
        {
          listBoneTranslation.Add( tr.localPosition );
          listBoneRotation.Add( tr.localRotation );
          listBoneScale.Add( tr.localScale );
        }
        else
        {
          listBoneTranslation.Add( Vector3.zero );
          listBoneRotation.Add( Quaternion.identity );
          listBoneScale.Add( Vector3.one );
        }
      }   
    }

    #endregion

  }
}

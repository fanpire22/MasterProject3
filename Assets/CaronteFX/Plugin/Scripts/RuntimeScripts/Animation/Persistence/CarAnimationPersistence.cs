using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  [System.Serializable]
  public class CarMementoOriginal
  {
    [SerializeField]
    readonly GameObject originalGO_;

    [SerializeField]
    readonly bool isSkinnedMemento_;

    public CarMementoOriginal(GameObject originalGO, bool isSkinnedMemento)
    {
      originalGO_ = originalGO;
      isSkinnedMemento_ = isSkinnedMemento;
    }

    public override bool Equals(object obj)
    {
      CarMementoOriginal mementoOriginal = obj as CarMementoOriginal;
      if (mementoOriginal != null)
      {
        return (mementoOriginal.originalGO_ == originalGO_ &&
                mementoOriginal.isSkinnedMemento_ == isSkinnedMemento_);
      }

      return false;
    }

    public override int GetHashCode()
    {
      int hash = originalGO_.GetHashCode();
      hash = hash ^ isSkinnedMemento_.GetHashCode();
      return hash;
    }
  }

  [System.Serializable]
  public class CarMementoTarget
  {
    [SerializeField]
    readonly string targetPath_;
    public string TargetPath
    {
      get { return targetPath_; }                                           
    }

    [SerializeField]
    readonly string targetPathSkinnedGO_;
    public string TargetPathSkinnedGO
    {
      get { return targetPathSkinnedGO_; }
    }

    public CarMementoTarget(string targetPath, string targetPathSkinnedGO)
    {
      targetPath_ = targetPath;
      targetPathSkinnedGO_ = targetPathSkinnedGO;
    }

    public override bool Equals(object obj)
    {
      CarMementoTarget mementoTarget = obj as CarMementoTarget;
      if (mementoTarget != null)
      {
        return (mementoTarget.targetPath_ == targetPath_ &&
                mementoTarget.targetPathSkinnedGO_ == targetPath_);
      }
      
      return false;
    }

    public override int GetHashCode()
    {    
      int hash = targetPath_.GetHashCode();
      hash = hash ^ targetPathSkinnedGO_.GetHashCode();
      return ( hash );
    }
  }

  [System.Serializable]
  public class CarGameObjectMemento : CarSerializableDictionary<CarMementoOriginal, CarMementoTarget>
  {

  }

  [System.Serializable]
  public class CarBonePathToOptimizedBoneIdx: CarSerializableDictionary<string, int>
  {

  }

  public class CarAnimationPersistence : MonoBehaviour
  {
	[SerializeField, HideInInspector]
    private CarGameObjectMemento bakeGameObjectMemento_ = new CarGameObjectMemento();

    [SerializeField, HideInInspector]
    private CarBonePathToOptimizedBoneIdx dictBonePathToOptimizedBoneIdx_ = new CarBonePathToOptimizedBoneIdx();
    [SerializeField, HideInInspector]
    private List<CarOptimizedBone> listOptimizedBone_ = new List<CarOptimizedBone>();
    [SerializeField, HideInInspector]
    private List<CarOptimizedGO> listOptimizedGO_ = new List<CarOptimizedGO>();

    #region Optimized Hierarchy

    public bool HasOptimizedBones()
    {
      return (listOptimizedBone_.Count != 0);
    }

    public int GetOptimizedBoneCount()
    {
      return listOptimizedBone_.Count;
    }

    public bool HasOptimizedBone(string relativeBonePath)
    {
      return dictBonePathToOptimizedBoneIdx_.ContainsKey(relativeBonePath);
    }

    public int GetIndexOfOptimizedBone(string relativeBonePath)
    {
      return dictBonePathToOptimizedBoneIdx_[relativeBonePath];
    }

    public CarOptimizedBone GetOptimizedBone(string relativeBonePath)
    {
      int boneIdx = dictBonePathToOptimizedBoneIdx_[relativeBonePath];
      return listOptimizedBone_[boneIdx];
    }

    public CarOptimizedBone GetOptimizedBone(int boneIdx)
    {
      return listOptimizedBone_[boneIdx];
    }

    public void AddOptimizedBone(string bonePath, Transform originalBone, Transform tr, int boneIdx)
    {
      CarOptimizedBone optimizedBone = new CarOptimizedBone(tr, listOptimizedGO_.Count, boneIdx, bonePath, originalBone.localPosition, originalBone.localRotation, originalBone.localScale);

      dictBonePathToOptimizedBoneIdx_.Add(bonePath, listOptimizedBone_.Count);
      listOptimizedBone_             .Add(optimizedBone);
    }

    public bool HasOptimizedGameObjects()
    {
      return (listOptimizedGO_.Count != 0);
    }

    public int GetOptimizedGameObjectCount()
    {
      return listOptimizedGO_.Count;
    }

    public CarOptimizedGO GetOptimizedGameObject(int index)
    {
      return listOptimizedGO_[index];
    }

    public void AddOptimizedGameObject(GameObject go, int vertexCount, int boneCount, bool hasRootBone)
    {
      CarOptimizedGO optimizedGO = new CarOptimizedGO(go, boneCount, hasRootBone);
      listOptimizedGO_.Add(optimizedGO);
    }

    public void ClearOptimizedHierarchy()
    {
      dictBonePathToOptimizedBoneIdx_.Clear();
      listOptimizedBone_.Clear();
      listOptimizedGO_  .Clear();
    }
    #endregion

    #region Memento
    public void AddGameObjectBakeMemento(CarMementoOriginal mementoOriginal, string relativePath)
    {
      bakeGameObjectMemento_.Add(mementoOriginal, new CarMementoTarget(relativePath, string.Empty) );
    }

    public void AddGameObjectBakeMemento(CarMementoOriginal mementoOriginal, string relativePath1, string relativePath2)
    {
      bakeGameObjectMemento_.Add(mementoOriginal, new CarMementoTarget(relativePath1, relativePath2));
    }

    public bool HasGameObjectBakeMemento(CarMementoOriginal originalObject)
    {
      return bakeGameObjectMemento_.ContainsKey(originalObject);
    }

    public CarMementoTarget GetGameObjectBakeMemento(CarMementoOriginal originalObject)
    {
      return bakeGameObjectMemento_[originalObject];
    }
    #endregion

  }
}


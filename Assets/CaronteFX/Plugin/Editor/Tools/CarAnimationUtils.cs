using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CaronteFX
{
  public static class CarAnimationUtils
  {
    public static CarAnimationPersistence CreateAnimationPersistence(GameObject parentGameObject)
    {
      GameObject persistenceGO = new GameObject("_persistence");
      persistenceGO.transform.parent = parentGameObject.transform;
      persistenceGO.transform.SetAsFirstSibling();

      return (persistenceGO.AddComponent<CarAnimationPersistence>());      
    }

    public static void OptimizeTransformHierarchy(CRAnimation crAnimation)
    {
      GameObject rootAnimationGameObject = crAnimation.gameObject;

      CarAnimationPersistence animationPersistence = crAnimation.AnimationPersistence;
      if (animationPersistence == null)
      {
        animationPersistence = CreateAnimationPersistence(rootAnimationGameObject);
        crAnimation.AnimationPersistence = animationPersistence;
        EditorUtility.SetDirty(crAnimation);
      }
      
      SkinnedMeshRenderer[] arrSmr = rootAnimationGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
      foreach(SkinnedMeshRenderer smr in arrSmr)
      {
        OptimizeSkinnedMesh(crAnimation, smr);
      }
      EditorUtility.SetDirty(animationPersistence);

      foreach(SkinnedMeshRenderer smr in arrSmr)
      {
        DeleteGameObjectBones(smr);
      }

      foreach(SkinnedMeshRenderer smr in arrSmr)
      {
        ReplaceSkinnedMeshRendererForMeshRenderer(smr);
      }

      crAnimation.GPUSkinnedAnimation = true;
      EditorUtility.SetDirty(crAnimation);

#if UNITY_5_3 || UNITY_5_4_OR_NEWER
      UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();   
#else
      EditorApplication.MarkSceneDirty();
#endif

    }

    private static void OptimizeSkinnedMesh(CRAnimation crAnimation, SkinnedMeshRenderer smr)
    { 
      GameObject rootAnimationGameObject = crAnimation.gameObject;
      CarAnimationPersistence animationPersistence = crAnimation.AnimationPersistence;

      GameObject smrGameObject = smr.gameObject;
      Transform myTransform = smrGameObject.transform;

      Transform rootBoneTr = smr.rootBone;
      if (rootBoneTr != null)
      {
        GameObject rootBoneGO = rootBoneTr.gameObject;
        string relativeRootBonePath = rootBoneGO.GetRelativePathTo(rootAnimationGameObject);
        if (!animationPersistence.HasOptimizedBone(relativeRootBonePath))
        {
          animationPersistence.AddOptimizedBone(relativeRootBonePath, rootBoneTr, myTransform, -1);
        }
      }

      Transform[] arrBone = smr.bones;
      if (arrBone != null)
      {
        int nBones = arrBone.Length;
        for (int i = 0; i < nBones; i++)
        {
          Transform boneTr = arrBone[i];
          if (boneTr != null)
          {
            GameObject boneGO = boneTr.gameObject;
            string relativeBonePath = boneGO.GetRelativePathTo(rootAnimationGameObject);
            if (!animationPersistence.HasOptimizedBone(relativeBonePath))
            {
              animationPersistence.AddOptimizedBone(relativeBonePath, boneTr, myTransform, i);
            }
          }
        }

        if (nBones > 0)
        {
          animationPersistence.AddOptimizedGameObject(smrGameObject, smr.sharedMesh.vertexCount, nBones, rootBoneTr != null );
        }
      }
    }

    private static void DeleteGameObjectBones(SkinnedMeshRenderer smr)
    {
      Transform[] arrBone = smr.bones;
      if (arrBone != null)
      {
        int nBones = arrBone.Length;

        if (nBones > 0)
        {
          Transform boneTr = arrBone[0];
          if (boneTr != null)
          {
            Transform boneParentTr = boneTr.parent;
            if (boneParentTr != null)
            {
              GameObject boneParentGO = boneParentTr.gameObject;
              Object.DestroyImmediate(boneParentGO);
            }
          }
        }
      }
    }

    private static void ReplaceSkinnedMeshRendererForMeshRenderer(SkinnedMeshRenderer smr)
    {
      GameObject go = smr.gameObject;
      go.ReplaceSkinnedMeshRendererForMeshRenderer(true);
    }

    public static void UnoptimizeTransformHierarchy(CRAnimation crAnimation)
    {
      GameObject rootAnimationGameObject = crAnimation.gameObject;

      Transform rootAnimationTransform = rootAnimationGameObject.transform;

      CarAnimationPersistence animationPersistence = crAnimation.AnimationPersistence;
      if (animationPersistence == null)
      {
        return;
      }

      Dictionary<Renderer, Tuple2<Transform, List<Transform>> > dictionaryMRBones = new Dictionary<Renderer, Tuple2<Transform, List<Transform>>>();
      int nOptimizedBone = animationPersistence.GetOptimizedBoneCount();

      for (int i = 0; i < nOptimizedBone; i++)
      {
        CarOptimizedBone optimizedBone = animationPersistence.GetOptimizedBone(i);
        string originalPath = optimizedBone.GetOriginalPath();

        Transform tr = rootAnimationTransform.Find(originalPath);
        if (tr == null)
        {
          GameObject boneGO = CarEditorUtils.CreateGameObjectPath(rootAnimationTransform, originalPath);
          Transform boneTr = boneGO.transform;

          boneTr.localPosition = optimizedBone.GetOriginalLocalPosition();
          boneTr.localRotation = optimizedBone.GetOriginalLocalRotation();
          boneTr.localScale    = optimizedBone.GetOriginalLocalScale();

          Transform mrTransform = optimizedBone.GetOptimizedTransform();
          if (mrTransform != null)
          {
            Renderer rn = mrTransform.GetComponent<Renderer>();       
            if (rn != null)
            {
              Tuple2<Transform, List<Transform>> tBones;
              if (!dictionaryMRBones.ContainsKey(rn))
              {
                tBones = new Tuple2< Transform, List<Transform> >( null, new List<Transform>() );
                dictionaryMRBones[rn] = tBones;
              }
              
              tBones = dictionaryMRBones[rn];
              int boneIdx = optimizedBone.GetBoneIdx();
              if (boneIdx == -1)
              {
                tBones.First = boneTr;
              }
              else
              {
                tBones.Second.Add(boneTr);
              }
            }          
          }
        }
      }

      foreach( var pair in dictionaryMRBones )
      {
        Renderer mr = pair.Key;
        GameObject go = mr.gameObject;
        go.ReplaceMeshRendererForSkinnedMeshRenderer(true);

        SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();

        if (smr != null)
        {
          Tuple2<Transform, List<Transform>> tBones = pair.Value;
          smr.rootBone = tBones.First;
          smr.bones    = tBones.Second.ToArray();

          smr.updateWhenOffscreen = true;

          Mesh mesh = smr.sharedMesh;
          mesh.RecalculateBounds();
        }
      }

      animationPersistence.ClearOptimizedHierarchy();
      EditorUtility.SetDirty(animationPersistence);

      crAnimation.GPUSkinnedAnimation = false;
      EditorUtility.SetDirty(crAnimation);

#if UNITY_5_3 || UNITY_5_4_OR_NEWER
      UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#else
      EditorApplication.MarkSceneDirty();
#endif

    }
  }
}


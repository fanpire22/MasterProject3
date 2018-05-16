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
using System.Collections.Generic;
using UnityEngine;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif

using CaronteSharp;

namespace CaronteFX
{
  public class CarGOManager
  {
    /// <summary>
    /// GameObject - IdCaronte dictionaries
    /// </summary>
    CarBiDictionary<GameObject, uint> goToIdCaronte_ = new CarBiDictionary<GameObject, uint>(); //Bidirectional dictionary to translate GO - IDCaronte

    List<uint> listDeferredIdsToDelete_ = new List<uint>();

    List<Transform> listTransformAux_ = new List<Transform>();
    List<int> listGameObjectIdAux_ = new List<int>();
    //-----------------------------------------------------------------------------------
    public void HierarchyChange()
    {
#if UNITY_5_3_OR_NEWER
      RegisterUnityGameObjectsInCaronte2();
#else
      RegisterUnityGameObjectsInCaronte1();
#endif
      ReleaseDeletedObjectsFromCaronte();
    }
    //-----------------------------------------------------------------------------------
    public void Clear()
    {
      goToIdCaronte_.Clear();
      listDeferredIdsToDelete_.Clear();

      listTransformAux_.Clear();
      listGameObjectIdAux_.Clear();

      GOManager.unregisterAllGameObjects();
    }
    //-----------------------------------------------------------------------------------
    public bool GetIdCaronteFromGO(GameObject go, out uint id)
    {
      return (goToIdCaronte_.TryGetByFirst(go, out id));
    }
    //-----------------------------------------------------------------------------------
    public bool GetGOFromIdCaronte(uint id, out GameObject go)
    {
      return (goToIdCaronte_.TryGetBySecond(id, out go));
    }
    //-----------------------------------------------------------------------------------
    private void RegisterUnityGameObjectsInCaronte1()
    {
      GameObject[] sceneObjects = CarEditorUtils.GetAllGameObjectsInScene();
      int length = sceneObjects.Length;

      for (int i = 0; i < length; ++i)
      {
        GameObject go = sceneObjects[i];
        CarEditorUtils.GetChildrenGameObjectsIds(go, listGameObjectIdAux_);
        uint idCaronte;
        bool exists = goToIdCaronte_.TryGetByFirst(go, out idCaronte);
        if (!exists)
        {
          idCaronte = GOManager.RegisterGameObject(go.name, go.GetInstanceID(), listGameObjectIdAux_.ToArray());
          goToIdCaronte_.Add(go, idCaronte);
        }
        else
        {
          GOManager.ReregisterGameObject(idCaronte, go.name, go.GetInstanceID(), listGameObjectIdAux_.ToArray());
        }
      }
    }
#if UNITY_5_3_OR_NEWER
    //-----------------------------------------------------------------------------------
    private void RegisterUnityGameObjectsInCaronte2()
    {
      int nScene = EditorSceneManager.sceneCount;
      for (int i = 0; i < nScene; i++)
      {
        Scene iScene = EditorSceneManager.GetSceneAt(i);
        GameObject[] arrGameObject = iScene.GetRootGameObjects();
        foreach (GameObject go in arrGameObject)
        {
          RegisterRootGameObjectInCaronte(go);
        }
      }
    }
    //-----------------------------------------------------------------------------------
    private void RegisterRootGameObjectInCaronte(GameObject go)
    {
      int idUnity = go.GetInstanceID();

      uint idCaronte;
      bool exists = goToIdCaronte_.TryGetByFirst(go, out idCaronte);
      if (!exists)
      {
        idCaronte = GOManager.RegisterRootGameObject(go.name, idUnity);
        goToIdCaronte_.Add(go, idCaronte);
      }
      else
      {
        GOManager.ReregisterRootGameObject(idCaronte, go.name, idUnity);
      }

      Transform tr = go.transform;
      int nChildren = tr.childCount;

      for (int i = 0; i < nChildren; i++)
      {
        Transform childTr = tr.GetChild(i);
        RegisterGameObjectInCaronte(childTr.gameObject, idUnity);
      }
    }
    //-----------------------------------------------------------------------------------
    private void RegisterGameObjectInCaronte(GameObject go, int parentGOId)
    {
      int idUnity = go.GetInstanceID();

      uint idCaronte;
      bool exists = goToIdCaronte_.TryGetByFirst(go, out idCaronte);
      if (!exists)
      {
        idCaronte = GOManager.RegisterGameObject(go.name, idUnity, parentGOId);
        goToIdCaronte_.Add(go, idCaronte);
      }
      else
      {
        GOManager.ReregisterGameObject(idCaronte, go.name, idUnity, parentGOId);
      }

      Transform tr = go.transform;
      int nChildren = tr.childCount;

      for (int i = 0; i < nChildren; i++)
      {
        Transform childTr = tr.GetChild(i);
        RegisterGameObjectInCaronte(childTr.gameObject, idUnity);
      }
    }
#endif
    //-----------------------------------------------------------------------------------
    private void ReleaseDeletedObjectsFromCaronte()
    {
      foreach (var kvPair in goToIdCaronte_)
      {
        GameObject go = kvPair.Key;
        uint id = kvPair.Value;

        if (go == null)
        {
          listDeferredIdsToDelete_.Add(id);
        }
      }

      DeleteDeferredGameObjects();
    }
    //-----------------------------------------------------------------------------------
    private void DeleteDeferredGameObjects()
    {
      foreach (uint id in listDeferredIdsToDelete_)
      {
        goToIdCaronte_.TryRemoveBySecond(id);
        GOManager.unregisterGameObject(id);
      }

      listDeferredIdsToDelete_.Clear();
    }
    //-----------------------------------------------------------------------------------
  }
}
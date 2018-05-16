using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public static class CarBodyUtils
  {
    public static Caronte_Fx_Body AddBodyComponentIfHasMesh(GameObject go)
    {
      Caronte_Fx_Body bodyComponent = go.GetComponent<Caronte_Fx_Body>();
   
      if (bodyComponent == null && go.HasMesh() )
      {
        bodyComponent = go.AddComponent<Caronte_Fx_Body>();
      }

      return bodyComponent;
    }
    //-----------------------------------------------------------------------------------
    public static bool AddBodyComponentIfHasMeshReturnHasValidCollider(GameObject go)
    {
      Caronte_Fx_Body bodyComponent = AddBodyComponentIfHasMesh(go);

      return (bodyComponent != null && bodyComponent.HasValidCollider() );
    }
    //-----------------------------------------------------------------------------------
    public static bool AddBodyComponentIfHasMeshReturnHasValidColliderOrBalltree(GameObject go)
    {
      Caronte_Fx_Body bodyComponent = AddBodyComponentIfHasMesh(go);

      return (bodyComponent != null && (bodyComponent.HasValidCollider() || bodyComponent.IsUsingBalltree()) );
    }

    //-----------------------------------------------------------------------------------
    public static bool AddBodyComponentIfHasMeshReturnHasValidRenderMesh(GameObject go)
    {
      Caronte_Fx_Body bodyComponent = AddBodyComponentIfHasMesh(go);

      return (bodyComponent != null && go.HasMesh() );
    }
    //-----------------------------------------------------------------------------------
    public static bool HasValidRenderMesh(GameObject go)
    {
      return go.HasMesh();
    }
    //-----------------------------------------------------------------------------------
    public static bool HasValidColliderMesh(GameObject go)
    {
      Caronte_Fx_Body bodyComponent = go.GetComponent<Caronte_Fx_Body>();
   
      if (bodyComponent != null )
      {
        return bodyComponent.HasValidCollider();
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    public static void GetRenderMeshData( GameObject go, ref Mesh meshRender, out Matrix4x4 m_Render_MODEL_to_WORLD, ref bool isBakedRenderMesh )
    {
      Caronte_Fx_Body bodyComponent = go.GetComponent<Caronte_Fx_Body>();
      isBakedRenderMesh = bodyComponent.GetRenderMesh(ref meshRender, out m_Render_MODEL_to_WORLD);
    }
    //-----------------------------------------------------------------------------------
    public static void GetColliderMeshData( GameObject go, ref Mesh meshCollider, out Matrix4x4 m_Collider_MODEL_to_WORLD, ref bool isBakedColliderMesh )
    {     
      Caronte_Fx_Body bodyComponent = go.GetComponent<Caronte_Fx_Body>();
      isBakedColliderMesh = bodyComponent.GetColliderMesh(ref meshCollider, out m_Collider_MODEL_to_WORLD);
    }
    //-----------------------------------------------------------------------------------
    public static void GetDefinitionAndTileMeshes( GameObject go, ref Mesh meshDefinition, out Matrix4x4 m_Definition_MODEL_to_WORLD, ref bool isBakedDefinitionMesh, ref Mesh meshTile, ref bool smoothRenderMesh )
    {     
      Caronte_Fx_Body bodyComponent = go.GetComponent<Caronte_Fx_Body>();
      isBakedDefinitionMesh = bodyComponent.GetRenderMesh(ref meshDefinition, out m_Definition_MODEL_to_WORLD);
      smoothRenderMesh = bodyComponent.DoSmoothRopeRenderMesh;
      meshTile = bodyComponent.GetTileMesh();
    }
    //-----------------------------------------------------------------------------------
    public static void GetBalltreeAsset( GameObject go, ref CRBalltreeAsset btAsset)
    {     
      Caronte_Fx_Body bodyComponent = go.GetComponent<Caronte_Fx_Body>();
      btAsset =  bodyComponent.GetBalltreeAsset();   
    }
    //-----------------------------------------------------------------------------------
    public static void GetSkeletonWeights( GameObject go, out SkelWeights skelweights )
    {
      skelweights = null;

      SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
      if (smr != null)
      {
        Mesh mesh = smr.sharedMesh;    
        BoneWeight[] arrBoneW = mesh.boneWeights;

        if ( arrBoneW != null && arrBoneW.Length > 0 )
        {
          int arr_size = arrBoneW.Length;

          CaronteSharp.Tuple4_INDEX[] arrBoneIndex  = new CaronteSharp.Tuple4_INDEX[arr_size];
          CaronteSharp.Tuple4_FLOAT[] arrBoneWeight = new CaronteSharp.Tuple4_FLOAT[arr_size];

          for(int i = 0; i < arrBoneIndex.Length; i++)
          {
            BoneWeight bw = arrBoneW[i];

            arrBoneIndex[i]  = new CaronteSharp.Tuple4_INDEX( (uint)bw.boneIndex0, (uint)bw.boneIndex1, (uint)bw.boneIndex2, (uint)bw.boneIndex3 );
            arrBoneWeight[i] = new CaronteSharp.Tuple4_FLOAT( bw.weight0, bw.weight1, bw.weight2, bw.weight3 );            
          }

          skelweights = new SkelWeights();
          skelweights.arrIndexBone_ = arrBoneIndex;
          skelweights.arrWeightBone_ = arrBoneWeight;
        }
      }   
    }
    //-----------------------------------------------------------------------------------
  }
}

using UnityEngine;
using System.Collections.Generic;

namespace CaronteFX
{

  public class CarBalltreeMeshes
  {
    List<Mesh>         listMesh_    = new List<Mesh>();
    CarSpheresGenerator spGenerator_ = new CarSpheresGenerator();
    //-----------------------------------------------------------------------------------
    public CarBalltreeMeshes()
    {
    }
    //-----------------------------------------------------------------------------------
    public void AddSphere(Color color, Vector3 position, float radius)
    {
      if (!spGenerator_.CanAddSphere128Faces66Vertices())
      {
        listMesh_.Add(spGenerator_.GenerateMeshTmp());
      }

      spGenerator_.AddOrthoSphere128Faces66Vertices(position, radius, color);
    }
    //-----------------------------------------------------------------------------------
    public void FinishAddingSpheres()
    {
      if (spGenerator_.CanGenerateMesh())
      {
        listMesh_.Add(spGenerator_.GenerateMeshTmp());
      }
    }
    //-----------------------------------------------------------------------------------
    public void DrawMeshesSolid(Matrix4x4 m_Local_To_World, Material material)
    {
      int nMeshes = listMesh_.Count;

      material.SetPass(0);
      for (int i = 0; i < nMeshes; i++)
      {
        Mesh mesh = listMesh_[i];    
        Graphics.DrawMeshNow(mesh, m_Local_To_World);
      }
    }
    //-----------------------------------------------------------------------------------
  }


  public class CarBalltreeMeshesManager
  {
    Dictionary<CRBalltreeAsset, CarBalltreeMeshes> dictionaryBalltreeMeshes = new Dictionary<CRBalltreeAsset, CarBalltreeMeshes>();
    public static Material material_;

    private static CarBalltreeMeshesManager instance_;
    public static CarBalltreeMeshesManager Instance
    {
      get
      {
        if ( instance_ == null )
        {
          instance_ = new CarBalltreeMeshesManager();
        }
        return instance_;
      }
    }

    private CarBalltreeMeshesManager()
    {

    }

    public bool HasBalltreeMeshes(CRBalltreeAsset btAsset)
    {
      return dictionaryBalltreeMeshes.ContainsKey(btAsset);
    }

    public CarBalltreeMeshes GetBalltreeMeshes(CRBalltreeAsset btAsset)
    {
      if (!dictionaryBalltreeMeshes.ContainsKey(btAsset))
      {
        CarBalltreeMeshes btMeshes = CreateBalltreeMeshes(btAsset);
        dictionaryBalltreeMeshes.Add(btAsset, btMeshes);
      }

      return dictionaryBalltreeMeshes[btAsset];
    }

    public Material GetBalltreeMaterial()
    {
      return material_;
    }

    private CarBalltreeMeshes CreateBalltreeMeshes(CRBalltreeAsset btAsset)
    {
      CarBalltreeMeshes btMeshes = new CarBalltreeMeshes();
      CarSphere[] arrSphere = btAsset.LeafSpheres;
      int arrSphere_size = arrSphere.Length;
      for (uint i = 0; i < arrSphere_size; i++)
      {
        CarSphere sphere = arrSphere[i];
        btMeshes.AddSphere(CarColor.ColorBasic42(i), sphere.center_, sphere.radius_);
      }

      btMeshes.FinishAddingSpheres();
      return btMeshes;
     }
  }
}

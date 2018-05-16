using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  [AddComponentMenu("CaronteFX/Miscellaneous/FPS Counter")]
  public class CarFPSCounter : MonoBehaviour
  {
    public int updatesPerSecond = 3;

    float accumulatedDt = 0.0f;
    int frameCount_ = 0;
    
    float fps_   = 0;
    float msecs_ = 0;

    GUIStyle style_; 

    private void Start()
    {    
		  style_ = new GUIStyle();
      style_.alignment = TextAnchor.UpperLeft;
		  style_.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
    }

	  void Update()
    {
      float dtUpdate = 1.0f / updatesPerSecond;

		  frameCount_++;
      accumulatedDt += Time.deltaTime;

      if ( accumulatedDt > dtUpdate )
      {
        fps_ = frameCount_ / accumulatedDt;
        msecs_ = 1000.0f / fps_ ;
        accumulatedDt -= dtUpdate;
        frameCount_ = 0;
      }
	  }

	  void OnGUI()
	  {
		  int w = Screen.width, h = Screen.height;
		  string text = string.Format("{0:0.0} ms ({1:0.} fps)", msecs_, fps_);

      style_.fontSize = h * 2 / 50;
      Rect rect = new Rect(0, 0, w, style_.fontSize);
		  GUI.Label(rect, text, style_);
	  }
  }
}

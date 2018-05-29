using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField] Text txtTrace;
    [SerializeField] Text txtTraceSombra;
    [SerializeField] GameManager GameOverlay;
    [SerializeField] GameManager GameOverOverlay;

    public float TraceLvl;

    // Update is called once per frame
    void Update ()
    {

        if (TraceLvl >= 100)
        {
            //Game Over
            TraceLvl = 100;

            GameOver();
        }
        else
        {
            txtTrace.text = string.Format("TRACE: {0}%", TraceLvl);
            txtTraceSombra.text = string.Format("TRACE: {0}%", TraceLvl);
        }

    }

    public void GameOver()
    {
        GameOverlay.enabled = false;
        GameOverOverlay.enabled = true;
    }
}

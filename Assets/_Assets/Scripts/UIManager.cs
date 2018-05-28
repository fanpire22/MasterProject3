using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField] Text txtTrace;
    [SerializeField] Text txtTraceSombra;

    public float TraceLvl;

    // Update is called once per frame
    void Update () {
        txtTrace.text = string.Format("TRACE: {0}%", TraceLvl);
        txtTraceSombra.text = string.Format("TRACE: {0}%", TraceLvl);
    }
}

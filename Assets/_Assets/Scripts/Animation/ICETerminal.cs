using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICETerminal : MonoBehaviour {

    [SerializeField] ICE[] _controlledICE;

    Interactive _terminal;

    private void Awake()
    {
        _terminal = GetComponent<Interactive>();
    }

    private void Start()
    {

        //Nos suscribimos al evento de Activación
        _terminal.OnInteraction += DestroyICE;
    }

    void DestroyICE()
    {
        for(int i = 0; i < _controlledICE.Length; i++)
        {
            _controlledICE[i].Die();
        }
    }

}

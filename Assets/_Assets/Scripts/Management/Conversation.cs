using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Conversation : MonoBehaviour
{

    public string Texto
    {
        set
        {
			_txtBocadillo.text = string.Empty;
            canv.enabled = true;
            _bHayQueEscribir = true;
            _iPalabra = 0;
            _siguientePalabra = Time.time + TiempoEscritura;
            _arrayPalabras = value.Split(' ');
        }
    }
    public float TiempoEscritura;

    private Text _txtBocadillo;
    private string[] _arrayPalabras;
    private bool _bHayQueEscribir = false;
    private float _siguientePalabra;
    private int _iPalabra;
    private Canvas canv;

    // Use this for initialization
    void Awake()
    {
        _txtBocadillo = GetComponentInChildren<Text>();

        canv = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_bHayQueEscribir) return;

        if (Time.time > _siguientePalabra)
        {
            _txtBocadillo.text = _txtBocadillo.text + _arrayPalabras[_iPalabra] + " ";
            _iPalabra++;

            _siguientePalabra = Time.time + TiempoEscritura;
			_bHayQueEscribir = (_iPalabra < _arrayPalabras.Length);

		}
	}

    public void ShutUp()
    {
        canv.enabled = false;
    }
}

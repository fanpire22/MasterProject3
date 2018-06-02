using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConverManager : MonoBehaviour
{

    public string Texto
    {
        set
        {
            canv.enabled = true;
            _txtBocadillo.text = string.Empty;
            _bHayQueEscribir = true;
            _iPalabra = 0;
            _siguienteLetra = Time.time + TiempoEscritura;
            _arrayTexto = value.ToCharArray();
        }
    }
    public float TiempoEscritura;
    public Sprite Avatar
    {
        set
        {
            avatar.sprite = value;
        }
    }


    private Text _txtBocadillo;
    private char[] _arrayTexto;
    private bool _bHayQueEscribir = false;
    private float _siguienteLetra;
    private int _iPalabra;
    private Canvas canv;
    private Image avatar;

    // Use this for initialization
    void Awake()
    {
        _txtBocadillo = GetComponentInChildren<Text>();

        canv = GetComponent<Canvas>();
        avatar = GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_bHayQueEscribir) return;

        if (Time.time > _siguienteLetra)
        {
            _txtBocadillo.text = _txtBocadillo.text + _arrayTexto[_iPalabra];
            _iPalabra++;

            _siguienteLetra = Time.time + TiempoEscritura;
            _bHayQueEscribir = (_iPalabra < _arrayTexto.Length);

        }
    }

    public void ShutUp()
    {
        canv.enabled = false;
    }
}

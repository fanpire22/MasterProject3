using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CinematicCheckPoint : MonoBehaviour {

    [SerializeField] AudioSource BGMSource;
    public Conversacion[] Conversaciones;
    
    float waitConvoTime;
    int _iConvo;
    bool _bConvoStart = false;
    Interactive _terminal;
    private void Awake()
    {
        _terminal = GetComponent<Interactive>();
    }

    private void Start()
    {

        //Nos suscribimos al evento de Activación
        _terminal.OnInteraction += StartConvo;
    }

    void StartConvo()
    {
        _bConvoStart = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_bConvoStart)
        {
            //Iniciamos la conversacion
            Conversacion();
        }
    }

    void Conversacion()
    {
        if (Time.time > waitConvoTime)
        {
            if (_iConvo >= Conversaciones.Length)
            {
                if (Conversaciones[_iConvo - 1].ConvoCamera)
                {
                    //Estábamos usando una cámara
                    Conversaciones[_iConvo - 1].ConvoCamera.gameObject.SetActive(false);
                }
                this.enabled = false;
            }
            else
            {

                ConverManager conver = GameManager.instance.Conver;
                conver.Texto = Conversaciones[_iConvo].Texto;

                waitConvoTime = Time.time + Conversaciones[_iConvo].Duracion;

                CancelInvoke("ShutUp");
                Invoke("ShutUp", Conversaciones[_iConvo].Duracion);

                if (Conversaciones[_iConvo].Avatar)
                {
                    //Tenemos un avatar: Lo ponemos en su imagen correspondiente
                    conver.Avatar = Conversaciones[_iConvo].Avatar;
                }

                if (_iConvo > 0 && Conversaciones[_iConvo - 1].ConvoCamera)
                {
                    //Estábamos usando una cámara
                    Conversaciones[_iConvo - 1].ConvoCamera.gameObject.SetActive(false);
                }

                if (Conversaciones[_iConvo].ConvoCamera)
                {
                    //Estamos usando una cámara
                    Conversaciones[_iConvo].ConvoCamera.gameObject.SetActive(true);
                }

                if (Conversaciones[_iConvo].BGM)
                {
                    if (BGMSource)
                    {
                        BGMSource.clip = Conversaciones[_iConvo].BGM;
                        BGMSource.Play();
                    }
                    else
                    {
                        AudioSource.PlayClipAtPoint(Conversaciones[_iConvo].BGM, this.transform.position);
                    }
                }

                _iConvo++;
            }
        }
    }
    void ShutUp()
    {
        GameManager.instance.Conver.ShutUp();
    }


}

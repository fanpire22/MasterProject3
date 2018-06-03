using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ending : MonoBehaviour {

    [SerializeField] AudioSource BGMSource;
    [SerializeField] ParticleSystem Explosion;
    [SerializeField] ParticleSystem LogOut;
    [SerializeField] GameObject FinalNode;
    public Conversacion[] Conversaciones;
    CinemachineVirtualCamera _cam;

    float waitConvoTime;
    int _iConvo;
    private bool _LastConversation = false;

    private void Awake()
    {
        _cam = GetComponent<CinemachineVirtualCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _LastConversation = true;
        }
    }

    private void Update()
    {
        if (_LastConversation)
        {
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
                _LastConversation = false;
                //Destruimos el nodo final, y Victoria
                Instantiate(Explosion, FinalNode.transform.position, FinalNode.transform.rotation);
                Instantiate(LogOut, GameManager.instance.player.transform.position, LogOut.transform.rotation);

                Destroy(FinalNode);

                Invoke("TheEnd", 0.5f);
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

    void TheEnd()
    {
        GameManager.instance.UI.Victory();
    }
}

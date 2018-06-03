using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.UI;

[System.Serializable]
public struct Conversacion
{
    public string Texto;
    public float Duracion;
    public CinemachineVirtualCamera ConvoCamera;
    public string Nombre;
    public Sprite Avatar;
    public AudioClip BGM;
}

public class CinematicIntro : MonoBehaviour
{

    [SerializeField] CinemachineVirtualCamera CMIntro;
    [SerializeField] CinemachineVirtualCamera CMMainCamera;
    [SerializeField] AudioSource BGMSource;
    public Conversacion[] Conversaciones;

    PlayableDirector _dir;
    NavMeshAgent _agent;
    float waitConvoTime;
    int _iConvo;
    AI2Animator _anim;
    bool _bConvoStart = false;
    Player _player;
    InteractionIK _interaction;

    private void Awake()
    {
        _dir = GetComponent<PlayableDirector>();
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<AI2Animator>();
        _interaction = GetComponent<InteractionIK>();
    }

    private void Start()
    {
        GameManager.instance.player.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_bConvoStart)
        {
            if (_dir.time >= _dir.duration - 0.3f)
            {
                _bConvoStart = true;
            }
        }
        else
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
                //Ha acabado la intro. Desactivamos la cámara, desactivamos el director, y activamos el NavMeshAgent del personaje;
                _agent.enabled = true;
                _anim.enabled = true;
                _dir.enabled = false;
                if (Conversaciones[_iConvo - 1].ConvoCamera)
                {
                    //Estábamos usando una cámara
                    Conversaciones[_iConvo - 1].ConvoCamera.gameObject.SetActive(false);
                }
                CMIntro.gameObject.SetActive(false);
                CMMainCamera.gameObject.SetActive(true);
                GameManager.instance.UI.IsTracing = true;
                _interaction.enabled = true;
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

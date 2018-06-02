using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public class Interactive : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] Transform _interactionPoint;
    [SerializeField] Transform _LeftHandler;
    [SerializeField] Transform _RightHandler;
    [SerializeField] PlayableDirector _dir;
    [SerializeField] int _maxActivations = 1;
    VCamTrigger _camTrigger;

    public Transform InteractionPoint { get { return _interactionPoint; } }
    public Transform LeftHandler { get { return _LeftHandler; } }
    public Transform RightHandler { get { return _RightHandler; } }

    public Action OnInteraction;

    Animator _ani;
    bool _isActive = false;
    int _nActivated = 0;

    private void Awake()
    {
        _ani = GetComponent<Animator>();
        _dir = GetComponent<PlayableDirector>();
        _camTrigger = GetComponentInChildren<VCamTrigger>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_nActivated < _maxActivations || _maxActivations == 0)
        {
            GameManager.instance.player.InteractWithItem(this);
        }
    }

    public IEnumerator ActivateInteractionCoRoutine()
    {
        if (_ani != null)
        {
            //Activamos el botón, esperamos dos décimas de segundo y activamos el director
            _ani.SetTrigger("Press");
        }

        yield return new WaitForSeconds(0.2f);

        if (_dir != null)
        {
            _dir.Play();
            yield return new WaitForSeconds((float)_dir.duration);
        }
        
        _nActivated++;
        if (_camTrigger != null && _nActivated == _maxActivations)
        {
            _camTrigger.DisableMe();
        }
    }

    public void OnActivationInteraction()
    {
        OnInteraction.Invoke() ;
    }
}

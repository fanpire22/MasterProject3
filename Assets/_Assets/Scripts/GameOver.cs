using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour {

    [SerializeField] GameObject _gameOverPanel;

    private void Awake()
    {
        _gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        //Nos suscribimos al evento de muerte
        GameManager.instance.player.OnDie += OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        _gameOverPanel.SetActive(true);
    }

    private void Update()
    {
        
    }
}

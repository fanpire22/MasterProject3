﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

	[SerializeField] Text txtTrace;
	[SerializeField] Text txtTraceSombra;
	[SerializeField] GameObject GameOverlay;
	[SerializeField] GameObject GameOverOverlay;

	public float TraceLvl;
	public bool IsTracing = false;

    private void Start()
    {
        //Nos suscribimos al evento de muerte
        GameManager.instance.player.OnDie += GameOver;
    }

    // Update is called once per frame
    void Update()
	{
		if (!IsTracing) return;

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
		GameOverlay.SetActive(false);
		GameOverOverlay.SetActive(true);
	}
}
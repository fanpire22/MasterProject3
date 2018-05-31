using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

	[SerializeField] Text txtTrace;
	[SerializeField] Text txtTraceSombra;
	[SerializeField] GameObject GameOverlay;
	[SerializeField] GameObject GameOverOverlay;
	Conversation ConversationOverlay;

	public float TraceLvl;
	public bool IsTracing = false;

	private void Awake()
	{
		ConversationOverlay = GetComponentInChildren<Conversation>();
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

	public void Talk(string Texto, float WaitTime)
	{
		ConversationOverlay.Texto = Texto;
	}

	public void ShutUp()
	{
		ConversationOverlay.ShutUp();
	}

	public void GameOver()
	{
		GameOverlay.SetActive(false);
		GameOverOverlay.SetActive(true);
	}
}

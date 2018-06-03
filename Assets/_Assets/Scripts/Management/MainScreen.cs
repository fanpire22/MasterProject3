using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreen : MonoBehaviour {

    [SerializeField] GameObject LayoutMain;
    [SerializeField] GameObject LayoutCargando;
    [SerializeField] Animator _player;

    private void Start()
    {
        _player.SetBool("crouch", true);
    }

    public void NewGame()
    {
        LayoutMain.SetActive(false);
        LayoutCargando.SetActive(true);
        SceneManager.LoadSceneAsync("Game");
    }

    /// <summary>
    ///Hacemos una validación PRAGMA: Si estamos en modo ejecución dentro de Unity, lo paramos. 
    ///Si estamos en ejecución, cerramos la aplicación 
    /// </summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

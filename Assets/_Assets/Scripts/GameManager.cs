using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    static GameManager _instance;

    public static GameManager instance {
        get { return _instance; }        
    }

    public Player player { get; private set; }    

    private void Awake()
    {
        _instance = this;

        player = FindObjectOfType<Player>();
    }
    
}

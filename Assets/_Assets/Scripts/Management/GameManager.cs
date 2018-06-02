using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    static GameManager _instance;

    public static GameManager instance {
        get { return _instance; }        
    }

    public Player player { get; private set; }
    public UIManager UI { get; private set; }
    public bool Pause;
    public BlackICE[] BlackICEInScenario;
    public ConverManager Conver;

    public void AlertICE()
    {
        for(int i = 0; i < BlackICEInScenario.Length; i++)
        {
            //Alertamos a todos los hielos negros de la posición del jugador, para que vayan a por él.
            //Añadimos "ruido" (variación de la posición) para que no converjan todos en el mismo punto exactamente
            Vector3 NoisePosition = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            BlackICEInScenario[i].BeginAlerted(player.transform.position + NoisePosition);
        }
    }


    private void Awake()
    {
        _instance = this;

        player = FindObjectOfType<Player>();
        UI = FindObjectOfType<UIManager>();
        Conver = FindObjectOfType<ConverManager>();
        Pause = false;
        BlackICEInScenario = FindObjectsOfType<BlackICE>();
    }
    
}

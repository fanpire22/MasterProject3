using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowCover : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player)
        {
            player.IsCrouching = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player)
        {
            player.IsCrouching = false;
        }
    }
}

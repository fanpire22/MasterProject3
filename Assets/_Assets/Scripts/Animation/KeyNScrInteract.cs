using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyNScrInteract : MonoBehaviour {

    [SerializeField] GameObject _screen;
    [SerializeField] GameObject _keyboard;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _screen.SetActive(true);
            _keyboard.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _screen.SetActive(false);
            _keyboard.SetActive(false);
        }
    }
}

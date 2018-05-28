using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMovement : MonoBehaviour
{

    [SerializeField] private float _period = 1;
    [SerializeField] private float _offset = 0;
    [SerializeField] private Vector3 AxesSine;
    [SerializeField] private float _amplitude = 0.06f;
    private Vector3 initialPos;

    private void Awake()
    {
        initialPos = transform.localPosition;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        float sin = (Mathf.Sin(Time.time * _period) * _amplitude + _offset);

        Vector3 frameLocation = initialPos + (AxesSine *  sin);

        transform.localPosition = frameLocation;
    }
}

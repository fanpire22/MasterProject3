using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {

    [SerializeField] LayerMask _laserLayerMask;

    LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Update () {

        RaycastHit hitInfo;
        if (Physics.Raycast(this.transform.position, this.transform.forward, out hitInfo, 100, _laserLayerMask))
        {
            Vector3 lineEnd = new Vector3(0, 0, hitInfo.distance);
            _lineRenderer.SetPosition(1, lineEnd);
        }
        else
        {
            _lineRenderer.SetPosition(1, new Vector3(0, 0, 100));
        }

	}
}

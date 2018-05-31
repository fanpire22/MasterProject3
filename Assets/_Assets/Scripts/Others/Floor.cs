using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Floor : MonoBehaviour, IPointerClickHandler, IDragHandler {

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 clickedPoint = eventData.pointerCurrentRaycast.worldPosition;
        GameManager.instance.player.GoToDestination(clickedPoint);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 clickedPoint = eventData.pointerCurrentRaycast.worldPosition;
        GameManager.instance.player.GoToDestination(clickedPoint);
    }

}

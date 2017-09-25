using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnClickEvent : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Image img = GameObject.Find("Selected Button").GetComponent<Image>();
        img.overrideSprite = eventData.pointerPress.GetComponent<Image>().sprite;
    }
}

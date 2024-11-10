using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.Extras;

public class SteamVR_LaserHandler : MonoBehaviour
{
    private SteamVR_LaserPointer laserPointer;

    [Header("启用LaserPointer"), SerializeField]
    private bool enablePointer = true;

    private void Awake()
    {
        laserPointer = GetComponent<SteamVR_LaserPointer>();
    }

    private void Start()
    {
        if (!enablePointer)
        {
            laserPointer.enabled = false;
        }

        laserPointer.PointerIn += LaserPointer_PointerIn;
        laserPointer.PointerClick += LaserPointer_PointerClick;
        laserPointer.PointerOut += LaserPointer_PointerOut;
    }

    private void LaserPointer_PointerIn(object sender, PointerEventArgs e)
    {
        IPointerEnterHandler pointerEnterHandler = e.target.GetComponent<IPointerEnterHandler>();
        if (pointerEnterHandler != null)
        {
            pointerEnterHandler.OnPointerEnter(new PointerEventData(EventSystem.current));
        }
    }

    private void LaserPointer_PointerClick(object sender, PointerEventArgs e)
    {
        IPointerClickHandler pointerClickHandler = e.target.GetComponent<IPointerClickHandler>();
        if (pointerClickHandler != null)
        {
            pointerClickHandler.OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }

    private void LaserPointer_PointerOut(object sender, PointerEventArgs e)
    {
        IPointerExitHandler pointerExitHandler = e.target.GetComponent<IPointerExitHandler>();
        if (pointerExitHandler != null)
        {
            pointerExitHandler.OnPointerExit(new PointerEventData(EventSystem.current));
        }
    }
}
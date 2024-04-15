using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragImage : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float pressScale = 1.2f;
    [SerializeField] private Toggle onButton;

    private float canvasScaling = 1;
    //private Vector3 defaultPosition;

    private float scale = 1;
    private RectTransform rectTrns;
    private Vector2 origSize;

    internal bool hasInteracted = false;
    internal bool isInteracting = false;
    internal bool isWelderOn = false;

    void Start()
    {
        //defaultPosition = this.transform.position;
        rectTrns = GetComponent<RectTransform>();
        origSize = rectTrns.sizeDelta;
    }

    void LateUpdate()
    {
        if (pressScale != 1)
        {
            rectTrns.sizeDelta = Vector2.Lerp(rectTrns.sizeDelta, origSize * scale, Time.deltaTime * 8);
        }


        if (onButton)
            isWelderOn = onButton.isOn;
    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        //Show on top
        transform.SetAsLastSibling();

    }

    public void OnDrag(PointerEventData eventData)
    {

        this.transform.position += (Vector3)eventData.delta * canvasScaling;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        scale = 1.1f;
        hasInteracted = true;
        isInteracting = true;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        scale = 1f;
        isInteracting = false;

        //Check if off Screen
        Vector3 screenOffset = GetGUIElemenOffset(rectTrns);

        rectTrns.position += screenOffset;
    }

    private Vector3 GetGUIElemenOffset(RectTransform rect)
    {
        Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);
        Vector3[] objectCorners = new Vector3[4];
        rect.GetWorldCorners(objectCorners);

        var xnew = 0f;
        var ynew = 0f;
        var znew = 0f;

        for (int i = 0; i < objectCorners.Length; i++)
        {
            if (objectCorners[i].x < screenBounds.xMin)
            {
                xnew = screenBounds.xMin - objectCorners[i].x;
            }
            if (objectCorners[i].x > screenBounds.xMax)
            {
                xnew = screenBounds.xMax - objectCorners[i].x;
            }
            if (objectCorners[i].y < screenBounds.yMin)
            {
                ynew = screenBounds.yMin - objectCorners[i].y;
            }
            if (objectCorners[i].y > screenBounds.yMax)
            {
                ynew = screenBounds.yMax - objectCorners[i].y;
            }
        }

        return new Vector3(xnew, ynew, znew);

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHandcontrol : MonoBehaviour
{
    private DragImage dragImage;
    internal WeldingHandle weldingHandle;

    public bool HasInteracted()
    {
        return dragImage.hasInteracted;
    }
    public bool IsInteracting()
    {
        return dragImage.isInteracting;
    }

    public bool IsOn()
    {
        return dragImage.isWelderOn;
    }

    private void Awake()
    {
        dragImage = GetComponent<DragImage>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (dragImage.isInteracting && dragImage.isWelderOn)
        {
            weldingHandle.StartWelding();
        }
        else
        {
            weldingHandle.StopWelding();
        }

    }
}

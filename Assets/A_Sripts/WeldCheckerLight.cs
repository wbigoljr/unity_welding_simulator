using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldCheckerLight : MonoBehaviour
{
    public MeshRenderer capsuleRend;
    public Light weldLight;

    public Color goodWeldColor;
    public Color badWeldColor;

    internal void ShowColor(bool isGoodWeld)
    {
        
        if (isGoodWeld)
        {
            if (weldLight.color != goodWeldColor)
            {
                capsuleRend.material.SetColor("_BaseColor", goodWeldColor);
                weldLight.color = goodWeldColor;
            }

        }
        else
        {
            if (weldLight.color != badWeldColor)
            {
                capsuleRend.material.SetColor("_BaseColor", badWeldColor);
                weldLight.color = badWeldColor;
            }
        }
            

    }
}

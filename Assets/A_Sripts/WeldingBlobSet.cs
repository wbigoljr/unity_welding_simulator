using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldingBlobSet : MonoBehaviour
{
    public Mesh flatBlobMesh;
    public Material blobCooledMaterial, blobHotMaterial;

    [SerializeField] private float coolingDelay = 1.5f;
    [SerializeField] private float coolingFade = 1.5f;

    internal bool tiltForward = false;

    Color fadeToCool = new Color(0.5f, 0.5f, 0.3f, 1f);

    // Start is called before the first frame update
    private void Start()
    {
        LeanTween.value(gameObject, 0, 1, coolingDelay).setOnComplete(() =>
        {

            Material newMaterial = Instantiate(blobHotMaterial);
            GetComponent<MeshRenderer>().material = newMaterial;

            LeanTween.value(gameObject, 0, 1, coolingFade).setOnUpdate((float val) => {

                newMaterial.color = Color.Lerp(newMaterial.color, fadeToCool, val);
                newMaterial.SetColor("_EmissionColor", new Color(1 - val, 1 - val, 0, 1 - val));

            }).setOnComplete(() =>
            {
                //if (tiltForward)
                //{
                //    //Add slight tilt forward after cooling
                //    Quaternion tiltRot = transform.rotation * Quaternion.Euler(8, 0, 0);
                //    LeanTween.rotateLocal(gameObject, tiltRot.eulerAngles, 0.31f);
                //}

                GetComponent<MeshRenderer>().material = blobCooledMaterial;
                GetComponent<MeshFilter>().mesh = flatBlobMesh;

                Destroy(newMaterial);
            });

        });

    }


    internal void ShowGlow()
    {
        GetComponent<MeshRenderer>().material = blobHotMaterial;
        tiltForward = false;
        Start();

    }

}

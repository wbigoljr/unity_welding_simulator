using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldingHandle : MonoBehaviour
{
    public Transform weldBlobSet, weldHoleMask, weldingTip;
    public MeshRenderer tipRenderer;

    public GameObject weldingPP, weldingLight, envLights, glowEffect;

    private AudioSource audioSource;

    private Material tipOriginalMat;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        tipOriginalMat = tipRenderer.material;
    }

    private RaycastHit weldHit;
    bool isWeldingLayer = false;
    private float weldTimer;
    private float travelTimer;
    public void StartWelding()
    {
        if (holdOn)
            return;

        weldTimer += Time.deltaTime;

        //Delay start
        if (weldTimer >= 1f)
        {
            if (isWeldingLayer)
            {
                ShowEffects(true);
                ShowBlob();
                travelTimer += Time.deltaTime;
            }
            else
            {
                ShowEffects(false);
                travelTimer = 0;
            }
        }
    }

    public void StopWelding(bool resetTimers = true)
    {
        if (resetTimers)
        {
            holdOn = false;
            weldTimer = 0;
        }

        ShowEffects(false);
        ResetBlobSettings(true);
    }

    private bool hasBlob = false;
    private Transform currentBlob;
    private float blobSizeTimer;
    private Transform currentPanel;
    private void ShowBlob()
    {

        float blobInitSize = 0.2f;

        if (!hasBlob)
        {
            if (weldHit.transform.gameObject.layer == 7) //Hit is panel
            {
                currentPanel = weldHit.transform;

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, weldHit.normal);
                currentBlob = Instantiate(weldBlobSet, weldHit.point, rotation);
                currentBlob.localScale = Vector3.one * blobInitSize;
                BlobThickness(currentBlob);

                SetBlobTravelTime(weldHit.transform.parent.GetComponent<WeldingPanel>());
            }
            else if (weldHit.transform.gameObject.layer == 6) //Hit is Blob
            {
                currentBlob = weldHit.transform;
                currentBlob.parent = null; //Remove parent

                blobInitSize = currentBlob.localScale.x;
                currentBlob.GetComponent<WeldingBlobSet>().ShowGlow();
            }

            hasBlob = true;
            blobSizeTimer = 0;

        }

        if (hasBlob)
        {
            //float blobDistance = Vector3.Distance(currentBlob.position, weldHit.point);
            //Debug.Log("blobDistance " + blobDistance + "blobSizeTimer " + blobSizeTimer);
            
            blobSizeTimer += Time.deltaTime * 0.2f; //With filler use 0.5f

            if(weldHit.transform == currentBlob)
            {
                if (currentBlob.localScale.magnitude < 0.7f) //Size limit
                {
                    currentBlob.localScale = Vector3.one * (blobInitSize + blobSizeTimer);
                    BlobThickness(currentBlob);

                }
                else //Overheat, replace blob with a Hole Mask
                {
                    InstansiateHoleMask(currentBlob);

                    //Stop welding without reseting timer
                    StopWelding(false);

                    StartCoroutine(HoldWeldingRoutine(0.5f));
                }
                    
            }
            else
            {
                ResetBlobSettings();
            }
        }
    }

    private void BlobThickness(Transform blob)
    {
        if (isCornerWeld)
        {
            //Make thicker
            blob.localScale = new Vector3(blob.localScale.x, blob.localScale.y * 1.3f, blob.localScale.z * 1.3f);
        }
        else
        {
            //Make thinner
            blob.localScale = new Vector3(blob.localScale.x, blob.localScale.y / 3, blob.localScale.z);
        }

    }

    Transform previousBlob;
    private void ResetBlobSettings(bool weldStop = false)
    {
        
        if (currentBlob)
        {
            currentBlob.parent = currentPanel;

            if (previousBlob)
            {
                //Look and Tilt Blob towards next blob
                previousBlob.LookAt(currentBlob, previousBlob.up);
                previousBlob.GetComponent<WeldingBlobSet>().tiltForward = true;

            }

            previousBlob = currentBlob;
        }

        if (weldStop)
        {
            if (previousBlob)
                previousBlob.GetComponent<WeldingBlobSet>().tiltForward = false;

            currentPanel = null;
            currentBlob = null;
            previousBlob = null;
        }


        hasBlob = false;
        blobSizeTimer = 0;
    }

    bool holdOn = false;
    private IEnumerator HoldWeldingRoutine(float duration)
    {
        holdOn = true;
        yield return new WaitForSeconds(duration);
        holdOn = false;
    }


    private void InstansiateHoleMask(Transform blob)
    {
        Transform holeMask = Instantiate(weldHoleMask, blob.position, blob.rotation);
        holeMask.localScale = Vector3.one * 0.2f;

        float width = holeMask.localScale.x + 0.4f;
        Vector3 finalScale = new Vector3(width, holeMask.localScale.y + 0.7f, width);

        LeanTween.scale(holeMask.gameObject, finalScale, 0.2f); //Animate

        Destroy(blob.gameObject);
    }

    private void SetBlobTravelTime(WeldingPanel panel)
    {

        if (panel && travelTimer > 0)
        {
            //Debug.Log("Weld Travel: " + travelTimer);
            panel.AddWeldTravel(travelTimer);
            travelTimer = 0;
        }

    }

    private void ShowEffects(bool show)
    {
        envLights.SetActive(!show);
        weldingLight.SetActive(show);
        weldingPP.SetActive(show);

        glowEffect.SetActive(show);

        if (show && !audioSource.isPlaying)
        {
            tipRenderer.material = weldBlobSet.GetComponent<WeldingBlobSet>().blobHotMaterial;
            audioSource.Play();
        }
        else if (!show && audioSource.isPlaying)
        {
            tipRenderer.material = tipOriginalMat;
            audioSource.Stop();
        }
    }

    public bool isCornerWeld = false;

    public Vector3 GetWeldPoint()
    {
        Vector3 weldPoint = weldingTip.position;

        if (Physics.Raycast(weldingTip.position, weldingTip.forward, out RaycastHit hit))
        {

            if (hit.transform.gameObject.layer == 6 || hit.transform.gameObject.layer == 7)
            {
                weldHit = hit;
                isWeldingLayer = true;
            }
            else
            {
                weldHit = new RaycastHit();
                isWeldingLayer = false;
            }

            weldPoint = hit.point;

            Debug.DrawLine(weldingTip.position, hit.point, Color.red);

        }

        bool hasFrontPanel = false;

        Vector3 weldHitPosition = weldHit.point - weldingTip.forward * 0.013f;

        //Raycast Front and check for corner weld
        if (Physics.Raycast(weldHitPosition, -Vector3.forward, out RaycastHit hit2))
        {
            if ((hit2.transform.gameObject.layer == 6 || hit2.transform.gameObject.layer == 7) && hit2.distance < 0.02f)
            {
                hasFrontPanel = true;
                Debug.DrawLine(weldHitPosition, hit2.point, Color.green);
            }
            else
            {
                hasFrontPanel = false;
                Debug.DrawLine(weldHitPosition, hit2.point, Color.yellow);
            }

        }

        bool hasBottomPanel = false;
        //Raycast Down and check for corner weld
        if (Physics.Raycast(weldHitPosition, Vector3.down, out hit2))
        {
            if ((hit2.transform.gameObject.layer == 6 || hit2.transform.gameObject.layer == 7) && hit2.distance < 0.02f)
            {
                hasBottomPanel = true;
                Debug.DrawLine(weldHitPosition, hit2.point, Color.green);
            }
            else
            {
                hasBottomPanel = false;
                Debug.DrawLine(weldHitPosition, hit2.point, Color.yellow);
            }

        }

        if (hasFrontPanel && hasBottomPanel)
            isCornerWeld = true;
        else
            isCornerWeld = false;


        return weldPoint;

    }

    public void SetTipRotation(Quaternion rotation)
    {
        weldingTip.rotation = rotation;
    }

    public void MoveHandle(Vector3 movePos)
    {
        transform.position = movePos;

        ////Raycast Front and check for corner weld
        //if (Physics.Raycast(weldingTip.position, Vector3.forward, out RaycastHit hit))
        //{
        //    transform.position = new Vector3(movePos.x, movePos.y, movePos.z + hit.distance);
        //}
        //else
        //{
        //    transform.position = movePos;
        //}

    }


}

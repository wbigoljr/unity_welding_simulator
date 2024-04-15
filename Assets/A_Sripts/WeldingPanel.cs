using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static WeldingPanel;

public class WeldingPanel : MonoBehaviour
{
    [SerializeField] private Collider weldingCollider;

    [SerializeField] private Transform[] panels;

    [SerializeField] private Material blobErrorMat, blobGoodMat;

    [SerializeField] private GameObject weldScanner;
    [SerializeField] private int checkTimeSec = 2;
    [SerializeField] private Transform[] checkingTransforms;

    private Transform checkerCapsule;
    private WeldCheckerLight checkerLight;
    private Vector3[] checkingPoints;

    public struct WeldingStats
    {
        public float uniformity;
        public float coveragePercent;
        public float travel;

        public int badweldCount;
        public int holesCount;

    }

    void Awake()
    {
        checkingPoints = new Vector3[checkingTransforms.Length];

        int i = 0;
        foreach (Transform t in checkingTransforms)
        {
            checkingPoints[i] = t.position;
            i++;
        }

    }

    bool isWeldingStatsDone = false;
    WeldingStats weldingStats;
    internal void PopulateWeldingStats(out int delayTimeSec)
    {
        delayTimeSec = checkTimeSec;

        isWeldingStatsDone = false;

        weldingStats = new WeldingStats();

        if (checkerCapsule == null)
            checkerCapsule = Instantiate(weldScanner, checkingPoints[0], Quaternion.identity).transform;

        if (checkerLight == null)
            checkerLight = checkerCapsule.GetComponent<WeldCheckerLight>();

        checkerCapsule.rotation = checkingTransforms[0].rotation; //Match rotation in case of corner welds needs a bit of tilt.

        weldingStats.uniformity = GetUniformity();
        weldingStats.travel = GetWeldTravelUniformity();

        weldingStats.badweldCount = GetBadWelds();
        weldingStats.holesCount = GetWeldHoles();

        int totalCount = 0;
        int blobCount = 0;

        LeanTween.move(checkerCapsule.gameObject, checkingPoints, checkTimeSec).setOnUpdate((Vector3 positionValue) =>
        {

            bool hasBlob = RaycastCheckWeld(checkerCapsule);
            totalCount++;

            if (hasBlob)
            {
                blobCount++;
                checkerLight.ShowColor(true);
                checkerCapsule.GetComponent<AudioSource>().pitch = 1f;
            }
            else
            {
                checkerCapsule.GetComponent<AudioSource>().pitch = 1.3f;
                checkerLight.ShowColor(false);
            }


        }).setOnComplete(() =>
        {

            if (checkerCapsule)
                Destroy(checkerCapsule.gameObject);

            weldingStats.coveragePercent = (float)blobCount / (float)totalCount;

            isWeldingStatsDone = true;
        });

    }

    internal bool GetWeldResults(out WeldingStats stats)
    {
        stats = weldingStats;
        return isWeldingStatsDone;
    }

    private bool RaycastCheckWeld(Transform checkPos)
    {
        bool hasBlob = false;

        Vector3 checkPosWithGap = checkPos.position + Vector3.up * 0.1f;

        if (Physics.Raycast(checkPosWithGap, Vector3.down, out RaycastHit hit))
        {
            if (hit.transform.gameObject.layer == 6) //Hits welding blob.
            {
                hasBlob = true;
                //Debug.DrawRay(checkPosWithGap, Vector3.down, Color.green, 100);
            }
            else
            {
                hasBlob = false;
                //Debug.DrawRay(checkPosWithGap, Vector3.down, Color.red, 100);
            }

        }

        return hasBlob;
    }

    //Blobs not in contact with welding line.
    private int GetBadWelds()
    {
        int badWeldsCount = 0;

        foreach (Transform panel in panels)
        {
            WeldingBlobSet[] blobs = panel.GetComponentsInChildren<WeldingBlobSet>();

            foreach (WeldingBlobSet blob in blobs)
            {
                //Change to Weld Panel Layer, to not get counted by coverage detection.
                blob.gameObject.layer = 7; 

                //Delay change color for effect
                LeanTween.value(0, 1, checkTimeSec).setOnComplete(() =>
                {
                    blob.GetComponent<Renderer>().material = blobErrorMat;
                });
            }


            badWeldsCount += blobs.Length;
        }

        //Good welds
        WeldingBlobSet[] goodBlobs = weldingCollider.transform.GetComponentsInChildren<WeldingBlobSet>();

        foreach (WeldingBlobSet blob in goodBlobs)
        {
            //Delay change color for effect
            LeanTween.value(0, 1, checkTimeSec).setOnComplete(() =>
            {
                blob.GetComponent<Renderer>().material = blobGoodMat;
            });
        }

        return badWeldsCount;
    }

    private int GetWeldHoles()
    {
       
        GameObject[] holeObjects = GameObject.FindGameObjectsWithTag("WeldHole");
        int holesCount = holeObjects.Length;

        return holesCount;

    }

    private float GetUniformity()
    {
        float uniformity = 0.0f;

        float smallestScale = Mathf.Infinity;
        float largestScale = 0;

        GameObject[] weldObjects = GameObject.FindGameObjectsWithTag("WeldObject");
        foreach (GameObject obj in weldObjects)
        {
            if(obj.transform.localScale.x < smallestScale)
                smallestScale = obj.transform.localScale.x;

            if(obj.transform.localScale.x > largestScale)
                largestScale = obj.transform.localScale.x;


        }

        uniformity = ((smallestScale + largestScale) / 2)/largestScale;

        return uniformity;
    }

    //Weld Travel
    List<float> weldTravels = new List<float>();
    internal void AddWeldTravel(float weldTravel)
    {
        weldTravels.Add(weldTravel);
    }
    internal void ResetWeldTravel()
    {
        weldTravels.Clear();
    }

    private float GetWeldTravelUniformity()
    {
        if (weldTravels.Count <= 10)
            return 0;


        float idealTime = 0.419f; //Ideal time for each blob to form before making another.

        float averageTime = weldTravels.Average();

        float travelPerf = 1 - (Mathf.Abs(idealTime - averageTime) / idealTime);


        //Debug.Log("GetWeldTravelPerformance: averageTime = " + averageTime);

        return travelPerf;
    }
}

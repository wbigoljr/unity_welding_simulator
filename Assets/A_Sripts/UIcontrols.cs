using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using static ScoreSystem;
using static UnityEngine.GraphicsBuffer;

public class UIcontrols : MonoBehaviour
{
    [SerializeField] private Canvas mainCanvas;

    [SerializeField] private GameObject playbutton, backButton, resetButton, doneButton, scorePanel, checkingText;

    [SerializeField] private WeldingHandle welderHandle;
    [SerializeField] private RightHandcontrol rightHandctrl;
    [SerializeField] private LeftHandcontrol leftHandctrl;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera playCamera;

    [SerializeField] private WeldingType weldingType;

    [SerializeField] private ScoreSystem scoreSys;

    private bool playCameraAnimation = false;
    private bool startGame = false;

    private CameraPositions titleCamPos, weldingCamPos, lerpCamValues;

    public enum WeldingType {Mig, Tig};

    enum CameraAnimation {ToTitle, ToWelding};

    private Vector3 rightCntrlOrigPos, lefthandCntrlOrigPos;

    public class CameraPositions
    {
        public CameraPositions()
        {
        }

        public CameraPositions(Vector3 pos, Quaternion rot, float fview)
        {
            position = pos;
            rotation = rot;
            fieldOfView = fview;
        }

        public Vector3 position;
        public Quaternion rotation;
        public float fieldOfView;
    }

    //internal ScorePanel GetScorePanel()
    //{
    //    return scorePanel.GetComponent<ScorePanel>();
    //}

    private void Awake()
    {
        rightHandctrl.gameObject.SetActive(false);
        rightHandctrl.weldingHandle = welderHandle;

        leftHandctrl.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        doneButton.gameObject.SetActive(false);
        scorePanel.gameObject.SetActive(false);

        Application.targetFrameRate = 60;
        //QualitySettings.vSyncCount = 1;

        titleCamPos = new CameraPositions(mainCamera.transform.position, mainCamera.transform.rotation, mainCamera.fieldOfView);
        weldingCamPos = new CameraPositions(playCamera.transform.position, playCamera.transform.rotation, playCamera.fieldOfView);
        lerpCamValues = new CameraPositions();


        rightCntrlOrigPos = rightHandctrl.transform.position;
        lefthandCntrlOrigPos = leftHandctrl.transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        //welderHandle.SetTipRotation(weldingCamPos.rotation);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (playCameraAnimation)
        {
            float animTime = 2;

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, lerpCamValues.position, Time.deltaTime * animTime);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, lerpCamValues.rotation, Time.deltaTime * animTime);

            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, lerpCamValues.fieldOfView, Time.deltaTime * animTime);

            if (mainCamera.transform.position.magnitude == lerpCamValues.position.magnitude * 0.1f)
                playCameraAnimation = false;
        }

    }

    private void PlayCameraAnimation(CameraAnimation cameraAnim)
    {
        playCameraAnimation = true;

        if (cameraAnim == CameraAnimation.ToWelding)
        {
            lerpCamValues.position = weldingCamPos.position;
            lerpCamValues.rotation = weldingCamPos.rotation;
            lerpCamValues.fieldOfView = weldingCamPos.fieldOfView;
        }
        else if (cameraAnim == CameraAnimation.ToTitle)
        {
            lerpCamValues.position = titleCamPos.position;
            lerpCamValues.rotation = titleCamPos.rotation;
            lerpCamValues.fieldOfView = titleCamPos.fieldOfView;
        }

    }

    private void Update()
    {
        if (startGame)
        {

            //Move handle to ui control
            welderHandle.MoveHandle(UISpaceToWorld(rightHandctrl.transform.position, 0.33f));

            //// Tip rotation parallel to screen
            Vector3 direction = mainCamera.transform.position - welderHandle.weldingTip.position;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
            welderHandle.SetTipRotation(rotation * Quaternion.Euler(90,0,0));

            Vector3 weldPoint = welderHandle.GetWeldPoint();

            Vector3 followCamPosition = new Vector3(welderHandle.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);

            //if (rightHandctrl.IsInteracting() && rightHandctrl.IsOn())
            if (rightHandctrl.IsOn())
            {
                float tipToWeldPointDist = Vector3.Distance(weldPoint, welderHandle.transform.position);
                followCamPosition = followCamPosition + (mainCamera.transform.forward * tipToWeldPointDist);
            }

            //Camera follow Handle
            if (rightHandctrl.HasInteracted())
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, followCamPosition, Time.deltaTime * 4);

            }
        }
    }


    private void StartGame(float showUIDeley = 2)
    {
        PlayCameraAnimation(CameraAnimation.ToWelding);

        StartCoroutine(EnableControlsRoutine(showUIDeley));
    }
    private void ResultGameUI()
    {
        startGame = false;
        ShowWeldingControls(false);
        
        backButton.SetActive(false);

        //PlayCameraAnimation(CameraAnimation.ToWelding);
    }

    IEnumerator EnableControlsRoutine(float delay)
    {
        playbutton.SetActive(false);

        yield return new WaitForSeconds(delay);

        rightHandctrl.gameObject.SetActive(true);
        
        if(weldingType == WeldingType.Tig)
            leftHandctrl.gameObject.SetActive(true);

        welderHandle.gameObject.SetActive(true);

        backButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        doneButton.gameObject.SetActive(true);


        rightHandctrl.transform.position = rightCntrlOrigPos;
        leftHandctrl.transform.position = lefthandCntrlOrigPos;

        scoreSys.ShowPanel(true);

        startGame = true;
    }

    public void BackButton()
    {
        PlayCameraAnimation(CameraAnimation.ToTitle);
        
        RemoveAllWeldBlobs();

        scoreSys.ShowPanel(false);

        StartCoroutine(ShowTitleSceneRoutine(2));
    }

    IEnumerator ShowTitleSceneRoutine(float delay)
    {
        rightHandctrl.gameObject.SetActive(false);
        leftHandctrl.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);

        doneButton.gameObject.SetActive(false);
        scorePanel.gameObject.SetActive(false);

        startGame = false;

        yield return new WaitForSeconds(delay);
        playbutton.gameObject.SetActive(true);
    }

    private void ShowWeldingControls(bool show)
    {
        rightHandctrl.gameObject.SetActive(show);
        welderHandle.gameObject.SetActive(show);
        
        resetButton.SetActive(show);
        doneButton.SetActive(show);

        if (show)
        {
            startGame = true;
            rightHandctrl.transform.position = rightCntrlOrigPos;
            leftHandctrl.transform.position = lefthandCntrlOrigPos;


        }
    }

    public void MainPlayButton()
    {
        StartGame(2);
    }

    public void ResetButton()
    {
        RemoveAllWeldBlobs();

        scorePanel.SetActive(false);

        ShowWeldingControls(true);

        scoreSys.ResetPanel();
    }

    public void DoneButton()
    {
        ResultGameUI();
        checkingText.SetActive(true);

        float delay = (float)scoreSys.PopulateScores() + 0.2f;

        LeanTween.value(gameObject, 0, 1, delay).setOnUpdate((float val) => {


        }).setOnComplete(() =>
        {
            checkingText.SetActive(false);
            WeldingScore scoreResult = scoreSys.GetResults();
            ShowResultPanel(scoreResult.uniformity, scoreResult.coverage, scoreResult.travel);

        });

    }


    public void NextButton()
    {
        //Get Next Panel

        ResetButton();
    }

    public void RetryButton()
    {
        backButton.SetActive(true);
        ResetButton();

    }

    private void ShowResultPanel(int uni, int cov, int spd)
    {
        //scorePanel.SetActive(true);
        scorePanel.GetComponent<ScorePanel>().ShowPanel(uni, cov, spd);

    }

    private void RemoveAllWeldBlobs()
    {
        GameObject[] weldObjects = GameObject.FindGameObjectsWithTag("WeldObject");
        foreach (GameObject obj in weldObjects)
            Destroy(obj);

        GameObject[] holeObjects = GameObject.FindGameObjectsWithTag("WeldHole");
        foreach (GameObject obj in holeObjects)
            Destroy(obj);
    }

    private Vector3 WorldToUISpace(Vector3 worldPos)
    {
        //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
        if (!mainCamera)
            return Vector3.zero;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        Vector2 movePos;

        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvas.transform as RectTransform, screenPos, mainCanvas.worldCamera, out movePos);
        //Convert the local point to world point
        return mainCanvas.transform.TransformPoint(movePos);

    }

    private Vector3 UISpaceToWorld(Vector3 screenPosition, float zPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, zPos));
        return worldPos;
    }

    private Vector3 UISpaceToWorldRaycast(Vector3 screenPosition)
    {
        Vector3 worldPos = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            worldPos = hit.point;
        }

        return worldPos;
    }



}

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static WeldingPanel;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] private WeldingPanel currentPanel;
    [SerializeField] private GameObject[] panelPrefabs;
    [SerializeField] private AudioClip panelDropSound;

    private int currentIndex;
    private AudioSource audioSource;

    public struct WeldingScore
    {
        public int uniformity;
        public int coverage;
        public int travel;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        currentPanel.gameObject.SetActive(false);
    }

    internal int PopulateScores()
    {
        currentPanel.PopulateWeldingStats(out int delay);
        return delay;
    }

    internal WeldingScore GetResults()
    {
        WeldingScore score = new WeldingScore();

        if (currentPanel.GetWeldResults(out WeldingStats weldingResults))
        {
            //Uniformity
            if (weldingResults.holesCount > 0)
                score.uniformity = 0;
            else
                score.uniformity = Mathf.Clamp((int)Mathf.Round(weldingResults.uniformity * 100) - weldingResults.badweldCount, 0, 100);

            //Coverage
            score.coverage = (int)Mathf.Round((weldingResults.coveragePercent * 100));

            //Travel
            score.travel = (int)Mathf.Round((weldingResults.travel * 100));

        }

        return score;

    }


    internal void NextPanel()
    {

    }

    internal void PrevPanel()
    {

    }

    internal void ShowPanel(bool show)
    {
        currentPanel.gameObject.SetActive(show);
        if (show)
        {
            AnimatePanel();
        }
    }

    internal void ResetPanel()
    {
        currentPanel.ResetWeldTravel();
        AnimatePanel();
    }

    internal void AnimatePanel()
    {
        currentPanel.transform.position = transform.position + Vector3.up * 0.06f;

        LeanTween.move(currentPanel.gameObject, transform.position, 0.2f).setEase(LeanTweenType.easeInSine).setOnComplete(() =>
        {
            audioSource.PlayOneShot(panelDropSound);
        });

    }
}

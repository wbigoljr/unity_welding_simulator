using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ScorePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        AnimationSetup();
    }

    internal void ShowPanel(int uni, int cov, int time)
    {

        string uniformityStr = uni.ToString();
        string coverageStr = "\n" + cov.ToString();
        string speedStr = "\n" + time.ToString();


        float totalValue = (uni + cov + time)/3;
        string spacerStr = "\n";

        string totalStr = "\n" + totalValue.ToString("00.0");

        this.gameObject.SetActive(true);

        LeanTween.scale(gameObject, Vector3.one, 0.3f);
        LeanTween.alpha(gameObject.GetComponent<RectTransform>(), 1, 0.3f);

        scoreText.text = uniformityStr + coverageStr + speedStr + spacerStr + totalStr;

    }

    internal void AnimationSetup()
    {
        gameObject.transform.localScale = Vector3.one * 0.8f;
        LeanTween.alpha(gameObject.GetComponent<RectTransform>(), 0, 0.01f);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class PanelHandler : MonoBehaviour
{

    [SerializeField] GameObject sp;
    static GameObject SelectPanel;
    [SerializeField] GameObject ap;
    static GameObject AddPanel;
    [SerializeField] GameObject ft;
    static GameObject Footer;


    [SerializeField]
    private GameObject lp;
    static GameObject LoadingPanel;
    [SerializeField]
    private GameObject ls;
    static GameObject LoadingSlider;

    private void Awake()
    {
        LoadingPanel = lp;
        LoadingSlider = ls;

        SelectPanel = sp;
        AddPanel = ap;
        Footer = ft;

    }

    // Start is called before the first frame update
    void Start()
    {
        ToSelectPanel();
        LoadingPanel.SetActive(false);
    }

    public static void ToSelectPanel()
    {
        SelectPanel.SetActive(true);
        AddPanel.SetActive(false);
        Footer.SetActive(false);
    }

    public static void ToAddPanel()
    {
        SelectPanel.SetActive(false);
        AddPanel.SetActive(true);
        Footer.SetActive(true);
    }

    public static UniTask ToLoading()
    {
        LoadingPanel.SetActive(true);
        LoadingSlider.GetComponentInChildren<Slider>().value = 0;

        return new UniTask();
    }

    public static void Loading(float rate)
    {
        LoadingSlider.GetComponentInChildren<Slider>().value = rate;
        if (rate == 1)
        {
            LoadingPanel.SetActive(false);
        }
    }
}

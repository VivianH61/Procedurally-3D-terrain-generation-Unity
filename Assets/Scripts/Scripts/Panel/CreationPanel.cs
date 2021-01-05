using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreationPanel : BasePanel
{
    private CanvasGroup canvasGroup;
    public Slider vegetationSlider, averageHeightSlider, roughnessSlider;
    public Text vegetationText, averageHeightText, roughnessText;
    public static float vegetationValue, averageHeightValue, roughnessValue;
    

    void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        

    }
    
    // Enter the Creation Panel
    public override void OnEnter()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;

        vegetationSlider.value = 0;
        averageHeightSlider.value = 80;
        roughnessSlider.value = 1;
    }

    // Exit the Creation Panel, make it disappear
    public override void OnExit()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnClosePanel()
    {
        UIManager.Instance.PopPanel();
        UIManager.Instance.PushPanel(UIPanelType.Menu);
    }

    public void OnOKButtonClick()
    {
        UIManager.Instance.PopPanel();
        UIManager.Instance.PushPanel(UIPanelType.Main);
    }

    public void VegetationSlider()
    {
        string sliderMessage = "" + vegetationSlider.value;
        vegetationText.text = sliderMessage;

        vegetationValue = vegetationSlider.value;
    }

    public void AverageHeightSlider()
    {
        string sliderMessage = "" + averageHeightSlider.value;
        averageHeightText.text = sliderMessage;

        averageHeightValue = averageHeightSlider.value;
    }

    public void RoughnessSlider()
    {
        string sliderMessage = "" + roughnessSlider.value;
        roughnessText.text = sliderMessage;

        roughnessValue = roughnessSlider.value;
    }

    public void OnCloseGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }





}

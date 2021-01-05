using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : BasePanel
{
    private CanvasGroup canvasGroup;
    private CreationPanel creationPanel;

    public Slider averageHeightSlider, roughnessSlider;
    public Slider mountainTopSlider, hillsideSlider, mountainFootSlider;
    public Slider grassDensitySlider, lightSlider;

    public Text averageHeightText, roughnessText;
    public Text mountainTopText, hillsideText, mountainFootText;
    public Text grassDensityText, lightText;

    void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
    }

    public override void OnEnter()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;

        // if creation panel has been initialized
        if (UIManager.creation == 1)
        {
            averageHeightSlider.value = CreationPanel.averageHeightValue;
            roughnessSlider.value = CreationPanel.roughnessValue;
            mountainTopSlider.value = CreationPanel.vegetationValue;
            hillsideSlider.value = CreationPanel.vegetationValue;
            mountainFootSlider.value = CreationPanel.vegetationValue;
            grassDensitySlider.value = 0;
            lightSlider.value = 0;
        }
    }

    public override void OnExit()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnMenuPanel()
    {
        UIManager.Instance.PopPanel();
        UIManager.Instance.PushPanel(UIPanelType.Menu);
    }

    public void OnCloseGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
           Application.Quit();
        #endif
    }

    public void AverageHeightSlider()
    {
        
        string sliderMessage = "" + averageHeightSlider.value;
        averageHeightText.text = sliderMessage;
    }

    public void RoughnessSlider()
    {
        string sliderMessage = "" + roughnessSlider.value;
        roughnessText.text = sliderMessage;
    }

    public void MountainTopSlider()
    {
        string sliderMessage = "" + mountainTopSlider.value;
        mountainTopText.text = sliderMessage;
    }

    public void HillsideSlider()
    {
        string sliderMessage = "" + hillsideSlider.value;
        hillsideText.text = sliderMessage;
    }

    public void MountainFootSlider()
    {
        string sliderMessage = "" + mountainFootSlider.value;
        mountainFootText.text = sliderMessage;
    }

    public void GrassDensitySlider()
    {
        string sliderMessage = "" + grassDensitySlider.value;
        grassDensityText.text = sliderMessage;
    }

    public void LightSlider()
    {
        string sliderMessage = "" + lightSlider.value;
        lightText.text = sliderMessage;
    }


}

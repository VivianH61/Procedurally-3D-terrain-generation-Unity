using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanel : BasePanel
{
    private CanvasGroup canvasGroup;

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
    }

    public override void OnExit()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnPushPanel(string panelTypeString)
    {
        //Close the Menu Panel unless it's InfoPanel
        if(panelTypeString != "Info")
            UIManager.Instance.PopPanel();
        else
            canvasGroup.blocksRaycasts = false;
        
        //Create a new Panel
        UIPanelType panelType = (UIPanelType)System.Enum.Parse(typeof(UIPanelType), panelTypeString);
        UIManager.Instance.PushPanel(panelType);
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

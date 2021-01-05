using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This is the manager of all the UI part
/// including push and pop panel
/// </summary>
public class UIManager 
{
    public static int creation = 0;
    

    // Single pattern is used to initialize the UIManager
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIManager();
            }
            return _instance;
        }
    }

    private Transform canvasTransform;
    private Transform CanvasTransform
    {
        get
        {
            if (canvasTransform == null)
            {
                canvasTransform = GameObject.Find("Canvas").transform;
            }
            return canvasTransform;
        }
    }
    
    private Dictionary<UIPanelType, string> panelPathDict;// Store all the Prefab path 
    private Dictionary<UIPanelType, BasePanel> panelDict;//Store all the components of BasePanel which is initialized
    private Stack<BasePanel> panelStack; //Stack to store the panels

    private UIManager()
    {
        ParseUIPanelTypeJson();
    }

    /// <summary>
    /// Push the panel on the scene
    /// </summary>
    /// <param name="panelType"></param>
    public void PushPanel(UIPanelType panelType)
    {
        if (panelStack == null)
            panelStack = new Stack<BasePanel>();

        if (panelType == UIPanelType.Creation)
            creation = 1;
        BasePanel panel = GetPanel(panelType);
        panel.OnEnter();
        panelStack.Push(panel);
    }

    /// <summary>
    /// Pop the panel from canvas
    /// </summary>
    public void PopPanel()
    {
        if (panelStack == null)
            panelStack = new Stack<BasePanel>(); //安全校验

        if (panelStack.Count <= 0) return;

        //pop the top panel on the stack
        BasePanel topPanel = panelStack.Pop();
        topPanel.OnExit();

    }

    /// <summary>
    /// according to the type of prefab, get the initialization of them
    /// </summary>
    /// <returns></returns>
    private BasePanel GetPanel(UIPanelType panelType)
    {
        if (panelDict == null)
        {
            panelDict = new Dictionary<UIPanelType, BasePanel>();
        }

        BasePanel panel = panelDict.TryGet(panelType);

        if (panel == null)
        {
            //If the panel cannot be found, try to find the prefab path of this panel and initialize it
            string path = panelPathDict.TryGet(panelType);
            GameObject instPanel = GameObject.Instantiate(Resources.Load(path)) as GameObject;
            instPanel.transform.SetParent(CanvasTransform, false);
            panelDict.Add(panelType, instPanel.GetComponent<BasePanel>());
            return instPanel.GetComponent<BasePanel>();
        }
        else
        {
            return panel;
        }

    }

    [Serializable]
    class UIPanelTypeJson
    {
        public List<UIPanelInfo> infoList;
    }
    private void ParseUIPanelTypeJson()
    {
        panelPathDict = new Dictionary<UIPanelType, string>();

        TextAsset ta = Resources.Load<TextAsset>("UIPanelType");

        UIPanelTypeJson jsonObject = JsonUtility.FromJson<UIPanelTypeJson>(ta.text);

        foreach (UIPanelInfo info in jsonObject.infoList)
        {
            //Debug.Log(info.panelType);
            panelPathDict.Add(info.panelType, info.path);
        }
    }

    /// <summary>
    /// just for test
    /// </summary>
    public void Test()
    {
        string path;
        panelPathDict.TryGetValue(UIPanelType.Menu, out path);
        Debug.Log(path);
    }
}

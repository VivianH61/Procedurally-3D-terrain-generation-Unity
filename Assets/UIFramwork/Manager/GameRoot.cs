using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    /// <summary>
    /// Start is called before the first frame update
    /// The starter of this system
    /// </summary>
    /// 
    void Start()
    {
        UIManager.Instance.PushPanel(UIPanelType.Terrain);
        UIManager.Instance.PushPanel(UIPanelType.Menu);
    }

    
}

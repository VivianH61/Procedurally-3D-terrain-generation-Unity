using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class UIPanelInfo : ISerializationCallbackReceiver
{
    [NonSerialized]
    public UIPanelType panelType;
    public string panelTypeString;
    
    public string path;

    // deserialization, the process from text information to objexts
    public void OnAfterDeserialize()
    {
        UIPanelType type = (UIPanelType)System.Enum.Parse(typeof(UIPanelType), panelTypeString);
        panelType = type;
    }

    public void OnBeforeSerialize()
    {

    }

}

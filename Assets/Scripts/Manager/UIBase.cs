using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public UIBase Instance { get {return this; } }

    public virtual void Init()
    {

    }
    public virtual void Show()
    { 
    
    }

    public virtual void UpdateUI()
    { 
    
    }

    public virtual void Refresh()
    { 
    
    }

    public virtual void Close()
    { 
    
    }
}

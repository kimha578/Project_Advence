using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager :MonoBehaviour
{
    const string PREFAB_PATH = "";
    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    Stack<UIBase> _uiStack = new Stack<UIBase>();
    Dictionary<string, GameObject> _UIDict = new Dictionary<string, GameObject>();

    void Awake()
    {
        _instance = this;
        
    }


    public void Show<T>() where T : UIBase, new()
    {
        GameObject go;
        var name = typeof(T).ToString();
        if (_UIDict.ContainsKey(name))
        {
            go = _UIDict[name];
        }
        else 
        {
            go = Resources.Load(PREFAB_PATH + name) as GameObject;
            _UIDict.Add(name, go);
        }

        T ui = go.GetComponent<T>();

        if (_uiStack.Count > 1)
            _uiStack.Peek().gameObject.SetActive(false);
        _uiStack.Push(ui);

        ui.Init();
        ui.Show();
    }

    public void Back()
    {
        if (_uiStack.Count > 1)
            StackPop();
        else
            Application.Quit();
    }

    public void StackPop()
    {
        if(_uiStack.Count > 1)
        _uiStack.Pop();
    }

    public void Refresh()
    {
        if (_uiStack.Count > 0)
            _uiStack.Peek().Refresh();
    }

    public UIBase GetCurrentUI()
    {
        return _uiStack.Peek();
    }
}

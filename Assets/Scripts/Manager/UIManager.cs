using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Const;

public class UIManager :MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    Stack<UIBase> _uiStack = new Stack<UIBase>();
    Dictionary<string, GameObject> _UIDict = new Dictionary<string, GameObject>();

    Stack<UIBase> _uiPopUpStack = new Stack<UIBase>();

    public Canvas _uiMainCanvas;
    public Canvas _uiPopUpCanvas;
    public Canvas _uiTopCanvas;

    void Awake()
    {
        GameObject.DontDestroyOnLoad(this);
        _instance = this;
        
    }


    public void Show<T>() where T : UIBase
    {
        GameObject go;
        var name = typeof(T).ToString();

        // 이미 불러온 UI일 경우
        if (_UIDict.ContainsKey(name))
        {
            go = _UIDict[name];
        }
        // 새로 불러올 UI일 경우
        else
        {
            go = Resources.Load(Consts.UI_PREFAB_PATH + name) as GameObject;

            switch (go.layer)
            {
                case 5:
                    go.transform.parent = _uiMainCanvas.transform;
                    break;

                case 6:
                    go.transform.parent = _uiTopCanvas.transform;
                    break;
                case 7:
                    go.transform.parent = _uiPopUpCanvas.transform;
                    break;
            }
            _UIDict.Add(name, go);
        }


        T ui = go.GetComponent<T>();

        go.SetActive(true);
        // 이전 UI 비활성화

        if (go.layer == 7)
            _uiPopUpStack.Push(ui);
        else
        {
            if (_uiStack.Count > 1)
                _uiStack.Peek().gameObject.SetActive(false);
            _uiStack.Push(ui);
        }

        ui.Init();
        ui.Show();
    }

    public void Back()
    {
        if (_uiStack.Count > 0)
            _uiPopUpStack.Pop();
        else if (_uiStack.Count > 1)
            _uiStack.Pop();
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

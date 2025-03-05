using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public GameObject debugPanelCanvas;
    public GameObject debugTextContent;
    public TextMeshProUGUI debugText;
    private float  scale_y;

    bool is_debugPanel_active = false;

    public ScrollRect mapScrollRect;
    public ScrollContentController scrollContentController;

    void Start(){
        is_debugPanel_active = false;
        debugPanelCanvas.SetActive(false);

        GetScaleY();

        DebugTextClear();
        
        DebugTextAdd("Debug Panel Initialized.");
        DebugTextAdd("Press ~ to toggle Debug Panel.");
    }

    void Update(){
        //KeyCode.BackQuote: 美式键盘波浪号键
        if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            SetStageActive(is_debugPanel_active);

            is_debugPanel_active = !is_debugPanel_active;
            debugPanelCanvas.SetActive(is_debugPanel_active);
            
        }
    }

/// <summary>
/// 调整Stage上所有物件的可操纵性为value
/// </summary>
/// <param name="value">设置激活状态true/false</param>
/// <returns>调整合法性检查结果</returns>
    bool SetStageActive(bool value){
        if(mapScrollRect.enabled == !value && scrollContentController.enabled == !value)
        {
            mapScrollRect.enabled = value;
            scrollContentController.enabled = value;
            return true;
        }
        else{
            Debug.LogError("Stage Active Not Synced. SetStageActive Failed.");
            return false;
        }
        
    }
/// <summary>
/// 获取DebugTextContent的缩放比例，存入scale_y
/// </summary>
    void GetScaleY(){
        RectTransform debugText_rtrans = debugText.GetComponent<RectTransform>();
        scale_y = debugText_rtrans.localScale.y;
    }

/// <summary>
/// 向DebugText中添加一行字符串显示
/// </summary>
/// <param name="text">待添加的字符串。不需要加换行符</param>
    public void DebugTextAdd(string text){
        debugText.text += text + "\n";

        RectTransform debugContent_rtrans = debugTextContent.GetComponent<RectTransform>();

        Vector2 new_size = debugContent_rtrans.sizeDelta;
        
        new_size.y = debugText.preferredHeight * scale_y;

        debugContent_rtrans.sizeDelta = new_size;
    }

/// <summary>
/// 清空DebugText
/// </summary>
    public void DebugTextClear(){
        debugText.text = "";
    }


}

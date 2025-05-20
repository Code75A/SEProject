
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //NOTE: Grid.Height/Width = tilemap.Size * tilemap.Scale * Content.Scale 
    public static UIManager Instance { get; private set; }
    public BuildManager buildManager = BuildManager.Instance;

    #region UI元素

    [Header("Debug表盘")]
    public GameObject debugPanelCanvas;
    public GameObject debugTextContent;
    public TextMeshProUGUI debugText;

    //相对于原始大小的缩放比例，初始需载入一次
    private float  scale_y;

    bool is_debugPanel_active = false;
    
    [Header("地图缩放")]
    public ScrollRect mapScrollRect;
    public ScrollContentController scrollContentController;

    [Header("建造菜单")]
    public Transform buildingMenu;
    public GameObject buildingMenuBar;
    public GameObject buildingMenuSquare;

    //建造菜单UI定位相关参数
    //DeltaX/Y = Width/Height * Scale
    const float BAR_ANCHOR_X = -1136f, BAR_ANCHOR_Y = -673.9189f, BAR_DELTAX = 288f, BAR_DELTAY = 92.16f; //scale = 144f
    const float SQUARE_ANCHOR_X = -521f, SQUARE_ANCHOR_Y = -609.408f, SQUARE_DELTAX = 221.184f, SQUARE_DELTAY = 221.184f; //scale = 10.8f

    public Vector3 bar_anchor, square_anchor;
    public Vector3 bar_deltax, bar_deltay, square_deltax, square_deltay;
    
    public GameObject[] buildingMenuBars = new GameObject[(int)BuildManager.BuildingType.Total];
    public GameObject[] buildingMenuSquares ;//最多一行放8个

    [Header("选中详细")]
    public GameObject selectedObjectPanel;

    #endregion

    //单例模式
    void Awake(){
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start(){
        InitDebugPanel();
        InitBuildingMenu();
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
    
//--------------------------初始化函数--------------------------
    void InitDebugPanel(){
        is_debugPanel_active = false;
        debugPanelCanvas.SetActive(false);

        RectTransform debugText_rtrans = debugText.GetComponent<RectTransform>();
        scale_y = debugText_rtrans.localScale.y;

        DebugTextClear();
        
        DebugTextAdd("Debug Panel Initialized.");
        DebugTextAdd("Press ~ to toggle Debug Panel.");
    }
    void InitBuildingMenu(){

        bar_anchor = new Vector3(BAR_ANCHOR_X, BAR_ANCHOR_Y, 0f);square_anchor = new Vector3(SQUARE_ANCHOR_X, SQUARE_ANCHOR_Y, 0f);
        bar_deltax = new Vector3(BAR_DELTAX, 0f, 0f);bar_deltay = new Vector3(0f, BAR_DELTAY, 0f);
        square_deltax = new Vector3(SQUARE_DELTAX, 0f, 0f); square_deltay = new Vector3(0f, SQUARE_DELTAY, 0f);

        Vector3 current_anchor = bar_anchor - bar_deltay;

        for(int i = 0; i < (int)BuildManager.BuildingType.Total; i++){

            buildingMenuBars[i] = Instantiate(buildingMenuBar,buildingMenu);
            buildingMenuBars[i].GetComponent<BuildingMenuBarLoadController>().Init(this,(BuildManager.BuildingType)i);

            if(i % 2 == 0)
            {
                current_anchor += bar_deltay;
                buildingMenuBars[i].GetComponent<RectTransform>().localPosition = current_anchor;
            }
            else{
                buildingMenuBars[i].GetComponent<RectTransform>().localPosition = current_anchor + bar_deltax;
            }

            buildingMenuBars[i].GetComponentInChildren<TextMeshProUGUI>().text = ((BuildManager.BuildingType)i).ToString();
        }
    }
//--------------------------初始化函数--------------------------



//--------------------------BuildingMenu相关函数--------------------------
    public void LoadBuildingMenuSquares(BuildManager.BuildingType type){
        //最多一行加载8个，如果超过8个时需要另行设计
        ClearBuildingMenuSquares();

        List<BuildManager.Building> currentBuildingList = buildManager.LoadBuildingList(type);
        buildingMenuSquares = new GameObject[currentBuildingList.Count];

        Vector3 current_anchor = square_anchor - square_deltay;

        for(int i = 0; i < currentBuildingList.Count; i++){
            int index = i % 8;
            
            buildingMenuSquares[i] = Instantiate(buildingMenuSquare,buildingMenu);
            
            if(index == 0){
                current_anchor += square_deltay;
                buildingMenuSquares[i].GetComponent<RectTransform>().localPosition = current_anchor;
            }
            else{
                buildingMenuSquares[i].GetComponent<RectTransform>().localPosition = current_anchor + square_deltax * index;
            }

            buildingMenuSquares[i].GetComponent<BuildingMenuSquareLoadController>().Init(currentBuildingList[i], currentBuildingList[i].texture);

        }
        
    }

    void ClearBuildingMenuSquares(){
        foreach(GameObject menuSquare in buildingMenuSquares)
            Destroy(menuSquare);
    }
//--------------------------BuildingMenu相关函数--------------------------



//--------------------------DebugPanel相关函数--------------------------
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
//--------------------------DebugPanel相关函数--------------------------
}

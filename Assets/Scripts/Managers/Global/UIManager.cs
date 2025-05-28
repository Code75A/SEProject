
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
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
    private float scale_y;

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
    public GameObject[] buildingMenuSquares;//最多一行放8个

    [Header("选中详细")]
    public GameObject selectedObjectPanel;
    public TextMeshProUGUI selectedObjectDescription;

    #endregion

    //单例模式
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitDebugPanel();
        InitBuildingMenu();
        HideSelectedObjectPanel();
    }

    void Update()
    {
        //KeyCode.BackQuote: 美式键盘波浪号键
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            SetStageActive(is_debugPanel_active);

            is_debugPanel_active = !is_debugPanel_active;
            debugPanelCanvas.SetActive(is_debugPanel_active);

        }
    }

    #region 初始化函数

    void InitDebugPanel()
    {
        is_debugPanel_active = false;
        debugPanelCanvas.SetActive(false);

        RectTransform debugText_rtrans = debugText.GetComponent<RectTransform>();
        scale_y = debugText_rtrans.localScale.y;

        DebugTextClear();

        DebugTextAdd("Debug Panel Initialized.");
        DebugTextAdd("Press ~ to toggle Debug Panel.");
    }
    void InitBuildingMenu()
    {

        bar_anchor = new Vector3(BAR_ANCHOR_X, BAR_ANCHOR_Y, 0f); square_anchor = new Vector3(SQUARE_ANCHOR_X, SQUARE_ANCHOR_Y, 0f);
        bar_deltax = new Vector3(BAR_DELTAX, 0f, 0f); bar_deltay = new Vector3(0f, BAR_DELTAY, 0f);
        square_deltax = new Vector3(SQUARE_DELTAX, 0f, 0f); square_deltay = new Vector3(0f, SQUARE_DELTAY, 0f);

        Vector3 current_anchor = bar_anchor - bar_deltay;

        for (int i = 0; i < (int)BuildManager.BuildingType.Total; i++)
        {

            buildingMenuBars[i] = Instantiate(buildingMenuBar, buildingMenu);
            buildingMenuBars[i].GetComponent<BuildingMenuBarLoadController>().Init(this, (BuildManager.BuildingType)i);

            if (i % 2 == 0)
            {
                current_anchor += bar_deltay;
                buildingMenuBars[i].GetComponent<RectTransform>().localPosition = current_anchor;
            }
            else
            {
                buildingMenuBars[i].GetComponent<RectTransform>().localPosition = current_anchor + bar_deltax;
            }

            buildingMenuBars[i].GetComponentInChildren<TextMeshProUGUI>().text = ((BuildManager.BuildingType)i).ToString();
        }
    }

    #endregion

    #region BuildingMenu相关函数

    public void LoadBuildingMenuSquares(BuildManager.BuildingType type)
    {
        //最多一行加载8个，如果超过8个时需要另行设计
        ClearBuildingMenuSquares();

        List<BuildManager.Building> currentBuildingList = buildManager.LoadBuildingList(type);
        buildingMenuSquares = new GameObject[currentBuildingList.Count];

        Vector3 current_anchor = square_anchor - square_deltay;

        for (int i = 0; i < currentBuildingList.Count; i++)
        {
            int index = i % 8;

            buildingMenuSquares[i] = Instantiate(buildingMenuSquare, buildingMenu);

            if (index == 0)
            {
                current_anchor += square_deltay;
                buildingMenuSquares[i].GetComponent<RectTransform>().localPosition = current_anchor;
            }
            else
            {
                buildingMenuSquares[i].GetComponent<RectTransform>().localPosition = current_anchor + square_deltax * index;
            }

            buildingMenuSquares[i].GetComponent<BuildingMenuSquareLoadController>().Init(currentBuildingList[i], currentBuildingList[i].texture);

        }

    }

    void ClearBuildingMenuSquares()
    {
        foreach (GameObject menuSquare in buildingMenuSquares)
            Destroy(menuSquare);
    }

    #endregion
    #region DebugPanel相关函数

    /// <summary>
    /// 调整Stage上所有物件的可操纵性为value
    /// </summary>
    /// <param name="value">设置激活状态true/false</param>
    /// <returns>调整合法性检查结果</returns>
    bool SetStageActive(bool value)
    {
        if (mapScrollRect.enabled == !value && scrollContentController.enabled == !value)
        {
            mapScrollRect.enabled = value;
            scrollContentController.enabled = value;
            return true;
        }
        else
        {
            Debug.LogError("Stage Active Not Synced. SetStageActive Failed.");
            return false;
        }

    }
    /// <summary>
    /// 向DebugText中添加一行字符串显示
    /// </summary>
    /// <param name="text">待添加的字符串。不需要加换行符</param>
    public void DebugTextAdd(string text)
    {
        debugText.text += text + "\n";

        RectTransform debugContent_rtrans = debugTextContent.GetComponent<RectTransform>();

        Vector2 new_size = debugContent_rtrans.sizeDelta;

        new_size.y = debugText.preferredHeight * scale_y;

        debugContent_rtrans.sizeDelta = new_size;
    }
    /// <summary>
    /// 清空DebugText
    /// </summary>
    public void DebugTextClear()
    {
        debugText.text = "";
    }

    #endregion
    #region SelectedObjectPanel相关函数

    public void HideSelectedObjectPanel() {
        selectedObjectPanel.SetActive(false);
    }
    public void ShowSelectedObjectPanel() {
        selectedObjectPanel.SetActive(true);
    }

    #region 设置描述文本
    public void SetPanelTextBuild(BuildManager.Building building) {
        selectedObjectDescription.text = "建筑物名称: " + building.name + "\n" +
                                        "建筑物类型: " + building.type.ToString() + "\n" +
                                        "建筑物耐久度: " + building.durability.ToString() + "\n" +
                                        "建筑物大小: " + building.width.ToString() + "x" + building.height.ToString() + "\n" +
                                        "建筑物可建造性: " + building.can_build.ToString() + "\n" +
                                        "建筑物可行走性: " + building.can_walk.ToString() + "\n" +
                                        "建筑物可种植性: " + building.can_plant.ToString();
        ShowSelectedObjectPanel();
    }
    public void SetPanelTextPawn(PawnManager.Pawn pawn) {
        selectedObjectDescription.text = "小人id: " + pawn.id.ToString() + "\n" +
                                        "小人执行任务中: " + pawn.isOnTask.ToString() + "\n";
        if (pawn.handlingTask != null)
            selectedObjectDescription.text += "小人当前任务: " + pawn.handlingTask.type.ToString() + "\n";
        else
            selectedObjectDescription.text += "小人当前任务: null\n";

        selectedObjectDescription.text += "搬运中物品: " + pawn.materialId.ToString() + "\n" +
                                        "搬运数量: " + pawn.materialAmount.ToString() + "\n";
        ShowSelectedObjectPanel();
    }
    Dictionary<Type, Func<ItemInstanceManager.ItemInstance,string> > instanceDescriptor = new Dictionary<Type, Func<ItemInstanceManager.ItemInstance, string>>(){
{ typeof(ItemInstanceManager.ToolInstance),         instance => {   return "耐久: " + (instance as ItemInstanceManager.ToolInstance).GetDurability().ToString(); }},
{ typeof(ItemInstanceManager.CropInstance),         instance => {   ItemInstanceManager.CropInstance crop_instance = instance as ItemInstanceManager.CropInstance;
                                                                    return "生长进度: " + crop_instance.growth.ToString() + "/" + crop_instance.real_lifetime.ToString(); }},
{ typeof(ItemInstanceManager.BuildingInstance),     instance => {   return "耐久: " + (instance as ItemInstanceManager.BuildingInstance).durability.ToString(); }},
{ typeof(ItemInstanceManager.PrintInstance),        instance => {   ItemInstanceManager.PrintInstance print_instance = instance as ItemInstanceManager.PrintInstance;
                                                                    string tmp = "当前进度: \n";
                                                                    foreach(var pair in print_instance.material_list){
                                                                        tmp += ItemManager.Instance.GetItem(pair.Key).name + ": " +
                                                                                pair.Value.current + "/" + pair.Value.need + "\n";}
                                                                    return tmp;}},
{ typeof(ItemInstanceManager.ResourceInstance),     instance =>{    return "耐久: " + (instance as ItemInstanceManager.ResourceInstance).durability.ToString(); }}
    };

    public void SetPanelTextInstance(ItemInstanceManager.ItemInstance instance)
    {
        if (instance != null)
        {
            selectedObjectDescription.text = instance.GetText() + "\n" +
                                            "物品id: " + instance.GetModelId().ToString() + "\n" +
                                            "地块位置: " + instance.GetPosition().ToString() + "\n" +
                                            "种类: " + instance.type.ToString() + "\n";

            if (instanceDescriptor.TryGetValue(instance.GetType(), out var func)){
                selectedObjectDescription.text += func(instance);
            }
        }
        else
        {
            selectedObjectDescription.text = "这玩意不是一个Instance\n就不显示了哈";
        }

        ShowSelectedObjectPanel();

    }
        #endregion

        #endregion
    }

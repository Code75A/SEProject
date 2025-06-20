using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MouseInteractManager : MonoBehaviour
{
    public static MouseInteractManager Instance { get; private set; }
    public MapManager mapManager = MapManager.Instance;
    public PawnManager pawnManager = PawnManager.Instance;
    public ItemInstanceManager itemInstanceManager = ItemInstanceManager.Instance;

    public enum InstructTypes{
        move, grow, Harvest, Mine, total
    }
    Dictionary<InstructTypes, string> InstructNameCastTable = new Dictionary<InstructTypes, string>(){
        {InstructTypes.move,"移动"},
        {InstructTypes.grow,"种植"},
        {InstructTypes.Harvest,"收割"},
        {InstructTypes.Mine,"采掘"},
        {InstructTypes.total,"边界错误"}
    };
    Dictionary<InstructTypes, Action<Vector3Int>> InstructDirector = new Dictionary<InstructTypes, Action<Vector3Int>>(){
        {InstructTypes.move, (pos) => Instance.ApplyInstructMove(pos)},
        {InstructTypes.grow, (pos) => Instance.ApplyInstructGrow(pos)},
        {InstructTypes.Harvest, (pos) => Instance.ApplyInstructHarvest(pos)},
        ///{InstructTypes.Mine, (pos) => Instance.ApplyInstructMine(pos)}
    };

    public List<InstructTypes> pawn_instructs = new List<InstructTypes>()
    {InstructTypes.move,InstructTypes.grow,InstructTypes.Harvest};

    #region 状态缓存
    protected MouseInteractState currentState = new StateNull();
    protected MouseInteractState preState = new StateNull();
    protected Collider2D currentSprite = null;
    protected StateNull NullState = new StateNull();

    private Vector3 mouseWorldPos;

    #region 鼠标交互射线状态缓存
    RaycastHit2D hitSprite;
    RaycastHit2D[] hits;
    RaycastHit2D[] sorted_hits;
    Ray ray;
    #endregion

    public GameObject selectingObject = null;
    #region StateBuilding
    public Building currentBuilding = null;
    public GameObject currentBuilding_preview;
    public SpriteRenderer currentBuilding_preview_spriteRenderer;
    #endregion
    #region StatePawn
    public PawnManager.Pawn selectingPawn = null; // 当前被选中的 Pawn
    public GameObject preSelectedPawn = null;
    public PawnInteractController pawnController;
    #endregion
    #region StateInstruct
    public InstructTypes selectingInstruct = InstructTypes.total;
    #endregion

    #endregion

    #region 鼠标交互状态
    protected abstract class MouseInteractState
    {
        public abstract void OnClickSprite(RaycastHit2D hitSprite);
        public abstract void OnClickNull();
        public abstract void LoadPanel();
    }

    protected class StateNull : MouseInteractState
    {
        public override void OnClickSprite(RaycastHit2D hitSprite)
        {
            GameObject gameObject = hitSprite.collider.gameObject;
            if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                Instance.currentState = new StateUI(gameObject);
            }
            else if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Pawn"))
            {
                Instance.currentState = new StatePawn(gameObject);
            }
            else if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Instance"))
            {
                Instance.currentState = new StateInstance(gameObject);
            }
        }
        public override void OnClickNull()
        {
            Instance.ResetSelectingObject();
        }
        public override void LoadPanel()
        {
            UIManager.Instance.HideSelectedObjectPanel();
        }
    }
    protected class StateUI : MouseInteractState
    {
        public StateUI(GameObject gameObject)
        {
            Instance.selectingObject = gameObject;
        }
        public override void OnClickSprite(RaycastHit2D hitSprite)
        {
            GameObject gameObject = hitSprite.collider.gameObject;
            if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                //TODO: necessary?
                if (gameObject != Instance.selectingObject)
                {
                    Instance.currentState = new StateUI(gameObject);
                }
            }
            else if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Pawn"))
            {
                Instance.currentState = new StatePawn(gameObject);
            }
            else if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Instance"))
            {
                Instance.currentState = new StateInstance(gameObject);
            }
        }
        public override void OnClickNull()
        {
            Instance.ResetSelectingObject();
        }
        public override void LoadPanel()
        {
            //UIManager.Instance.HideSelectedObjectPanel();
        }
    }
    protected class StatePawn : MouseInteractState
    {
        public StatePawn(GameObject gameObject)
        {
            Instance.pawnController = gameObject.GetComponent<PawnInteractController>();
            Instance.selectingPawn = Instance.pawnController.pawn;
            Instance.selectingObject = gameObject;

            Instance.preSelectedPawn = gameObject;
        }
        public override void OnClickSprite(RaycastHit2D hitSprite)
        {
            GameObject gameObject = hitSprite.collider.gameObject;
            if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                Instance.currentState = new StateUI(gameObject);
            }
            else if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Pawn"))
            {
                Instance.currentState = new StatePawn(gameObject);
            }
            else if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Instance"))
            {
                Instance.currentState = new StateInstance(gameObject);
            }
        }
        public override void OnClickNull()
        {
            Instance.selectingPawn = null;
            Instance.ResetSelectingObject();
        }
        public override void LoadPanel()
        {
            UIManager.Instance.SetPanelTextPawn(Instance.pawnController.pawn);
        }

        public void MoveByPlayer()
        {
            UnityEngine.Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int onMouseCellPos = Instance.mapManager.landTilemap.WorldToCell(mouseWorldPos);
            Instance.pawnController.MovePawnToPositionByPlayer(onMouseCellPos);

            LoadPanel();
        }
    }
    protected class StateInstance : MouseInteractState
    {
        public StateInstance(GameObject gameObject)
        {
            Instance.selectingObject = gameObject;
        }
        public override void OnClickSprite(RaycastHit2D hitSprite)
        {
            GameObject gameObject = hitSprite.collider.gameObject;
            if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                Instance.currentState = new StateUI(gameObject);
            }
            else if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Pawn"))
            {
                Instance.currentState = new StatePawn(gameObject);
            }
            else if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Instance"))
            {
                Instance.currentState = new StateInstance(gameObject);
            }
        }
        public override void OnClickNull()
        {
            Instance.ResetSelectingObject();
        }
        public override void LoadPanel()
        {
            if (int.TryParse(Instance.selectingObject.name, out int id))
            {
                ItemInstanceManager.ItemInstance instance = ItemInstanceManager.Instance.GetInstance(id);
                UIManager.Instance.SetPanelTextInstance(instance);
            }
            else
            {
                UIManager.Instance.SetPanelTextInstance(null);
            }

        }
    }
    protected class StateBuilding : MouseInteractState{
        public StateBuilding(Building building)
        {
            Instance.SetCurrentBuilding(building);
        }

        public override void OnClickSprite(RaycastHit2D hitSprite)
        {
            GameObject gameObject = hitSprite.collider.gameObject;
            if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                Instance.CancelCurrentBuilding();
                Instance.currentState = new StateUI(gameObject);
            }
            else
            {
                ;
            }
        }
        public override void OnClickNull()
        {
            Instance.CancelCurrentBuilding();
            Instance.ResetSelectingObject();
        }
        public override void LoadPanel()
        {
            UIManager.Instance.SetPanelTextBuild(Instance.currentBuilding);
        }
    }
    protected class StateInstruct : MouseInteractState{
        public InstructTypes instruct_type;
        public StateInstruct(InstructTypes type){
            instruct_type = type;
        }
        public override void OnClickSprite(RaycastHit2D hitSprite)
        {
            GameObject gameObject = hitSprite.collider.gameObject;
            if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI")){
                Instance.currentState = new StateUI(gameObject);
            }
            else
            {
                ;
            }
        }
        public override void OnClickNull(){
            Instance.ResetSelectingObject();
        }
        public override void LoadPanel(){
            UIManager.Instance.SetPanelTextInstruct(instruct_type);
        }
    }
    #endregion
    void Awake(){
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        currentState = NullState;
    }
    void Start(){
        //UNHEALTHY
        // const float ORE_SPAWN_LINE = 0.85f;
        // for (int x = 0; x < 64; x++)
        // {
        //     for (int y = 0; y < 64; y++)
        //     {
        //         if (mapManager.landformDatas[x, y] > ORE_SPAWN_LINE)
        //         {
        //             ItemInstanceManager.Instance.SpawnItem(new Vector3Int(x, y, 0), 16, ItemInstanceManager.ItemInstanceType.ResourceInstance);
        //             mapManager.mapDatas[x, y].can_build = false;
        //         }
        //     }
        // }
    }

    int GetSortingLayerOrder(Collider2D col){
        var renderer = col.GetComponent<Renderer>();
        if (renderer == null)
            return int.MinValue; // 没有 Renderer 就排最下面

        // 用排序层ID和层内顺序拼成一个整体的排序指标
        return (renderer.sortingLayerID << 16) + renderer.sortingOrder;
    }
    void GetMouseInteract()
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //<多个collider重叠时时循环选择>
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hits = Physics2D.RaycastAll(ray.origin, ray.direction);
        sorted_hits = hits.OrderByDescending(hit => GetSortingLayerOrder(hit.collider)).ToArray();

        hitSprite = sorted_hits.FirstOrDefault();
    }
    void UpdateMouseInteractData()
    {
        if (selectingObject != null)
            currentSprite = selectingObject.GetComponent<Collider2D>();

        int index = -1;
        if (selectingObject != null)
            index = Array.FindIndex(sorted_hits, hit => hit.collider == currentSprite);

        if (index != -1)
        {
            if (hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
                hitSprite = sorted_hits[index];
            else if (index == sorted_hits.Count() - 1)
                hitSprite = default;
            else hitSprite = sorted_hits[index + 1];
        }
        //<多个collider重叠时时循环选择>
        if (hitSprite != default)
            currentState.OnClickSprite(hitSprite);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            preState = currentState;

            GetMouseInteract();
            UpdateMouseInteractData();

            if (currentState is StateInstruct)
            {
                if (InstructStateAvailable())
                {
                    Vector3Int cellPos = mapManager.landTilemap.WorldToCell(mouseWorldPos);
                    if (mapManager.IsInBoard(cellPos))
                    {
                        if (InstructDirector.TryGetValue((currentState as StateInstruct).instruct_type, out Action<Vector3Int> action))
                        {
                            action(cellPos);
                        }
                        else
                        {
                            Debug.LogError("Invalid Instruct");
                        }
                    }
                }
            }
            else if (currentState is StateBuilding)
            {
                if (BuildingStateAvailable())
                {
                    Vector3Int cellPos = mapManager.landTilemap.WorldToCell(mouseWorldPos);
                    if (mapManager.IsInBoard(cellPos))
                    {
                        mapManager.BuildByPlayer(cellPos, currentBuilding);
                    }
                }
            }
            else if (hitSprite.collider != null)
            {
                //currentState.OnClickSprite(hitSprite);

                if (currentState is StateUI){
                    UIManager.Instance.HideSelectedObjectPanel();

                    GameObject hit_gameObject = hitSprite.collider.gameObject;
                    if (hit_gameObject.tag == "Untagged")
                    {
                        if (SelectBuildSquare()) {; }
                        else if (SelectInstructSquare()) {; }
                        else Debug.Log("Invalid StateUI");
                    }
                    else if (hit_gameObject.tag == "ExitUI")//TODO:should Expand for more menu
                    {
                        UIManager.Instance.HideTraderMenuPanel();
                        TraderManager.Instance.inTraderPanel = false;
                    }
                    else if (hit_gameObject.tag == "UpperUI" || hit_gameObject.tag == "LowerUI")
                    {
                        GoodsContentController goods_content = hit_gameObject.GetComponentInParent<GoodsContentController>();
                        if (goods_content != null)
                            if (hitSprite.collider == goods_content.collider_upper)
                                goods_content.Add();
                            else if (hitSprite.collider == goods_content.collider_lower)
                                goods_content.Minus();
                            else Debug.Log("collider Mismatch");
                        else Debug.Log("Get GoodsContent Failed");
                    }

                    if (currentState is not StatePawn && currentState is not StateInstruct)
                        UIManager.Instance.ClearInstructMenuSquares();
                }
                else if (TraderManager.Instance.inTraderPanel)
                    return;
                else if (currentState is StatePawn)
                {
                    if (preState is not StatePawn)
                    {
                        UIManager.Instance.LoadInstructMenuSquares(pawn_instructs);
                    }
                }
            }
            else
            {
                currentState.OnClickNull();
            }
            Debug.Log("左键:" + currentState.GetType());
        }

        if (Input.GetMouseButtonDown(1))
        {
            GetMouseInteract();
            UpdateMouseInteractData();

            if (currentState is StateInstance)
            {
                if (int.TryParse(Instance.selectingObject.name, out int id))
                {
                    ItemInstanceManager.ItemInstance instance = ItemInstanceManager.Instance.GetInstance(id);
                    if (instance.item_id == TraderManager.TRADER_ID)
                    {
                        UIManager.Instance.ShowTraderMenuPanel();
                        TraderManager.Instance.inTraderPanel = true;
                    }

                }
            }

            if (currentState is StatePawn)
            {
                mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int onMouseCellPos = mapManager.landTilemap.WorldToCell(mouseWorldPos);

                if (Instance.PawnStateAvailable())
                {
                    if (MapManager.Instance.PawnCanMoveTo(onMouseCellPos))
                    {
                        (currentState as StatePawn).MoveByPlayer();
                    }
                }
                else
                {
                    Debug.Log("Error: PawnState自检未通过，请查看log");
                }
            }
            else if (currentState is StateInstruct)
            {
                if (preSelectedPawn != null)
                    currentState = new StatePawn(preSelectedPawn);
                else currentState.OnClickNull();
            }
            else
            {
                currentState.OnClickNull();
            }
            Debug.Log("右键" + currentState.GetType());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentState.OnClickNull();
            Debug.Log("Esc: 取消选中");
        }

        currentState.LoadPanel();
    }
    public string CastInstructName(InstructTypes type)
    {
        string name = "查询失败";
        InstructNameCastTable.TryGetValue(type, out name);
        return name;
    }

    #region 状态设定函数
    public void CancelCurrentBuilding(){
        currentBuilding_preview_spriteRenderer.sprite = null;
        currentBuilding_preview.SetActive(false);
        currentBuilding = null;
    }
    public void SetCurrentBuilding(Building building){
        currentBuilding_preview_spriteRenderer.sprite = building.texture;
        currentBuilding_preview.SetActive(true);
        currentBuilding = building;
    }
    public void ResetSelectingObject(){
        UIManager.Instance.ClearInstructMenuSquares();
        UIManager.Instance.HideSelectedObjectPanel();
        
        selectingObject = null;
        currentState = NullState;
    }
    #endregion

    #region 指令操作函数
    public void ApplyInstructMove(Vector3Int pos) {
        if (selectingPawn != null){
            pawnController.MovePawnToPositionByPlayer(pos);
        }
        else{
            Debug.LogError("When ApplyInstructMove, selectingPawn is Null");
        }
        
    }
    public void ApplyInstructGrow(Vector3Int pos) {
        //TODO: Grow指令
    }
    public void ApplyInstructHarvest(Vector3Int pos) {
        if (selectingPawn != null){
            pawnController.HarvestAtPositionByPlayer(pos);
        }
        else{
            Debug.LogError("When ApplyInstructHarvest, selectingPawn is Null");
        }
    }
    // public void ApplyInstructMine(Vector3Int pos) {
    //     if (selectingPawn != null){
    //         pawnController.MineByPlayer(pos);
    //     }
    //     else{
    //         Debug.LogError("When ApplyInstructMine, selectingPawn is Null");
    //     }
    // }
    #endregion
    #region 自检

    private bool PawnStateAvailable()
    {
        if (selectingPawn == null)
        {
            Debug.LogError("Error: 存在无主selectingPawn为null");
            return false;
        }
        if (selectingObject.GetComponent<PawnInteractController>() == null)
        {
            Debug.LogError("Error: 选中对象不含PawnInteractController，可能不是Pawn对象");
            return false;
        }
        return true;
    }
    private bool BuildingStateAvailable(){
        if (currentBuilding == null)
        {
            Debug.LogError("Error: 当前建筑为null");
            return false;
        }

        return true;
    }
    private bool InstructStateAvailable(){
        return true;//TODO 
    }

    private bool SelectBuildSquare()
    {
        BuildingMenuSquareLoadController controller = selectingObject.GetComponent<BuildingMenuSquareLoadController>();
        if (controller != null)
        {
            Instance.currentState = new StateBuilding(controller.building);
            return true;
        }
        else return false;
    }
    private bool SelectInstructSquare(){
        InstructMenuSquareLoadController controller = selectingObject.GetComponent<InstructMenuSquareLoadController>();
        if (controller != null) {
            Instance.currentState = new StateInstruct(controller.instruct_type);
            return true;
        }
        else return false;
    }

    #endregion

}

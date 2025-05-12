using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseInteractManager : MonoBehaviour
{
    public static MouseInteractManager Instance{get; private set;}
    public MapManager mapManager = MapManager.Instance;
    public PawnManager pawnManager = PawnManager.Instance;
    public ItemInstanceManager itemInstanceManager = ItemInstanceManager.Instance;

    #region 鼠标交互状态
    protected abstract class MouseInteractState{
        public abstract void OnClickSprite(RaycastHit2D hitSprite);
        public abstract void OnClickNull();
    }

    protected class StateNull : MouseInteractState{
        public override void OnClickSprite(RaycastHit2D hitSprite){
            GameObject gameObject = hitSprite.collider.gameObject;
            if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI")){
                Instance.currentState = new StateUI(gameObject);
            }
            else if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Pawn")){
                Instance.currentState = new StatePawn(gameObject);
            }
            else if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Instance")){
                Instance.currentState = new StateInstance(gameObject);
            }
        }

        public override void OnClickNull(){
            Instance.ResetSelectingObject();
        }
    }
    protected class StateUI :MouseInteractState{
        public StateUI(GameObject gameObject){
            Instance.selectingObject = gameObject;
        }
        public override void OnClickSprite(RaycastHit2D hitSprite){
            GameObject gameObject = hitSprite.collider.gameObject;
            if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI")){
                if(gameObject != Instance.selectingObject){
                    Instance.currentState = new StateUI(gameObject);
                }
            }
            else if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Pawn")){
                Instance.currentState = new StatePawn(gameObject);
            }
            else if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Instance")){
                Instance.currentState = new StateInstance(gameObject);
            }
        }
        public override void OnClickNull(){
            Instance.ResetSelectingObject();
        }
    }
    protected class StateBuilding : MouseInteractState{
        public StateBuilding(BuildManager.Building building){
            Instance.SetCurrentBuilding(building);
        }
        public override void OnClickSprite(RaycastHit2D hitSprite)
        {
            //TODO: Switch State
        }
        public override void OnClickNull(){
            Instance.CancelCurrentBuilding();
            Instance.ResetSelectingObject();
        }

        public void BuildOnMap(){
            //TODO: build
        }
    }

    public interface PileClickNext {
        public void OnClickNext();
    }
    protected class StatePawn : MouseInteractState, PileClickNext{
        private PawnInteractController pawnController;
        public void OnClickNext(){//Pawn -> ItemInstance
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int onMouseCellPos = Instance.mapManager.landTilemap.WorldToCell(mouseWorldPos);
            if(Instance.mapManager.HasItemAt(onMouseCellPos)){
                Instance.currentState = new StateInstance(Instance.mapManager.GetItem(onMouseCellPos));
            }
            else{
                OnClickNull();
            }
        }   
        public StatePawn(GameObject gameObject){
            pawnController = gameObject.GetComponent<PawnInteractController>();
            Instance.selectingPawn = pawnController.pawn;
            Instance.selectingObject = gameObject;
        }
        public override void OnClickSprite(RaycastHit2D hitSprite){
            GameObject gameObject = hitSprite.collider.gameObject;
            if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI")){
                OnClickNull();
            }
            else if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Pawn")){
                if(gameObject == Instance.selectingObject){
                    OnClickNext();
                }
                else{
                    Instance.currentState = new StatePawn(gameObject);
                }
            }
            else if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Instance")){
                Instance.currentState = new StateInstance(gameObject);
            }
        }

        public override void OnClickNull(){
            Instance.selectingPawn = null;
            Instance.ResetSelectingObject();
        }

        public void MoveByPlayer(){
            UnityEngine.Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int onMouseCellPos = Instance.mapManager.landTilemap.WorldToCell(mouseWorldPos);
            pawnController.MovePawnToPositionByPlayer(onMouseCellPos);
        }
    }
    protected class StateInstance : MouseInteractState, PileClickNext{
        public void OnClickNext(){//ItemInstance -> Null/ItemInstance
            OnClickNull();
            //TODO: 未来循环选择ItemInstance
        }  
        public StateInstance(GameObject gameObject){
            Instance.selectingObject = gameObject;
        }
        public override void OnClickSprite(RaycastHit2D hitSprite){
            GameObject gameObject = hitSprite.collider.gameObject;
            if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("UI")){
                OnClickNull();
            }
            else if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Pawn")){
                Instance.currentState = new StatePawn(gameObject);
            }
            else if(hitSprite.collider.gameObject.layer == LayerMask.NameToLayer("Instance")){
                if(gameObject == Instance.selectingObject){
                    OnClickNext();
                }
                else{
                    Instance.currentState = new StateInstance(gameObject);
                }
            }
        }

        public override void OnClickNull(){
            Instance.ResetSelectingObject();
        }
    }
    
    
    #endregion
    void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
        currentState = NullState;
    }

    #region 状态缓存
    protected MouseInteractState currentState;
    protected StateNull NullState = new StateNull();


    private Vector3 mouseWorldPos;
    //TODO: CellPos

    public GameObject selectingObject = null;
        #region StateBuilding
    public BuildManager.Building currentBuilding = null;
    public GameObject currentBuilding_preview;
    public SpriteRenderer currentBuilding_preview_spriteRenderer;
        #endregion

        #region StatePawn
    public PawnManager.Pawn selectingPawn = null; // 当前被选中的 Pawn
        #endregion
    #endregion

    //TODO: StateCache
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            RaycastHit2D hitSprite = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f);
            if (hitSprite.collider != null){
                currentState.OnClickSprite(hitSprite);

                if(currentState is StateUI){
                    BuildingMenuSquareLoadController controller =selectingObject.GetComponent<BuildingMenuSquareLoadController>();
                    if(controller != null){
                        Debug.Log("转BuildingState");
                        Instance.currentState = new StateBuilding(controller.building);
                    }
                }
            }
            else{
                if(currentState is StateBuilding){
                    // if(BuildStateValid){

                    // }
                    Vector3Int cellPos = mapManager.landTilemap.WorldToCell(mouseWorldPos);
                    if(mapManager.IsInBoard(cellPos)){
                        mapManager.BuildByPlayer(cellPos, currentBuilding);
                    }
                }
                else
                    currentState.OnClickNull();
            }
            Debug.Log("左键:"+currentState.GetType());
        }

        if(Input.GetMouseButtonDown(1)){
            if(currentState is StatePawn){
                mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
                Vector3Int onMouseCellPos = mapManager.landTilemap.WorldToCell(mouseWorldPos);

                if(Instance.PawnStateAvalid()){
                    if(MapManager.Instance.IsWalkable(onMouseCellPos) && !MapManager.Instance.HasPawnAt(onMouseCellPos)){
                        (currentState as StatePawn).MoveByPlayer();
                    }
                }
                else{
                    Debug.Log("Error: PawnState自检未通过，请查看log");
                }
            }
            else{
                currentState.OnClickNull();
            }
            Debug.Log("右键"+currentState.GetType());
        }

        if(Input.GetKeyDown(KeyCode.Escape)){
            currentState.OnClickNull();
            Debug.Log("Esc: 取消选中");
        }
    }

    public void CancelCurrentBuilding(){
        currentBuilding_preview_spriteRenderer.sprite = null;
        currentBuilding_preview.SetActive(false);
        currentBuilding = null;
    }
    public void SetCurrentBuilding(BuildManager.Building building){ 
        currentBuilding_preview_spriteRenderer.sprite = building.texture;
        currentBuilding_preview.SetActive(true);
        currentBuilding = building;
    }
    public void ResetSelectingObject(){
        selectingObject = null;
        currentState = NullState;
    }

    #region 自检
    private bool PawnStateAvalid(){
        if(selectingPawn == null){
            Debug.LogError("Error: 存在无主selectingPawn为null");
            return false;
        }
        if(selectingObject.GetComponent<PawnInteractController>() == null){
            Debug.LogError("Error: 选中对象不含PawnInteractController，可能不是Pawn对象");
            return false;
        }
        return true;
    }
    #endregion

}

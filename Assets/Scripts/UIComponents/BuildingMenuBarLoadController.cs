
using UnityEngine;

public class BuildingMenuBarLoadController : MonoBehaviour
{
    public UIManager uiManager;
    BuildingType buildingType;

    /// <summary>
    /// 设置buildManager和buildingType
    /// </summary>
    /// <param name="buildManager">全局buildManager</param>
    /// <param name="type">BuildManager：enum BuildingType</param>
    public void Init(UIManager uiManager,BuildingType type)
    {
        this.uiManager = uiManager;
        buildingType = type;
    }

    void OnMouseDown()
    {
        uiManager.LoadBuildingMenuSquares(buildingType);
    }

}

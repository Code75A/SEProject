# 项目SBOM与代码说明文档 - [基于Unity引擎的沙盒模拟经营游戏]

## 项目信息

* 项目名称： [基于Unity引擎的沙盒模拟经营游戏]
* 版本： v0.0.0.3
* 简述： 一款基于Unity开发的沙盒模拟农场经营游戏。

## 文件结构说明 (`Assets`核心部分)

### 1. `Assets/Scripts`

* **`DataStructure/`**
  * `MyPriorityQueue.cs` **(来源：LLM提供)**
    * **用途：** .NET6版本以下框架不支持PrioriyQueue数据结构，故手动实现一个优先队列，以支持A*寻路算法
    * **提示词：** "在某网格状地图中，存在可通行地块与不可通行地块，在unity内A*算法实现从st到ed的八向移动的寻路，假设位于NET6以下，如何构建一个优先队列并进行寻路算法？"
  * `IntPair.cs` **(来源：自研)**
    * **用途：** 用于记录物品需求表的KeyValuePair具有类似哈希表的不可序列化特性，不利于静态数据存储。故自行构建一组int对类型数据结构，用以实现配方数据的静态存储。
* **`Managers/`**
  * **`Global/`**
    * `ItemManager.cs` **(来源：自研)**
      * **用途：** 管理各Item条目具体属性，并在外部模块请求时通过接口予以相应信息。
    * `MapManager.cs` **(来源：自研)**
      * **用途：** 管理和维护地图地格具体信息，并在收到对象交互时更新相应参数。
    * `MouseInteractManager.cs` **(来源：自研)**
      * **用途：** 捕获玩家的鼠标输入，并根据状态机设计让玩家能与对象产生交互。
    * `PawnManager.cs` **(来源：自研)**
      * **用途：** 管理玩家的主要交互对象之一Pawn，维护Pawn执行任务的函数，使得Pawn能够自动进行移动、采集、建造等交互。
    * `SLManager.cs` **(来源：自研)**
      * **用途：** 储存和加载游玩过程产生或改变的动态数据，实现存档功能。
    * `TaskManager.cs` **(来源：自研)**
      * **用途：** 维护任务列表，并在逻辑帧更新时自动将其分派给空闲Pawn执行任务，从而实现Pawn行为的自动化。
    * `TimeManager.cs` **(来源：自研)**
      * **用途：** 用于记录时钟与管理游戏内时间流失和事件更新速度
    * `UIManager.cs` **(来源：自研)**
      * **用途：** 用于以代码自动化构建图形化用户交互界面
    * (`AeraManager.cs` **(来源：自研)**)
      * **用途：** 在未来用于实现区块化管理地格，减少遍历成本。
  * **`Local/`**
    * `BuildManager.cs` **(来源：自研)**
      * **用途：** 管理各Building条目具体属性，维护当前选中的BuildingList，并在外部模块请求时通过接口予以相应信息。
    * `CropManager.cs` **(来源：自研)**
      * **用途：** 管理各Crop条目具体属性，维护Factor类及其影响，自动更新作物生长进度，并在外部模块请求时通过接口予以相应信息。
    * `EventManager.cs` **(来源：自研)**
      * **用途：** 管理随机事件的属性与触发，将在未来进一步完善。
    * `IDManager.cs` **(来源：自研)**
      * **用途：** 为其它Manager提供一个封装良好的ID分配器。
    * `ItemInstanceManager.cs` **(来源：自研)**
      * **用途：** 负责ItemInstance实例对象的生成、更新、删除、管理等事宜。
    * `StorageManager.cs` **(来源：自研)**
      * **用途：** 维护材料总量的统计和分配，在面临离散却大量的查询请求时通过其维护降低查询成本。
    * `TraderManager.cs` **(来源：自研)**
      * **用途：** 维护贸易系统商品列表和刷新机制


* **`Pawn/`**
  * `pawnharvesttest.cs` **(来源：自研)**
    * **用途：** 提供一则可手动触发的样例用于测试Pawn的Harvest任务处理逻辑
  * `PawnInteractController.cs` **(来源：自研)**
    * **用途：** 作为组件控制Pawn的移动和任务指派功能
  * `TestPawnSpawner.cs` **(来源：自研)**
    * **用途：** 提供一个按键触发召唤Pawn用以调试的功能
  * (`pawnscalefollower.cs` **(来源：自研)**)
    * **用途：** 原用以消除Pawn的缩放影响，现已将功能统合进PawnManager中
  * (`PawnMovementTest.cs` **(来源：自研)**)
    * **用途：** 提供一则可手动触发的样例用于测试Pawn的移动任务处理逻辑，已测试完毕
  
* **`SerializedData/`**
  * `Crop.cs` **(来源：自研)**
    * **用途：** Crop类定义，为实现静态化储存功能，数据类需要单独作顶层定义。
  * `Buildings/` **(来源：自研)**
    * **用途：** 静态化储存Building类数据，每种子类需要分别以独立脚本定义。
  * (`Factors/` **(来源：自研)**)
    * **用途：** 静态化储存Factor类数据，由于结构上不合适，计划于未来更新中删除。
* **`UIComponents/`**
  * `InheritUIComponents/KeyboardScrollRect.cs` **(来源：LLM提供参考 + 自研)**
    * **用途：** 继承自Unity脚本ScrollRect，在原有功能上新增按住shift加速屏幕移动的功能。（实现上稍有缺陷，需要未来更新完善）
    * **LLM部分：** 提供对ScrollRect一些数据结构的介绍，介绍关键参数及其作用。
  * `(BuildingMenuBar|BuildingMenuSquare|InstructMenuSquare|StorageMenuBar)LoadController.cs` **(来源：自研)**
    * **用途：** 用于装载和维护BuildingMenuBar,BuildingMenuSquare,InstructMenuSquare,StorageMenuBar的参数与资产等
  * `DebugTextShowcase.cs` **(来源：自研)**
    * **用途：** 提供可实时显示当前运行参数
  * `ExitCross.cs` **(来源：自研)**
    * **用途：** 提供被挂载的对象被点击后退出(隐藏)当前菜单的功能
  * `ExitTheGame.cs` **(来源：自研)**
    * **用途：** 提供被挂载的对象被点击后退出游戏或推出演算模式的功能
  * `FollowingMousePreview.cs` **(来源：自研)**
    * **用途：** 提供一个可以显示当前选中将要建造建筑预览的小图标(有小瑕疵有待修复)
  * `GoodsContentController.cs` **(来源：自研)**
    * **用途：** 维护贸易菜单中单个条目的各参数
  * `ScrollContentController.cs` **(来源：自研)**
    * **用途：** 实现滚轮缩放地图的功能


### 2. `Assets/Resources`

* `BuildingData/` **(来源：自研)**
   * **用途：** 储存建筑属性静态数据的.asset对象
* `CropData/` **(来源：自研)**
   * **用途：** 储存作物属性静态数据的.asset对象
* `ItemData/` **(来源：自研)**
   * **用途：** 储存物品属性静态数据的.asset对象
* (`FactorData/` **(来源：自研)**)
   * **用途：** 储存属性变量因素属性静态数据的.asset对象
   （计划在未来更新中删除）


### 3. `Assets/Scenes`
* `MainScene.unity` **(来源：Unity自动加载 + 人工加工)**
   * **用途：** 游戏中所有对象活动的场景
   * **人工加工：** 一切开发活动均在此场景上开发

### 4. `Assets/Prefabs`
* `ItemInstance.prefab` **(来源：自研)**
   * **用途：** 作为所有ItemInstance对象的预设体模板
* `pawnPrefab.prefab` **(来源：自研)**
   * **用途：** 作为所有Pawn对象实例的预设体模板
* `UIComponent/` **(来源：自研)**
   * **用途：** 储存GUI界面对象的预设体

### 5. `Assets/Sprites`
* `testSprites.png` **(来源：网络免费素材)**
   * **用途：** 网上找的一张像素风免费素材
* `cropTexture/` **(来源：自研)**
   * **用途：** 储存农作物成长四阶段的贴图
* `textTexture/` **(来源：自研)**
   * **用途：** 临时贴图储存各对象贴图
* `UI/` **(来源：自研)**
   * **用途：** 储存GUI界面对象所使用的贴图

### 6. `Assets/TextMesh Pro`
* `Documentation/` **(来源：Unity自动加载)**
   * **用途：** 储存记录TextMeshPro组件的说明文档
* `Fonts/` **(来源：Unity自动加载 + 人工加工)**
  * **用途：** 储存用于生成字体渲染文件的初始.ttf文件
  * **人工加工：** 引入标准宋体
* `Resources/` **(来源：Unity自动加载 + 人工加工)**
  * **用途：** 储存字体渲染配置文件
  * **人工加工：** 使用标准宋体与常用3500字生成了支持中文的字体渲染配置文件
* `Shaders/` **(来源：Unity自动加载)**
  * **用途：** 储存.shader着色器配置文件
* `Sprites/` **(来源：Unity自动加载)**
  * **用途：** 储存用于渲染emoji的图形文件

### 7. `Assets/TilePalettes`
* `testPalette/` **(来源：人工加工)**
   * **用途：** 储存调色盘配置文件与相应sprite文件

## 附录：外部依赖

### 依赖组件 (Package Manager)

本项目没有其他插件依赖，外部依赖仅含Unity Package。详见附录Packages。

### Asset Store 资源

无
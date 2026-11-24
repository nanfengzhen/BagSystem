# 商城界面 · 第一阶段

## 打开与运行

- 场景：`Assets/Scenes/SampleScene.unity`
- 场景入口：`Canvas/ShopScreen`，挂有 `ShopBootstrap` 和 `ShopView`。
- 完整商城 Prefab：`Assets/UISystem/Prefabs/Shop/ShopScreen.prefab`
- 商品卡片 Prefab：`Assets/UISystem/Prefabs/Shop/ProductCard.prefab`
- 配置：`Assets/UISystem/Data/ShopCatalog.asset`
- 素材：`Assets/UISystem/Texture/`

打开场景后点击 Play。编辑状态已放置10张商品预览卡片；进入运行后由配置重新创建列表，不会叠加两份卡片。

界面采用1920×1080、16:9设计，CanvasScaler为Scale With Screen Size、Match=0.5。其他比例的屏幕适配未在本阶段扩展。

## 已实现

- 全部商品10件；消耗品4件、武器装备2件、材料道具4件。
- 分类切换、自动选择分类内首件商品、商品卡片选中框、右侧详情联动。
- 列表滚动；商品数量超过两行时可向下滚动。
- 数量1～99：加减按钮、整数输入、结束编辑时纠正空值和越界值。
- 总价使用long计算并随数量更新。
- 购买确认弹窗、取消、一次性确认和购买请求预览结果。
- 充值、背包、关闭按钮提供本阶段说明提示，后续再接入对应页面。

## 本阶段数据边界

余额1280、容量18/40、持有量和价格都是可编辑的预览参数。确认购买不会扣款、加库存或保存数据；没有背包业务、支付、角色效果或本地存档。

商品分类中“材料道具”暂时包含钥匙、金币袋和材料。编辑ShopCatalog即可调整类别、名称、图标、说明、价格和预览持有量。新增商品需填写唯一productId并分配Sprite。单次购买数量上限可在同一配置中修改。

## 9个运行时文件

1. `ShopBootstrap.cs`：连接配置、View与Presenter。
2. `Data/ShopCatalog.cs`：商品数据结构、分类枚举与ScriptableObject配置。
3. `Presentation/ShopView.cs`：汇总控件事件、创建卡片和更新界面。
4. `Presentation/ShopPresenter.cs`：分类、商品选择、数量、展示总价与确认流程。
5. `Views/ProductCardView.cs`：单件商品显示及点击。
6. `Views/ProductDetailView.cs`：右侧详情显示。
7. `Views/CategoryButtonView.cs`：分类按钮及选中标记。
8. `Views/QuantitySelectorView.cs`：输入和加减数量。
9. `Views/ConfirmDialogView.cs`：确认、取消和提示弹窗。

`Editor/ShopSceneBuilder.cs`是仅在编辑器使用的搭建工具，不随游戏运行。它通过Unity Editor API创建配置、Prefab和层级；不会在每次Play时重建场景。为保护已有编辑，它拒绝直接覆盖已存在的商城根、配置和Prefab。

## 后续业务接入点

`ShopPresenter.PurchaseConfirmed`发出商品ID和数量。后续由业务层重新查询配置、检查余额与容量，成功提交后再刷新UI。现在的成功提示明确标注为购买请求预览，不代表业务购买成功。

不要直接在View里改金币和库存。页面导航接入前，先把Presenter里的充值/背包/关闭提示替换为实际导航调用。

## 美术与字体

已将原素材设为Sprite，关闭Mip Maps，设置Clamp、Bilinear和正确九宫格边距。主面板、按钮、标签使用Sliced；道具保持比例。

为在当前机器显示中文，使用本机Windows等线字体创建`Assets/UISystem/Fonts/ShopChinese SDF.asset`；源文件为同目录Deng.ttf。此项不表示已获得字体再分发许可。正式发布或分享工程前请换成你有授权的中文字体，并替换TMP字体引用。

TMP Essential Resources从已安装的TextMeshPro3.0.6包导入，提供文字着色器和基础配置；没有导入TMP示例场景。

## 验证范围

在Unity2022.3.46f1c1中完成编译、运行模式UI点击及数据读回。覆盖：装备筛选2件、选盾牌、数量3对应总价540、上限99、下限1、购买取消和确认预览、消耗品4件、材料4件、滚动后选择第10件商品、返回全部商品。

确认与取消后预览余额均保持1280；退出Play后读取控制台未发现错误或警告。场景与Prefab保存后通过Unity API读回，10件商品、4个分类、三栏面板和序列化引用存在，没有Missing Script。

本阶段没有执行独立Player构建，也没有真机和其他长宽比验证。

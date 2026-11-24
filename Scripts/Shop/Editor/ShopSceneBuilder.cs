using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace BagSystem.Shop.Editor
{
    public static class ShopSceneBuilder
    {
        public const string AssetRoot = "Assets/UISystem";
        private const string TextureRoot = AssetRoot + "/Texture/";
        private static TMP_FontAsset font;
        private static readonly Color TextColor = Hex("ECF0F5"), Muted = Hex("AEB8C7"), Gold = Hex("EDC16B");

        private static Color Hex(string value) { Color c; ColorUtility.TryParseHtmlString("#" + value, out c); return c; }
        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Missing asset: " + path);
            return asset;
        }
        private static Sprite Sprite(string group, string name) { return RequireAsset<Sprite>(TextureRoot + group + "/" + name + ".png"); }
        private static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            Folder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false); rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y); rt.sizeDelta = new Vector2(width, height); return rt;
        }
        private static void Stretch(RectTransform rt)
        { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; }
        private static Image Image(string name, Transform parent, string group, string sprite, float x, float y, float w, float h, bool sliced = true)
        {
            var rt = Rect(name, parent, x, y, w, h);
            var image = rt.gameObject.AddComponent<Image>(); image.sprite = Sprite(group, sprite);
            image.type = sliced ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            image.raycastTarget = false; return image;
        }
        private static TextMeshProUGUI Text(string name, Transform parent, string content, float x, float y, float w, float h,
            float size = 24, bool bold = false, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, Color? color = null)
        {
            var rt = Rect(name, parent, x, y, w, h); var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font; t.text = content; t.fontSize = size; t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            t.alignment = alignment; t.color = color ?? TextColor; t.raycastTarget = false;
            t.enableWordWrapping = true; t.overflowMode = TextOverflowModes.Ellipsis; t.margin = Vector4.zero;
            return t;
        }
        private static Button Button(string name, Transform parent, string prefix, float x, float y, float w, float h)
        {
            var image = Image(name, parent, "03_buttons", prefix + "_normal", x, y, w, h);
            image.raycastTarget = true; var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image; button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState { highlightedSprite = Sprite("03_buttons", prefix + "_hover"),
                pressedSprite = Sprite("03_buttons", prefix + "_pressed"),
                selectedSprite = Sprite("03_buttons", prefix + "_hover"), disabledSprite = Sprite("03_buttons", prefix + "_disabled") };
            var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
            return button;
        }
        private static Button IconButton(string name, Transform parent, string sprite, float x, float y, float w, float h)
        {
            var rt = Rect(name, parent, x, y, w, h); var image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(1, 1, 1, 0.001f); var b = rt.gameObject.AddComponent<Button>(); b.targetGraphic = image;
            Image("Icon", rt, "04_icons", sprite, (w - 30) / 2, (h - 30) / 2, 30, 30, false);
            var nav = b.navigation; nav.mode = Navigation.Mode.None; b.navigation = nav; return b;
        }
        private static void Heading(Transform parent, string text)
        {
            Image("HeadingDiamond", parent, "04_icons", "icon_heading_diamond", 27, 29, 27, 27, false);
            Text("Heading", parent, text, 69, 21, 300, 44, 29, true);
        }

        public static object Build(Canvas canvas)
        {
            if (canvas == null) throw new InvalidOperationException("Target Canvas missing");
            if (canvas.transform.Find("ShopScreen") != null) throw new InvalidOperationException("ShopScreen already exists. Inspect before rebuilding.");
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null) throw new InvalidOperationException("Scene EventSystem missing");
            Folder(AssetRoot + "/Prefabs/Shop"); Folder(AssetRoot + "/Data"); Folder(AssetRoot + "/Fonts");
            ImportSprites(); font = CreateFont();
            var catalog = CreateCatalog();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) throw new InvalidOperationException("CanvasScaler missing");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = .5f;
            var root = Rect("ShopScreen", canvas.transform, 0, 0, 1920, 1080); Stretch(root);
            var view = root.gameObject.AddComponent<ShopView>();
            Image("Background", root, "01_backgrounds", "shop_background", 0, 0, 1920, 1080, false);
            var header = Rect("Header", root, 0, 0, 1920, 144);
            Image("HeaderBackground", header, "01_backgrounds", "header_background", 0, 0, 1920, 144, false);
            Text("BrandEnglish", header, "TRAVELER'S EXCHANGE", 61, 20, 420, 28, 17, true);
            Text("Title", header, "旅人商会", 59, 51, 400, 61, 46, true);
            Text("ShopTab", header, "商城", 597, 39, 140, 76, 29, true, TextAlignmentOptions.Center);
            Image("ActiveTabUnderline", header, "03_buttons", "tab_underline", 632, 120, 68, 12);
            var bag = IconButton("InventoryTab", header, "icon_backpack", 758, 41, 140, 74);
            var bagIcon = bag.transform.Find("Icon"); if (bagIcon != null) UnityEngine.Object.DestroyImmediate(bagIcon.gameObject);
            Text("Label", bag.transform, "背包", 0, 0, 140, 74, 29, true, TextAlignmentOptions.Center, Muted);
            Image("BalanceBackground", header, "03_buttons", "balance_background", 1390, 40, 304, 76);
            Image("Coin", header, "04_icons", "icon_coin", 1408, 56, 43, 43, false);
            var balance = Text("Balance", header, "1,280", 1466, 51, 173, 50, 32, true);
            var recharge = IconButton("RechargeButton", header, "icon_plus", 1635, 48, 50, 58);
            var close = IconButton("CloseButton", header, "icon_close", 1785, 42, 66, 66);
            var left = Panel(root, "CategoryPanel", 56, 190, 332, 778);
            var center = Panel(root, "ProductPanel", 415, 190, 954, 778);
            var right = Panel(root, "DetailPanel", 1392, 190, 472, 778);
            Heading(left, "分类"); Heading(center, "全部商品"); Heading(right, "物品详情");
            var categories = new CategoryButtonView[4];
            var catNames = new[] { "全部商品", "消耗品", "武器装备", "材料道具" };
            for (int i = 0; i < 4; i++)
            {
                var b = Button("Category_" + (ShopCategory)i, left, "category", 32, 104 + i * 94, 268, 76);
                var mark = Image("SelectedMark", b.transform, "04_icons", "icon_category_selected", 18, 23, 30, 30, false);
                Text("Label", b.transform, catNames[i], 43, 0, 210, 76, 26, true, TextAlignmentOptions.Center);
                var category = b.gameObject.AddComponent<CategoryButtonView>();
                category.Configure(b, b.GetComponent<Image>(), Sprite("03_buttons", "category_normal"), Sprite("03_buttons", "category_selected"), mark.gameObject, (ShopCategory)i);
                category.SetSelected(i == 0); categories[i] = category;
            }
            Image("CapacityDivider", left, "05_decoration", "divider_horizontal", 32, 574, 268, 3, false);
            Image("BackpackIcon", left, "04_icons", "icon_backpack", 35, 607, 34, 34, false);
            Text("CapacityLabel", left, "背包空间", 80, 603, 210, 44, 24);
            var capacity = Text("Capacity", left, "18 / 40", 44, 653, 250, 59, 40, true);
            Image("CapacityTrack", left, "05_decoration", "capacity_track", 44, 721, 242, 24);
            var fill = Image("CapacityFill", left, "05_decoration", "capacity_fill", 44, 721, 242, 24, false);
            fill.type = UnityEngine.UI.Image.Type.Filled; fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal; fill.fillAmount = .45f;
            var count = Text("ProductCount", center, "10 件商品  ·  默认排序", 565, 21, 352, 44, 22, false, TextAlignmentOptions.MidlineRight, Muted);
            var scrollRoot = Rect("ProductScroll", center, 26, 92, 904, 618);
            var scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 50;
            var viewport = Rect("Viewport", scrollRoot, 0, 0, 904, 618); viewport.gameObject.AddComponent<RectMask2D>();
            var catchImage = viewport.gameObject.AddComponent<Image>(); catchImage.color = new Color(0, 0, 0, .001f);
            var content = Rect("Content", viewport, 0, 0, 890, 1254);
            var grid = content.gameObject.AddComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(286, 296); grid.spacing = new Vector2(16, 22);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3; grid.childAlignment = TextAnchor.UpperLeft;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content;
            Text("ScrollHint", center, "滚动查看更多商品", 45, 730, 850, 26, 17, false, TextAlignmentOptions.Center, Hex("8396AF"));
            var cardPrefab = CreateCardPrefab(root);
            foreach (var product in catalog.Products)
            {
                var card = (ProductCardView)PrefabUtility.InstantiatePrefab(cardPrefab, content);
                card.name = "Product_" + product.productId; card.Bind(product);
                card.SetSelected(product == catalog.Products[0]);
            }
            Image("DetailSpotlight", right, "02_panels", "detail_spotlight", 43, 66, 376, 290, false);
            var detailIcon = Image("ItemImage", right, "06_items/png", "item_001_health_potion", 124, 68, 224, 266, false); detailIcon.preserveAspect = true;
            var detailName = Text("ItemName", right, "生命药水", 36, 340, 400, 53, 37, true);
            Image("TypeTag", right, "03_buttons", "item_type_tag", 37, 400, 160, 38);
            var type = Text("ItemType", right, "普通 · 消耗品", 41, 400, 152, 38, 19, false, TextAlignmentOptions.Center);
            var description = Text("Description", right, "恢复 100 点生命值。", 37, 465, 398, 95, 23, false, TextAlignmentOptions.TopLeft);
            Image("DetailDivider", right, "05_decoration", "divider_horizontal", 37, 564, 396, 3, false);
            var unit = Text("UnitPrice", right, "单价  10 金币", 37, 581, 240, 38, 22);
            var owned = Text("OwnedCount", right, "已拥有  8", 274, 581, 158, 38, 21, false, TextAlignmentOptions.MidlineRight);
            Text("QuantityLabel", right, "数量", 37, 629, 85, 60, 23);
            var quantityRoot = Image("QuantitySelector", right, "03_buttons", "quantity_normal", 130, 626, 304, 60);
            var quantityMinus = IconButton("Minus", quantityRoot.transform, "icon_minus", 3, 3, 59, 54);
            var quantityPlus = IconButton("Plus", quantityRoot.transform, "icon_plus", 242, 3, 59, 54);
            var inputRoot = Rect("Input", quantityRoot.transform, 65, 3, 174, 54);
            var inputImage = inputRoot.gameObject.AddComponent<Image>(); inputImage.color = new Color(0, 0, 0, .001f);
            var input = inputRoot.gameObject.AddComponent<TMP_InputField>();
            var inputViewport = Rect("TextViewport", inputRoot, 4, 0, 166, 54); inputViewport.gameObject.AddComponent<RectMask2D>();
            var inputText = Text("Text", inputViewport, "1", 0, 0, 166, 54, 26, true, TextAlignmentOptions.Center);
            input.textViewport = inputViewport; input.textComponent = inputText; input.targetGraphic = inputImage; input.lineType = TMP_InputField.LineType.SingleLine;
            input.selectionColor = new Color(.4f, .6f, .9f, .4f);
            var quantity = quantityRoot.gameObject.AddComponent<QuantitySelectorView>(); quantity.Configure(input, quantityMinus, quantityPlus);
            var buy = Button("PurchaseButton", right, "purchase", 28, 697, 418, 64);
            var buyText = Text("Label", buy.transform, "购买  /  10 金币", 10, 0, 398, 64, 27, true, TextAlignmentOptions.Center);
            var detail = right.gameObject.AddComponent<ProductDetailView>(); detail.Configure(detailIcon, detailName, type, description, unit, owned); detail.Render(catalog.Products[0]);
            var status = Text("PreviewStatus", root, "UI PREVIEW  ·  余额 / 持有量 / 容量均为预览数据", 58, 1004, 1600, 32, 16, false, TextAlignmentOptions.MidlineLeft, Hex("8191A5"));
            var dialog = CreateDialog(root);
            view.Configure(content, cardPrefab, detail, categories, quantity, dialog, balance, capacity, count, buyText, status, fill, buy, recharge, bag, close);
            var bootstrap = root.gameObject.AddComponent<ShopBootstrap>(); bootstrap.Configure(catalog, view);
            quantity.SetRange(1, catalog.MaxQuantity); quantity.SetQuantity(1);
            view.RenderPreviewStatus(catalog);
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog);
            Canvas.ForceUpdateCanvases(); LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            EditorSceneManager.SaveScene(canvas.gameObject.scene);
            Selection.activeGameObject = root.gameObject;
            return new { rootId = root.gameObject.GetInstanceID(), scene = canvas.gameObject.scene.path,
                catalog = AssetDatabase.GetAssetPath(catalog), products = catalog.Products.Count,
                runtimeScriptFiles = 9, cardPrefab = AssetRoot + "/Prefabs/Shop/ProductCard.prefab" };
        }

        private static RectTransform Panel(Transform parent, string name, float x, float y, float w, float h)
        {
            Image(name + "Shadow", parent, "02_panels", "panel_shadow", x - 64, y - 64, w + 128, h + 128);
            return (RectTransform)Image(name, parent, "02_panels", "panel_main", x, y, w, h).transform;
        }
        private static ProductCardView CreateCardPrefab(Transform parent)
        {
            const string path = AssetRoot + "/Prefabs/Shop/ProductCard.prefab";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) throw new InvalidOperationException("Prefab already exists: " + path);
            var button = Rect("ProductCard", parent, 0, 0, 286, 296).gameObject.AddComponent<Button>();
            var bg = button.gameObject.AddComponent<Image>(); bg.sprite = Sprite("02_panels", "product_card"); bg.type = UnityEngine.UI.Image.Type.Sliced;
            button.targetGraphic = bg; var colors = button.colors; colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f); button.colors = colors;
            var nav = button.navigation; nav.mode = Navigation.Mode.None; button.navigation = nav;
            Image("ItemWell", button.transform, "02_panels", "item_well", 14, 14, 258, 169);
            var icon = Image("ItemIcon", button.transform, "06_items/png-256", "item_001_health_potion", 40, -2, 206, 205, false); icon.preserveAspect = true;
            var title = Text("Name", button.transform, "", 20, 188, 249, 40, 26, true);
            var type = Text("Type", button.transform, "", 20, 229, 249, 28, 17);
            Image("Coin", button.transform, "04_icons", "icon_coin", 18, 258, 30, 30, false);
            var price = Text("Price", button.transform, "", 58, 251, 207, 43, 26, true, TextAlignmentOptions.MidlineLeft, Gold);
            var border = Image("SelectedBorder", button.transform, "02_panels", "product_selected_border", 0, 0, 286, 296);
            var tag = Image("SelectedTag", button.transform, "03_buttons", "selected_tag", 204, 12, 66, 40);
            Text("Label", tag.transform, "已选", 0, 0, 66, 40, 17, true, TextAlignmentOptions.Center, Hex("4F361B"));
            var card = button.gameObject.AddComponent<ProductCardView>(); card.Configure(button, icon, title, type, price, border.gameObject, tag.gameObject);
            var prefab = PrefabUtility.SaveAsPrefabAsset(button.gameObject, path);
            UnityEngine.Object.DestroyImmediate(button.gameObject);
            if (prefab == null) throw new InvalidOperationException("Could not save ProductCard prefab");
            return prefab.GetComponent<ProductCardView>();
        }
        private static ConfirmDialogView CreateDialog(Transform root)
        {
            var overlay = Rect("DialogLayer", root, 0, 0, 1920, 1080); Stretch(overlay);
            var blocker = overlay.gameObject.AddComponent<Image>(); blocker.color = new Color(.025f, .035f, .055f, .8f); blocker.raycastTarget = true;
            var panel = Image("ConfirmDialog", overlay, "02_panels", "panel_main", 600, 275, 720, 530);
            var title = Text("Heading", panel.transform, "确认购买", 40, 24, 580, 58, 32, true);
            var body = Text("Body", panel.transform, "", 40, 110, 640, 270, 25, false, TextAlignmentOptions.TopLeft);
            body.overflowMode = TextOverflowModes.Overflow;
            var close = IconButton("Close", panel.transform, "icon_close", 645, 25, 46, 46);
            var cancel = Button("Cancel", panel.transform, "category", 44, 420, 290, 70);
            var noText = Text("Label", cancel.transform, "取消", 0, 0, 290, 70, 25, true, TextAlignmentOptions.Center);
            var confirm = Button("Confirm", panel.transform, "purchase", 386, 420, 290, 70);
            var okText = Text("Label", confirm.transform, "确认", 0, 0, 290, 70, 25, true, TextAlignmentOptions.Center);
            var view = root.gameObject.AddComponent<ConfirmDialogView>(); view.Configure(overlay.gameObject, title, body, okText, noText, confirm, cancel, close);
            overlay.gameObject.SetActive(false); return view;
        }
        private static void ImportSprites()
        {
            var borders = new Dictionary<string, Vector4> {
                {"panel_main", Vector4.one*36}, {"panel_shadow", Vector4.one*80},
                {"product_card", Vector4.one*26}, {"product_selected_border", Vector4.one*26},
                {"item_well",Vector4.one*20}, {"tab_underline",new Vector4(8,0,8,0)},
                {"quantity_normal",Vector4.one*24}, {"quantity_disabled",Vector4.one*24},
                {"balance_background",Vector4.one*36}, {"selected_tag",Vector4.one*18},
                {"item_type_tag",Vector4.one*14}, {"capacity_fill",Vector4.one*12}, {"capacity_track",Vector4.one*12}
            };
            foreach (string path in AssetDatabase.FindAssets("t:Texture2D", new[] { TextureRoot.TrimEnd('/') }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("TextureImporter missing: " + path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.sRGBTexture = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp; importer.spritePixelsPerUnit = 100;
                importer.maxTextureSize = path.Contains("01_backgrounds") ? 2048 : 1024;
                string name = Path.GetFileNameWithoutExtension(path); Vector4 border;
                if (borders.TryGetValue(name, out border)) importer.spriteBorder = border;
                else if (name.StartsWith("category_")) importer.spriteBorder = Vector4.one*28;
                else if (name.StartsWith("purchase_")) importer.spriteBorder = Vector4.one*32;
                else if (name.StartsWith("tab_")) importer.spriteBorder = Vector4.one*20;
                else importer.spriteBorder = Vector4.zero;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }
        private static TMP_FontAsset CreateFont()
        {
            const string path = AssetRoot + "/Fonts/ShopChinese SDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing != null) return existing;
            var source = RequireAsset<Font>(AssetRoot + "/Fonts/Deng.ttf");
            var asset = TMP_FontAsset.CreateFontAsset(source, 56, 6, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
            if (asset == null) throw new InvalidOperationException("TMP font generation failed");
            asset.name = "ShopChinese SDF"; asset.atlasTextures[0].name = "ShopChinese Atlas"; asset.material.name = "ShopChinese Material";
            AssetDatabase.CreateAsset(asset, path); AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset); AssetDatabase.AddObjectToAsset(asset.material, asset);
            string missing;
            const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz /×,.-+·：，。！()（）全部商品消耗武器装备材料道具旅人会商城背包分类数量单价金币已拥有空间默认排序滚动查看更多物品详情请选择购买确认取消知道了请求预览当前为界面不扣增加库存后续可在此接入业务本次未款也充值入口尚阶段实现与互关闭留事件主可切回上页面生命药水魔力体旅行者长剑守卫之盾卷轴紫晶碎片袋宝箱钥匙强化石恢复点值途中缺少补给槽位显示";
            if (!asset.TryAddCharacters(characters, out missing) && !string.IsNullOrEmpty(missing))
                throw new InvalidOperationException("Font missing characters: " + missing);
            EditorUtility.SetDirty(asset); AssetDatabase.SaveAssetIfDirty(asset); return asset;
        }
        private static ShopCatalog CreateCatalog()
        {
            const string path = AssetRoot + "/Data/ShopCatalog.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) throw new InvalidOperationException("Catalog already exists: " + path);
            var catalog = ScriptableObject.CreateInstance<ShopCatalog>();
            var ids = new[] { "item_001_health_potion","item_002_mana_potion","item_003_adventurer_sword","item_004_guardian_shield","item_005_return_scroll","item_006_amethyst","item_007_coin_pouch","item_008_chest_key","item_009_stamina_potion","item_010_upgrade_stone" };
            var names = new[] {"生命药水","魔力药水","旅行者长剑","守卫者之盾","回城卷轴","紫晶碎片","金币袋","宝箱钥匙","体力药水","强化石"};
            var labels = new[] {"普通 · 消耗品","普通 · 消耗品","稀有 · 武器","稀有 · 装备","精良 · 道具","史诗 · 材料","普通 · 道具","精良 · 道具","普通 · 消耗品","稀有 · 材料"};
            var descriptions = new[] {"恢复 100 点生命值。\n旅途中不可缺少的补给。","恢复 80 点魔力值。\n为下一次施法做好准备。","轻便而可靠的长剑。\n适合旅途中的日常战斗。","金属包边的坚固盾牌。\n为冒险者提供可靠防护。","记录回城印记的卷轴。\n地图传送效果尚未接入。","蕴含魔力的紫色晶体。\n用于制作与研究的材料。","装有金币的皮革钱袋。\n开启效果尚未接入。","一把带有六角柄的钥匙。\n开箱效果尚未接入。","恢复 50 点体力值。\n为接下来的旅程补充体力。","刻有金属铭牌的石材。\n强化效果尚未接入。"};
            var prices = new[] {10,15,240,180,50,80,100,60,20,120};
            var owned = new[] {8,5,0,0,2,3,0,1,2,4};
            var cats = new[] {ShopCategory.Consumable,ShopCategory.Consumable,ShopCategory.Equipment,ShopCategory.Equipment,ShopCategory.Consumable,ShopCategory.Material,ShopCategory.Material,ShopCategory.Material,ShopCategory.Consumable,ShopCategory.Material};
            var list = new List<ShopProduct>();
            for (int i = 0; i < ids.Length; i++) list.Add(new ShopProduct { productId = ids[i], displayName = names[i], icon = Sprite("06_items/png", ids[i]),
                description = descriptions[i], typeLabel = labels[i], category = cats[i], unitPrice = prices[i], previewOwned = owned[i],
                typeColor = labels[i].StartsWith("史诗") ? Hex("BDA0E5") : labels[i].StartsWith("稀有") ? Hex("87B2E0") : labels[i].StartsWith("精良") ? Hex("8BC6B9") : Hex("BAC6D5") });
            catalog.SetProducts(list); AssetDatabase.CreateAsset(catalog, path); return catalog;
        }
    }
}

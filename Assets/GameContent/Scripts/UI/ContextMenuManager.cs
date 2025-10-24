using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenuManager : MonoBehaviour
{
    // Singleton-ish access
    public static ContextMenuManager Instance { get; private set; }

    [Header("Style")]
    [SerializeField] private Font defaultFont;
    [SerializeField] private int fontSize = 14;
    [SerializeField] private Vector2 padding = new Vector2(8, 8);
    [SerializeField] private Vector2 itemPadding = new Vector2(10, 6);
    [SerializeField] private Vector2 screenMargin = new Vector2(8, 8);

    private Canvas _canvas;
    private RectTransform _canvasRect;
    private RectTransform _menuRoot;
    private GameObject _blocker;   // closes menu when user clicks outside

    // Simple data object for items
    public class MenuItem
    {
        public string Label;
        public Action Action;
        public bool Interactable = true;

        public static MenuItem Separator() => new MenuItem { Label = null, Action = null, Interactable = false };
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null) _canvas = GetComponent<Canvas>();
        if (_canvas == null) throw new Exception("ContextMenuManager must be on/under a Canvas.");
        _canvasRect = _canvas.transform as RectTransform;

        CreateMenuRootIfNeeded();
    }

    private void CreateMenuRootIfNeeded()
    {
        if (_menuRoot != null) return;

        // Root object
        var go = new GameObject("ContextMenu", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(Shadow));
        go.transform.SetParent(_canvas.transform, false);
        _menuRoot = go.GetComponent<RectTransform>();
        _menuRoot.pivot = new Vector2(0, 1); // top-left pivot
        _menuRoot.gameObject.SetActive(false);

        //_menuRoot.anchorMin = new Vector2(0f, 1f); // top-left anchor
        //_menuRoot.anchorMax = new Vector2(0f, 1f);
        //_menuRoot.pivot = new Vector2(0f, 1f); // top-left pivot

        _menuRoot.anchorMin = new Vector2(0.5f, 0.5f);
        _menuRoot.anchorMax = new Vector2(0.5f, 0.5f);
        _menuRoot.pivot = new Vector2(0f, 1f);   // top-left

        _menuRoot.SetAsLastSibling(); // ensure menu renders above the blocker


        // Background
        var bg = go.GetComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.12f, 0.98f); // near-black with slight alpha

        // Shadow
        var sh = go.GetComponent<Shadow>();
        sh.effectDistance = new Vector2(0, -2);
        sh.effectColor = new Color(0, 0, 0, 0.6f);

        // Layout
        var vlg = go.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset((int)padding.x, (int)padding.x, (int)padding.y, (int)padding.y);
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 2;

        var fitter = go.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    // Public entry point when you already have items
    public void Show(GameObject target, IEnumerable<MenuItem> items, Vector2 screenPosition)
    {
        if (items == null) return;
        EnsureBlocker();

        ClearMenu();

        foreach (var it in items)
        {
            if (it.Label == null && it.Action == null)
            {
                AddSeparator();
            }
            else
            {
                AddItem(it.Label, it.Action, it.Interactable);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_menuRoot);
        PositionMenu(screenPosition);

        // Make sure the blocker is directly under the menu, and the menu is on top.
        _blocker.transform.SetSiblingIndex(_menuRoot.GetSiblingIndex());
        _menuRoot.SetAsLastSibling();

        _menuRoot.gameObject.SetActive(true);
        _blocker.SetActive(true);
    }

    // Public entry point to ask the clicked object what to show
    public void ShowFor(GameObject clicked, Vector2 screenPosition)
    {
        var items = BuildItemsFromProviders(clicked);
        Show(clicked, items, screenPosition);
    }

    private List<MenuItem> BuildItemsFromProviders(GameObject clicked)
    {
        var list = new List<MenuItem>();
        if (clicked == null) return list;

        // Gather providers from the clicked object and its parents
        var providers = clicked.GetComponentsInParent<IContextMenuProvider>(true);
        foreach (var p in providers)
        {
            try { p.BuildMenuItems(clicked, list); }
            catch (Exception e) { Debug.LogException(e); }
        }

        // Fallback example if nobody provided anything
        if (list.Count == 0)
        {
            list.Add(new MenuItem { Label = $"No actions for {clicked.name}", Action = null, Interactable = false });
        }
        return list;
    }

    private void AddItem(string label, Action action, bool interactable)
    {
        var row = new GameObject("Item", typeof(RectTransform), typeof(Image), typeof(Button),
                                 typeof(LayoutElement), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(_menuRoot, false);

        var rt = row.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);         // stretch horizontally
        rt.pivot = new Vector2(0, 1);

        // Background (hover colors)
        var img = row.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0);

        var btn = row.GetComponent<Button>();
        btn.interactable = interactable;
        var colors = btn.colors;
        colors.normalColor = new Color(1, 1, 1, 0);
        colors.highlightedColor = new Color(1, 1, 1, 0.08f);
        colors.pressedColor = new Color(1, 1, 1, 0.12f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(1, 1, 1, 0.05f);
        btn.colors = colors;
        if (action != null) btn.onClick.AddListener(() => { action.Invoke(); Hide(); });

        // Row layout handles padding and height
        var hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset((int)itemPadding.x, (int)itemPadding.x, (int)itemPadding.y, (int)itemPadding.y);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = false;
        hlg.spacing = 0;

        // Ensure a sensible min height based on font size + vertical padding
        var le = row.GetComponent<LayoutElement>();
        le.minHeight = fontSize + (itemPadding.y * 2) + 4;

        // ---- Label (UnityEngine.UI.Text) ----
        var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(ContentSizeFitter));
        textGO.transform.SetParent(row.transform, false);

        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0, 0.5f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.richText = true;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        tmp.text = label ?? "";
        tmp.color = interactable ? new Color(0.95f, 0.95f, 0.95f, 1f) : new Color(0.7f, 0.7f, 0.7f, 0.85f);

        var csf = textGO.GetComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_menuRoot);
    }

    private void AddSeparator()
    {
        var sep = new GameObject("Separator", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        sep.transform.SetParent(_menuRoot, false);
        var img = sep.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0.08f);
        var rt = sep.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 1f);
        var le = sep.GetComponent<LayoutElement>();
        le.preferredHeight = 1f;
        le.minHeight = 1f;
    }

    private void ClearMenu()
    {
        for (int i = _menuRoot.childCount - 1; i >= 0; i--)
            Destroy(_menuRoot.GetChild(i).gameObject);
    }

    private void PositionMenu(Vector2 screenPos)
    {
        var cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        // Get local point in the canvas' rect space (origin at canvas center, +Y up)
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, cam, out localPoint);

        // Because our pivot is top-left, placing the rect at 'localPoint' puts its top-left at the cursor.
        _menuRoot.anchoredPosition = localPoint;

        // Rebuild to know size before clamping
        LayoutRebuilder.ForceRebuildLayoutImmediate(_menuRoot);

        // Clamp inside canvas
        var canvasRect = _canvasRect.rect;     // centered rect
        var menuRect = _menuRoot.rect;       // menu size (width/height)

        // Limits in centered space with top-left pivot:
        float leftLimit = canvasRect.xMin + screenMargin.x;                       // left edge
        float rightLimit = canvasRect.xMax - screenMargin.x - menuRect.width;      // cursor pos for menu's left so menu stays inside right edge
        float topLimit = canvasRect.yMax - screenMargin.y;                       // top edge (remember +Y up)
        float bottomLimit = canvasRect.yMin + screenMargin.y + menuRect.height;     // cursor pos for menu's top so bottom stays inside

        Vector2 anchored = _menuRoot.anchoredPosition;

        // Clamp X
        if (anchored.x < leftLimit) anchored.x = leftLimit;
        if (anchored.x > rightLimit) anchored.x = rightLimit;

        // Clamp Y (since pivot is top, the anchoredPosition.y is the menu's top)
        if (anchored.y > topLimit) anchored.y = topLimit;
        if (anchored.y < bottomLimit) anchored.y = bottomLimit;

        _menuRoot.anchoredPosition = anchored;
    }

    private void EnsureBlocker()
    {
        if (_blocker != null) return;

        _blocker = new GameObject("ContextMenuBlocker", typeof(RectTransform), typeof(Image), typeof(Button));

        // ⚠️ Parent FIRST, then set sibling index.
        _blocker.transform.SetParent(_canvas.transform, false);

        // Now that it has a parent, send it behind everything.
        _blocker.transform.SetAsFirstSibling();

        var rt = _blocker.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var img = _blocker.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0); // invisible click-catcher

        var btn = _blocker.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0, 0, 0, 0);
        btn.colors = colors;

        btn.onClick.AddListener(Hide);
        _blocker.SetActive(false);
    }

    private void Update()
    {
        if (_menuRoot != null && _menuRoot.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Hide();
        }
    }

    public void Hide()
    {
        if (_menuRoot != null) _menuRoot.gameObject.SetActive(false);
        if (_blocker != null) _blocker.SetActive(false);
        ClearMenu();
    }
}

using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Shared helpers for building scenes from code. Wraps UnityEngine.UI.DefaultControls /
    /// TMPro.TMP_DefaultControls (the same factories the GameObject > UI menu uses) so every
    /// generated control looks and behaves like a normal, hand-placed Unity UI element.
    /// </summary>
    public static class SceneBuildUtility
    {
        private static DefaultControls.Resources uguiResources;
        private static TMP_DefaultControls.Resources tmpResources;
        private static bool resourcesReady;

        public static void EnsureResources()
        {
            if (resourcesReady) return;
            resourcesReady = true;

            uguiResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            uguiResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            uguiResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            uguiResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            uguiResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
            uguiResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
            uguiResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");

            tmpResources.standard = uguiResources.standard;
            tmpResources.background = uguiResources.background;
            tmpResources.inputField = uguiResources.inputField;
            tmpResources.knob = uguiResources.knob;
            tmpResources.checkmark = uguiResources.checkmark;
            tmpResources.dropdown = uguiResources.dropdown;
            tmpResources.mask = uguiResources.mask;
        }

        // --- Scene-level objects -----------------------------------------------------------

        public static GameObject CreateCamera(Color backgroundColor, float size = 6f)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = size;
            cam.backgroundColor = backgroundColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);
            return go;
        }

        public static GameObject CreateGlobalLight()
        {
            var go = new GameObject("Global Light 2D");
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            // Light2D only populates its "target sorting layers" array lazily in Awake(), which
            // is not reliably invoked for components added via editor scripting outside Play
            // Mode. Without this, the light targets zero sorting layers and every Sprite-Lit
            // sprite in the scene renders pure black. Set it explicitly to every sorting layer.
            light.targetSortingLayers = SortingLayer.layers.Select(l => l.id).ToArray();
            return go;
        }

        public static GameObject CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var module = go.GetComponent<InputSystemUIInputModule>();

            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            var uiMap = asset != null ? asset.FindActionMap("UI") : null;
            if (uiMap != null)
            {
                module.actionsAsset = asset;
                module.point = InputActionReference.Create(uiMap.FindAction("Point"));
                module.leftClick = InputActionReference.Create(uiMap.FindAction("Click"));
                module.rightClick = InputActionReference.Create(uiMap.FindAction("RightClick"));
                module.middleClick = InputActionReference.Create(uiMap.FindAction("MiddleClick"));
                module.move = InputActionReference.Create(uiMap.FindAction("Navigate"));
                module.submit = InputActionReference.Create(uiMap.FindAction("Submit"));
                module.cancel = InputActionReference.Create(uiMap.FindAction("Cancel"));
            }
            else
            {
                Debug.LogWarning("SceneBuildUtility: could not find the UI action map in InputSystem_Actions.inputactions; falling back to default actions.");
                module.AssignDefaultActions();
            }
            return go;
        }

        public static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        // --- UI controls ---------------------------------------------------------------------

        public static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            EnsureResources();
            var go = DefaultControls.CreatePanel(uguiResources);
            go.name = name;
            SetParent(go, parent);
            go.GetComponent<Image>().color = color;
            return go;
        }

        public static CanvasGroup AddCanvasGroup(GameObject go)
        {
            var group = go.GetComponent<CanvasGroup>();
            return group != null ? group : go.AddComponent<CanvasGroup>();
        }

        public static (GameObject go, Button button, TMP_Text label) CreateButton(
            Transform parent, string name, string text, Color bgColor, Color textColor, int fontSize = 30, float preferredHeight = 56f)
        {
            EnsureResources();
            var go = TMP_DefaultControls.CreateButton(tmpResources);
            go.name = name;
            SetParent(go, parent);
            go.GetComponent<Image>().color = bgColor;

            var label = go.GetComponentInChildren<TMP_Text>();
            label.text = text;
            label.color = textColor;
            label.fontSize = fontSize;
            label.raycastTarget = false;
            label.gameObject.AddComponent<LucillesJourney.UI.ScalableText>();

            // Explicit LayoutElement so any parent VerticalLayoutGroup/HorizontalLayoutGroup with
            // childControlHeight=true sizes this button correctly instead of shrinking it to the
            // button sprite's small native size.
            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = preferredHeight;

            return (go, go.GetComponent<Button>(), label);
        }

        public static TMP_Text CreateText(
            Transform parent, string name, string text, int fontSize, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft, bool scalable = true)
        {
            EnsureResources();
            var go = TMP_DefaultControls.CreateText(tmpResources);
            go.name = name;
            SetParent(go, parent);

            var label = go.GetComponent<TMP_Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            if (scalable) go.AddComponent<LucillesJourney.UI.ScalableText>();
            return label;
        }

        public static Slider CreateSlider(Transform parent, string name)
        {
            EnsureResources();
            var go = DefaultControls.CreateSlider(uguiResources);
            go.name = name;
            SetParent(go, parent);
            return go.GetComponent<Slider>();
        }

        public static Image CreateImage(Transform parent, string name, Sprite sprite, Color color)
        {
            EnsureResources();
            var go = DefaultControls.CreateImage(uguiResources);
            go.name = name;
            SetParent(go, parent);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        // --- RectTransform / layout convenience -----------------------------------------------

        public static RectTransform Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static RectTransform Anchor(
            GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
            return rt;
        }

        public static VerticalLayoutGroup AddVerticalLayout(
            GameObject go, int spacing = 16, RectOffset padding = null, bool controlWidth = true, bool controlHeight = true)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(20, 20, 20, 20);
            layout.childControlWidth = controlWidth;
            layout.childControlHeight = controlHeight;
            layout.childForceExpandWidth = controlWidth;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;
            return layout;
        }

        public static HorizontalLayoutGroup AddHorizontalLayout(GameObject go, int spacing = 16)
        {
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return layout;
        }

        /// <summary>
        /// Forces every VerticalLayoutGroup/HorizontalLayoutGroup/ContentSizeFitter under (and
        /// including) the given root to recalculate immediately. Scenes built purely via editor
        /// scripting (never entering Play Mode or triggering a Canvas repaint) otherwise save
        /// with every laid-out child left at its raw creation position, causing overlapping UI.
        /// Call this once per canvas, right before saving the scene.
        /// </summary>
        public static void ForceRebuildLayout(GameObject canvasRoot)
        {
            Canvas.ForceUpdateCanvases();
            var rect = canvasRoot.GetComponent<RectTransform>();
            if (rect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        public static void SetParent(GameObject go, Transform parent) => go.transform.SetParent(parent, false);
    }
}

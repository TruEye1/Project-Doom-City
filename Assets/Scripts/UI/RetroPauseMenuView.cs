using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class RetroPauseMenuView : MonoBehaviour
{
    public static Canvas CreatePauseCanvas()
    {
        GameObject canvasObject = new GameObject("MenuPausa_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 540f);
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();
        return canvas;
    }

    public void Build(PauseManager pauseManager)
    {
        EnsureEventSystem();
        PrepararCanvas();
        LimpiarHijos();

        GameObject overlay = CrearUI("RetroPauseMenu_Root", transform, typeof(Image));
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color32(4, 6, 12, 205);

        GameObject panel = CrearUI("Panel", overlay.transform, typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(360f, 300f);
        panel.GetComponent<Image>().color = new Color32(232, 220, 146, 255);

        GameObject inner = CrearUI("Inner", panel.transform, typeof(Image));
        RectTransform innerRect = inner.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(8f, 8f);
        innerRect.offsetMax = new Vector2(-8f, -8f);
        inner.GetComponent<Image>().color = new Color32(18, 20, 34, 255);

        TMP_Text title = CrearTexto("Titulo", inner.transform, "PAUSA", 46f, TextAlignmentOptions.Center, new Color32(255, 238, 120, 255));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -24f);
        titleRect.sizeDelta = new Vector2(0f, 60f);

        CrearBoton(inner.transform, "CONTINUAR", new Vector2(0f, 28f), pauseManager.Reanudar);
        CrearBoton(inner.transform, "REINICIAR NIVEL", new Vector2(0f, -40f), pauseManager.ReiniciarNivel);
        CrearBoton(inner.transform, "SALIR AL MENU", new Vector2(0f, -108f), pauseManager.VolverAlMenuPrincipal);
    }

    private void PrepararCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 540f);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void LimpiarHijos()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.transform.SetAsLastSibling();
    }

    private GameObject CrearUI(string objectName, Transform parent, params System.Type[] components)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        foreach (System.Type component in components)
        {
            go.AddComponent(component);
        }

        go.transform.SetParent(parent, false);
        return go;
    }

    private TMP_Text CrearTexto(string objectName, Transform parent, string text, float fontSize, TextAlignmentOptions alignment, Color32 color)
    {
        GameObject go = CrearUI(objectName, parent, typeof(TextMeshProUGUI));
        TMP_Text tmp = go.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private void CrearBoton(Transform parent, string text, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CrearUI($"Boton_{text.Replace(" ", "_")}", parent, typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(260f, 46f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color32(64, 78, 142, 255);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        ColorBlock colors = button.colors;
        colors.normalColor = new Color32(64, 78, 142, 255);
        colors.highlightedColor = new Color32(96, 116, 204, 255);
        colors.pressedColor = new Color32(32, 42, 92, 255);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color32(40, 40, 46, 255);
        button.colors = colors;

        TMP_Text label = CrearTexto("Text", buttonObject.transform, text, 20f, TextAlignmentOptions.Center, new Color32(255, 245, 210, 255));
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }
}

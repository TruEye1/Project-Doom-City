using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private HealthBarUI playerBar;
    [SerializeField] private HealthBarUI enemyBar;

    private readonly Dictionary<EnemigoIA, Action<int, int>> enemyHandlers = new Dictionary<EnemigoIA, Action<int, int>>();
    private SaludJugador jugador;
    private EnemigoIA enemigoActual;
    private static bool sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        if (sceneHookRegistered)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneHookRegistered = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureHUDExists()
    {
        if (FindFirstObjectByType<GameHUD>() != null)
        {
            return;
        }

        if (FindFirstObjectByType<SaludJugador>() == null)
        {
            return;
        }

        GameObject hud = new GameObject("GameHUD_Runtime", typeof(RectTransform));
        hud.AddComponent<GameHUD>();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureHUDExists();
    }

    private void Awake()
    {
        ConstruirSiHaceFalta();
    }

    private void OnEnable()
    {
        VincularJugador();
        VincularEnemigos();
    }

    private void OnDisable()
    {
        if (jugador != null)
        {
            jugador.OnVidaCambiada -= ActualizarVidaJugador;
        }

        foreach (KeyValuePair<EnemigoIA, Action<int, int>> pair in enemyHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.OnVidaCambiada -= pair.Value;
            }
        }

        enemyHandlers.Clear();
    }

    private void VincularJugador()
    {
        jugador = FindFirstObjectByType<SaludJugador>();
        if (jugador == null)
        {
            return;
        }

        jugador.OnVidaCambiada -= ActualizarVidaJugador;
        jugador.OnVidaCambiada += ActualizarVidaJugador;
        ActualizarVidaJugador(jugador.VidaActual, jugador.VidaMaxima);
    }

    private void VincularEnemigos()
    {
        EnemigoIA[] enemigos = FindObjectsByType<EnemigoIA>(FindObjectsSortMode.None);
        foreach (EnemigoIA enemigo in enemigos)
        {
            RegisterEnemy(enemigo);
        }

        if (enemigoActual != null)
        {
            enemyBar.SetValue(enemigoActual.VidaActual, enemigoActual.VidaMaxima);
            enemyBar.gameObject.SetActive(true);
        }
        else if (enemyBar != null)
        {
            enemyBar.gameObject.SetActive(false);
        }
    }

    public void RegisterEnemy(EnemigoIA enemigo)
    {
        if (enemigo == null || enemyHandlers.ContainsKey(enemigo))
        {
            return;
        }

        Action<int, int> handler = (current, max) => ActualizarVidaEnemigo(enemigo, current, max);
        enemyHandlers.Add(enemigo, handler);
        enemigo.OnVidaCambiada += handler;

        if (enemigoActual == null || enemigoActual.EstaMuerto)
        {
            enemigoActual = enemigo;
        }

        if (enemyBar != null)
        {
            enemyBar.gameObject.SetActive(true);
            enemyBar.SetValue(enemigo.VidaActual, enemigo.VidaMaxima);
        }
    }

    public void UnregisterEnemy(EnemigoIA enemigo)
    {
        if (enemigo == null || !enemyHandlers.TryGetValue(enemigo, out Action<int, int> handler))
        {
            return;
        }

        enemigo.OnVidaCambiada -= handler;
        enemyHandlers.Remove(enemigo);

        if (enemigoActual == enemigo)
        {
            enemigoActual = null;
            if (enemyBar != null)
            {
                enemyBar.gameObject.SetActive(false);
            }
        }
    }

    private void ActualizarVidaJugador(int current, int max)
    {
        if (playerBar != null)
        {
            playerBar.SetValue(current, max);
        }
    }

    private void ActualizarVidaEnemigo(EnemigoIA enemigo, int current, int max)
    {
        if (enemigo == null || enemyBar == null)
        {
            return;
        }

        enemigoActual = enemigo;
        enemyBar.gameObject.SetActive(true);
        enemyBar.SetValue(current, max);
    }

    private void ConstruirSiHaceFalta()
    {
        if (playerBar != null && enemyBar != null)
        {
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

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

        RectTransform root = transform as RectTransform;
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        playerBar = CrearBarra("HUD_PlayerHealth", "CHRIS", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Color32(52, 220, 86, 255));
        enemyBar = CrearBarra("HUD_EnemyHealth", "PANDILLERO", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Color32(231, 68, 54, 255));
    }

    private HealthBarUI CrearBarra(string objectName, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Color fillColor)
    {
        GameObject container = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(HealthBarUI));
        container.transform.SetParent(transform, false);

        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(280f, 50f);

        Image border = container.GetComponent<Image>();
        border.color = new Color32(238, 238, 198, 255);

        GameObject inner = new GameObject("Back", typeof(RectTransform), typeof(Image));
        inner.transform.SetParent(container.transform, false);
        RectTransform innerRect = inner.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(4f, 4f);
        innerRect.offsetMax = new Vector2(-4f, -4f);
        inner.GetComponent<Image>().color = new Color32(18, 18, 28, 255);

        GameObject track = new GameObject("Track", typeof(RectTransform));
        track.transform.SetParent(inner.transform, false);
        RectTransform trackRect = track.GetComponent<RectTransform>();
        trackRect.anchorMin = Vector2.zero;
        trackRect.anchorMax = Vector2.one;
        trackRect.offsetMin = new Vector2(5f, 17f);
        trackRect.offsetMax = new Vector2(-5f, -5f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(track.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Simple;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;

        TMP_Text labelText = CrearTexto("Label", inner.transform, label, TextAlignmentOptions.Left, 18f, new Vector2(7f, 24f), new Vector2(172f, 18f));
        TMP_Text valueText = CrearTexto("Value", inner.transform, "100/100", TextAlignmentOptions.Right, 16f, new Vector2(174f, 24f), new Vector2(92f, 18f));

        HealthBarUI bar = container.GetComponent<HealthBarUI>();
        bar.Configure(fillImage, fillRect, labelText, valueText, label);
        bar.SetValue(1, 1);
        return bar;
    }

    private TMP_Text CrearTexto(string objectName, Transform parent, string text, TextAlignmentOptions alignment, float fontSize, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TMP_Text tmp = go.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = new Color32(255, 245, 198, 255);
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        return tmp;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArcadeAspectRatioController : MonoBehaviour
{
    private const float TargetAspect = 4f / 3f;
    private const int BaseWidth = 960;
    private const int BaseHeight = 720;

    private static ArcadeAspectRatioController instance;

    private RectTransform leftBar;
    private RectTransform rightBar;
    private RectTransform topBar;
    private RectTransform bottomBar;
    private int lastWidth = -1;
    private int lastHeight = -1;
    private bool forceRefresh;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject controller = new GameObject("ArcadeAspectRatioController_Runtime");
        instance = controller.AddComponent<ArcadeAspectRatioController>();
        DontDestroyOnLoad(controller);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        CrearBarras();
        AplicarResolucionStandalone();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        forceRefresh = true;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LateUpdate()
    {
        if (forceRefresh || Screen.width != lastWidth || Screen.height != lastHeight)
        {
            RefrescarAspecto();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        forceRefresh = true;
    }

    private void AplicarResolucionStandalone()
    {
#if !UNITY_EDITOR
        Screen.SetResolution(BaseWidth, BaseHeight, FullScreenMode.Windowed);
#endif
    }

    private void RefrescarAspecto()
    {
        if (DebeOmitirLetterboxEnEditor())
        {
            RestaurarViewportCompleto();
            return;
        }

        int width = Mathf.Max(1, Screen.width);
        int height = Mathf.Max(1, Screen.height);
        lastWidth = width;
        lastHeight = height;
        forceRefresh = false;

        Rect viewport = CalcularViewport(width, height);
        Camera[] cameras = Camera.allCameras;
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == null)
            {
                continue;
            }

            cameras[i].rect = viewport;
            cameras[i].backgroundColor = Color.black;
        }

        AjustarBarras(viewport);
    }

    private bool DebeOmitirLetterboxEnEditor()
    {
#if UNITY_EDITOR
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName == "MenuInicio" || sceneName == "PantallaCarga";
#else
        return false;
#endif
    }

    private void RestaurarViewportCompleto()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        forceRefresh = false;

        Camera[] cameras = Camera.allCameras;
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
            {
                cameras[i].rect = new Rect(0f, 0f, 1f, 1f);
            }
        }

        SetBar(leftBar, Vector2.zero, Vector2.zero);
        SetBar(rightBar, Vector2.zero, Vector2.zero);
        SetBar(topBar, Vector2.zero, Vector2.zero);
        SetBar(bottomBar, Vector2.zero, Vector2.zero);
    }

    private Rect CalcularViewport(int width, int height)
    {
        float currentAspect = (float)width / height;

        if (currentAspect > TargetAspect)
        {
            float normalizedWidth = TargetAspect / currentAspect;
            float x = (1f - normalizedWidth) * 0.5f;
            return new Rect(x, 0f, normalizedWidth, 1f);
        }

        float normalizedHeight = currentAspect / TargetAspect;
        float y = (1f - normalizedHeight) * 0.5f;
        return new Rect(0f, y, 1f, normalizedHeight);
    }

    private void CrearBarras()
    {
        GameObject canvasObject = new GameObject("ArcadeAspectBars_Canvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        RectTransform root = canvasObject.GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        leftBar = CrearBarra("LeftBar", root);
        rightBar = CrearBarra("RightBar", root);
        topBar = CrearBarra("TopBar", root);
        bottomBar = CrearBarra("BottomBar", root);
    }

    private RectTransform CrearBarra(string objectName, Transform parent)
    {
        GameObject bar = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(parent, false);
        bar.GetComponent<Image>().color = Color.black;
        return bar.GetComponent<RectTransform>();
    }

    private void AjustarBarras(Rect viewport)
    {
        SetBar(leftBar, new Vector2(0f, 0f), new Vector2(viewport.x, 1f));
        SetBar(rightBar, new Vector2(viewport.x + viewport.width, 0f), new Vector2(1f, 1f));
        SetBar(bottomBar, new Vector2(viewport.x, 0f), new Vector2(viewport.x + viewport.width, viewport.y));
        SetBar(topBar, new Vector2(viewport.x, viewport.y + viewport.height), new Vector2(viewport.x + viewport.width, 1f));
    }

    private void SetBar(RectTransform bar, Vector2 anchorMin, Vector2 anchorMax)
    {
        if (bar == null)
        {
            return;
        }

        bar.anchorMin = anchorMin;
        bar.anchorMax = anchorMax;
        bar.offsetMin = Vector2.zero;
        bar.offsetMax = Vector2.zero;
        bar.gameObject.SetActive(anchorMax.x - anchorMin.x > 0.0001f && anchorMax.y - anchorMin.y > 0.0001f);
    }

}

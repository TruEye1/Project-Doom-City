using UnityEngine;
using UnityEngine.SceneManagement;

public static class HealthPickupBootstrap
{
    private const string GameplaySceneName = "SampleScene";

    private static Sprite runtimeSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsurePickupExists()
    {
        TryEnsurePickupForCurrentScene();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != GameplaySceneName)
        {
            return;
        }

        TryEnsurePickupForCurrentScene();
    }

    private static void TryEnsurePickupForCurrentScene()
    {
        if (Object.FindAnyObjectByType<HealthPickup>() != null)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(activeScene.name) && activeScene.name != GameplaySceneName)
        {
            return;
        }

        SaludJugador jugador = Object.FindAnyObjectByType<SaludJugador>();
        if (jugador == null)
        {
            return;
        }

        GameObject pickup = new GameObject("Pickup_Health_Small");
        pickup.transform.position = jugador.transform.position + new Vector3(4f, 0.2f, 0f);

        SpriteRenderer renderer = pickup.AddComponent<SpriteRenderer>();
        renderer.sprite = GetRuntimeSprite();
        renderer.color = new Color32(255, 255, 255, 255);
        renderer.sortingLayerName = "Personajes";
        renderer.sortingOrder = 10;

        BoxCollider2D collider = pickup.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.65f, 0.55f);

        pickup.AddComponent<HealthPickup>();
    }

    private static Sprite GetRuntimeSprite()
    {
        if (runtimeSprite != null)
        {
            return runtimeSprite;
        }

        Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 outline = new Color32(36, 28, 38, 255);
        Color32 red = new Color32(210, 46, 58, 255);
        Color32 redLight = new Color32(238, 75, 84, 255);
        Color32 white = new Color32(255, 245, 220, 255);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        for (int y = 4; y <= 11; y++)
        {
            for (int x = 2; x <= 13; x++)
            {
                bool edge = x == 2 || x == 13 || y == 4 || y == 11;
                texture.SetPixel(x, y, edge ? outline : red);
            }
        }

        for (int y = 2; y <= 5; y++)
        {
            for (int x = 5; x <= 10; x++)
            {
                bool edge = x == 5 || x == 10 || y == 2;
                if (edge)
                {
                    texture.SetPixel(x, y, outline);
                }
            }
        }

        for (int y = 5; y <= 10; y++)
        {
            texture.SetPixel(7, y, white);
            texture.SetPixel(8, y, white);
        }

        for (int x = 5; x <= 10; x++)
        {
            texture.SetPixel(x, 7, white);
            texture.SetPixel(x, 8, white);
        }

        for (int x = 3; x <= 12; x++)
        {
            texture.SetPixel(x, 10, redLight);
        }

        for (int y = 5; y <= 10; y++)
        {
            texture.SetPixel(7, y, white);
            texture.SetPixel(8, y, white);
        }

        texture.Apply();
        runtimeSprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 24f);
        return runtimeSprite;
    }
}

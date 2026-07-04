using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyWaveSpawner : MonoBehaviour
{
    private const string GameplaySceneName = "SampleScene";

    [Serializable]
    private class EnemyWave
    {
        public string nombre = "Oleada";
        public float triggerX;
        public Vector2[] posicionesSpawn;
        [NonSerialized] public bool spawned;
    }

    [SerializeField] private Transform jugador;
    [SerializeField] private EnemigoIA enemyTemplate;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private bool ocultarTemplateAlIniciar = true;
    [SerializeField] private EnemyWave[] waves =
    {
        new EnemyWave
        {
            nombre = "Oleada 1",
            triggerX = -10.5f,
            posicionesSpawn = new[]
            {
                new Vector2(-7.6f, -2.65f),
                new Vector2(-6.1f, -2.35f)
            }
        },
        new EnemyWave
        {
            nombre = "Oleada 2",
            triggerX = 0.5f,
            posicionesSpawn = new[]
            {
                new Vector2(2.8f, -2.65f),
                new Vector2(4.3f, -2.35f)
            }
        },
        new EnemyWave
        {
            nombre = "Oleada 3",
            triggerX = 10.2f,
            posicionesSpawn = new[]
            {
                new Vector2(11.2f, -2.7f),
                new Vector2(12.4f, -2.4f),
                new Vector2(13.4f, -2.7f),
                new Vector2(9.9f, -2.35f)
            }
        }
    };

    private GameHUD hud;
    private bool initialized;
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
    private static void EnsureSpawnerExists()
    {
        TryEnsureSpawnerForCurrentScene();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryEnsureSpawnerForCurrentScene();
    }

    private static void TryEnsureSpawnerForCurrentScene()
    {
        if (FindFirstObjectByType<EnemyWaveSpawner>() != null)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(activeScene.name) && activeScene.name != GameplaySceneName)
        {
            return;
        }

        if (FindFirstObjectByType<SaludJugador>() == null || FindFirstObjectByType<EnemigoIA>() == null)
        {
            return;
        }

        GameObject spawner = new GameObject("EnemyWaveSpawner_Runtime");
        spawner.AddComponent<EnemyWaveSpawner>();
    }

    private void Awake()
    {
        ReiniciarOleadas();
        ResolverReferencias();
        PrepararTemplate();
        initialized = enemyTemplate != null && jugador != null;
    }

    private void Update()
    {
        if (!initialized)
        {
            ResolverReferencias();
            PrepararTemplate();
            initialized = enemyTemplate != null && jugador != null;

            if (!initialized)
            {
                return;
            }
        }

        float playerX = jugador.position.x;
        for (int i = 0; i < waves.Length; i++)
        {
            EnemyWave wave = waves[i];
            if (wave == null || wave.spawned || playerX < wave.triggerX)
            {
                continue;
            }

            SpawnWave(wave, i + 1);
        }
    }

    private void ResolverReferencias()
    {
        if (jugador == null)
        {
            SaludJugador saludJugador = FindFirstObjectByType<SaludJugador>();
            jugador = saludJugador != null ? saludJugador.transform : null;
        }

        if (enemyTemplate == null)
        {
            EnemigoIA[] enemigos = FindObjectsByType<EnemigoIA>(FindObjectsSortMode.None);
            if (enemigos.Length > 0)
            {
                enemyTemplate = SeleccionarTemplate(enemigos);
            }
        }

        if (enemyParent == null && enemyTemplate != null)
        {
            enemyParent = enemyTemplate.transform.parent;
        }

        if (hud == null)
        {
            hud = FindFirstObjectByType<GameHUD>();
        }
    }

    private void PrepararTemplate()
    {
        if (!ocultarTemplateAlIniciar || enemyTemplate == null || !enemyTemplate.gameObject.activeSelf)
        {
            return;
        }

        RegistrarHUDSiHaceFalta();
        if (hud != null)
        {
            hud.UnregisterEnemy(enemyTemplate);
        }

        enemyTemplate.gameObject.name = "EnemyTemplate_Hidden";
        enemyTemplate.gameObject.SetActive(false);
    }

    private EnemigoIA SeleccionarTemplate(EnemigoIA[] enemigos)
    {
        for (int i = 0; i < enemigos.Length; i++)
        {
            EnemigoIA enemigo = enemigos[i];
            if (enemigo != null && enemigo.gameObject.activeSelf && !enemigo.name.Contains("_W"))
            {
                return enemigo;
            }
        }

        return enemigos.Length > 0 ? enemigos[0] : null;
    }

    private void SpawnWave(EnemyWave wave, int waveNumber)
    {
        wave.spawned = true;

        if (enemyTemplate == null || wave.posicionesSpawn == null)
        {
            return;
        }

        for (int i = 0; i < wave.posicionesSpawn.Length; i++)
        {
            Vector2 spawnPosition = wave.posicionesSpawn[i];
            GameObject clone = Instantiate(
                enemyTemplate.gameObject,
                new Vector3(spawnPosition.x, spawnPosition.y, 0f),
                Quaternion.identity,
                enemyParent
            );

            clone.name = $"Enemigo_Pandillero_Verde_W{waveNumber}_{i + 1}";
            clone.SetActive(true);

            EnemigoIA enemigo = clone.GetComponent<EnemigoIA>();
            if (enemigo != null)
            {
                RegistrarEnHUD(enemigo);
            }
        }
    }

    private void RegistrarEnHUD(EnemigoIA enemigo)
    {
        RegistrarHUDSiHaceFalta();

        if (hud != null)
        {
            hud.RegisterEnemy(enemigo);
        }
    }

    private void RegistrarHUDSiHaceFalta()
    {
        if (hud == null)
        {
            hud = FindFirstObjectByType<GameHUD>();
        }
    }

    private void ReiniciarOleadas()
    {
        if (waves == null)
        {
            return;
        }

        for (int i = 0; i < waves.Length; i++)
        {
            if (waves[i] != null)
            {
                waves[i].spawned = false;
            }
        }
    }
}

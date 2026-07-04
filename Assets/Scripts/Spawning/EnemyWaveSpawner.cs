using System;
using System.Collections.Generic;
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
    private int enemigosVivos;
    private readonly Dictionary<EnemigoIA, Action> deathHandlers = new Dictionary<EnemigoIA, Action>();
    private static bool sceneHookRegistered;

    public int EnemigosVivos => enemigosVivos;
    public bool TodasOleadasGeneradas => TodasLasOleadasFueronGeneradas();
    public bool TodasOleadasCompletadas => initialized && TodasOleadasGeneradas && enemigosVivos <= 0;

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

    private void OnDisable()
    {
        LimpiarEventosMuerte();
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
                RegistrarEnemigoVivo(enemigo);
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
        enemigosVivos = 0;
        LimpiarEventosMuerte();

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

    private void RegistrarEnemigoVivo(EnemigoIA enemigo)
    {
        if (enemigo == null || deathHandlers.ContainsKey(enemigo))
        {
            return;
        }

        enemigosVivos++;
        Action handler = () => RegistrarMuerteEnemigo(enemigo);
        deathHandlers.Add(enemigo, handler);
        enemigo.OnMuerto += handler;
    }

    private void RegistrarMuerteEnemigo(EnemigoIA enemigo)
    {
        if (enemigo == null || !deathHandlers.TryGetValue(enemigo, out Action handler))
        {
            return;
        }

        enemigo.OnMuerto -= handler;
        deathHandlers.Remove(enemigo);
        enemigosVivos = Mathf.Max(0, enemigosVivos - 1);
    }

    private void LimpiarEventosMuerte()
    {
        foreach (KeyValuePair<EnemigoIA, Action> pair in deathHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.OnMuerto -= pair.Value;
            }
        }

        deathHandlers.Clear();
    }

    private bool TodasLasOleadasFueronGeneradas()
    {
        if (waves == null || waves.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < waves.Length; i++)
        {
            if (waves[i] != null && !waves[i].spawned)
            {
                return false;
            }
        }

        return true;
    }
}

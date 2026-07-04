using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalDemoController : MonoBehaviour
{
    private const string GameplaySceneName = "SampleScene";

    [SerializeField] private float finalTriggerX = 13.8f;

    private SaludJugador jugador;
    private EnemyWaveSpawner spawner;
    private bool finalMostrado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureFinalControllerExists()
    {
        TryEnsureForCurrentScene();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryEnsureForCurrentScene();
    }

    private static void TryEnsureForCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(activeScene.name) && activeScene.name != GameplaySceneName)
        {
            return;
        }

        if (FindAnyObjectByType<FinalDemoController>() != null)
        {
            return;
        }

        if (FindAnyObjectByType<SaludJugador>() == null)
        {
            return;
        }

        GameObject controller = new GameObject("FinalDemoController_Runtime");
        controller.AddComponent<FinalDemoController>();
    }

    private void Update()
    {
        if (finalMostrado || GameOverMenuView.GameOverActivo || AlphaCompletionView.Activo)
        {
            return;
        }

        ResolverReferencias();
        if (jugador == null || spawner == null || jugador.EstaMuerto)
        {
            return;
        }

        if (jugador.transform.position.x >= finalTriggerX && spawner.TodasOleadasCompletadas)
        {
            finalMostrado = true;
            AlphaCompletionView.Show();
        }
    }

    private void ResolverReferencias()
    {
        if (jugador == null)
        {
            jugador = FindAnyObjectByType<SaludJugador>();
        }

        if (spawner == null)
        {
            spawner = FindAnyObjectByType<EnemyWaveSpawner>();
        }
    }
}

using UnityEngine;

public static class CombatAudioPlayer
{
    private const int SampleRate = 22050;
    private const float TwoPi = 6.28318530718f;

    private static AudioClip playerHitFallback;
    private static AudioClip enemyHitFallback;

    public static void PlayPlayerHit(Vector3 position, AudioClip overrideClip = null)
    {
        Play(overrideClip != null ? overrideClip : GetPlayerHitFallback(), position, 0.8f);
    }

    public static void PlayEnemyHit(Vector3 position, AudioClip overrideClip = null)
    {
        Play(overrideClip != null ? overrideClip : GetEnemyHitFallback(), position, 0.7f);
    }

    private static AudioClip GetPlayerHitFallback()
    {
        if (playerHitFallback == null)
        {
            playerHitFallback = CreateHitClip("Runtime_PlayerHit", 0.09f, 185f, 72f, 0.22f);
        }

        return playerHitFallback;
    }

    private static AudioClip GetEnemyHitFallback()
    {
        if (enemyHitFallback == null)
        {
            enemyHitFallback = CreateHitClip("Runtime_EnemyHit", 0.075f, 285f, 110f, 0.16f);
        }

        return enemyHitFallback;
    }

    private static void Play(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    private static AudioClip CreateHitClip(string clipName, float duration, float startFrequency, float endFrequency, float noiseAmount)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = sampleCount <= 1 ? 1f : (float)i / (sampleCount - 1);
            float frequency = startFrequency + (endFrequency - startFrequency) * t;
            float envelope = (1f - t) * (1f - t);
            float grit = Mathf.Sin(i * 17.17f) * Mathf.Sin(i * 0.073f);

            phase += TwoPi * frequency / SampleRate;
            samples[i] = (Mathf.Sin(phase) * 0.82f + grit * noiseAmount) * envelope;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

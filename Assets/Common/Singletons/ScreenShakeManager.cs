using Assets.Helpers;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenShakeManager : MonoBehaviour
{
    const float MagnitudeDecaySpeed_UnitPerSec = 0.75f;
    const float SpiralShakeRotationSpeed_DegPerSec = 360 * Helper.Phi * 30;
    const float DirectionalShakeSpeed_CyclePerSec = 30;
    const float MagnitudeThresholdToConsiderAsFreeSlot = 0.0001f;

    static ScreenShakeManager instance;

    //will use as pool for shake data
    //increase length when needed, don't decrease length.
    //reset to 4 on scene change

    //SPIRAL SHAKE DATA
    float[] spiralShakeMagnitudes;
    float[] spiralShakeRotations;

    //DIRECTIONAL SHAKE DATA
    float[] directionalShakeMagnitudes;
    Vector2[] directionalShakeDirections;
    float[] directionalShakeTimers;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            Initialize();
            DontDestroyOnLoad(gameObject);
        }
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        Initialize();
    }

    static void Initialize()
    {
        instance.spiralShakeMagnitudes = new float[4];
        instance.spiralShakeRotations = new float[4];

        instance.directionalShakeMagnitudes = new float[4];
        instance.directionalShakeDirections = new Vector2[4];
        instance.directionalShakeTimers = new float[4];
    }
    public static void AddDirectionalShake(Vector2 direction, float shakeMagnitude)
    {
        direction.Normalize();
        int length = instance.directionalShakeMagnitudes.Length;
        for (int i = 0; i < length; i++)
        {
            if (instance.directionalShakeMagnitudes[i] < MagnitudeThresholdToConsiderAsFreeSlot)
            {
                instance.directionalShakeMagnitudes[i] = shakeMagnitude;
                instance.directionalShakeDirections[i] = direction;
                instance.directionalShakeTimers[i] = 0;
                return;
            }
        }
        length++;
        Array.Resize(ref instance.directionalShakeDirections, length);
        Array.Resize(ref instance.directionalShakeMagnitudes, length);
        Array.Resize(ref instance.directionalShakeTimers, length);
        length--;
        instance.directionalShakeMagnitudes[length] = shakeMagnitude;
        instance.directionalShakeDirections[length] = direction;
        instance.directionalShakeTimers[length] = 0;
    }
    public static void AddSpiralShake(float shakeMagnitude)
    {
        int length = instance.spiralShakeMagnitudes.Length;
        for (int i = 0; i < length; i++)
        {
            if (instance.spiralShakeMagnitudes[i] < MagnitudeThresholdToConsiderAsFreeSlot)
            {
                instance.spiralShakeMagnitudes[i] = shakeMagnitude;
                instance.spiralShakeRotations[i] = Random2.Float(Helper.Tau);
                return;
            }
        }
        length++;
        Array.Resize(ref instance.spiralShakeRotations, length);
        Array.Resize(ref instance.spiralShakeMagnitudes, length);
        length--;
        instance.spiralShakeRotations[length] = Random2.Float(Helper.Tau);
        instance.spiralShakeMagnitudes[length] = shakeMagnitude;
    }

    public static void AddTinyShake()
    {
        AddDirectionalShake(Vector2.up, .075f);
    }
    public static void AddSmallShake()
    {
        AddDirectionalShake(Vector2.up, .1f);
    }
    public static void AddMediumShake()
    {
        AddSpiralShake(.15f);
    }
    public static void AddLargeShake()
    {
        AddSpiralShake(0.25f);
    }
    public static Vector2 GetCameraOffset()
    {
        Vector2 finalShakeOffset = Vector2.zero;
        for (int i = 0; i < instance.spiralShakeRotations.Length; i++)
        {
            float magnitude = instance.spiralShakeMagnitudes[i];
            if (magnitude < MagnitudeThresholdToConsiderAsFreeSlot)//basically no shake
            {
                continue;
            }
            float rotation = instance.spiralShakeRotations[i];
            finalShakeOffset.x += Mathf.Cos(rotation) * magnitude;
            finalShakeOffset.y += Mathf.Sin(rotation) * magnitude;
        }
        for (int i = 0; i < instance.directionalShakeMagnitudes.Length; i++)
        {
            float magnitude = instance.directionalShakeMagnitudes[i];
            if (magnitude < MagnitudeThresholdToConsiderAsFreeSlot)//basically no shake
            {
                continue;
            }
            float timer = instance.directionalShakeTimers[i];
            Vector2 direction = instance.directionalShakeDirections[i];
            finalShakeOffset += magnitude * Mathf.Sin(timer) * direction;
        }
        return finalShakeOffset;
    }
    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        int length = instance.spiralShakeMagnitudes.Length;
        for (int i = 0; i < length; i++)
        {
            instance.spiralShakeMagnitudes[i] -= dt * MagnitudeDecaySpeed_UnitPerSec;
            if (instance.spiralShakeMagnitudes[i] < 0)
            {
                instance.spiralShakeMagnitudes[i] = 0;
            }
            instance.spiralShakeRotations[i] += dt * SpiralShakeRotationSpeed_DegPerSec;
        }
        length = instance.directionalShakeMagnitudes.Length;
        for (int i = 0; i < length; i++)
        {
            instance.directionalShakeMagnitudes[i] -= dt * MagnitudeDecaySpeed_UnitPerSec;
            if (instance.directionalShakeMagnitudes[i] < 0)
            {
                instance.directionalShakeMagnitudes[i] = 0;
            }
            instance.directionalShakeTimers[i] += dt * DirectionalShakeSpeed_CyclePerSec * Helper.Tau;
        }
    }
}

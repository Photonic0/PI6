using System;
using UnityEngine;

namespace Assets.DataStructures
{
    public class Counter
    {
        float accumulatedDeltaTime;
        float totalDuration;
        public Counter(float totalDuration, float startingValue = 0)
        {
            this.totalDuration = totalDuration;
            accumulatedDeltaTime = startingValue;
        }
        public void Update()
        {
            accumulatedDeltaTime += UnityEngine.Time.deltaTime;
        }
        public void Reset(float totalDuration, float startingValue)
        {
            this.totalDuration = totalDuration;
            accumulatedDeltaTime = startingValue;
        }
        public float Time => accumulatedDeltaTime;
        public bool Ended => accumulatedDeltaTime >= totalDuration;
        public float Progress => accumulatedDeltaTime / totalDuration;
        public float ProgressClamped => Mathf.Clamp01(accumulatedDeltaTime / totalDuration);
        public Vector2 Lerp(Vector2 v1, Vector2 v2) => Vector2.Lerp(v1, v2, Progress);
        public Vector3 Lerp(Vector3 v1, Vector3 v2) => Vector3.Lerp(v1, v2, Progress);
        public float Lerp(float f1, float f2) => Mathf.Lerp(f1, f2, Progress);
        public float LerpUnclamped(float f1, float f2) => Mathf.LerpUnclamped(f1, f2, Progress);
        public Color Lerp(Color c1, Color c2) => Color.Lerp(c1, c2, Progress);
        public Color LerpUnclamped(Color c1, Color c2) => Color.LerpUnclamped(c1, c2, Progress);
        public Vector2 Lerp(Vector2 v1, Vector2 v2, Func<float, float> easing) => Vector2.Lerp(v1, v2, easing(ProgressClamped));
        public Vector3 Lerp(Vector3 v1, Vector3 v2, Func<float, float> easing) => Vector3.Lerp(v1, v2, easing(ProgressClamped));
        public float Lerp(float f1, float f2, Func<float, float> easing) => Mathf.Lerp(f1, f2, easing(ProgressClamped));
        public float LerpUnclamped(float f1, float f2, Func<float, float> easing) => Mathf.LerpUnclamped(f1, f2, easing(Progress));
        public Color Lerp(Color c1, Color c2, Func<float, float> easing) => Color.Lerp(c1, c2, easing(ProgressClamped));
        public Color LerpUnclamped(Color c1, Color c2, Func<float, float> easing) => Color.LerpUnclamped(c1, c2, easing(Progress));
    }
}

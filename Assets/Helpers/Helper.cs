using Assets.Common.Consts;
using System;
using UnityEngine;

namespace Assets.Helpers
{
    public static class Helper
    {
        public const float Tau = Mathf.PI * 2;
        public const float Phi = 1.61803398875f;
        public const float CamerasZPos = -8.660254f;
        public static Vector2 MouseWorld
        {
            get
            {
                Camera cam = Camera.main;
                Vector3 posToCheck = Input.mousePosition;
                posToCheck.z = -CamerasZPos;
                return (Vector2)cam.ScreenToWorldPoint(posToCheck);
            }
        }
        public static bool PointInView(Vector2 pos, int padding = 2)
        {
            Vector2 topLeft = GameManager.CurrentCamTransform.position;
            topLeft.y += padding + 5;
            topLeft.x -= padding + 9;
            Vector2 bottomRight = topLeft;
            padding += padding;
            bottomRight.x += padding + 18;
            bottomRight.y -= padding + 10;
            return pos.x >= topLeft.x && pos.x <= bottomRight.x && pos.y <= topLeft.y && pos.y >= bottomRight.y;
        }
        public static bool EnemyAggroCheck(Vector3 enemyPos, Vector3 playerPos, float aggroRange, float verticalRange = 7)
        {
            return Mathf.Abs(enemyPos.x - playerPos.x) < aggroRange && Mathf.Abs(enemyPos.y - playerPos.y) < verticalRange;
        }
        public static Vector3 Remap(float fromValue, float fromMin, float fromMax, Vector3 toMin, Vector3 toMax, bool clamped = true)
        {
            float t = (fromValue - fromMin) / (fromMax - fromMin);
            if (clamped)
            {
                t = Mathf.Clamp01(t);
            }
            return Vector3.LerpUnclamped(toMin, toMax, t);
        }
        public static float Remap(float fromValue, float fromMin, float fromMax, float toMin, float toMax, bool clamped = true)
        {
            float t = (fromValue - fromMin) / (fromMax - fromMin);
            if (clamped)
            {
                t = Mathf.Clamp01(t);
            }
            return Mathf.LerpUnclamped(toMin, toMax, t);
        }
        public static float Remap(float fromValue, float fromMin, float fromMax, float toMin, float toMax, Func<float, float> easing, bool clamped = true)
        {
            float t = (fromValue - fromMin) / (fromMax - fromMin);
            if (clamped)
            {
                t = Mathf.Clamp01(t);
            }
            t = easing(t);
            return Mathf.LerpUnclamped(toMin, toMax, t);
        }
        public static Color Remap(float fromValue, float fromMin, float fromMax, Color toMin, Color toMax, bool clamped = true)
        {
            float t = (fromValue - fromMin) / (fromMax - fromMin);
            if (clamped)
            {
                t = Mathf.Clamp01(t);
            }
            return Color.LerpUnclamped(toMin, toMax, t);
        }
        public static Color Remap(float fromValue, float fromMin, float fromMax, Color toMin, Color toMax, Func<float, float> easing, bool clamped = true)
        {
            float t = (fromValue - fromMin) / (fromMax - fromMin);
            if (clamped)
            {
                t = Mathf.Clamp01(t);
            }
            t = easing(t);
            return Color.LerpUnclamped(toMin, toMax, t);
        }
        public static Color Remap(float fromValue, float fromMin, float fromMax, Color toMin, Color toMax, Func<float, float, float> easing, float exponentForEasing = 2, bool clamped = true)
        {
            float t = (fromValue - fromMin) / (fromMax - fromMin);
            if (clamped)
            {
                t = Mathf.Clamp01(t);
            }
            t = easing(t, exponentForEasing);
            return Color.LerpUnclamped(toMin, toMax, t);
        }
        public static Quaternion ToRotation(this Vector2 v)
        {
            return Quaternion.Euler(0, 0, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
        }
        public static Quaternion ToRotation(this Vector2 v, float extraRotationDegrees)
        {
            return Quaternion.Euler(0, 0, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + extraRotationDegrees);
        }
        public static float Atan2Deg(this Vector2 v, float extraRotationDegrees)
        {
            return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + extraRotationDegrees;
        }
        public static float Atan2Deg(this Vector2 v)
        {
            return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        }
        /// <summary>
        /// expects radians
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="magnitude"></param>
        /// <returns></returns>
        public static Vector2 PolarVector(this float angle, float magnitude)
        {
            return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * magnitude;
        }
        /// <summary>
        /// expects radians
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector2 PolarVector(this float angle)
        {
            return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        }
        public static Vector2 RotatedBy(this Vector2 v, float radians, Vector2 center = default)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            Vector2 relative = v - center;
            center.x += relative.x * cos - relative.y * sin;
            center.y += relative.x * sin + relative.y * cos;
            return center;
        }
        //public static void RefreshIndices<T>(ref List<T> list, int startIndex = 0) where T : Entity2D
        //{
        //    for (int i = startIndex; i < list.Count; i++)
        //    {
        //        list[i].index = i;
        //    }
        //}
        public static bool IndexInRange(this Array array, int index)
        {
            return index >= 0 && index < array.Length;
        }
        public static Vector2 ClampVector(Vector2 min, Vector2 max, Vector2 value)
        {
            if (value.x < min.x)
            {
                value.x = min.x;
            }
            if (value.y < min.y)
            {
                value.y = min.y;
            }
            if (value.x > max.x)
            {
                value.x = max.x;
            }
            if (value.y > max.y)
            {
                value.y = max.y;
            }
            return value;
        }
        public static Vector3 ClampVector(Vector3 min, Vector3 max, Vector3 value)
        {
            if (value.x < min.x)
            {
                value.x = min.x;
            }
            if (value.y < min.y)
            {
                value.y = min.y;
            }
            if (value.z < min.z)
            {
                value.z = min.z;
            }
            if (value.x > max.x)
            {
                value.x = max.x;
            }
            if (value.y > max.y)
            {
                value.y = max.y;
            }
            if (value.z > max.z)
            {
                value.z = max.z;
            }
            return value;
        }
        public static Color InvertedRGB(this Color col)
        {
            col.r = 1 - col.r;
            col.g = 1 - col.g;
            col.b = 1 - col.b;
            return col;
        }


        //bad
        public static float WeightedRandom(params (float input, float chance)[] inputsChances)
        {
            //normalize the values
            float sum = 0;
            for (int i = 0; i < inputsChances.Length; i++)
            {
                sum += inputsChances[i].chance;
            }
            for (int i = 0; i < inputsChances.Length; i++)
            {
                inputsChances[i].chance /= sum;
            }

            float roll = UnityEngine.Random.value;
            float currentChance = 0;
            for (int i = 0; i < inputsChances.Length; i++)
            {
                currentChance += inputsChances[i].chance;
                if (roll < currentChance)
                {
                    return inputsChances[i].input;
                }
            }
            return inputsChances[^1].input;
        }
        public static bool TryFindFreeIndex<T>(T[] objects, out int index) where T : MonoBehaviour
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (!objects[i].gameObject.activeInHierarchy)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        public static int FindFreeIndex<T>(T[] objects) where T : MonoBehaviour
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (!objects[i].gameObject.activeInHierarchy)
                {
                    return i;
                }
            }
            return -1;
        }
        public static bool TryFindFreeIndex(GameObject[] objects, out int index)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (!objects[i].activeInHierarchy)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        public static bool TryFindFreeIndex(GameObject[] objects, out GameObject obj)
        {

            for (int i = 0; i < objects.Length; i++)
            {
                if (!objects[i].activeInHierarchy)
                {
                    obj = objects[i];
                    return true;
                }
            }
            obj = null;
            return false;
        }
        public static int FindFreeIndex(GameObject[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (!objects[i].activeInHierarchy)
                {
                    return i;
                }
            }
            return -1;
        }
        public static Color OffsetHueBy(this Color c, float normalizedAmount)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            h += normalizedAmount;
            h = Mathf.Repeat(h, 1);
            Color result = Color.HSVToRGB(h, s, v);
            result.a = c.a;
            return result;
        }
        /// <summary>
        /// lower decay value = slower movement
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="decay"></param>
        /// <returns></returns>
        public static float Decay(float currentValue, float targetValue, float decay)
        {
            return targetValue + (currentValue - targetValue) * Mathf.Exp(-decay * Time.deltaTime);
        }
        public static Quaternion Decay(Quaternion currentValue, Quaternion targetValue, float decay)
        {
            return Quaternion.LerpUnclamped(currentValue, targetValue, Mathf.Exp(-decay * Time.deltaTime));
        }
        public static Vector2 Decay(Vector2 currentValue, Vector2 targetValue, float decay)
        {
            return targetValue + (currentValue - targetValue) * Mathf.Exp(-decay * Time.deltaTime);
        }
        //unity has implicit conversion between vec3 and vec2 so I have to do this to prevent ambiguity in some calls -.-
        public static Vector3 DecayVec3(Vector3 currentValue, Vector3 targetValue, float decay)
        {
            return targetValue + (currentValue - targetValue) * Mathf.Exp(-decay * Time.deltaTime);
        }
        public static bool TileCollision(Collision2D collision)
        {
            return collision.gameObject.CompareTag(Tags.Tiles);
        }
        /// <summary>
        /// miliseconds to fps
        /// </summary>
        public static float MsToFps(float milliseconds) => 1000.0f / milliseconds;
        public static void TelegraphLightning(float timer, Vector2 pointA, Vector2 pointB, float telegraphDuration, ParticleSystem lightningTelegraphParticles, float timerOffset = -0.2f)
        {
            if (lightningTelegraphParticles != null)
            {
                float chancePer45thSec = .5f;
                Vector2 node1 = pointA;
                Vector2 node2 = pointB;
                float timeLeftUntilActivation = telegraphDuration - timer - timerOffset;
                float sizeIncrease = Remap(timer, 0, telegraphDuration - timerOffset, 0, 0.25f);

                if (Random2.Percent(chancePer45thSec, 45))
                {
                    ParticleSystem.EmitParams emitParams = new();
                    Vector2 deltaPos = node2 - node1;
                    float lifetime = Random2.Float(.2f, .45f);
                    emitParams.position = node1 + Random2.Circular(0.1f);
                    emitParams.velocity = deltaPos / (1.7f * lifetime) + Random2.Circular(1);
                    emitParams.startLifetime = Mathf.Min(lifetime, timeLeftUntilActivation);
                    emitParams.startColor = FlipnoteColors.Yellow;
                    emitParams.startSize = Random2.Float(.01f, .11f) + sizeIncrease;
                    lightningTelegraphParticles.Emit(emitParams, 1);
                }
                if (Random2.Percent(chancePer45thSec, 45))
                {
                    ParticleSystem.EmitParams emitParams = new();
                    Vector2 deltaPos = node1 - node2;
                    float lifetime = Random2.Float(.2f, .45f);
                    emitParams.position = node2 + Random2.Circular(0.1f);
                    emitParams.velocity = deltaPos / (1.7f * lifetime) + Random2.Circular(1);
                    emitParams.startLifetime = Mathf.Min(lifetime, timeLeftUntilActivation);
                    emitParams.startColor = FlipnoteColors.Yellow;
                    emitParams.startSize = Random2.Float(.01f, .11f) + sizeIncrease;
                    lightningTelegraphParticles.Emit(emitParams, 1);
                }

            }
        }
        public static Vector2[] ConvertArray(Vector3[] array)
        {
            Vector2[] result = new Vector2[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i];
            }
            return result;
        }
        public static Vector3[] ConvertArray(Vector2[] array)
        {
            Vector3[] result = new Vector3[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i];
            }
            return result;
        }
    }
}

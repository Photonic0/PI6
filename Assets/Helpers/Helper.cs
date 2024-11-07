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
        public static Vector2 MouseWorld { get 
            {
                Camera cam = Camera.main;
                Vector3 posToCheck = Input.mousePosition;
                posToCheck.z = -CamerasZPos;
                return (Vector2)cam.ScreenToWorldPoint(posToCheck); 
            } }
        public static bool EnemyAggroCheck(Vector3 enemyPos, Vector3 playerPos, float aggroRange, float verticalRange = 8)
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
            if(value.z < min.z)
            {
                value.z = min.z;
            }
            if(value.x > max.x)
            {
                value.x = max.x;
            }
            if(value.y > max.y)
            {
                value.y = max.y;
            }
            if(value.z > max.z)
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
                if(roll < currentChance)
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

        public static Color OffsetHueBy(this Color c, float normalizedAmount)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            h += normalizedAmount;
            h = Mathf.Repeat(h, 1);
            Color result =  Color.HSVToRGB(h, s, v);
            result.a = c.a;
            return result;
        }
        /// <summary>
        /// lower decay value = slower movement
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="decay"></param>
        /// <returns></returns>
        public static float Decay(float a, float b, float decay)
        {
            return b + (a - b) * Mathf.Exp(-decay * Time.deltaTime);
        }
        public static bool TileCollision(Collision2D collision)
        {
            return collision.gameObject.CompareTag(Tags.Tiles);
        }
        /// <summary>
        /// miliseconds to fps
        /// </summary>
        public static float MsToFps(float milliseconds) => 1000.0f / milliseconds;
    }
}

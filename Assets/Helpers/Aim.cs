//using Assets.Interfaces;
using System;
using UnityEngine;

namespace Assets.Helpers
{
    public static class Aim
    {
        public struct ChaseResults
        {
            public bool InterceptionHappens;

            public Vector2 InterceptionPosition;

            public float InterceptionTime;

            public Vector2 ChaserVelocity;
        }
        //public static ChaseResults GetChaseResults(Vector2 chaserPosition, float chaserSpeed, ITargetable target)
        //{
        //    if (target == null)
        //    {
        //        return default;
        //    }
        //    return GetChaseResults(chaserPosition, chaserSpeed, target.Position, target.Velocity);
        //}
        public static ChaseResults GetChaseResults(Vector2 chaserPosition, float chaserSpeed, Vector2 runnerPosition, Vector2 runnerVelocity)
        {
            ChaseResults result = default;
            if (chaserPosition == runnerPosition)
            {
                ChaseResults result2 = default;
                result2.InterceptionHappens = true;
                result2.InterceptionPosition = chaserPosition;
                result2.InterceptionTime = 0f;
                result2.ChaserVelocity = Vector2.zero;
                return result2;
            }
            if (chaserSpeed <= 0f)
            {
                return default;
            }
            Vector2 toChaser = chaserPosition - runnerPosition;
            float distToTunner = toChaser.magnitude;
            float runnerVelocityMagnitude = runnerVelocity.magnitude;
            if (runnerVelocityMagnitude == 0f)
            {
                result.InterceptionTime = distToTunner / chaserSpeed;
                result.InterceptionPosition = runnerPosition;
            }
            else
            {
                float a = chaserSpeed * chaserSpeed - runnerVelocityMagnitude * runnerVelocityMagnitude;
                float b = 2f * Vector2.Dot(toChaser, runnerVelocity);
                float c = -distToTunner * distToTunner;
                if (!SolveQuadratic(a, b, c, out float x1, out float x2))
                {
                    return default;
                }
                if (x1 < 0f && x2 < 0f) // if both solutions < 0
                {
                    return default;
                }
                if (x1 > 0f && x2 > 0f)//if both solutions > 0
                {
                    result.InterceptionTime = Math.Min(x1, x2);
                }
                else // if only 1 solution > 0
                {
                    result.InterceptionTime = Math.Max(x1, x2);
                }
                result.InterceptionPosition = runnerPosition + runnerVelocity * result.InterceptionTime;
            }
            result.ChaserVelocity = (result.InterceptionPosition - chaserPosition) / result.InterceptionTime;
            result.InterceptionHappens = true;
            return result;
        }
        private static bool SolveQuadratic(float a, float b, float c, out float result1, out float result2)
        {
            float delta = b * b - 4f * a * c;
            result1 = 0f;
            result2 = 0f;
            a *= 2;
            if (delta > 0f)
            {
                delta = MathF.Sqrt(delta);
                result1 = (delta - b) / a;
                result2 = (- b - delta) / a;
                return true;
            }
            if (delta < 0f)
            {
                return false;
            }
            result1 = result2 = (Mathf.Sqrt(delta) - b) / a;
            return true;
        }
    }
}

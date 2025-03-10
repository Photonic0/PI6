﻿using System;

namespace Assets.Systems
{
    public static class Easings
    {
        public static float InBack(float x)
        {
            return 2.70158f * x * x * x - 1.70158f * x * x;
        }


        public static float SqrInOut(float t)
        {
            if (t < 0.5f)
            {
                return 2 * t * t;
            }
            else
            {
                return -2 * t * t + 4 * t - 1;
            }
        }
        public static float Out(float progress, float exponent)
        {
            progress = 1 - progress;
            progress = MathF.Pow(progress, exponent);
            return 1 - progress;
        }
        public static float In(float progress, float exponent)
        {
            return MathF.Pow(progress, exponent);
        }
        public static float SqrIn(float progress)
        {
            return progress * progress;
        }
        public static float SqrOut(float progress)
        {
            progress = 1 - progress;
            progress *= progress;
            return 1 - progress;
        }
        public static float CubeIn(float progress)
        {
            return progress * progress * progress;
        }
        public static float CubeOut(float progress)
        {
            progress = 1 - progress;
            progress *= progress * progress;
            return 1 - progress;
        }
        public static float QuadIn(float progress)
        {
            return progress * progress * progress * progress;
        }
        public static float InOutSineClamped(float progress)
        {
            if (progress < 0)
                progress = 0;
            else if (progress > 1)
                progress = 1;
            return 0.5f - 0.5f * MathF.Cos(progress * MathF.PI);
        }
    }
}

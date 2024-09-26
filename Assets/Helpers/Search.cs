//using Assets.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Systems
{
    public static class Search
    {
        /// <returns>if pos1 is closer to the center than pos2 is</returns>
        public static bool CompareDistance(Vector2 pos1, Vector2 pos2, Vector2 center)
        {
            return Vector2.Distance(pos1, center) < Vector2.Distance(pos2, center);
        }
        //public static ITargetable Closest<T>(ref List<T> targets, Vector2 pos) where T : ITargetable
        //{
        //    ITargetable result = null;
        //    if (targets.Count <= 0)
        //    {
        //        return result;
        //    }
        //    for (int i = 0; i < targets.Count; i++)
        //    {
        //        ITargetable compare = targets[i];
        //        if (compare.Targetable && (result == null || CompareDistance(compare.Position, result.Position, pos)))
        //        {
        //            result = compare;
        //        }
        //    }
        //    return result;
        //}
        //public static ITargetable Closest<T>(ref T[] targets, Vector2 pos) where T : ITargetable
        //{
        //    ITargetable result = null;
        //    if (targets.Length <= 0)
        //    {
        //        return result;
        //    }
        //    for (int i = 0; i < targets.Length; i++)
        //    {
        //        ITargetable compare = targets[i];
        //        if (compare.Targetable && (result == null || CompareDistance(compare.Position, result.Position, pos)))
        //        {
        //            result = compare;
        //        }
        //    }
        //    return result;
        //}
        //public static ITargetable Closest<T>(ref T[] targets, Vector2 pos, int ignoreIndex) where T : ITargetable
        //{
        //    ITargetable result = null;
        //    if (targets.Length <= 0)
        //    {
        //        return result;
        //    }
        //    for (int i = 0; i < targets.Length; i++)
        //    {
        //        if (i == ignoreIndex)
        //            continue;
        //        ITargetable compare = targets[i];
        //        if (compare.Targetable && (result == null || CompareDistance(compare.Position, result.Position, pos)))
        //        {
        //            result = compare;
        //        }
        //    }
        //    return result;
        //}
    }
}

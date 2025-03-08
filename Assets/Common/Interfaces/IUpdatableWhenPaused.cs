using UnityEngine;

namespace Assets.Common.Interfaces
{
    public interface IUpdatableWhenPaused
    {
        GameObject GameObject { get; }
        bool IsNull { get; }
        void PausedUpdate(float unscaledDeltaTime);
    }
}
using UnityEngine;

namespace Assets.Common.Interfaces
{
    public interface IUpdatableWhenPaused
    {
        GameObject GameObject { get; }
        void PausedUpdate(float unscaledDeltaTime);
    }
}
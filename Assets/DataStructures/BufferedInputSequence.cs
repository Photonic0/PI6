using System.Collections.Generic;
using UnityEngine;

namespace Assets.Systems
{
    public class BufferedInputSequence
    {
        public BufferedInputSequence(float timeToStoreInputInSeconds, KeyCode[] keysToWatchFor)
        {
            this.keysToWatchFor = keysToWatchFor;
            timeToStoreInput = timeToStoreInputInSeconds;
        }
        readonly List<BufferedInput> pressedKeys = new();
        readonly float timeToStoreInput;
        readonly KeyCode[] keysToWatchFor;
        void WatchForKeysDown()
        {
            if (Input.anyKeyDown)
            {
                for (int i = 0; i < keysToWatchFor.Length; i++)
                {
                    KeyCode pressedKey = keysToWatchFor[i];
                    if (Input.GetKeyDown(pressedKey))
                        pressedKeys.Add(new(pressedKey, timeToStoreInput));
                }
            }
        }
        void Update()
        {
            for (int i = 0; i < pressedKeys.Count; i++)
            {
                pressedKeys[i].Update();
                if (pressedKeys[i].timeLeft <= 0)
                {
                    pressedKeys.RemoveAt(i);
                    i--;
                }
            }
        }
        public void WatchForKeysDownAndUpdate()
        {
            WatchForKeysDown();
            Update();
        }
        public bool GetBufferedKeyDown(KeyCode keyToCheck)
        {
            if (pressedKeys.Count <= 0)
                return false;
            for (int i = 0; i < pressedKeys.Count; i++)
            {
                BufferedInput pressedKey = pressedKeys[i];
                if (pressedKey.key == keyToCheck)
                {
                    pressedKeys.Remove(pressedKey);
                    return true;
                }
            }
            return false;
        }
        public bool GetBufferedKeyDown(KeyCode keyToCheck, out KeyRemovalRequest keyRemoval)
        {
            keyRemoval = null;
            if (pressedKeys.Count <= 0)
                return false;
            for (int i = 0; i < pressedKeys.Count; i++)
            {
                BufferedInput pressedKey = pressedKeys[i];
                if (pressedKey.key == keyToCheck)
                {
                    keyRemoval = new(i, this);
                    return true;
                }
            }
            return false;
        }
        public bool HasKey(KeyCode key)
        {
            for (int i = 0; i < pressedKeys.Count; i++)
            {
                BufferedInput pressedKey = pressedKeys[i];
                if (pressedKey.key == key)
                    return true;
            }
            return false;
        }
        public class KeyRemovalRequest
        {
            readonly int index;
            readonly BufferedInputSequence inputBuffer;
            public KeyRemovalRequest(int index, BufferedInputSequence originBuffer)
            {
                this.index = index;
                inputBuffer = originBuffer;
            }
            public void ConfirmKeyRemoval()
            {
                inputBuffer.pressedKeys.RemoveAt(index);
            }
        }
        private class BufferedInput
        {
            public KeyCode key;
            public float timeLeft;
            public BufferedInput(KeyCode key, float time)
            {
                this.key = key;
                timeLeft = time;
            }
            public void Update()
            {
                timeLeft -= Time.deltaTime;
            }
        }
    }
}


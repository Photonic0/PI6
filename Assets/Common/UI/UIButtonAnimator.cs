using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonAnimator : MonoBehaviour
{
    float timer;
    [SerializeField] Image buttonImage;
    [SerializeField] Sprite[] frames;
    int currentFrameIndex;
    void Start()
    {
        currentFrameIndex = 0;
        timer = 0;
    }
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > 0.1f)
        {
            timer %= 0.1f;
            currentFrameIndex++;
            currentFrameIndex %= frames.Length;
            buttonImage.sprite = frames[currentFrameIndex];
        }
    }
}

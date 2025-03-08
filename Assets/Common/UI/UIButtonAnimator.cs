using Assets.Common.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonAnimator : MonoBehaviour, IUpdatableWhenPaused
{
    float timer;
    [SerializeField] Image buttonImage;
    [SerializeField] Sprite[] frames;
    int currentFrameIndex;
    [SerializeField] new GameObject gameObject;
    public GameObject GameObject => gameObject;
    public bool IsNull => this == null || gameObject == null;
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
            timer -= 0.1f;
            currentFrameIndex++;
            currentFrameIndex %= frames.Length;
            buttonImage.sprite = frames[currentFrameIndex];
        }
    }

    public void PausedUpdate(float unscaledDeltaTime)
    {
        timer += unscaledDeltaTime;
        if (timer > 0.1f)
        {
            timer %= 0.1f;
            currentFrameIndex++;
            currentFrameIndex %= frames.Length;
            buttonImage.sprite = frames[currentFrameIndex];
        }
    }
    private void OnDestroy()
    {
       GameManager.RemoveFromPausedUpdateObjs(this);
    }
}

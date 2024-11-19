using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoBossBallProj : MonoBehaviour
{
    VerletSimulator rope;
    private void Awake()
    {
        Vector2 ballTopPos = anchor.position;
        rope = new(1, 5);
        int dotCount = 40;
        List<Dot> dots = new(dotCount);
        rope.dots = dots;
        Dot firstDot = new(anchor.position, true);
        dots.Add(firstDot);
        firstDot.isLocked = true;
        for (int i = 1; i < dotCount; i++)
        {
            Dot dot = new(ballTopPos + new Vector2(.71f, .70f) * (i - 1) / (ballDist * dotCount), false);
            Dot.Connect(dot, dots[i - 1]);
            dots.Add(dot);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

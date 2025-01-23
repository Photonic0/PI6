using System.Collections.Generic;
using UnityEngine;

public class VerletSimulator
{
    public List<Dot> dots;
    public float mass;
    private Vector2 currentForce;
    public int iterations;
    public VerletSimulator(float mass, int iterations)
    {
        this.mass = mass;
        this.iterations = iterations;
    }
    public void AddForce(Vector2 force)
    {
        currentForce += force;
    }
    public void Simulate(float deltaTime)
    {
        ApplyPhysicsToDots(deltaTime);
        ConstrainLength();
    }

    private void ConstrainLength()
    {
        for (int k = 0; k < iterations; k++)
        {
            for (int i = 0; i < dots.Count; i++)
            {
                Dot dotA = dots[i];
                for (int j = 0; j < dotA.connections.Count; j++)
                {
                    DotConnection connection = dotA.connections[j];
                    Dot dotB = connection.Other(dotA);
                    Vector2 center = (dotA.position + dotB.position) * 0.5f;
                    Vector2 direction = (dotA.position - dotB.position).normalized;
                    Vector2 connectionSize = 0.5f * connection.length * direction;
                    if (!dotA.isLocked)
                    {
                        dotA.position = center + connectionSize;
                    }
                    if (!dotB.isLocked)
                    {
                        dotB.position = center - connectionSize;
                    }

                }
            }
        }
    }

    private void ApplyPhysicsToDots(float deltaTime)
    {
        Vector2 positionVariation = (currentForce / mass) * (deltaTime * deltaTime);
        for (int i = 0; i < dots.Count; i++)
        {
            Dot dot = dots[i];
            if (dot.isLocked) continue;
            Vector2 oldPos = dot.position;

            dot.position += (dot.position - dot.lastPosition);
            dot.position += positionVariation;
            dot.lastPosition = oldPos;
        }
        currentForce = Vector2.zero;
    }
    public void TransferPositions(Transform[] transforms)
    {
        int loopCount = dots.Count;
        if (transforms.Length < loopCount)
        {
            loopCount = transforms.Length;
        }
        for (int i = 0; i < loopCount; i++)
        {
            transforms[i].position = dots[i].position;
        }
    }
}

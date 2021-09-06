using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public List<RopePoint> points;
    public List<RopeStick> sticks;
    public float gravity = 9;
    public int numIterations = 10;

    public void Simulate()
    {
        foreach (var point in points)
        {
            if (!point.locked)
            {
                Vector2 positionBeforeUpdate = point.position;
                point.position += point.position - point.prevPosition;
                point.position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime;
                point.prevPosition = positionBeforeUpdate;
            }
        }

        for (int i = 0; i < numIterations; i++)
        {
            foreach (var stick in sticks)
            {
                var center = stick.Center;
                var direction = stick.Direction;
                var halfLength = stick.length / 2;
                var halfDirection = direction * halfLength;
                if (!stick.pointA.locked)
                {
                    stick.pointA.position = center + halfDirection;
                }
                if (!stick.pointB.locked)
                {
                    stick.pointB.position = center - halfDirection;
                }
            }
        }
    }
}

public class RopePoint
{
    public Vector2 position, prevPosition;
    public bool locked;
}

public class RopeStick
{
    public RopePoint pointA, pointB;
    public float length;
    public Vector2 Center
    {
        get
        {
            return (pointA.position + pointB.position) / 2;
        }
    }
    public Vector2 Direction
    {
        get
        {
            return (pointA.position - pointB.position).normalized;
        }
    }
}

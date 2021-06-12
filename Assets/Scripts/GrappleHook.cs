using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    public GameObject lineAndHook;

    public float lineLength = 5f;
    public float lineTimeToFullLength = .5f;
    bool isLineFired = false;
    float timeLineFired;
    Line firedLine;

    private void Update()
    {
        if (isLineFired)
        {
            if (Time.time >= timeLineFired + lineTimeToFullLength)
            {
                firedLine.StopLine();
            }
        }
    }

    public void Fire(Vector3 fromPosition, Vector3 toPosition)
    {
        var direction = (toPosition - fromPosition).normalized;
        var lineUpwards = Vector3.Cross(direction, Vector3.back).normalized;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, lineUpwards);
        firedLine = GameObject.Instantiate(lineAndHook, fromPosition, rotation).GetComponent<Line>();

        firedLine.SetSpeed(GetSpeedPerSecondForLine());
        firedLine.SetStartingPosition(transform);
        timeLineFired = Time.time;
        isLineFired = true;
    }

    float GetSpeedPerSecondForLine()
    {
        return lineLength / lineTimeToFullLength;
    }
}

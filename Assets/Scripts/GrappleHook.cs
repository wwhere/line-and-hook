using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    public GameObject lineAndHook;
    public AudioClip fireLineSound;

    public float lineLength = 5f;
    public float lineTimeToFullLength = .5f;

    GrappleHookState _state = GrappleHookState.ReadyToFire;

    float _timeLineFired;
    Line _firedLine;

    private void Update()
    {
        if (_state == GrappleHookState.Firing)
        {
            if (Time.time >= _timeLineFired + lineTimeToFullLength)
            {
                _state = GrappleHookState.ShotComplete;
                _firedLine.StopLine();
            }
        }
    }

    public Line Fire(Vector3 fromPosition, Vector3 toPosition)
    {
        if (_state == GrappleHookState.ReadyToFire)
        {
            PlayFireLineSound();
            var direction = (toPosition - fromPosition).normalized;
            var lineUpwards = Vector3.Cross(direction, Vector3.back).normalized;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, lineUpwards);
            _firedLine = GameObject.Instantiate(lineAndHook, fromPosition, rotation).GetComponent<Line>();

            _firedLine.SetSpeed(GetSpeedPerSecondForLine());
            _firedLine.SetStartingPosition(transform);
            _timeLineFired = Time.time;
            _state = GrappleHookState.Firing;
            return _firedLine;
        }
        else
        {
            return null;
        }
    }

    float GetSpeedPerSecondForLine()
    {
        return lineLength / lineTimeToFullLength;
    }

    void PlayFireLineSound()
    {
        AudioSource.PlayClipAtPoint(fireLineSound, transform.position);
    }
    
    private enum GrappleHookState
    {
        ReadyToFire,
        Firing,
        ShotComplete,
        GettingReadyToFire
    }
}

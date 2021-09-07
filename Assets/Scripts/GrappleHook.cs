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
                _firedLine.GetUnhooked();
            }
        }
    }

    public Vector3 UpdatePositions(Vector3 moveAmount, bool isGrounded)
    {
        var newMoveAmount = moveAmount;
        switch (_state)
        {
            case GrappleHookState.ReadyToFire:
                break;
            case GrappleHookState.Firing:
                break;
            case GrappleHookState.ShotComplete:
                newMoveAmount = _firedLine.UpdatePositions(transform.position + moveAmount, isGrounded) - transform.position;
                break;
            case GrappleHookState.GettingReadyToFire:
                break;
            default:
                break;
        }

        return newMoveAmount;
    }

    public Line Reel()
    {
        if (_state == GrappleHookState.ShotComplete)
        {
            _firedLine.Climb();
            return _firedLine;
        }
        return null;
    }

    Line Retract()
    {
        _firedLine.StartRetracting();
        _state = GrappleHookState.GettingReadyToFire;
        _firedLine.OnFinishRetracting += DoneRetracting;
        return _firedLine;
    }

    void DoneRetracting()
    {
        _state = GrappleHookState.ReadyToFire;
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

            _firedLine.StartExtending(transform, GetSpeedPerSecondForLine());
            _firedLine.OnStartRetracting += () =>
            {
                _state = GrappleHookState.GettingReadyToFire;
            };
            _firedLine.OnFinishRetracting += () =>
            {
                _state = GrappleHookState.ReadyToFire;
            };
            _firedLine.OnGettingHooked += () =>
            {
                _state = GrappleHookState.ShotComplete;
                _firedLine.OnClimbing += UpdatePositionWhenClimbing;
            };
            _timeLineFired = Time.time;
            _state = GrappleHookState.Firing;
            return _firedLine;
        }
        else if (_state == GrappleHookState.Firing || _state == GrappleHookState.ShotComplete)
        {
            return Retract();
        }
        else
        {
            return null;
        }
    }

    public event System.Func<Vector3, Vector3> OnReeling;

    Vector3 UpdatePositionWhenClimbing(Vector3 newPosition)
    {
        transform.position = newPosition;
        if (OnReeling != null)
        {
            transform.position = OnReeling.Invoke(transform.position);
        }
        return transform.position;
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

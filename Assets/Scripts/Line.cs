using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    public float retractingSpeed = 15f;
    public float maxLineLength = 8f;
    [Range(2, 1000)]
    public int numberOfPoints = 10;
    public float gravity = 5f;
    public float maxGravitySpeed = 8f;
    public float lineGravity = 2f;
    public float maxLineGravitySpeed = 6f;
    public SpriteRenderer hookRenderer;
    public GameObject breakEffect;
    public LayerMask layerMask;
    public AudioClip impactSound;
    public int numberOfIterations = 10;

    [HideInInspector]
    public LineState LineState = LineState.Idle;

    Material _lineRendererMaterial;
    LineRenderer _lineRenderer;
    Transform _startingPosition;
    LinePoint[] _linePoints;
    LineSegment[] _lineSegments;

    float _speed;
    float _gravitySpeed;
    float _lineGravitySpeed;
    float _hookWidth;

    bool _isClimbing;

    const float SKIN_WIDTH = 0.01f;
    const float COLLISION_DISTANCE = 0.02f;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        _lineRendererMaterial = _lineRenderer.material;
        _lineRendererMaterial.SetFloat("_AmountX", transform.right.y);
        _lineRendererMaterial.SetFloat("_AmountY", transform.right.x);

        _startingPosition = transform;
        _hookWidth = hookRenderer.bounds.size.x;

        _speed = 0;
        _gravitySpeed = 0;
        _lineGravitySpeed = 0;
        _isClimbing = false;

        LineState = LineState.Idle;
    }

    void Update()
    {
        switch (LineState)
        {
            case LineState.Extending:
                HandleExtending();
                break;
            case LineState.Hooked:
                HandleHooked();
                break;
            case LineState.Unhooked:
                HandleUnhooked();
                break;
            case LineState.Retracting:
                HandleRetracting();
                break;
            default:
            case LineState.Idle:
                //Do nothing
                break;
        }
    }

    void HandleUnhooked()
    {

    }

    void HandleHooked()
    {
        if (_isClimbing)
        {
            ClimbLine();

            _isClimbing = false;
        }
    }

    void HandleExtending()
    {
        if (HasMadeContact())
        {
            GetHooked();
        }
        else
        {
            UpdateGravitySpeed();
            var moveAmount = GetExtendingMoveAmount();
            transform.Translate(moveAmount, Space.World);
            UpdateLineOndulation();
            AlignHookToMovement(moveAmount);
        }
        UpdateLinePointsAndSegments(_startingPosition.position, transform.position, numberOfPoints);
        UpdateLineRendererPositions(_lineRenderer, _linePoints.Select(l => l.position));
    }

    void HandleRetracting()
    {
        //each line position moves towards the previous one
        UpdateLineGravitySpeed();
        UpdateGravitySpeed();
        RetractLine();
    }

    #region State changes and events

    public event System.Action<Transform, float> OnStartExtending;
    public event System.Action OnStopExtending;

    public event System.Action OnGettingHooked;
    public event System.Func<Vector3, Vector3> OnClimbing;
    public event System.Action OnReleasingHooked;

    public event System.Action OnGettingUnhooked;

    public event System.Action OnStartRetracting;
    public event System.Action OnFinishRetracting;

    public void StartExtending(Transform startingPosition, float speed)
    {
        OnStartExtending?.Invoke(startingPosition, speed);
        _startingPosition = startingPosition;
        UpdateLinePointsAndSegments(_startingPosition.position, transform.position, numberOfPoints);
        _speed = speed;
        LineState = LineState.Extending;
    }

    public void GetHooked()
    {
        OnStopExtending?.Invoke();
        OnGettingHooked?.Invoke();

        _speed = 0;
        StopLineOndulating();
        PlayImpactSound();
        ShowBreakEffect();

        LineState = LineState.Hooked;
    }

    public void GetUnhooked()
    {
        OnStopExtending?.Invoke();
        OnGettingUnhooked?.Invoke();

        _speed = 0;
        StopLineOndulating();

        LineState = LineState.Unhooked;
        StartRetracting();
    }

    public void StartRetracting()
    {
        if (LineState == LineState.Hooked)
        {
            OnReleasingHooked?.Invoke();
        }
        OnStopExtending?.Invoke();
        OnStartRetracting?.Invoke();
        StopLineOndulating();
        HideBreakEffect();
        LineState = LineState.Retracting;
    }

    void FinishRetracting()
    {
        OnFinishRetracting?.Invoke();
        Destroy(gameObject);
    }

    #endregion

    #region Input

    public void Climb()
    {
        _isClimbing = true;
    }

    #endregion

    #region Helper functions

    void UpdateLinePointsAndSegments(Vector3 initialPosition, Vector3 finalPosition, int numberOfPoints)
    {
        if (numberOfPoints < 2)
        {
            throw new System.ArgumentException("Number of points must be at least 2", nameof(numberOfPoints));
        }
        if (initialPosition == null)
        {
            throw new System.ArgumentException("Initial position must have value", nameof(initialPosition));
        }
        if (finalPosition == null)
        {
            throw new System.ArgumentException("Final position must have value", nameof(finalPosition));
        }

        _linePoints = new LinePoint[numberOfPoints];
        _lineSegments = new LineSegment[numberOfPoints - 1];

        _linePoints[0] = new LinePoint
        {
            position = initialPosition,
            prevPosition = initialPosition,
            locked = true
        };
        _linePoints[numberOfPoints - 1] = new LinePoint
        {
            position = finalPosition,
            prevPosition = finalPosition,
            locked = true
        };

        if (numberOfPoints > 2)
        {
            for (var i = 1; i < numberOfPoints - 1; i++)
            {
                var pointPosition = Vector3.Lerp(initialPosition, finalPosition, i / (float)(numberOfPoints - 1));

                _linePoints[i] = new LinePoint
                {
                    position = pointPosition,
                    prevPosition = pointPosition,
                    locked = false
                };
            }
        }

        for (var i = 0; i < numberOfPoints - 1; i++)
        {
            _lineSegments[i] = new LineSegment
            {
                pointA = _linePoints[i],
                pointB = _linePoints[i + 1],
                length = Vector3.Distance(_linePoints[i].position, _linePoints[i + 1].position)
            };
        }
    }

    /// <summary>
    /// Updates the lineRenderer set of positions and the position count
    /// </summary>
    /// <param name="lineRenderer"></param>
    /// <param name="linePositions"></param>
    static void UpdateLineRendererPositions(LineRenderer lineRenderer, IEnumerable<Vector3> linePositions)
    {
        lineRenderer.positionCount = linePositions.Count();
        lineRenderer.SetPositions(linePositions.ToArray());
    }

    /// <summary>
    /// Returns the current length of the line
    /// </summary>
    /// <returns></returns>
    float GetLineLength()
    {
        return _lineSegments.Sum(l => l.length);
    }

    #endregion

    #region Physics

    /// <summary>
    /// Returns whether the Hook has impacted an object in the layerMask
    /// </summary>
    /// <returns></returns>
    bool HasMadeContact()
    {
        var hookSize = transform.right * (_hookWidth - SKIN_WIDTH);

        return Physics2D.Raycast(transform.position + hookSize, transform.right, COLLISION_DISTANCE, layerMask);
    }

    void UpdateGravitySpeed()
    {
        _gravitySpeed += gravity * Time.deltaTime;
        _gravitySpeed = Mathf.Min(maxGravitySpeed, _gravitySpeed);
    }

    void UpdateLineGravitySpeed()
    {
        _lineGravitySpeed += lineGravity * Time.deltaTime;
        _lineGravitySpeed = Mathf.Min(maxLineGravitySpeed, _lineGravitySpeed);
    }

    Vector3 GetExtendingMoveAmount()
    {
        var move = _speed * Time.deltaTime * transform.right;
        move -= _gravitySpeed * Time.deltaTime * Vector3.up;
        return move;
    }
    void AlignHookToMovement(Vector3 move)
    {
        var lineUpwards = Vector3.Cross(move, Vector3.back).normalized;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, lineUpwards);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, _speed * Time.deltaTime);
    }

    void RetractLine()
    {
        var linePositions = _linePoints.Select(p => p.position).ToArray();
        var originalPositions = linePositions.ClonePositions();
        var removedPositions = new HashSet<int>();

        //start at 1, 0 is always starting position
        for (var i = 1; i < linePositions.Length; i++)
        {
            var direction = (originalPositions[i - 1] - originalPositions[i]).normalized;
            var moveAmount = direction * retractingSpeed * Time.deltaTime;
            moveAmount -= Vector3.up * _lineGravitySpeed * Time.deltaTime;
            linePositions[i] += moveAmount;
            if (Vector3.Distance(linePositions[i - 1], linePositions[i]) < 0.1f)
            {
                removedPositions.Add(i);
            }
        }
        var newPositionsCount = linePositions.Length - removedPositions.Count;

        if (newPositionsCount == 1)
        {
            FinishRetracting();
        }
        else
        {
            var newPositions = new Vector3[newPositionsCount];
            for (int j = 0, k = 0; j < linePositions.Length; j++)
            {
                if (!removedPositions.Contains(j))
                {
                    newPositions[k++] = linePositions[j];
                }
            }
            linePositions = newPositions;

            //apply hook gravity to last point
            linePositions[linePositions.Length - 1] -= Vector3.up * _gravitySpeed * Time.deltaTime;
            //update hook position
            transform.position = linePositions[linePositions.Length - 1];

            UpdateLineRendererPositions(_lineRenderer, linePositions);
        }
    }

    void ClimbLine()
    {
        var linePositions = _linePoints.Select(p => p.position).ToArray();
        var originalPositions = linePositions.ClonePositions();

        //start at 0, last one is always hook position which is static here
        for (var i = 0; i < linePositions.Length - 1; i++)
        {
            var direction = (originalPositions[i + 1] - originalPositions[i]).normalized;
            var moveAmount = direction * retractingSpeed * Time.deltaTime;
            moveAmount -= Vector3.up * _lineGravitySpeed * Time.deltaTime;
            linePositions[i] += moveAmount;
        }

        var newPosition = OnClimbing?.Invoke(linePositions[0]);
        if (newPosition.HasValue)
            linePositions[0] = newPosition.Value;

        UpdateLineRendererPositions(_lineRenderer, linePositions);
    }

    public Vector3 UpdatePositions(Vector3 firstPointPosition, bool isFirstPointLocked)
    {
        _linePoints[0].prevPosition = _linePoints[0].position;
        _linePoints[0].position = firstPointPosition;
        _linePoints[0].locked = isFirstPointLocked;

        foreach (var point in _linePoints)
        {
            if (!point.locked)
            {
                Vector3 positionBeforeUpdate = point.position;
                point.position += point.position - point.prevPosition;
                point.position += gravity * Time.deltaTime * Time.deltaTime * Vector3.down;
                point.prevPosition = positionBeforeUpdate;
            }
        }

        for (int i = 0; i < numberOfIterations; i++)
        {
            foreach (var stick in _lineSegments)
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
        UpdateLineRendererPositions(_lineRenderer, _linePoints.Select(p => p.position));
        return _linePoints[0].position;
    }

    #endregion

    #region vfx

    void UpdateLineOndulation()
    {
        _lineRendererMaterial.SetFloat("_FixX", transform.position.x);
        _lineRendererMaterial.SetFloat("_FixY", transform.position.y);
    }

    void StopLineOndulating()
    {
        _lineRendererMaterial.SetFloat("_Amount", 0);
    }

    void ShowBreakEffect()
    {
        breakEffect.SetActive(true);
    }

    void HideBreakEffect()
    {
        breakEffect.SetActive(false);
    }

    #endregion

    #region Sound

    void PlayImpactSound()
    {
        AudioSource.PlayClipAtPoint(impactSound, transform.position);
    }

    #endregion
}

public class LineSegment
{
    public LinePoint pointA, pointB;
    public float length;
    public Vector3 Center
    {
        get
        {
            return (pointA.position + pointB.position) / 2;
        }
    }
    public Vector3 Direction
    {
        get
        {
            return (pointA.position - pointB.position).normalized;
        }
    }
}

public class LinePoint
{
    public Vector3 position, prevPosition;
    public bool locked;
}

public enum LineState
{
    Idle,
    Extending,
    Hooked,
    Unhooked,
    Retracting
};

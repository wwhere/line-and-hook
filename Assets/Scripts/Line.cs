using System.Collections;
using System.Collections.Generic;
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

    [HideInInspector]
    public LineState LineState = LineState.Idle;

    Material _lineRendererMaterial;
    LineRenderer _lineRenderer;
    Transform _startingPosition;
    Vector3[] _linePositions;

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
        _linePositions = GetLinePositions(_startingPosition.position, transform.position, numberOfPoints);
        UpdateLineRendererPositions(_lineRenderer, _linePositions);
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
        _linePositions = GetLinePositions(_startingPosition.position, transform.position, numberOfPoints);
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

    /// <summary>
    /// Calculates the intermediate points in a line from initialPosition to finalPosition
    /// </summary>
    /// <param name="initialPosition"></param>
    /// <param name="finalPosition"></param>
    /// <param name="numberOfPoints"></param>
    /// <returns></returns>
    static Vector3[] GetLinePositions(Vector3 initialPosition, Vector3 finalPosition, int numberOfPoints)
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

        var linePositions = new Vector3[numberOfPoints];
        linePositions[0] = initialPosition;
        linePositions[numberOfPoints - 1] = finalPosition;

        if (numberOfPoints > 2)
        {
            for (var i = 1; i < numberOfPoints - 1; i++)
            {
                linePositions[i] = Vector3.Lerp(initialPosition, finalPosition, i / (float)(numberOfPoints - 1));
            }
        }

        return linePositions;
    }

    /// <summary>
    /// Updates the lineRenderer set of positions and the position count
    /// </summary>
    /// <param name="lineRenderer"></param>
    /// <param name="linePositions"></param>
    static void UpdateLineRendererPositions(LineRenderer lineRenderer, Vector3[] linePositions)
    {
        lineRenderer.positionCount = linePositions.Length;
        lineRenderer.SetPositions(linePositions);
    }

    /// <summary>
    /// Returns the current length of the line
    /// </summary>
    /// <returns></returns>
    float GetLineLength()
    {
        float lineLength = 0;
        for (var i = 1; i < numberOfPoints; i++)
        {
            lineLength += (_linePositions[i] - _linePositions[i - 1]).magnitude;
        }
        return lineLength;
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
        var move = transform.right * _speed * Time.deltaTime;
        move -= Vector3.up * _gravitySpeed * Time.deltaTime;
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
        var originalPositions = _linePositions.ClonePositions();
        var removedPositions = new HashSet<int>();

        //start at 1, 0 is always starting position
        for (var i = 1; i < _linePositions.Length; i++)
        {
            var direction = (originalPositions[i - 1] - originalPositions[i]).normalized;
            var moveAmount = direction * retractingSpeed * Time.deltaTime;
            moveAmount -= Vector3.up * _lineGravitySpeed * Time.deltaTime;
            _linePositions[i] += moveAmount;
            if (Vector3.Distance(_linePositions[i - 1], _linePositions[i]) < 0.1f)
            {
                removedPositions.Add(i);
            }
        }
        var newPositionsCount = _linePositions.Length - removedPositions.Count;

        if (newPositionsCount == 1)
        {
            FinishRetracting();
        }
        else
        {
            var newPositions = new Vector3[newPositionsCount];
            for (int j = 0, k = 0; j < _linePositions.Length; j++)
            {
                if (!removedPositions.Contains(j))
                {
                    newPositions[k++] = _linePositions[j];
                }
            }
            _linePositions = newPositions;

            //apply hook gravity to last point
            _linePositions[_linePositions.Length - 1] -= Vector3.up * _gravitySpeed * Time.deltaTime;
            //update hook position
            transform.position = _linePositions[_linePositions.Length - 1];

            UpdateLineRendererPositions(_lineRenderer, _linePositions);
        }
    }

    void ClimbLine()
    {
        var originalPositions = _linePositions.ClonePositions();

        //start at 0, last one is always hook position which is static here
        for (var i = 0; i < _linePositions.Length - 1; i++)
        {
            var direction = (originalPositions[i + 1] - originalPositions[i]).normalized;
            var moveAmount = direction * retractingSpeed * Time.deltaTime;
            moveAmount -= Vector3.up * _lineGravitySpeed * Time.deltaTime;
            _linePositions[i] += moveAmount;
        }

        var newPosition = OnClimbing?.Invoke(_linePositions[0]);
        if (newPosition.HasValue)
            _linePositions[0] = newPosition.Value;

        UpdateLineRendererPositions(_lineRenderer, _linePositions);
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

public enum LineState
{
    Idle,
    Extending,
    Hooked,
    Unhooked,
    Retracting
};

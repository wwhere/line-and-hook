using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    public float speed = 10f;
    public float retractingSpeed = 15f;
    public float maxLineLength = 8f;
    public int numberOfPoints = 10;
    public float gravity = 5f;
    public float lineGravity = 2f;
    public SpriteRenderer hookRenderer;
    public GameObject breakEffect;
    public LayerMask layerMask;
    public AudioClip impactSound;

    Material _lineRendererMaterial;
    LineRenderer _lineRenderer;
    Transform _startingPosition;
    Vector3[] _linePositions;
    float _gravitySpeed;
    float _lineGravitySpeed;

    float _hookWidth;

    bool _isRetracting;

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
        _gravitySpeed = 0;
        _lineGravitySpeed = 0;
        _isRetracting = false;
    }

    void UpdateLinePositions()
    {
        _linePositions = new Vector3[numberOfPoints];
        _linePositions[0] = _startingPosition.position;
        _linePositions[numberOfPoints - 1] = transform.position;
        if (numberOfPoints > 2)
        {
            for (var i = 1; i < numberOfPoints - 1; i++)
            {
                _linePositions[i] = Vector3.Lerp(_linePositions[0], _linePositions[numberOfPoints - 1], i / (float)numberOfPoints);
            }
        }
        UpdateLineRendererPositions();
    }

    void UpdateLineRendererPositions()
    {
        _lineRenderer.positionCount = _linePositions.Length;
        _lineRenderer.SetPositions(_linePositions);
    }

    void CheckForImpact()
    {
        if (speed != 0)
        {
            var hookSize = transform.right * (_hookWidth - SKIN_WIDTH);
            var hit = Physics2D.Raycast(transform.position + hookSize, transform.right, COLLISION_DISTANCE, layerMask);
            if (hit)
            {
                PlayImpactSound();
                ShowBreak();
                StopLine();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isRetracting)
        {
            CheckForImpact();
            if (speed != 0)
            {
                _gravitySpeed += gravity * Time.deltaTime;
                var move = transform.right * speed * Time.deltaTime;
                move -= Vector3.up * _gravitySpeed * Time.deltaTime;
                transform.Translate(move, Space.World);
                _lineRendererMaterial.SetFloat("_FixX", transform.position.x);
                _lineRendererMaterial.SetFloat("_FixY", transform.position.y);

                var lineUpwards = Vector3.Cross(move, Vector3.back).normalized;
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, lineUpwards);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, speed * Time.deltaTime);
            }
            else
            {
                var lineLength = GetLineLength();
                if (GetLineLength() > maxLineLength)
                {
                    //TODO
                }
                else
                {
                    //TODO
                }
            }
            UpdateLinePositions();
        }
        else
        {
            //is retracting

            //each line position moves towards the previous one
            _lineGravitySpeed += lineGravity * Time.deltaTime;
            _gravitySpeed += gravity * Time.deltaTime;
            var originalPositions = CloneLinePositions();
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
                DoneRetracting();
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
                _linePositions[_linePositions.Length-1] -= Vector3.up * _gravitySpeed * Time.deltaTime;
                //update hook position
                transform.position = _linePositions[_linePositions.Length-1];

                UpdateLineRendererPositions();
            }
        }
    }

    Vector3[] CloneLinePositions()
    {
        var clone = new Vector3[_linePositions.Length];
        for (var i = 0; i < _linePositions.Length; i++)
        {
            clone[i] = new Vector3(_linePositions[i].x, _linePositions[i].y, _linePositions[i].z);
        }
        return clone;
    }

    public void DoneRetracting()
    {
        if (OnDoneRetracting != null)
        {
            OnDoneRetracting();
        }
        Destroy(gameObject);
    }

    public event System.Action OnDoneRetracting;

    public void Retract()
    {
        _lineRendererMaterial.SetFloat("_Amount", 0);
        HideBreak();
        _isRetracting = true;
    }

    float GetLineLength()
    {
        return (_linePositions[0] - _linePositions[numberOfPoints - 1]).magnitude;
    }

    public void SetStartingPosition(Transform startingPosition)
    {
        _startingPosition = startingPosition;
        _linePositions = new Vector3[] { _startingPosition.position, transform.position };
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    void ShowBreak()
    {
        breakEffect.SetActive(true);
    }
    void HideBreak()
    {
        breakEffect.SetActive(false);
    }

    public void StopLine()
    {
        speed = 0;
        _lineRendererMaterial.SetFloat("_Amount", 0);
    }

    void PlayImpactSound()
    {
        AudioSource.PlayClipAtPoint(impactSound, transform.position);
    }
}

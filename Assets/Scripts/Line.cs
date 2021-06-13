using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    public float speed = 10f;
    public float maxLineLength = 8f;
    public int numberOfPoints = 10;
    public float gravity = 5f;
    public SpriteRenderer hookRenderer;
    public GameObject breakEffect;
    public LayerMask layerMask;
    public AudioClip impactSound;


    Material _lineRendererMaterial;
    LineRenderer _lineRenderer;
    Transform _startingPosition;
    Vector3[] _linePositions;
    float _gravitySpeed;

    float _hookWidth;

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
        _lineRenderer.positionCount = numberOfPoints;
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

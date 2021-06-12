using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    public float speed = 10f;
    public float maxLineLength = 8f;
    Material _trailRendererMaterial;
    TrailRenderer _trailRenderer;
    LineRenderer _lineRenderer;
    Transform _startingPosition;
    Vector3[] _linePositions;
    Vector3 _lastAcceptableStartingPosition;

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        _lineRenderer = GetComponent<LineRenderer>();

        _trailRendererMaterial = _trailRenderer.material;
        _trailRendererMaterial.SetFloat("_AmountX", transform.right.y);
        _trailRendererMaterial.SetFloat("_AmountY", transform.right.x);

        _lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (speed != 0)
        {
            var move = transform.right * speed * Time.deltaTime;
            transform.Translate(move, Space.World);
            _trailRendererMaterial.SetFloat("_FixX", transform.position.x);
            _trailRendererMaterial.SetFloat("_FixY", transform.position.y);
        }
        else
        {
            var lineLength = GetLineLength();
            print($"Line length ={lineLength}");
            if (GetLineLength() > maxLineLength)
            {
                _startingPosition.position = _lastAcceptableStartingPosition;
            }
            else
            {
                _lastAcceptableStartingPosition = _startingPosition.position;
            }
        }
    }

    float GetLineLength()
    {
        return (_linePositions[0] - _linePositions[1]).magnitude;
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

    public void StopLine()
    {
        speed = 0;
        _trailRenderer.enabled = false;
        _lineRenderer.enabled = true;
        _lineRenderer.SetPositions(_linePositions);
        _linePositions = new Vector3[] { _startingPosition.position, transform.position };
        _lastAcceptableStartingPosition = _startingPosition.position;
    }
}

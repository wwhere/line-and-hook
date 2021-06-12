using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    public float speed = 10f;
    public float maxLineLength = 8f;
    public Collider2D hook;
    public LayerMask layerMask;
    Material _trailRendererMaterial;
    TrailRenderer _trailRenderer;
    LineRenderer _lineRenderer;
    Transform _startingPosition;
    Vector3[] _linePositions;

    float _hookWidth;

    const float SKIN_WIDTH = 0.1f;
    const float COLLISION_DISTANCE = 0.05f;

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        _lineRenderer = GetComponent<LineRenderer>();

        _trailRendererMaterial = _trailRenderer.material;
        _trailRendererMaterial.SetFloat("_AmountX", transform.right.y);
        _trailRendererMaterial.SetFloat("_AmountY", transform.right.x);

        _lineRenderer.enabled = false;

        _hookWidth = hook.bounds.size.x;
    }

    void CheckForImpact()
    {
        var hookSize = transform.right * (_hookWidth - SKIN_WIDTH);
        var hit = Physics2D.Raycast(transform.position + hookSize, transform.right, COLLISION_DISTANCE, layerMask);
        if (hit)
        {
            StopLine();
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckForImpact();
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
            if (GetLineLength() > maxLineLength)
            {
                //TODO
            }
            else
            {
                //TODO
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
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(GrappleHook))]
public class Player : MonoBehaviour
{
    public float speed = 100f;
    public float gravity = 5f;
    public float maxGravitySpeed = 8f;
    public LayerMask floorLayerMask;

    const float MAX_RAY_SEPARATION = 0.2f;
    int _numberOfHorizontalRays;
    int _numberOfVerticalRays;
    float _horizontalRaySeparation;
    float _verticalRaySeparation;

    Vector3 _moveAmount;
    float _gravitySpeed;

    GrappleHook _grappleHook;
    CircleCollider2D _collider;

    Vector2[] _bottomRayOrigins;
    Vector2[] _topRayOrigins;
    Vector2[] _leftRayOrigins;
    Vector2[] _rightRayOrigins;

    private void Start()
    {
        _grappleHook = GetComponent<GrappleHook>();
        _collider = GetComponent<CircleCollider2D>();

        _grappleHook.OnReeling += UpdatePositionWhenReeling;

        _numberOfHorizontalRays = Mathf.FloorToInt(_collider.bounds.size.y / MAX_RAY_SEPARATION) + 1;
        _horizontalRaySeparation = _collider.bounds.size.y / (_numberOfHorizontalRays - 1);
        _numberOfVerticalRays = Mathf.FloorToInt(_collider.bounds.size.x / MAX_RAY_SEPARATION) + 1;
        _verticalRaySeparation = _collider.bounds.size.x / (_numberOfVerticalRays - 1);

        _bottomRayOrigins = new Vector2[_numberOfHorizontalRays];
        _topRayOrigins = new Vector2[_numberOfHorizontalRays];
        _leftRayOrigins = new Vector2[_numberOfVerticalRays];
        _rightRayOrigins = new Vector2[_numberOfVerticalRays];
    }

    void UpdateRayOrigins()
    {
        var bounds = _collider.bounds;
        var bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        var bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        var topLeft = new Vector2(bounds.min.x, bounds.max.y);

        for (var i = 0; i < _numberOfHorizontalRays; i++)
        {
            _leftRayOrigins[i] = bottomRight + new Vector2(-0.01f, i * _horizontalRaySeparation);
            _rightRayOrigins[i] = bottomLeft + new Vector2(0.01f, i * _horizontalRaySeparation);
        }

        for (var i = 0; i < _numberOfVerticalRays; i++)
        {
            _bottomRayOrigins[i] = bottomLeft + new Vector2(i * _verticalRaySeparation, 0.01f);
            _topRayOrigins[i] = topLeft + new Vector2(i * _verticalRaySeparation, -0.01f);
        }
    }

    Vector3 UpdatePositionWhenReeling(Vector3 newPosition)
    {
        transform.position = newPosition;
        return transform.position;
    }

    bool IsGrounded(ref float verticalMovement)
    {
        foreach (var rayOrigin in _bottomRayOrigins)
        {
            var hit = Physics2D.Raycast(rayOrigin, Vector2.down, verticalMovement + 0.01f, floorLayerMask);
            if (hit)
            {
                verticalMovement = hit.distance - 0.01f;
                if (hit.distance < 0.02f) return true;
            }
        }
        return false;
    }

    void Update()
    {
        UpdateRayOrigins();

        UpdateGravitySpeed();

        _moveAmount = new Vector3(Input.GetAxis("Horizontal") * speed * Time.deltaTime, -_gravitySpeed, 0);

        _moveAmount = UpdateForVerticalCollisions(_moveAmount);
        _moveAmount = UpdateForHorizontalCollisions(_moveAmount);

        if (Input.GetMouseButtonDown(0))
        {
            FireLine();
        }


        if (Input.GetMouseButton(1))
        {
            ReelLine();
        }
    }

    Vector3 UpdateForVerticalCollisions(Vector3 moveAmount)
    {
        return moveAmount;
    }

    Vector3 UpdateForHorizontalCollisions(Vector3 moveAmount)
    {
        return moveAmount;
    }

    private void LateUpdate()
    {
        transform.Translate(_moveAmount);
    }

    void UpdateGravitySpeed()
    {
        _gravitySpeed += gravity * Time.deltaTime;
        _gravitySpeed = Mathf.Min(maxGravitySpeed, _gravitySpeed);
        if (IsGrounded(ref _gravitySpeed))
        {
            _gravitySpeed = 0;
        }
    }

    void FireLine()
    {
        var firedLine = _grappleHook.Fire(transform.position, GetMouseWorldPosition());
        if (firedLine != null)
        {
            //TODO
        }
    }

    void ReelLine()
    {
        var retractingLine = _grappleHook.Reel();
        if (retractingLine != null)
        {
            //TODO
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        var worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        worldPosition.z = 0;
        return worldPosition;
    }
}

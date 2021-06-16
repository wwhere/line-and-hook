using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(GrappleHook))]
public class Player : MonoBehaviour
{
    public float speed = 100f;

    float _moveAmount;

    GrappleHook _grappleHook;
    CircleCollider2D _collider;

    float _apotema;
    float _side;

    private void Start()
    {
        _grappleHook = GetComponent<GrappleHook>();
        _collider = GetComponent<CircleCollider2D>();

        CalculateHexagonValues();
    }

    void CalculateHexagonValues()
    {
        _side = _collider.bounds.size.x / 2;
        _apotema = _side / (2 * Mathf.Tan(30));
    }

    void Update()
    {
        _moveAmount = Input.GetAxis("Horizontal") * speed * Time.deltaTime;


        if (Input.GetMouseButtonDown(0))
        {
            FireLine();
        }


        if (Input.GetMouseButtonDown(1))
        {
            ReelLine();
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

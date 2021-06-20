using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(GrappleHook))]
public class Player : MonoBehaviour
{
    public float speed = 100f;

    Vector3 _moveAmount;

    GrappleHook _grappleHook;
    CircleCollider2D _collider;

    private void Start()
    {
        _grappleHook = GetComponent<GrappleHook>();
        _collider = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        _moveAmount = new Vector3(Input.GetAxis("Horizontal") * speed * Time.deltaTime, 0, 0);
        transform.Translate(_moveAmount);

        if (Input.GetMouseButtonDown(0))
        {
            FireLine();
        }


        if (Input.GetMouseButton(1))
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

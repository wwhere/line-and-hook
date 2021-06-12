using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GrappleHook))]
public class Player : MonoBehaviour
{
    public float speed = 100f;

    float _moveAmount;

    Rigidbody2D _rigidbody;

    GrappleHook _grappleHook;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _grappleHook = GetComponent<GrappleHook>();
    }

    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") == 0)
        {
            _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
        }
        else
        {
            _moveAmount = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            _rigidbody.AddForce(new Vector2(_moveAmount, 0));
        }

        if (Input.GetMouseButtonDown(0))
        {
            FireLine();            
        }
    }

    void FireLine()
    {
        _grappleHook.Fire(transform.position, GetMouseWorldPosition());
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

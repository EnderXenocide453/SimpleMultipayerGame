using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Joystick : MonoBehaviour
{
    private RectTransform _transform;
    private Vector3 _origin;

    private int _fingerID;

    private bool _pressed = false;

    public float deathDist = 0;
    public float additionalDist = 0;
    public float maxDist = 1;

    [SerializeField]
    private Vector2 _axis = Vector2.zero;

    void Start()
    {
        _transform = GetComponent<RectTransform>();
        _origin = _transform.position;
    }

    void Update()
    {
        if (_pressed)
            Follow();
    }

    public void Press()
    {
        _pressed = true;
        _fingerID = Input.touches[Input.touchCount - 1].fingerId;
    }

    public void Release() 
    { 
        _pressed = false;
        _transform.position = _origin;
        _axis = Vector2.zero;
    }

    public void Follow()
    {
        int id = TryGetTouchIDByFinger(_fingerID);
        if (id < 0 || Input.touches[id].phase == TouchPhase.Ended || Input.touches[id].phase == TouchPhase.Canceled) {
            Release();
            return;
        }

        Vector2 dir = Vector2.ClampMagnitude(Camera.main.ScreenToWorldPoint(Input.touches[id].position) - _origin, maxDist);
        _transform.position = (Vector2)_origin + dir;
        _axis = dir / maxDist;
    }

    public int TryGetTouchIDByFinger(int fingerID)
    {
        if (Input.touchCount == 0) return -1;

        for (int i = 0; i < Input.touchCount; i++) {
            if (Input.touches[i].fingerId == fingerID) return i;
        }

        return -1;
    }

    public Vector2 GetAxis() => _axis;
}

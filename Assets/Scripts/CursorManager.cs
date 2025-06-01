using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursor_normal; // default cursor image
    public Texture2D cursor_onClick; // cursor image when clicked

    void Awake()
    {
        Cursor.SetCursor(cursor_normal, Vector2.zero, CursorMode.Auto);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Cursor.SetCursor(cursor_onClick, Vector2.zero, CursorMode.Auto);
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            Cursor.SetCursor(cursor_normal, Vector2.zero, CursorMode.Auto);
        }
    }
}

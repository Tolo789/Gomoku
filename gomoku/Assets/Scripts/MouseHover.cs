using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MouseHover : MonoBehaviour {
    public Color StartColor;
    public Color MouseOverColor;
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    public void OnMouseEnter(){
        GetComponent<Button>().GetComponentInChildren<Text>().color= MouseOverColor;
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

    public void OnMouseExit(){
        GetComponent<Button>().GetComponentInChildren<Text>().color = StartColor;
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
    
}
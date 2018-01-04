using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;


public class InGameMenu : MonoBehaviour {
    
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    void Start() {
    }
    public void ExitClick() {
        Application.Quit();
    }
    public void GoClick(GameObject playSettings) {
        playSettings.SetActive(false);
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
        SceneManager.LoadScene("Game");
    }
}
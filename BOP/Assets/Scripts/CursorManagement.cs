using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManagement : MonoBehaviour
{
    private void Start()
    {
        if(!Cursor.visible)
        {
            Cursor.visible = true;
        }
    }

    // Start is called before the first frame update
    public void CursorVisibility()
    {
        Cursor.visible = Cursor.visible ? !Cursor.visible : Cursor.visible;
    }
}

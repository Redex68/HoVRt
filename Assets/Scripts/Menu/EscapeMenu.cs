using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EscapeMenu : MonoBehaviour
{
    [SerializeField] GameObject escapeMenu;
    bool opened = false;
    void Start()
    {
        escapeMenu.SetActive(false);
    }

    public void OpenEscapeMenu(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            escapeMenu.SetActive(!opened);
        }
    }
}

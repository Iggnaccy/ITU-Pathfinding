using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickRaycaster : MonoBehaviour
{
    [SerializeField] private InputActionReference clickAction, rightclickAction;
    private string targetLayer = "Tile";

    private void Start()
    {
        clickAction.action.performed += OnClick;
        rightclickAction.action.performed += OnRightClick;

        clickAction.action.Enable();
        rightclickAction.action.Enable();
    }

    private void OnDestroy()
    {
        clickAction.action.performed -= OnClick;
        rightclickAction.action.performed -= OnRightClick;
        clickAction.action.Disable();
        rightclickAction.action.Disable();
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        var tile = RaycastTile();
        if (tile != null)
        {
            tile.OnRightClick();
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        var tile = RaycastTile();
        if (tile != null)
        {
            tile.OnLeftClick();
        }
    }

    private TileEvents RaycastTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, LayerMask.GetMask(targetLayer)))
        {
            return hitInfo.collider.GetComponent<TileEvents>();
        }
        return null;
    }
}

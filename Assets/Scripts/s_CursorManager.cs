using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class s_CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D blockedCursor;
    [SerializeField] private s_ComputerStation station;

    private readonly List<RaycastResult> hits = new List<RaycastResult>();
    private bool showingBlocked;

    void Update()
    {
        bool blocked = station != null && station.IsSeated && IsOverUnclickableUI();

        if (blocked == showingBlocked) return;
        showingBlocked = blocked;

        if (blocked)
        {
            Vector2 hotspot = new Vector2(blockedCursor.width / 2f, blockedCursor.height / 2f);
            Cursor.SetCursor(blockedCursor, hotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // back to normal
        }
    }

        bool IsOverUnclickableUI()
    {
        if (EventSystem.current == null || Mouse.current == null) return false;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        hits.Clear();
        EventSystem.current.RaycastAll(pointer, hits);
        if (hits.Count == 0) return false; 

        Selectable selectable = hits[0].gameObject.GetComponentInParent<Selectable>();
        return selectable == null || !selectable.IsInteractable();
    }

    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}

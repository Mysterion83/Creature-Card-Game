using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelectionController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _ground;
    [SerializeField] bool _enableDebug = false;
    private Vector3 _lastMousePosition;

    /// <summary>
    /// Attempts to return a valid board position by firing a ray from the camera to the mouse
    /// </summary>
    /// <param name="hitPosition">The position if hit</param>
    /// <returns>True if position found, False if not</returns>
    public bool TryGetSelectedWorldPosition(out Vector3 hitPosition)
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = _camera.nearClipPlane;

        Ray ray = _camera.ScreenPointToRay(mousePos);
        
        if (_enableDebug) Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _ground))
        {
            hitPosition = hit.point;
            return true;
        }

        hitPosition = Vector3.zero;
        return false;
    }
}
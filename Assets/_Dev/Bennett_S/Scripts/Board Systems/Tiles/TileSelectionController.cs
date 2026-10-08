using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelectionController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _ground;
    [SerializeField] bool _enableDebug = false;
    private Vector3 _lastMousePosition;

    public Vector3 GetSelectedTilePosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = _camera.nearClipPlane;

        Ray ray = _camera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (_enableDebug) Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

        if (Physics.Raycast(ray, out hit, 100, _ground))
        {
            _lastMousePosition = hit.point;
        }
        return _lastMousePosition;
    }
}
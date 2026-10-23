using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _originalFOV;
    [SerializeField] private float _targetFOV;
    [SerializeField] private float _cameraZoomSpeed = 10f;
    [SerializeField] private bool _isZoomedIn = false;

    private GameControls _inputMapping;

    private void Awake()
    {
        _inputMapping = new GameControls();
        _originalFOV = Camera.main.fieldOfView;
        _targetFOV = Camera.main.fieldOfView;
    }

    private void OnEnable()
    {
        _inputMapping.Enable();
        _inputMapping.Gameplay.CameraZoom.performed += OnCameraZoom;
    }
    private void OnDisable()
    {
        _inputMapping.Disable();
        _inputMapping.Gameplay.CameraZoom.performed -= OnCameraZoom;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, _targetFOV, Time.deltaTime * _cameraZoomSpeed);
    }

    private void HandleMovement()
    {
        Vector2 input = _inputMapping.Gameplay.CameraMovement.ReadValue<Vector2>();
        Vector3 forward = transform.forward * input.y;
        Vector3 right = transform.right * input.x;
        
        forward.y = 0;
        right.y = 0;
        Vector3 movementDirection = (forward + right).normalized;
        gameObject.transform.position += movementDirection * Time.deltaTime * _movementSpeed;
    }

    private void OnCameraZoom(InputAction.CallbackContext context)
    {
        if (!_isZoomedIn)
        {
            ChangeFOV(0.33f);
        }
        else
        {
            ResetCamera();
        }
        _isZoomedIn = !_isZoomedIn;
    }

    public void ResetCamera()
    {
        _targetFOV = _originalFOV;
    }

    public void SetCameraFOV(float targetFOV)
    {
        _targetFOV = targetFOV;
    }
    public void ChangeFOV(float multiplier)
    {
        _targetFOV *= multiplier;
    }
}

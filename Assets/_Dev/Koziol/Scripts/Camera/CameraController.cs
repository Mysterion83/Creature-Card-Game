using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private const float MIN_PITCH = 20f;
    private const float MAX_PITCH = 89.99f;

    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _originalFOV;
    [SerializeField] private float _targetFOV;
    [SerializeField] private float _cameraZoomSpeed = 10f;
    [SerializeField] private bool _isZoomedIn = false;

    [HideInInspector] private float _xRotation = 0f;
    [HideInInspector] private float _yRotation = 0f;
    [SerializeField] private float _sensitivity = 0.2f;

    private GameControls _inputMapping;

    private void Awake()
    {
        _inputMapping = new GameControls();
        _originalFOV = Camera.main.fieldOfView;
        _targetFOV = Camera.main.fieldOfView;
        //sets initial values on current rotation - avoids snapping camera to 0,0,0 when rotation
        _xRotation = transform.localEulerAngles.x;
        _yRotation = transform.localEulerAngles.y;
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
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, _targetFOV, Time.deltaTime * _cameraZoomSpeed);
        HandleMovement();
        if (_inputMapping.Gameplay.EnableCameraRotation.IsPressed()) HandleRotation();
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
    private void HandleRotation()
    {
        Vector2 mouseDelta = _inputMapping.Gameplay.CameraRotation.ReadValue<Vector2>();

        _xRotation -= mouseDelta.y * _sensitivity;
        _yRotation += mouseDelta.x * _sensitivity;
        _xRotation = Mathf.Clamp(_xRotation, MIN_PITCH, MAX_PITCH);

        transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
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

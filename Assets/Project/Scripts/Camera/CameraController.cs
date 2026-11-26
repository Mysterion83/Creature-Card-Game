using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private const float MIN_PITCH = 40f;
    private const float MAX_PITCH = 89.99f;

    private const float BOARD_LINK_ATTEMPT_INTERVAL = 5f;
    private const float BOUNDS_BUFFER = 1f;

    private int _boardLinkAttempts = 10;

    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _originalFOV;
    [SerializeField] private float _targetFOV;
    [SerializeField] private float _cameraZoomSpeed = 10f;
    [SerializeField] private bool _isZoomedIn = false;

    [HideInInspector] private float _xRotation = 0f;
    [HideInInspector] private float _yRotation = 0f;
    [SerializeField] private float _sensitivity = 0.2f;

    [SerializeField] private Board _gameBoard;
    [SerializeField] private Vector2[] _cameraBounds = new Vector2[2];

    private GameControls _inputMapping;

    private void Awake()
    {
        _inputMapping = new GameControls();
        _originalFOV = Camera.main.fieldOfView;
        _targetFOV = Camera.main.fieldOfView;
        //sets initial values on current rotation - avoids snapping camera to 0,0,0 when rotating
        _xRotation = transform.localEulerAngles.x;
        _yRotation = transform.localEulerAngles.y;
    }

    private void Start()
    {
        AttemptBoardLink();
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
        Vector3 forward = (transform.forward * input.y);
        Vector3 right = transform.right * input.x;
        forward.y = 0;
        right.y = 0;
        Vector3 movementDirection = (forward + right).normalized;
        gameObject.transform.position += movementDirection * Time.deltaTime * _movementSpeed;
        BoundsCheck();
    }
    private void BoundsCheck()
    {
        if (_gameBoard == null) return;
        if (transform.position.x < _cameraBounds[0].x) transform.position = new Vector3(_cameraBounds[0].x, transform.position.y, transform.position.z);
        if (transform.position.x > _cameraBounds[1].x) transform.position = new Vector3(_cameraBounds[1].x, transform.position.y, transform.position.z);
        if (transform.position.z < _cameraBounds[0].y) transform.position = new Vector3(transform.position.x, transform.position.y, _cameraBounds[0].y);
        if (transform.position.z > _cameraBounds[1].y) transform.position = new Vector3(transform.position.x, transform.position.y, _cameraBounds[1].y);
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

    public void AttemptBoardLink()
    {
        _gameBoard = FindObjectOfType<Board>();
        if (_gameBoard == null)
        {
            _boardLinkAttempts--;
            if (_boardLinkAttempts <= 0)
            {
                Debug.LogError("CameraController: Failed to link to Board after multiple attempts.");
                return;
            }
            Invoke(nameof(AttemptBoardLink), BOARD_LINK_ATTEMPT_INTERVAL);
        }
        else
        {
            Debug.Log("CameraController: Successfully linked to Board.");
            MapData mapData = _gameBoard.GetData();
            _cameraBounds[0] = new Vector2(0 - mapData.MapSize.x * 0.5f - BOUNDS_BUFFER, 0 - mapData.MapSize.y * 0.5f - BOUNDS_BUFFER);
            _cameraBounds[1] = new Vector2(mapData.MapSize.x * 0.5f + BOUNDS_BUFFER, mapData.MapSize.y * 0.5f + BOUNDS_BUFFER);
        }
    }
}

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 5f;

    private GameControls _inputMapping;

    private void Awake()
    {
        _inputMapping = new GameControls();
    }

    private void OnEnable()
    {
        _inputMapping.Enable();
    }
    private void OnDisable()
    {
        _inputMapping.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
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
}

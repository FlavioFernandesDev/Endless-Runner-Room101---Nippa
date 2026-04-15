using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 6;
    public float horizontalSpeed = 3;
    
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private Animator _animator;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponentInChildren<Animator>();
        if (_playerInput != null && _playerInput.actions != null)
        {
            // Assuming your Action Map is named "Player" and the Action is named "Move"
            _moveAction = _playerInput.actions["Move"];
        }
    }

    void Update()
    {
        transform.Translate(Vector3.forward * playerSpeed * Time.deltaTime, Space.World);

        if (_animator != null)
        {
            // Since it's an endless runner moving forward, we set speed to 1 to play run animation
            _animator.SetFloat("Speed", playerSpeed);
        }

        // Read the Vector2 value (A/D or Left/Right arrows)
        if (_moveAction != null)
        {
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            
            if (moveInput.x < 0) // Left
            {
                transform.Translate(Vector3.left * horizontalSpeed * Time.deltaTime);
            }
            else if (moveInput.x > 0) // Right
            {
                transform.Translate(Vector3.right * horizontalSpeed * Time.deltaTime);
            }
        }
    }
}
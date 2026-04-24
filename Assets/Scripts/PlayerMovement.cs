using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float playerSpeed = 6;
    public float maxSpeed = 15;
    public float acceleration = 0.1f;
    public float laneChangeSpeed = 10f;
    public float jumpForce = 5; 
    public float minX = -2;
    public float maxX = 1.7f;
    public float groundedRayDistance = 1.2f;
    public float inputDeadzone = 0.5f;
    public float deathReturnDelay = 1.5f;
    public float protectedHitSpeedMultiplier = 0.6f;
    public float protectedHitDuration = 0.5f;
    public float protectedDoorGraceDuration = 0.6f;
    
    [Header("Estado do Jogador")]
    [SerializeField] bool isRunning;
    [SerializeField] bool isGrounded;
    [SerializeField] bool isDead;
    [SerializeField] int currentLane = 1;

    [Header("Referências (Arraste aqui no Inspector)")]
    public Animator _animator;        // Arraste o modelo do boneco (Filho)
    public GameObject painelBotoesMenu; // Arraste o painel com Start/Sair

    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private Rigidbody _rb;
    private bool _jumpQueued;
    private bool _holdingLaneInput;
    private float _targetLaneX;
    private float _protectedHitUntil;
    private float _protectedDoorGraceUntil;

    void Start()
    {
        if (_animator != null)
        {
            _animator.ResetTrigger("Jump");
            _animator.Play("Running", 0, 0f);
        }

        currentLane = Mathf.Clamp(currentLane, 0, 2);
        _targetLaneX = GetLanePosition(currentLane);
        Vector3 startPosition = transform.position;
        startPosition.x = _targetLaneX;
        transform.position = startPosition;
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        _playerInput = GetComponent<PlayerInput>();
        
        if (_playerInput != null && _playerInput.actions != null)
        {
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
        }
    }

    void Update()
    {
        if (isDead || RunManager.Instance.IsPaused)
        {
            if (_animator != null)
            {
                _animator.SetFloat("Speed", 0f);
            }

            return;
        }

        if (playerSpeed < maxSpeed)
        {
            playerSpeed += acceleration * Time.deltaTime;
        }

        if (!isRunning)
        {
            isRunning = true;
        }

        if (_animator != null)
        {
            _animator.SetFloat("Speed", GetEffectiveForwardSpeed());
        }

        HandleLaneInput();

        if (_jumpAction != null && _jumpAction.triggered && isGrounded)
        {
            _jumpQueued = true;
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        UpdateGroundedState();

        Vector3 velocity = _rb.linearVelocity;
        float targetX = _targetLaneX - _rb.position.x;
        float lateralVelocity = Mathf.Clamp(targetX * laneChangeSpeed, -laneChangeSpeed, laneChangeSpeed);
        float effectiveForwardSpeed = GetEffectiveForwardSpeed();

        velocity.x = lateralVelocity;
        velocity.z = effectiveForwardSpeed;
        _rb.linearVelocity = velocity;

        if (_jumpQueued && isGrounded)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (_animator != null) 
            {
                _animator.SetTrigger("Jump");
            }
        }

        _jumpQueued = false;
        RunManager.Instance.SetForwardSpeed(effectiveForwardSpeed);
        RunManager.Instance.AddDistance(effectiveForwardSpeed * Time.fixedDeltaTime);
    }

    private void HandleLaneInput()
    {
        if (_moveAction == null)
        {
            return;
        }

        float horizontalInput = _moveAction.ReadValue<Vector2>().x;
        if (Mathf.Abs(horizontalInput) < inputDeadzone)
        {
            _holdingLaneInput = false;
            return;
        }

        if (_holdingLaneInput)
        {
            return;
        }

        _holdingLaneInput = true;
        currentLane = Mathf.Clamp(currentLane + (horizontalInput > 0 ? 1 : -1), 0, 2);
        _targetLaneX = GetLanePosition(currentLane);
    }

    private float GetLanePosition(int laneIndex)
    {
        if (laneIndex <= 0)
        {
            return minX;
        }

        if (laneIndex >= 2)
        {
            return maxX;
        }

        return (minX + maxX) * 0.5f;
    }

    private void UpdateGroundedState()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundedRayDistance);
    }

    private float GetEffectiveForwardSpeed()
    {
        bool protectedHitActive = Time.time < _protectedHitUntil;
        return protectedHitActive ? playerSpeed * protectedHitSpeedMultiplier : playerSpeed;
    }

    private void ApplyProtectedDoorPenalty()
    {
        _protectedHitUntil = Time.time + protectedHitDuration;
        _protectedDoorGraceUntil = Time.time + protectedDoorGraceDuration;
    }

    public bool TryHandleProtectedDoorHit(Transform hitTransform)
    {
        if (hitTransform == null)
        {
            return false;
        }

        RandomDoor door = hitTransform.GetComponentInParent<RandomDoor>();
        if (door == null)
        {
            return false;
        }

        if (Time.time < _protectedDoorGraceUntil)
        {
            return true;
        }

        if (!door.IsOpen || !RunManager.Instance.TryConsumeKey())
        {
            return false;
        }

        if (!door.TryConsumeDoorHit())
        {
            RunManager.Instance.AddKey();
            return false;
        }

        ApplyProtectedDoorPenalty();
        return true;
    }

    private bool TryHandleProtectedDoorCollision(Collision collision)
    {
        return TryHandleProtectedDoorHit(collision.transform);
    }

    public void HandleFatalCollision(bool applyImpact = true)
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isRunning = false;
        playerSpeed = 0f;
        RunManager.Instance.EndRun();

        if (painelBotoesMenu != null)
        {
            painelBotoesMenu.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (_rb != null)
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _rb.linearVelocity = Vector3.zero;
            if (applyImpact)
            {
                _rb.AddForce(new Vector3(0f, 4f, -4f), ForceMode.Impulse);
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (isDead || RunManager.Instance.IsGameOver)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (TryHandleProtectedDoorCollision(collision))
            {
                return;
            }

            HandleFatalCollision();
            StartCoroutine(GameOverTransition.Play(null, deathReturnDelay));
        }
    }
}

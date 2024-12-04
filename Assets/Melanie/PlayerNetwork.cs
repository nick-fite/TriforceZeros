using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

/*public struct PlayerStats : INetworkSerializeByMemcpy
{
    public ForceNetworkSerializeByMemcpy<float> MoveSpeed;
    public ForceNetworkSerializeByMemcpy<float> TurnSpeed;
    public ForceNetworkSerializeByMemcpy<float> JumpHeight;
    public ForceNetworkSerializeByMemcpy<bool> BCanMove;
    public ForceNetworkSerializeByMemcpy<Vector2> RawMovementInput;

    public ForceNetworkSerializeByMemcpy<float> Gravity;
    public ForceNetworkSerializeByMemcpy<bool> BIsGrounded;
    public ForceNetworkSerializeByMemcpy<Vector3> PlayerVelocity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref MoveSpeed);
        serializer.SerializeValue(ref TurnSpeed);
        serializer.SerializeValue(ref JumpHeight);
        serializer.SerializeValue(ref BCanMove);
        serializer.SerializeValue(ref RawMovementInput);

        serializer.SerializeValue(ref Gravity);
        serializer.SerializeValue(ref BIsGrounded);
        serializer.SerializeValue(ref PlayerVelocity);
    }
}*/

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkAnimator))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerNetwork : NetworkBehaviour, ITeamInterface
{
    public delegate void OnUpdateAllConnectionsDelegate();
    public OnUpdateAllConnectionsDelegate OnUpdateAllConnections;

    public delegate void OnPlayerDisconnectDelegate();
    public OnPlayerDisconnectDelegate OnPlayerDisconnect;

    ///[Actions and Animator dependencies]
    Animator _animator;
    PlayerInput _playerInput;
    private MultiplayerInputAction _multiplayerInputAction;
    private CharacterController _characterController;


    ///[Additional Components]
    DamageComponent _damageComponent;

    [Header("Player Options")]
    //private PlayerStats _playerStats;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 2f;
    Vector2 _rawMovementInput;
    bool _bCanMove = true; //<-- if we want to lock the player movement at some point

    [SerializeField] private float jumpHeight = 3f;
    private bool _bHasJumped = false;

    private bool _bIsGrounded;
    private float _gravity = -9.81f;
    private Vector3 _playerVelocity;

    [Header("Other [Read Only]")]
    [SerializeField ]private IinteractionInteface _targetInteractible;

    public void SetTargetInteractible(GameObject objectToSet) 
    {
        if (objectToSet == null)
        {
            _targetInteractible = null;
            return;
        }
        IinteractionInteface targetInteractible = objectToSet.GetComponent<IinteractionInteface>();
        if (targetInteractible == null) 
        {
            _targetInteractible = null;
            return;
        }
        _targetInteractible = targetInteractible;
    }

    private void Awake()
    {
        //UpdatePlayerStats(_playerStats);
        OnUpdateAllConnections += UpdateAllConnections;

        _multiplayerInputAction = new MultiplayerInputAction();

        if (IsLocalPlayer)
        { 
            _multiplayerInputAction.Enable();
            //InputSystem.settings.defaultDeadzoneMax = 0.924f;  //<-- fixes client not moving w/ new input system bug
        }
        _playerInput = GetComponent<PlayerInput>();
        _bCanMove = true;

        _animator = GetComponentInChildren<Animator>();
        _characterController = GetComponent<CharacterController>();
        _damageComponent = GetComponent<DamageComponent>();
    }
    /*public void UpdatePlayerStats(PlayerStats playerStats) 
    {
        if (IsServer && IsLocalPlayer)
        {
            UpdateLocalPlayerStats(_playerStats);
        }
        else if (IsClient && IsLocalPlayer)
        {
            UpdatePlayerStatsServerRpc(_playerStats);
        }
    }
    [Rpc(SendTo.Server)]
    public void UpdatePlayerStatsServerRpc(PlayerStats playerStats) 
    {
        UpdatePlayerStatsClientRpc(_playerStats);
    }
    [Rpc(SendTo.Everyone)]
    public void UpdatePlayerStatsClientRpc(PlayerStats playerStats)
    {
        UpdateLocalPlayerStats(_playerStats);
    }

    private void UpdateLocalPlayerStats(PlayerStats playerStats)
    {
        _playerStats.MoveSpeed = moveSpeed;
        _playerStats.TurnSpeed = turnSpeed;
        _playerStats.JumpHeight = jumpHeight;
        _playerStats.BCanMove = _bCanMove;

        _playerStats.Gravity = _gravity;
        _playerStats.BIsGrounded = _bIsGrounded;
        _playerStats.PlayerVelocity = _playerVelocity;
    }*/

    public void Start()
    {
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject player = client.PlayerObject.gameObject;
            if (!player || player == gameObject)
            {
                continue;
            }
            Debug.Log(player.name);
            PlayerNetwork playerNetwork = player.GetComponent<PlayerNetwork>();
            if (playerNetwork)
            {
                Debug.Log("Player network test found!!");
                playerNetwork.OnUpdateAllConnections?.Invoke();
            }
        }
    }
    private void Update()
    {
        if (!_characterController) { return; }

        _bIsGrounded = _characterController.isGrounded;
    }

    private void FixedUpdate()
    {
        if (!_characterController) { return; }

        ProcessAllMovement(_rawMovementInput, moveSpeed);
    }

    private void ProcessAllMovement(Vector2 rawInput, float movementSpeed)
    {
        if (IsServer && IsLocalPlayer)
        {
            ProcessAllMovementServerRpc(rawInput, movementSpeed);
        }
        else if (IsClient && IsLocalPlayer)
        { 
            ProcessAllMovementServerRpc(rawInput, movementSpeed);
        }
    }
    [Rpc(SendTo.Server)]
    private void ProcessAllMovementServerRpc(Vector2 rawInput, float movementSpeed) 
    {
        ProcessAllMovementClientRpc(rawInput, movementSpeed);
    }
    [Rpc(SendTo.Everyone)]
    private void ProcessAllMovementClientRpc(Vector2 rawInput, float movementSpeed)
    {
        ProcessLocalMovement(rawInput, movementSpeed);
    }
    private void ProcessLocalMovement(Vector2 rawInput, float movementSpeed) 
    {
        ProcessMove(rawInput, movementSpeed);
        ProcessGravity();
    }

    private void ProcessMove(Vector2 rawInput, float movementSpeed)
    {
        if (!_bCanMove) { return; }

        //Moving character
        Vector3 movementValue = new Vector3(rawInput.x, 0, rawInput.y);
        Vector3 moveInDirection = transform.TransformDirection(movementValue);
        _characterController.Move(moveInDirection * (movementSpeed * Time.deltaTime));

        //animation
        if (!_animator) { return; }

        if (movementValue != Vector3.zero)
        {
            _animator.SetBool("walking", true);
        }
        else
        { 
            _animator.SetBool("walking", false);
        }

        //rotating whole character in movement direction
        /*Quaternion goalRotation = Quaternion.LookRotation(moveInDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, goalRotation, Time.deltaTime * turnSpeed);*/
    }
    private void ProcessGravity()
    {
        _playerVelocity.y += Time.deltaTime * _gravity;

        if (_bIsGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = -2f;
        }
        _characterController.Move(Time.deltaTime * _playerVelocity);
    }

    private void UpdateAllConnections()
    {
        GetComponent<NameComponent>().RefreshName();
        GetComponent<HealthComponent>().RefreshHealth();
        //UpdatePlayerStats(_playerStats);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsLocalPlayer)
        {
            _multiplayerInputAction.Disable();
        }
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        OnPlayerDisconnect?.Invoke();
    }

    public void MoveAction(InputAction.CallbackContext context) 
    {
        if (!IsLocalPlayer) { return; }
        Debug.Log("Performed!!");
        if (context.performed)
        {
            _rawMovementInput = context.ReadValue<Vector2>();
        }
        if (context.canceled)
        {
            _rawMovementInput = Vector2.zero;
        }
    }
    public void AttackAction(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) { return; }

        if (context.started && _damageComponent)
        {
            _damageComponent.DoDamage();
        }
    }
    public void InteractAction(InputAction.CallbackContext context)//might change to pickup/throw
    {
        if (!IsLocalPlayer) { return; }

        if (context.started && _targetInteractible != null)
        {
            _targetInteractible.InteractAction(gameObject);
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref moveSpeed);
    }
}

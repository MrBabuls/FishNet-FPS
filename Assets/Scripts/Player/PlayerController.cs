using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PlayerController : NetworkBehaviour
{
    public static Dictionary<int, PlayerController> Players = new Dictionary<int, PlayerController>();
    
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.75f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    [SerializeField] private int playerSelfLayer = 7;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    
    [HideInInspector] 
    public bool canMove = true;

    [SerializeField] 
    private float cameraYOffset = 0.4f;
    private Camera playerCamera;

    [Header("Animator setup")] 
    public Animator anim;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Players.Add(OwnerId, this);
        PlayerManager.InitializeNewPlayer(OwnerId);
        UIManager.PlayerJoined(OwnerId);
        
        if (base.IsOwner)
        {
            playerCamera = Camera.main;
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset,
                transform.position.z);
            playerCamera.transform.SetParent(transform);

            if (TryGetComponent(out PlayerWeapon playerWeapon))
                playerWeapon.InitializeWeapons(playerCamera.transform);

            gameObject.layer = playerSelfLayer;
            foreach (Transform child in transform)
            {
                child.gameObject.layer = playerSelfLayer;
            }
        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        
        Players.Remove(OwnerId);
        PlayerManager.PlayerDisconnected(OwnerId);
        UIManager.PlayerLeft(OwnerId);
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!canMove)
            return;
        
        bool isRunning = false;

        //press left shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);
        
        //we are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        
        //Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
        
        //Player and Camera rotation
        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    public static void SetPlayerPosition(int clientID, Vector3 position)
    {
        if (!Players.TryGetValue(clientID, out PlayerController player))
            return;
        
        player.SetPlayerPositionServer(position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerPositionServer(Vector3 position)
    {
        SetPlayerPositionTarget(Owner, position);
    }

    [TargetRpc]
    private void SetPlayerPositionTarget(NetworkConnection connection, Vector3 position)
    {
        transform.position = position;
    }
    
    public static void TogglePlayer(int clientID, bool toggle)
    {
        if (!Players.TryGetValue(clientID, out PlayerController player))
            return;
        
        player.TogglePlayerServer(toggle);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePlayerServer(bool toggle)
    {
        DisablePlayerObserver(toggle);
    }

    [ObserversRpc]
    private void DisablePlayerObserver(bool toggle)
    {
        canMove = toggle;

        if (TryGetComponent(out Renderer playerRenderer))
            playerRenderer.enabled = toggle;
        
        if (TryGetComponent(out CapsuleCollider collider))
            collider.enabled = toggle;

        characterController.enabled = toggle;

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in allRenderers)
        {
            rend.enabled = toggle;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerPickup : NetworkBehaviour
{
    [SerializeField] private float pickupRange = 4f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    [SerializeField] private LayerMask pickupLayers;

    private Transform _cameraTransform;

    private PlayerWeapon _playerWeapon;
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        _cameraTransform = Camera.main.transform;
        
        if (TryGetComponent(out PlayerWeapon pWeapon))
            _playerWeapon = pWeapon;
        else
            Debug.LogError("Couldn't find PlayerWeapon", gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pickupKey))
            Pickup();
    }

    private void Pickup()
    {
        if (!Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, pickupRange,
                pickupLayers))
            return;

        if (hit.transform.TryGetComponent(out GroundWeapon groundWeapon))
        {
            _playerWeapon.InitializeWeapon(groundWeapon.PickupWeapon());
        }
    }
}

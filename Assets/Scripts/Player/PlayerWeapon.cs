using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private List<APlayerWeapon> weapons = new List<APlayerWeapon>();

    [SerializeField] private APlayerWeapon currentWeapon;

    private readonly SyncVar<int> _currentWeaponIndex = new(-1);

    private void Awake()
    {
        _currentWeaponIndex.OnChange += OnCurrentWeaponIndexChanged;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            enabled = false;
            return;
        }
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
            FireWeapons();
    }

    public void InitializeWeapons(Transform parentOfWeapons)
    {
        for (int i = 0; i < weapons.Count; i++)
            weapons[i].transform.SetParent(parentOfWeapons);
        
        InitializeWeapon(0);
    }

    public void InitializeWeapon(int weaponIndex)
    {
        SetWeaponIndex(weaponIndex);
    }

    [ServerRpc]
    private void SetWeaponIndex(int weaponIndex) => _currentWeaponIndex.Value = weaponIndex;

    private void OnCurrentWeaponIndexChanged(int oldIndex, int newIndex, bool asServer)
    {
        for (int i = 0; i < weapons.Count; i++)
            weapons[i].gameObject.SetActive(false);

        if (weapons.Count > newIndex)
        {
            currentWeapon = weapons[newIndex];
            currentWeapon.gameObject.SetActive(true);
        }
    }

    private void FireWeapons()
    {
        currentWeapon.Fire();
    }
}

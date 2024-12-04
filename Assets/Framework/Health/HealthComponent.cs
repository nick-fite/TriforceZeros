using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


/*public struct HealthInfo : INetworkSerializable
{
    int _health;
    int _maxHealth;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _health);
        serializer.SerializeValue(ref _maxHealth);
    }
}*/

public class HealthComponent : NetworkBehaviour, INetworkSerializable
{
    public delegate void OnHealthChangedDelegate(float newHealth, float delta, float maxHealth);
    public delegate void OnDeadDelegate();
    public event OnHealthChangedDelegate OnHealthChanged;
    public event OnHealthChangedDelegate OnTakenDamage;
    public event OnDeadDelegate OnDead;
    
    [SerializeField] private float maxHealth = 100;
    private float _health;

    [SerializeField] Button debugDamageBtnPrefab;
    Button damageBtn;
   
    private void Awake()
    {
        Debug.Log($"Awake b4 statement: h={_health}, mH={maxHealth}");
       _health = maxHealth;
        Debug.Log($"Awake aftr statement: h={_health}, mH={maxHealth}");

        /*GameObject layout = GameObject.Find("Debug");
        damageBtn = Instantiate(debugDamageBtnPrefab, layout.transform);

        if (damageBtn)
        {
            damageBtn.onClick.AddListener(() =>
            {
                ChangeHealth(-10f);
            });
       }*/
    }
    public void RefreshHealth() 
    {
        if (IsServer && IsLocalPlayer)
        {
            RefreshHealthServerRpc(_health);
        }
        else if (IsClient && IsLocalPlayer)
        {
            RefreshHealthServerRpc(_health);
        }
    }

    [Rpc(SendTo.Server)]
    private void RefreshHealthServerRpc(float health)
    {
        RefreshHealthClientRpc(health);
    }

    [Rpc(SendTo.Everyone)]
    private void RefreshHealthClientRpc(float health)
    {
        RefreshCurrentHealth(health);
    }

    private void RefreshCurrentHealth(float health)
    {
        OnHealthChanged?.Invoke(health, 0, maxHealth);
    }

    public void ChangeHealth(float amt)
    {
        if (IsServer && IsLocalPlayer)
        {
            UpdateHealthServerRpc(amt);
        }
        else if (IsClient && IsLocalPlayer)
        { 
            UpdateHealthServerRpc(amt);
        }
    }
    [Rpc(SendTo.Server)]
    public void UpdateHealthServerRpc(float amt)
    {
        UpdateHealthClientRpc(amt);
    }
    [Rpc(SendTo.Everyone)]
    public void UpdateHealthClientRpc(float amt)
    {
        UpdateHealth(amt);
    }
    public void UpdateHealth(float amt)
    {
        Debug.Log($"Update Health: amt={amt}, health={_health}");
        if (_health == 0)
            return;

        _health = Mathf.Clamp(_health + amt, 0, maxHealth);
        if (amt < 0)
        {
            OnTakenDamage?.Invoke(_health, amt, maxHealth);//not used right now
        }
        OnHealthChanged?.Invoke(_health, amt, maxHealth);

        if (_health <= 0)
        {
            OnDead?.Invoke();
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _health);
        serializer.SerializeValue(ref maxHealth);
    }
}

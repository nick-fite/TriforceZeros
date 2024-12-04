using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TestPlayerNetwork : NetworkBehaviour, ITeamInterface
{
    public delegate void OnUpdateAllConnectionsDelegate();
    public OnUpdateAllConnectionsDelegate OnUpdateAllConnections;

    public delegate void OnPlayerDisconnectDelegate();
    public OnPlayerDisconnectDelegate OnPlayerDisconnect;

    [SerializeField] Animator anim;
    [SerializeField] BoxTriggerDamageComponent damageComponent;
    [SerializeField] ThrowComponent throwComponent;
    [SerializeField] float turnSpeed;

    public void Awake()
    {
        OnUpdateAllConnections += UpdateAllConnections;

    }

    public void Start()
    {
        anim = GetComponentInChildren<Animator>();
        Debug.Log(anim.name);
        damageComponent = GetComponent<BoxTriggerDamageComponent>();
        throwComponent = GetComponent<ThrowComponent>();

        /*NetworkManagerUI networkUI = FindFirstObjectByType<NetworkManagerUI>();
        NetworkString playerName = networkUI.GetPlayerName();//<-- only works on 
        GetComponent<NameComponent>().OnNameChanged?.Invoke(playerName);
        Debug.Log(playerName);*/

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject player = client.PlayerObject.gameObject;
            if (!player || player == gameObject)
            {
                continue;
            }
            Debug.Log(player.name);
            TestPlayerNetwork playerNetwork = player.GetComponent<TestPlayerNetwork>();
            if (playerNetwork)
            {
                Debug.Log("Player network test found!!");
                playerNetwork.OnUpdateAllConnections?.Invoke();
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 input = Vector2.zero;

        if(Input.GetKey(KeyCode.W)) input.y += 1f;
        if(Input.GetKey(KeyCode.S)) input.y -= 1f;
        if(Input.GetKey(KeyCode.D)) input.x += 1f;
        if(Input.GetKey(KeyCode.A)) input.x -= 1f;

        if(IsServer && IsLocalPlayer)
        {
            Move(input);
            RotFaceDirection(input);
        } else if (IsClient && IsLocalPlayer)
        {
            MovePlayerServerRpc(input);
            RotFaceDirectionServerRpc(input);
        }

        if (damageComponent && Input.GetKey(KeyCode.R)) 
        {
            Debug.Log("Try Damage");
            damageComponent.DoDamage();
        }

        if (throwComponent && Input.GetKey(KeyCode.E))
        {
            Debug.Log("Try Throw/Pickup");
            throwComponent.TryPickUpThrowableObject();
        }

    }

    [ServerRpc]
    public void MovePlayerServerRpc (Vector2 input)
    {
        Move(input);
    }


    public void Move(Vector2 input){
        if(input.x > 0 || input.x < 0 || input.y > 0 || input.y < 0)
        {
            anim.SetBool("walking", true);
        }
        else
            anim.SetBool("walking", false);

        float moveSpeed = 3f;
        Vector3 calcMove = input.x * transform.right + input.y * transform.forward;
        transform.position += calcMove * moveSpeed * Time.fixedDeltaTime;
    }

    [ServerRpc]
    public void RotFaceDirectionServerRpc(Vector2 input)
    { 
        RotFaceDirection(input);
    }
    private void RotFaceDirection(Vector2 input)
    {
        /*Vector3 movementVal = new Vector3(input.x, input.y, 0);
        Vector3 rotInDir = transform.TransformDirection(movementVal);
        Quaternion goalRot = Quaternion.LookRotation(rotInDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, goalRot, Time.deltaTime * turnSpeed);*/
    }

    private void UpdateAllConnections() 
    {
        GetComponent<NameComponent>().RefreshName();
        GetComponent<HealthComponent>().RefreshHealth();
    }

    public override void OnNetworkDespawn() 
    {
        base.OnNetworkDespawn();
        OnPlayerDisconnect?.Invoke();
    }
}

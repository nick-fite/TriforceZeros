using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TestPlayerNetwork : NetworkBehaviour
{
    [SerializeField] Animator anim;

    public void Start()
    {
        anim = GetComponentInChildren<Animator>();
        Debug.Log(anim.name);
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
        } else if (IsClient && IsLocalPlayer)
        {
            MovePlayerServerRpc(input);
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
}

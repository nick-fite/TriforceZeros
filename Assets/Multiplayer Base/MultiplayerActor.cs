using UnityEngine;

public class MultiplayerActor : MultiplayerObject
{
    [SerializeField] GameObject Obj;
    float moveSpeed = 3f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 input = Vector2.zero;

        if(Input.GetKey(KeyCode.W)) input.y += 1f;
        if(Input.GetKey(KeyCode.S)) input.y -= 1f;
        if(Input.GetKey(KeyCode.D)) input.x += 1f;
        if(Input.GetKey(KeyCode.A)) input.x -= 1f;

        Vector3 calcMove = input.x * transform.right + input.y * transform.forward;
        Vector3 newPos = transform.position;
        newPos += calcMove * moveSpeed * Time.fixedDeltaTime;
        if(IsServer && IsLocalPlayer)
        {
            transform.position = newPos;
        }
        else if (IsClient && IsLocalPlayer)
        {
            ChangePositionServerRpc(newPos);
        }
    }
}

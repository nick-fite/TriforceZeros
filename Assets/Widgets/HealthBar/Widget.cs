using UnityEngine;

public abstract class Widget : MonoBehaviour
{
    private GameObject owner;

    public virtual void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }
}

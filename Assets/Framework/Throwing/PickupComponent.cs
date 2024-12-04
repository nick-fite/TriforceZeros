using UnityEngine;

public class PickupComponent : MonoBehaviour, IinteractionInteface
{
    private bool _bIsPickedUp = false;

    public void ProcessPickup(bool stateToSet) 
    {
        if (stateToSet)
        {
            PickupAction();
        }
        else 
        {
            ReleaseAction();
        }
    }

    public virtual void PickupAction()
    {
        _bIsPickedUp=true;//disable other player movement?
        Debug.Log("PickupComp: PickedUp");
        //DEBUG
        GetComponent<Rigidbody>().useGravity = false;
    }

    public virtual void ReleaseAction() 
    {
        _bIsPickedUp=false;//renable player movement here?
        Debug.Log("PickupComp: released");
        GetComponent<Rigidbody>().useGravity = true;
    }

    public void InteractAction(GameObject interactor)
    {
        _bIsPickedUp = !_bIsPickedUp;

        if (interactor.GetComponent<ThrowComponent>().TryPickUpThrowableObject())
        {
            ProcessPickup(_bIsPickedUp);
        }
    }
    public bool ShouldInteract(GameObject interactor)
    {
        if (interactor != gameObject)
        {
            return true;
        }

        return false;
    }
}

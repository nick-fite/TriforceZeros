using UnityEngine;

public interface IinteractionInteface
{
    public bool ShouldInteract(GameObject interactor) 
    {
        return false;//<-- so unity wont be upset (gets overridden)
    }
    public void InteractAction(GameObject interactor) { }
    public void StartInteract() { }
    public void StopInteract() { }
}

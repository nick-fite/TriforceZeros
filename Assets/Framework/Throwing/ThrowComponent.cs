using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThrowComponent : MonoBehaviour
{
    [SerializeField] private Transform holdTransform;
    [SerializeField]private Transform heldObjTransform;

    [SerializeField] private float holdSpeed = 2f;
    private bool _bIsHolding = false;

    private List<GameObject> _currentOverlappingTargets = new List<GameObject>();

    private void Update()
    {
        if (_bIsHolding)
        {
            HoldObject();
        }
    }

    private void HoldObject()
    {
        if (holdTransform && heldObjTransform)
        {
            heldObjTransform.position = Vector3.Slerp(heldObjTransform.position, holdTransform.position, holdSpeed * Time.deltaTime);
        }
    }
    private bool ShouldHoldObject(GameObject other) 
    {
        return !_currentOverlappingTargets.Contains(other.gameObject);
    }
    public bool TryPickUpThrowableObject()
    {
        if (!heldObjTransform)
        {
            return false;
        }
        _bIsHolding = !_bIsHolding;
        return true;
    }
    public void OnTriggerEnter(Collider other)
    {
        PickupComponent pickUpComponent = other.GetComponent<PickupComponent>();
        if (pickUpComponent == null)
        {
            return;
        }

        if (ShouldHoldObject(other.gameObject) && pickUpComponent.ShouldInteract(this.gameObject))
        {
            Debug.Log("Should pickup");
            _currentOverlappingTargets.Add(other.gameObject);
            heldObjTransform = other.gameObject.transform;

            GetComponent<PlayerNetwork>().SetTargetInteractible(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (heldObjTransform == null || heldObjTransform.gameObject != other.gameObject)
        {
            return;//held object is still in range.
        }
        _currentOverlappingTargets.Remove(other.gameObject);

        if (_currentOverlappingTargets.Count > 0)
        {
            heldObjTransform = _currentOverlappingTargets[0].transform;
            GetComponent<PlayerNetwork>().SetTargetInteractible(heldObjTransform.gameObject);
        }
        else
        {
            heldObjTransform = null;
            GetComponent<PlayerNetwork>().SetTargetInteractible(null);
        }
    }
}

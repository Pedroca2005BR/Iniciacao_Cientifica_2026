using Pedroca2005BR.Utilities;
using System;
using UnityEngine;

public class FireExtinguisher : MonoBehaviour, IInteractable
{
    [Header("EPI info")]
    public EPIType type;

    [Header("Extinguisher info")]
    [SerializeField] private ExtinguisherMixture mixtureController;
    public ExtinguisherMixture.MixtureType mixtureType;

    public EPIType Type { get => type; }

    

    public void Select()
    {
        ConnectorUtils.DetachObject(gameObject);
        EventManager.TriggerEvent("ItemPickedUp", gameObject);
    }

    public void Deselect()
    {
        EventManager.TriggerEvent("ItemDropped", gameObject);
        Deactivate();
    }

    public void Activate()
    {
        mixtureController.PlayMixture(mixtureType);
    }

    public void Deactivate()
    {
        mixtureController.StopMixture();
    }

    public GameObject GetObject()
    {
        return gameObject;
    }
}

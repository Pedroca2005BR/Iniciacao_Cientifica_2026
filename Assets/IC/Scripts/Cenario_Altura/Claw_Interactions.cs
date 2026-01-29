using UnityEngine;
using System.Collections.Generic;
using Pedroca2005BR.Utilities;

//[RequireComponent(typeof(SphereCollider))]
public class Claw_Interactions : MonoBehaviour, IInteractable
{
    //[SerializeField] private List<Material> sucessMaterial;
    //[SerializeField] private List<Material> phantomMaterial;

    //private GameObject phantomClaw;

    private Rigidbody rb;

    [Header("Objects to render and swap")]
    [SerializeField] GameObject garraAberta;
    [SerializeField] GameObject garrafechada;

    [Header("EPI info")]
    public EPIType type;

    public EPIType Type { get => type; }

    public void Activate()
    {
        Select();
        garraAberta.SetActive(true);
        garrafechada.SetActive(false);
        EventManager.TriggerEvent("ItemPickedUp", gameObject);
        //Debug.Log("Abriu garra!");
    }

    public void Deactivate()
    {
        garrafechada.SetActive(true);
        garraAberta.SetActive(false);
        EventManager.TriggerEvent("ItemDropped", gameObject);
        //Debug.Log("Fechou garra!");
    }

    public void Deselect()
    {
        Deactivate();
        //EventManager.TriggerEvent("ItemDropped", gameObject);
    }

    public void Select()
    {
        ConnectorUtils.DetachObject(gameObject);
    }

    public GameObject GetObject()
    {
        return gameObject;
    }
}

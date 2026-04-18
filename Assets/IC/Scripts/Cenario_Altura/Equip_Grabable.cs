using UnityEngine;

public class Equip_Grabable : MonoBehaviour, IInteractable
{
    public EPIType Type => type;
    EPIType type = EPIType.Colete;

    [SerializeField] GameObject XROrigin;

    public void Select()
    {
        Equip();
    }

    public void Deselect()
    {
        
    }

    public void Activate()
    {
        
    }

    public void Deactivate()
    {
        
    }

    public GameObject GetObject()
    {
        return gameObject;
    }


    void Equip()
    {

    }
}

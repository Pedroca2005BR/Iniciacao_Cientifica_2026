using Pedroca2005BR.EquipmentSystem;
using UnityEngine;

public class Equip_Grabable : MonoBehaviour, IInteractable
{
    public EPIType Type => type;
    EPIType type = EPIType.Colete;

    [SerializeField] Equipment equipment;

    public void Select()
    {
        
    }

    public void Deselect()
    {
        
    }

    public void Activate()
    {
        Equip();
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
        EquipmentManager.Instance.EquipItem(equipment);
    }
}

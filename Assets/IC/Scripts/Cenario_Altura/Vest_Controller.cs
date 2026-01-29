using UnityEngine;

public class Vest_Controller : MonoBehaviour, IInteractable
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
        throw new System.NotImplementedException();
    }

    public void Activate()
    {
        throw new System.NotImplementedException();
    }

    public void Deactivate()
    {
        throw new System.NotImplementedException();
    }

    public GameObject GetObject()
    {
        throw new System.NotImplementedException();
    }


    void Equip()
    {
        ConnectorUtils.AttachObjects(gameObject, XROrigin);
    }
}

using Pedroca2005BR.Utilities;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(SphereCollider))]
public class PhantomObjectController : MonoBehaviour
{
    [Header("Phantom Mechanics")]
    public EPIType type;

    //public GameObject baseObject;

    [Tooltip("0 = phantom;\n1 = success")]
    public Material[] materials;

    public Renderer rend;

    private bool isOnRange = false; // Usado para saber se o objeto esta na area

    private int itemsOnHand = 0;

    // Snap helpers
    public GameObject ObjectConnected { get; set; } = null;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.TryGetComponent<IInteractable>(out IInteractable component))
        {
            if (component.Type == type)
            {
                TryChangeMaterial(1);
                isOnRange = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent.TryGetComponent<IInteractable>(out IInteractable component))
        {
            if (component.Type == type)
            {
                TryChangeMaterial(0);
                isOnRange = false;
            }
        }
    }

    public bool TryChangeMaterial(int index)
    {
        if (materials == null || materials.Length <= index)
        {
            return false;
        }

        rend.material = materials[index];
        return true;
    }


    private void SnapToPosition(GameObject obj)
    {
        Debug.Log($"{obj.name}: Snap preparado!");
        ObjectConnected = obj;
        rend.enabled = false;
        ConnectorUtils.AttachObjects(obj, gameObject);
    }

    


    /// EVENT SYSTEM -------------------------------------------------------------------------------

    private void OnEnable()
    {
        EventManager.Subscribe("ItemDropped", OnItemDropped);
        EventManager.Subscribe("ItemPickedUp", OnItemPickedUp);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe("ItemDropped", OnItemDropped);
        EventManager.Unsubscribe("ItemPickedUp", OnItemPickedUp);

    }

    public void OnItemDropped(object parameter)
    {
        if (parameter.GetType() != typeof(GameObject))
        {
            Debug.LogError("Passaram parametro errado");
            return;
        }

        GameObject obj = (GameObject)parameter;

        if (!obj.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            // Se nao for um interativo especial, nao importa para esse codigo
            return;
        }
        // Se nao for do tipo do fantasma, nao importa tambem
        if (interactable.Type != type)
        {
            return;
        }

        // Permite a conexao entre objetos fantasmas e reais
        //ConnectorUtils.IgnoreAllCollisionsBetween(gameObject, obj, true);
        itemsOnHand--;  // controla o comportamento do renderer

        // Desativar o renderer do objeto faz ele sumir visualmente
        //if (itemsOnHand < 0) Debug.LogWarning("Não tem objetos suficiente!");
        if (itemsOnHand <= 0)
            rend.enabled = false;
        if (isOnRange && ObjectConnected == null)
        {
            SnapToPosition(interactable.GetObject());
        }
    }

    public void OnItemPickedUp(object parameter)
    {
        //Debug.Log("Entrou!");

        if (parameter.GetType() != typeof(GameObject))
        {
            Debug.LogError("Passaram parametro errado");
            return;
        }

        GameObject obj = (GameObject)parameter;

        if (!obj.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            // Se nao for um interativo especial, nao importa para esse codigo
            return;
        }
        // Se nao for do tipo do fantasma, nao importa tambem
        if (interactable.Type != type)
        {
            return;
        }

        // Permite a conexao entre objetos fantasmas e reais
        //ConnectorUtils.IgnoreAllCollisionsBetween(gameObject, obj, false);
        itemsOnHand++;  // controla o comportamento do renderer

        // Desativar o renderer do objeto faz ele sumir visualmente
        if (ObjectConnected == null)
        {
            rend.enabled = true;
            if (isOnRange) TryChangeMaterial(1);
            else TryChangeMaterial(0);
        }   
    }
}

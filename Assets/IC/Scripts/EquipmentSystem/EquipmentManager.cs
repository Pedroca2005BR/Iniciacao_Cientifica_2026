using UnityEngine;
using System.Collections.Generic;

namespace Pedroca2005BR.EquipmentSystem
{
    public class EquipmentManager : MonoBehaviour
    {
        #region Singleton
        public static EquipmentManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        public List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();

        public void EquipItem(Equipment item)
        {
            if (item == null)
            {
                Debug.LogWarning("Attempted to equip a null item.");
                return;
            }
            else if (IsSlotOccupied(item.equipmentSlotType))
            {
                Debug.LogWarning("Attempted to equip an item in an occupied slot.");
                return;
            }

            // Implement logic to equip the item, e.g., apply player stats, visuals, etc.
            var slot = GetSlotByType(item.equipmentSlotType);
            
            GameObject itemObject = Instantiate(item.gameObject, slot.offset.position, slot.offset.rotation, slot.offset);
            

            itemObject.GetComponent<Equipment>().OnEquip();
            


            Debug.Log($"Equipped: {item.itemName}");
        }

        public void UnequipItem(Equipment item)
        {
            if (IsSlotOccupied(item.equipmentSlotType))
            {
                var slot = GetSlotByType(item.equipmentSlotType);
                if (slot.currentEquipment != null)
                {
                    slot.currentEquipment.OnUnequip();
                    Destroy(slot.currentEquipment.gameObject);
                    slot.currentEquipment = null;
                }
            }


            Debug.Log($"Unequipped: {item.itemName}");
        }




        private bool IsSlotOccupied(EquipmentSlotType slotType)
        {
            var slot = GetSlotByType(slotType);
            if (slot != null)
            {
                return slot.currentEquipment != null;
            }
            return false;
        }

        private EquipmentSlot GetSlotByType(EquipmentSlotType slotType)
        {
            foreach (var slot in equipmentSlots)
            {
                if (slot.type == slotType)
                {
                    return slot;
                }
            }
            return null;
        }
    }
}

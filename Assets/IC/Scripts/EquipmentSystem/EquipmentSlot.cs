using UnityEngine;

namespace Pedroca2005BR.EquipmentSystem
{
    [System.Serializable]
    public class EquipmentSlot
    {
        public EquipmentSlotType type;
        public Transform offset;
        public Equipment currentEquipment;
    }

    public enum EquipmentSlotType
    {
        None = 0,
        Face = 1,
        Chest = 2,
        Legs = 3,
        Hands = 4,
        Feet = 5,
        Ears = 6
    }
}


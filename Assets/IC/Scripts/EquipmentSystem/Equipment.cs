using UnityEngine;
using System.Collections.Generic;

namespace Pedroca2005BR.EquipmentSystem
{
    public class Equipment : MonoBehaviour
    {
        public string itemName;
        public Sprite itemIcon;
        public List<SpecialEffectsBase> specialEffects;
        public EquipmentSlotType equipmentSlotType;

        public void OnEquip()
        {
            foreach (var effect in specialEffects)
            {
                effect.ActivateEffect();
            }
        }

        public void OnUnequip()
        {
            foreach (var effect in specialEffects)
            {
                effect.DeactivateEffect();
            }
        }
    }
}


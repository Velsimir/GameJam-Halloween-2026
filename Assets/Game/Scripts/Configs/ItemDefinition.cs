using UnityEngine;

namespace Game.Scripts.Configs
{
    [CreateAssetMenu(fileName = "Item", menuName = "Configs/ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _pickupThoughtKey;

        public string Id => _id;
        public string NameKey => _nameKey;
        public string PickupThoughtKey => _pickupThoughtKey;
    }
}

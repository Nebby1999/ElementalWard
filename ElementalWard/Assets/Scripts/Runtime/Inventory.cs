using UnityEngine;

namespace ElementalWard
{
    public interface IInventoryProvider
    {
        public Inventory Inventory { get; }
    }
    public class Inventory : MonoBehaviour
    {

    }
}
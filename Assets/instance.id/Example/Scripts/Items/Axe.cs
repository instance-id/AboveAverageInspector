using UnityEngine;

namespace instance.id.AAI
{
    [CreateAssetMenu(fileName = "Axe.asset", menuName = "instance.id/Example/Axe", order = 0)]
    public class Axe
        : WeaponBase
    {
        public int Damage;
        public int Weight;
        public int Durability;
        public Texture2D icon;
    }
}

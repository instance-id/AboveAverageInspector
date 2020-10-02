using UnityEngine;

namespace instance.id.AAI
{
    [CreateAssetMenu(fileName = "Sword.asset", menuName = "instance.id/Example/Sword", order = 0)]
    public class Sword : WeaponBase
    {
        public int Damage;
        public int Weight;
        public int Durability;
        public Texture2D icon;
    }
}

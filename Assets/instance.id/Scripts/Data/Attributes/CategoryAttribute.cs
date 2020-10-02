using System;
using UnityEngine;

namespace instance.id.AAI
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Field)]
    public class UICategory : Attribute
    {
        [SerializeField] public string category;
        [SerializeField] public int order;
        [SerializeField] public bool expand = false;

        public UICategory(string categoryName, int order, bool expand)
        {
            category = categoryName;
            this.order = order;
            this.expand = expand;
        }

        public UICategory(string categoryName, bool expand)
        {
            category = categoryName;
            this.expand = expand;
        }

        public UICategory(string categoryName, int order)
        {
            category = categoryName;
            this.order = order;
        }

        public UICategory(string categoryName)
        {
            category = categoryName;
        }
    }
}

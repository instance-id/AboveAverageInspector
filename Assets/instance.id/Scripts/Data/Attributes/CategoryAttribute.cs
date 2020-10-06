// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/AboveAverageInspector         --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace instance.id.AAI
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Field)]
    public class UICategory : Attribute, IEquatable<UICategory>
    {
        [SerializeField] public string category;
        [SerializeField] public int order;
        [SerializeField] public bool expand = false;
        [SerializeField] public string toolTip;

        /// <summary>
        /// UICategory Attribute is used to set options for your field's category, category order, expansion setting, and field tooltip
        /// </summary>
        /// <param name="categoryName">The name of this fields category parent</param>
        /// <param name="order">Draw order of parent category</param>
        /// <param name="expand">If the category should be expanded by default</param>
        /// <param name="toolTip">Tooltip for this field</param>
        public UICategory(string categoryName, int order, bool expand, string toolTip = "")
        {
            category = categoryName;
            this.order = order;
            this.expand = expand;
            this.toolTip = toolTip;
        }

        public UICategory(string categoryName, bool expand, string toolTip = "")
        {
            category = categoryName;
            this.expand = expand;
            this.toolTip = toolTip;
        }

        public UICategory(string categoryName, int order, string toolTip = "")
        {
            category = categoryName;
            this.order = order;
            this.toolTip = toolTip;
        }

        public UICategory(string categoryName, string toolTip = "")
        {
            category = categoryName;
            this.toolTip = toolTip;
        }

        public bool Equals(UICategory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && category == other.category && order == other.order && expand == other.expand && toolTip == other.toolTip;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UICategory) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (category != null ? category.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ order;
                hashCode = (hashCode * 397) ^ expand.GetHashCode();
                hashCode = (hashCode * 397) ^ (toolTip != null ? toolTip.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}

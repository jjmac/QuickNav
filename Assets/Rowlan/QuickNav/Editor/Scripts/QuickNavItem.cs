using System;

namespace Rowlan.QuickNav
{
    [Serializable]
    public class QuickNavItem
    {
        /// <summary>
        /// The selection instance id
        /// </summary>
        public int instanceId;

        /// <summary>
        /// The name of the selected object
        /// </summary>
        public string name;



        /// <summary>
        /// An item is identical if the unique instance id matches.
        /// Used eg to find out if the favorites quick nav items list already contains a history quick nav item
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is QuickNavItem item &&
                   instanceId == item.instanceId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(instanceId);
        }
    }
}
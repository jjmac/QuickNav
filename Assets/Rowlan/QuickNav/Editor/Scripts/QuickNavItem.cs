
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
    }
}
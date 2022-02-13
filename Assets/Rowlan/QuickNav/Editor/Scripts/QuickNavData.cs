using System;
using System.Collections.Generic;

namespace Rowlan.QuickNav
{
    [Serializable]
    public class QuickNavData
    {
        public List<QuickNavItem> history = new List<QuickNavItem>();

        public List<QuickNavItem> favorites = new List<QuickNavItem>();

        /// <summary>
        /// Check if the favorites list contains the specified item.
        /// Used e. g. to find out if a history item is already in the favorites list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsFavoritesItem( QuickNavItem item)
        {
            return favorites.Contains(item);
        }
    }
}

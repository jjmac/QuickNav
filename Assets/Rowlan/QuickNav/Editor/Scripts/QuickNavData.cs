using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.QuickNav
{
    [Serializable]
    public class QuickNavData : ScriptableObject
    {
        /// <summary>
        /// Maximum number of items in either of the lists
        /// </summary>
        public const int LIST_ITEMS_MAX = 20;

        public List<QuickNavItem> history;

        public List<QuickNavItem> favorites;

        /// <summary>
        /// Check if the favorites list contains the specified item.
        /// Used e. g. to find out if a history item is already in the favorites list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsFavoritesItem( UnityEngine.Object unityObject)
        {
            foreach( QuickNavItem currentItem in favorites)
            {
                if (currentItem.unityObject == unityObject)
                    return true;
            }

            return false;
        }

        public void Reset()
        {

            // Debug.Log("Reset");

            history = new List<QuickNavItem>();
            favorites = new List<QuickNavItem>();
        }

        public void OnValidate()
        {
            // Debug.Log( "OnValidate " + Time.time);
        }
    }
}

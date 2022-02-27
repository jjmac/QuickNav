using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.QuickNav
{
    [Serializable]
    public class QuickNavData : ScriptableObject
    {
        /// <summary>
        /// Maximum number of items in the history list
        /// </summary>
        public int historyItemsMax = 20;

        /// <summary>
        /// The history list
        /// </summary>
        public List<QuickNavItem> history;

        /// <summary>
        /// The favorites list
        /// </summary>
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

        /// <summary>
        /// Get the unity object using the object guid and assign it.
        /// This may become necessary e. g. after a restart of the unity editor.
        /// In that case the unity object could be lost, but using the object guid we can restore it.
        /// </summary>
        public void Refresh()
        {
            history.ForEach(x => x.Refresh());
            favorites.ForEach(x => x.Refresh());
        }
    }
}

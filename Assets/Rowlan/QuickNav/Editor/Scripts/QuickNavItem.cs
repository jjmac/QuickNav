using System;
using UnityEngine;

namespace Rowlan.QuickNav
{
    [Serializable]
    public class QuickNavItem
    {
        public enum Context
        {
            Scene,
            Project
        }

        /// <summary>
        /// Whether the selection came from the scene or the project
        /// </summary>
        public Context context;

        /// <summary>
        /// The selection instance id
        /// </summary>
        //public int instanceId;

        /// <summary>
        /// The name of the selected object
        /// </summary>
        public UnityEngine.Object unityObject;


        public QuickNavItem(UnityEngine.Object unityObject, bool isProjectContext)
        {
            this.unityObject = unityObject;
            this.context = isProjectContext ? Context.Project : Context.Scene;
        }

    }
}
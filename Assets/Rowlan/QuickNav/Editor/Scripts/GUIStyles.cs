using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.QuickNav
{
    public class GUIStyles
    {
        private static GUIStyle _objectButtonStyle;
        public static GUIStyle ObjectButtonStyle
        {
            get
            {
                if (_objectButtonStyle == null)
                {
                    _objectButtonStyle = new GUIStyle(GUI.skin.button);
                    _objectButtonStyle.alignment = TextAnchor.MiddleLeft;
                }
                return _objectButtonStyle;
            }
        }
    }
}
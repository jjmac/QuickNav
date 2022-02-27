using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

        private static GUIContent _jumpIcon;
        public static GUIContent JumpIcon
        {
            get
            {
                if (_jumpIcon == null)
                {
                    // jumpIcon = EditorGUIUtility.IconContent("d_SearchJump Icon", "Jump to Selection");
                    _jumpIcon = EditorGUIUtility.IconContent("d_search_icon@2x", "Jump to Selection");
                    _jumpIcon.tooltip = "Jump to Selection";
                }

                return _jumpIcon;
            }
        }

        private static GUIContent _projectIcon;
        public static GUIContent ProjectIcon
        {
            get
            {
                if (_projectIcon == null)
                {
                    _projectIcon = EditorGUIUtility.IconContent("d_Project@2x", "Project");
                    _projectIcon.tooltip = "Project";
                }

                return _projectIcon;
            }
        }

        private static GUIContent _sceneIcon;
        public static GUIContent SceneIcon
        {
            get
            {
                if (_sceneIcon == null)
                {
                    _sceneIcon = EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow@2x", "Scene");
                    _sceneIcon.tooltip = "Scene";
                }

                return _sceneIcon;
            }
        }

        private static GUIContent _addIcon;
        public static GUIContent AddIcon
        {
            get
            {
                if (_addIcon == null)
                {
                    _addIcon = EditorGUIUtility.IconContent("d_Toolbar Plus@2x", "Add Selected");
                    _addIcon.tooltip = "Add Selection to Favorites";
                }

                return _addIcon;
            }
        }

        private static GUIContent _clearIcon;
        public static GUIContent ClearIcon
        {
            get
            {
                if (_clearIcon == null)
                {
                    _clearIcon = new GUIContent("Clear");
                    _clearIcon.tooltip = "Remove all items";
                }

                return _clearIcon;
            }
        }

        private static GUIContent _deleteIcon;
        public static GUIContent DeleteIcon
        {
            get
            {
                if (_deleteIcon == null)
                {
                    _deleteIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Delete");
                    _deleteIcon.tooltip = "Delete";
                }

                return _deleteIcon;
            }
        }

        private static GUIContent _favoriteIcon;
        public static GUIContent FavoriteIcon
        {
            get
            {
                if (_favoriteIcon == null)
                {
                    _favoriteIcon = EditorGUIUtility.IconContent("d_Favorite Icon", "Favorite");
                    _favoriteIcon.tooltip = "Add to Favorites";
                }

                return _favoriteIcon;
            }
        }


        private static GUIContent _leftIcon;
        public static GUIContent LeftIcon
        {
            get
            {
                if (_leftIcon == null)
                {
                    _leftIcon = EditorGUIUtility.IconContent("d_scrollleft_uielements@2x", "Previous");
                    _leftIcon.tooltip = "Jump to Previous";
                }

                return _leftIcon;
            }
        }

        private static GUIContent _rightIcon;
        public static GUIContent RightIcon
        {
            get
            {
                if (_rightIcon == null)
                {
                    _rightIcon = EditorGUIUtility.IconContent("d_scrollright_uielements@2x", "Next");
                    _rightIcon.tooltip = "Jump to Next";
                }

                return _rightIcon;
            }
        }

        private static GUIContent _downIcon;
        public static GUIContent DownIcon
        {
            get
            {
                if (_downIcon == null)
                {
                    _downIcon = EditorGUIUtility.IconContent("d_ProfilerTimelineDigDownArrow@2x", "Previous");
                    _downIcon.tooltip = "Jump to Previous";
                }

                return _downIcon;
            }
        }

        private static GUIContent _upIcon;
        public static GUIContent UpIcon
        {
            get
            {
                if (_upIcon == null)
                {
                    _upIcon = EditorGUIUtility.IconContent("d_ProfilerTimelineRollUpArrow@2x", "Next");
                    _upIcon.tooltip = "Jump to Next";
                }

                return _upIcon;
            }
        }
    }
}
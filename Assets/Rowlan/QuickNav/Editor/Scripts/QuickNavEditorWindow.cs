using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rowlan.QuickNav
{
    public class QuickNavEditorWindow : EditorWindow
    {
        #region Editor Window

        [MenuItem("Tools/Rowlan/Quick Nav %h")]
        static void CreateWindow()
        {
            QuickNavEditorWindow wnd = EditorWindow.GetWindow<QuickNavEditorWindow>();
            wnd.titleContent.text = "Quick Nav";
        }

        #endregion Editor Window

        #region Serializable data

        [SerializeField] 
        List<QuickNavItem> selectionHistory = new List<QuickNavItem>();

        #endregion Serializable data

        #region Internal data

        /// <summary>
        /// Current index of the <see cref="selectionHistory"/> at which the user navigates.
        /// </summary>
        private int currentSelectionHistoryIndex = 0;

        private ScriptableObject scriptableObject;
        private SerializedObject serializedObject;
        private SerializedProperty serializedProperty;

        #endregion Internal data

        #region UI components

        private ReorderableList selectionHistoryReorderableList;

        #endregion UI components

        void OnEnable()
        {
            // initialize data
            scriptableObject = this;
            serializedObject = new SerializedObject(scriptableObject);
            serializedProperty = serializedObject.FindProperty("selectionHistory");

            #region Reorderable list

            // initialize UI components
            selectionHistoryReorderableList = new ReorderableList(serializedObject, serializedProperty)
            {
                draggable = false,
                displayAdd = false,
                displayRemove = false,

                // list header
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "History");
                },

                drawElementCallback = (rect, index, active, focused) =>
                {
                    // Get the currently to be drawn element from YourList
                    var element = serializedProperty.GetArrayElementAtIndex(index);

                    var instanceIdProperty = element.FindPropertyRelative("instanceId");
                    var nameProperty = element.FindPropertyRelative("name");

                    float margin = 3;

                    float left;
                    float width;
                    float right;

                    // EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), instanceIdProperty, GUIContent.none);

                    width = 50; 
                    left = 0; right = left + width;
                    EditorGUI.PropertyField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), instanceIdProperty, GUIContent.none);

                    width = 200; 
                    left = right + margin; right = left + width;
                    EditorGUI.PropertyField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                    width = 60;
                    left = right + margin; right = left + width;
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), EditorGUIUtility.IconContent("d_SearchJump Icon", "Jump to Selection")))
                    {
                        currentSelectionHistoryIndex = index;
                        JumpToQuickNavItem();
                    }

                    width = 60;
                    left = right + margin; right = left + width;
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), EditorGUIUtility.IconContent("d_Favorite Icon", "Favorite")))
                    {
                        Debug.Log("Favorite");
                    }

                    // advance to next line for the next property
                    rect.y += EditorGUIUtility.singleLineHeight;
                }
            };

            #endregion Reorderable list

            // initialize selection
            OnSelectionChange();
        }

        /// <summary>
        /// Analyze selection, add to history
        /// </summary>
        private void OnSelectionChange()
        {
            // skip adding to history if the new selected one is the current selected one;
            // this would be the case for the jump function
            if (Selection.instanceIDs.Length == 1)
            {
                QuickNavItem quickNavItem = GetCurrentHistoryQuickNavItem();
                if (quickNavItem != null)
                {
                    if (quickNavItem.instanceId == Selection.instanceIDs[0])
                        return;
                }
            }

            // ensure collection doesn't exceed max size
            if (selectionHistory.Count >= ProjectSettings.HISTORY_ITEMS_MAX)
            {
                selectionHistory.RemoveRange(ProjectSettings.HISTORY_ITEMS_MAX - 1, selectionHistory.Count - ProjectSettings.HISTORY_ITEMS_MAX + 1);
            }

            // single item selection / navigation => add to history
            if (Selection.instanceIDs.Length == 1)
            {
                int selectedInstanceId = Selection.instanceIDs[0];

                // get the object from the selection
                // we could use Selection.objects as well, but this way it's more general purpose in case we persist a list later (eg favorites)
                UnityEngine.Object selectedObject = EditorUtility.InstanceIDToObject(selectedInstanceId);

                QuickNavItem navItem = new QuickNavItem() { instanceId = selectedInstanceId, name = selectedObject.name };

                // insert new items first
                selectionHistory.Insert(0, navItem);
            }

        }

        void OnGUI()
        {
            serializedObject.Update();

            // EditorGUILayout.LabelField(string.Format("Current QuickNav Index: {0}", currentSelectionHistoryIndex));

            GUILayout.Space(6);

            float buttonHeight = 26;

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(new GUIContent("<"), GUILayout.Width(80), GUILayout.Height(buttonHeight)))
                {
                    currentSelectionHistoryIndex++;
                    if (currentSelectionHistoryIndex >= selectionHistory.Count - 1)
                        currentSelectionHistoryIndex = selectionHistory.Count - 1;

                    if (currentSelectionHistoryIndex < 0)
                        currentSelectionHistoryIndex = 0;

                    JumpToQuickNavItem();
                }

                if (GUILayout.Button(new GUIContent(">"), GUILayout.Width(80), GUILayout.Height(buttonHeight)))
                {
                    currentSelectionHistoryIndex--;
                    if (currentSelectionHistoryIndex < 0)
                        currentSelectionHistoryIndex = 0;

                    JumpToQuickNavItem();


                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent("Clear"), GUILayout.Height(buttonHeight)))
                {
                    selectionHistory.Clear();
                    currentSelectionHistoryIndex = 0;
                }

            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // show history list
            selectionHistoryReorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        QuickNavItem GetCurrentHistoryQuickNavItem()
        {
            if (selectionHistory.Count == 0)
                return null;

            if (currentSelectionHistoryIndex < 0 || currentSelectionHistoryIndex >= selectionHistory.Count)
                return null;

            QuickNavItem quickNavItem = selectionHistory[currentSelectionHistoryIndex];

            return quickNavItem;
        }

        /// <summary>
        /// Get the current quick nav item and jump to it by selecting it
        /// </summary>
        void JumpToQuickNavItem()
        {
            QuickNavItem quickNavItem = GetCurrentHistoryQuickNavItem();

            if (quickNavItem == null)
                return;

            // select in reorderable list
            selectionHistoryReorderableList.Select(currentSelectionHistoryIndex);

            // selection objects
            UnityEngine.Object[] objects = new UnityEngine.Object[] { EditorUtility.InstanceIDToObject(quickNavItem.instanceId) };

            // select objects
            Selection.objects = objects;
        }

        void OnInspectorUpdate()
        {
            // Call Repaint on OnInspectorUpdate as it repaints the windows
            // less times as if it was OnGUI/Update
            Repaint();
        }
    }
}
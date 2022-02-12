using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rowlan.QuickNav
{
    public class QuickNavEditorModule
    {

        private QuickNavEditorWindow editorWindow;

        private ReorderableList selectionHistoryReorderableList;

        private SerializedObject serializedObject;
        private SerializedProperty historyProperty;


        /// <summary>
        /// Current index of the <see cref="selectionHistory"/> at which the user navigates.
        /// </summary>
        private int currentSelectionHistoryIndex = 0;

        private string headerText;
        private bool reorderEnabled;


        private Vector2 historyScrollPosition;

        public QuickNavEditorModule(QuickNavEditorWindow editorWindow, SerializedObject serializedObject, SerializedProperty historyProperty, string headerText, bool reorderEnabled)
        {
            this.editorWindow = editorWindow;
            this.serializedObject = serializedObject;
            this.historyProperty = historyProperty;

            this.headerText = headerText;
            this.reorderEnabled = reorderEnabled;
        }

        public void OnEnable()
        {

            // initialize UI components
            selectionHistoryReorderableList = new ReorderableList(serializedObject, historyProperty)
            {
                draggable = reorderEnabled,
                displayAdd = false,
                displayRemove = false,

                // list header
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, headerText);
                },

                drawElementCallback = (rect, index, active, focused) =>
                {
                    // Get the currently to be drawn element from YourList
                    var element = historyProperty.GetArrayElementAtIndex(index);

                    var instanceIdProperty = element.FindPropertyRelative("instanceId");
                    var nameProperty = element.FindPropertyRelative("name");

                    float margin = 3;

                    float left = 0;
                    float width = 0;
                    float right = 0;

                    float objectIconWidth = 16;
                    float jumpButtonWidth = 30;
                    float favoriteButtonWidth = 30;
                    float deleteButtonWidth = 30;

                    // EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), instanceIdProperty, GUIContent.none);

                    // jump button
                    width = jumpButtonWidth;
                    left = right + margin; right = left + width;
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), EditorGUIUtility.IconContent("d_SearchJump Icon", "Jump to Selection")))
                    {
                        currentSelectionHistoryIndex = index;
                        JumpToQuickNavItem();
                    }

                    // icon
                    width = objectIconWidth;
                    left = right + margin; right = left + width;

                    // get the object
                    UnityEngine.Object currentObject = EditorUtility.InstanceIDToObject(instanceIdProperty.intValue);

                    // get icon for object
                    EditorGUIUtility.GetIconForObject(currentObject);

                    // create guicontent, but remove the text; we only want the icon
                    GUIContent gc = EditorGUIUtility.ObjectContent(currentObject, typeof(object));
                    gc.text = null;

                    // show icon
                    EditorGUI.LabelField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), gc);

                    // object name
                    // textfield is stretched => calculate it from total length - left position - all the buttons to the right - number of margins ... and the fixed number is just arbitrary
                    width = EditorGUIUtility.currentViewWidth - (right + margin) - jumpButtonWidth - favoriteButtonWidth - margin * 3 - 22;
                    left = right + margin; right = left + width;
                    EditorGUI.PropertyField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);

                    // favorite button
                    width = favoriteButtonWidth;
                    left = right + margin; right = left + width;
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), EditorGUIUtility.IconContent("d_Favorite Icon", "Favorite")))
                    {
                        QuickNavItem navItem = new QuickNavItem() { instanceId = instanceIdProperty.intValue, name = nameProperty.stringValue };

                        editorWindow.AddToFavorites( navItem);
                    }

                    // delete button
                    width = deleteButtonWidth;
                    left = right + margin; right = left + width;
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Delete")))
                    {
                        editorWindow.quickNavData.history.RemoveAt(index);
                    }

                    /* instance id; not relevant to show for now
                    width = 50;
                    left = right + margin; right = left + width;
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        EditorGUI.PropertyField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), instanceIdProperty, GUIContent.none);
                    }
                    EditorGUI.EndDisabledGroup();
                    */

                    // advance to next line for the next property
                    rect.y += EditorGUIUtility.singleLineHeight;
                }
            };
        }

        public void OnGUI()
        {
            // EditorGUILayout.LabelField(string.Format("Current QuickNav Index: {0}", currentSelectionHistoryIndex));

            GUILayout.Space(6);

            float buttonHeight = 26;

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(new GUIContent("<"), GUILayout.Width(80), GUILayout.Height(buttonHeight)))
                {
                    currentSelectionHistoryIndex++;
                    if (currentSelectionHistoryIndex >= editorWindow.quickNavData.history.Count - 1)
                        currentSelectionHistoryIndex = editorWindow.quickNavData.history.Count - 1;

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
                    editorWindow.quickNavData.history.Clear();
                    currentSelectionHistoryIndex = 0;
                }

            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // show history list
            historyScrollPosition = EditorGUILayout.BeginScrollView(historyScrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                selectionHistoryReorderableList.DoLayoutList();
            }
            EditorGUILayout.EndScrollView();
        }

        public QuickNavItem GetCurrentHistoryQuickNavItem()
        {
            if (editorWindow.quickNavData.history.Count == 0)
                return null;

            if (currentSelectionHistoryIndex < 0 || currentSelectionHistoryIndex >= editorWindow.quickNavData.history.Count)
                return null;

            QuickNavItem quickNavItem = editorWindow.quickNavData.history[currentSelectionHistoryIndex];

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
    }
}

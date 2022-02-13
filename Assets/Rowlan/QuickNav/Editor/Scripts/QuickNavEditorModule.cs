using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rowlan.QuickNav
{
    public class QuickNavEditorModule
    {
        public enum NavigationDirection
        {
            LeftRight,
            UpDown,
        }

        private QuickNavEditorWindow editorWindow;

        private ReorderableList reorderableList;
        private Vector2 reorderableListScrollPosition;

        private SerializedObject serializedObject;
        private SerializedProperty serializedProperty;
        private List<QuickNavItem> quickNavList;

        private int currentSelectionIndex = 0;

        public string headerText = "";
        public bool reorderEnabled = false;
        public bool addSelectedEnabled = false;
        public NavigationDirection navigationDirection = NavigationDirection.LeftRight;

        private GUIContent previousIcon;
        private GUIContent nextIcon;
        private GUIContent addIcon;

        private GUIContent jumpIcon;
        private GUIContent favoriteIcon;
        private GUIContent deleteIcon;

        // TODO: use only the serializedProperty, don't hand over the quicknavlist
        public QuickNavEditorModule(QuickNavEditorWindow editorWindow, SerializedObject serializedObject, SerializedProperty serializedProperty, List<QuickNavItem> quickNavList, NavigationDirection navigationDirection)
        {
            this.editorWindow = editorWindow;
            this.serializedObject = serializedObject;
            this.serializedProperty = serializedProperty;
            this.quickNavList = quickNavList;

            this.navigationDirection = navigationDirection;

            // icons

            switch (navigationDirection)
            {
                case NavigationDirection.LeftRight:
                    previousIcon = EditorGUIUtility.IconContent("d_scrollleft_uielements@2x", "Previous");
                    nextIcon = EditorGUIUtility.IconContent("d_scrollright_uielements@2x", "Next");
                    break;

                case NavigationDirection.UpDown:
                    previousIcon = EditorGUIUtility.IconContent("d_ProfilerTimelineDigDownArrow@2x", "Previous");
                    nextIcon = EditorGUIUtility.IconContent("d_ProfilerTimelineRollUpArrow@2x", "Next");
                    break;

            }
            
            addIcon = EditorGUIUtility.IconContent("d_Toolbar Plus@2x", "Add Selected");

            jumpIcon = EditorGUIUtility.IconContent("d_SearchJump Icon", "Jump to Selection");
            favoriteIcon = EditorGUIUtility.IconContent("d_Favorite Icon", "Favorite");
            deleteIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Delete");

        }

        private List<QuickNavItem> GetQuickNavItemList()
        {
            return quickNavList;
        }

        public void OnEnable()
        {

            // initialize UI components
            reorderableList = new ReorderableList(serializedObject, serializedProperty)
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
                    var element = serializedProperty.GetArrayElementAtIndex(index);

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
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), jumpIcon))
                    {
                        currentSelectionIndex = index;
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
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), favoriteIcon))
                    {
                        QuickNavItem navItem = new QuickNavItem() { instanceId = instanceIdProperty.intValue, name = nameProperty.stringValue };

                        editorWindow.AddToFavorites( navItem);
                    }

                    // delete button
                    width = deleteButtonWidth;
                    left = right + margin; right = left + width;
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), deleteIcon))
                    {
                        GetQuickNavItemList().RemoveAt(index);
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
                if (GUILayout.Button(previousIcon, GUILayout.Width(80), GUILayout.Height(buttonHeight)))
                {
                    currentSelectionIndex++;
                    if (currentSelectionIndex >= GetQuickNavItemList().Count - 1)
                        currentSelectionIndex = GetQuickNavItemList().Count - 1;

                    if (currentSelectionIndex < 0)
                        currentSelectionIndex = 0;

                    JumpToQuickNavItem();
                }

                if (GUILayout.Button(nextIcon, GUILayout.Width(80), GUILayout.Height(buttonHeight)))
                {
                    currentSelectionIndex--;
                    if (currentSelectionIndex < 0)
                        currentSelectionIndex = 0;

                    JumpToQuickNavItem();


                }

                GUILayout.FlexibleSpace();

                if(addSelectedEnabled)
                {

                    if (GUILayout.Button(addIcon, GUILayout.Width(60), GUILayout.Height(buttonHeight)))
                    {
                        editorWindow.AddSelectedToFavorites();
                    }

                }

                if (GUILayout.Button(new GUIContent("Clear"), GUILayout.Height(buttonHeight)))
                {
                    GetQuickNavItemList().Clear();
                    currentSelectionIndex = 0;
                }

            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // show history list
            reorderableListScrollPosition = EditorGUILayout.BeginScrollView(reorderableListScrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                reorderableList.DoLayoutList();
            }
            EditorGUILayout.EndScrollView();
        }

        public QuickNavItem GetCurrentQuickNavItem()
        {
            if (GetQuickNavItemList().Count == 0)
                return null;

            if (currentSelectionIndex < 0 || currentSelectionIndex >= GetQuickNavItemList().Count)
                return null;

            QuickNavItem quickNavItem = GetQuickNavItemList()[currentSelectionIndex];

            return quickNavItem;
        }

        /// <summary>
        /// Get the current quick nav item and jump to it by selecting it
        /// </summary>
        void JumpToQuickNavItem()
        {
            QuickNavItem quickNavItem = GetCurrentQuickNavItem();

            if (quickNavItem == null)
                return;

            // select in reorderable list
            reorderableList.Select(currentSelectionIndex);

            // selection objects
            UnityEngine.Object[] objects = new UnityEngine.Object[] { EditorUtility.InstanceIDToObject(quickNavItem.instanceId) };

            // select objects
            Selection.objects = objects;
        }
    }
}

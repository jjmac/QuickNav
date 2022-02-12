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
        QuickNavData quickNavData = new QuickNavData();

        #endregion Serializable data

        #region Internal data

        /// <summary>
        /// Current index of the <see cref="selectionHistory"/> at which the user navigates.
        /// </summary>
        private int currentSelectionHistoryIndex = 0;

        private ScriptableObject scriptableObject;
        private SerializedObject serializedObject;
        private SerializedProperty historyProperty;

        #endregion Internal data

        #region UI related

        private ReorderableList selectionHistoryReorderableList;

        private Vector2 historyScrollPosition;

        #endregion UI related

        void OnEnable()
        {
            // initialize data
            scriptableObject = this;
            serializedObject = new SerializedObject(scriptableObject);
            historyProperty = serializedObject.FindProperty("quickNavData.history");

            #region Reorderable list

            // initialize UI components
            selectionHistoryReorderableList = new ReorderableList(serializedObject, historyProperty)
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
                        Debug.Log("Favorite");
                    }

                    // delete button
                    width = deleteButtonWidth;
                    left = right + margin; right = left + width;
                    if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Delete")))
                    {
                        quickNavData.history.RemoveAt(index);
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
            if (quickNavData.history.Count >= ProjectSettings.HISTORY_ITEMS_MAX)
            {
                quickNavData.history.RemoveRange(ProjectSettings.HISTORY_ITEMS_MAX - 1, quickNavData.history.Count - ProjectSettings.HISTORY_ITEMS_MAX + 1);
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
                quickNavData.history.Insert(0, navItem);
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
                    if (currentSelectionHistoryIndex >= quickNavData.history.Count - 1)
                        currentSelectionHistoryIndex = quickNavData.history.Count - 1;

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
                    quickNavData.history.Clear();
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

            serializedObject.ApplyModifiedProperties();
        }

        QuickNavItem GetCurrentHistoryQuickNavItem()
        {
            if (quickNavData.history.Count == 0)
                return null;

            if (currentSelectionHistoryIndex < 0 || currentSelectionHistoryIndex >= quickNavData.history.Count)
                return null;

            QuickNavItem quickNavItem = quickNavData.history[currentSelectionHistoryIndex];

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
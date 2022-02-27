using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rowlan.QuickNav
{
    public class QuickNavEditorModule
    {
        public enum ModuleType
        {
            History,
            Favorites,
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
        public ModuleType moduleType;

        // icons depending on the navigation direction
        private GUIContent previousIcon;
        private GUIContent nextIcon;

        // TODO: use only the serializedProperty, don't hand over the quicknavlist
        public QuickNavEditorModule(QuickNavEditorWindow editorWindow, SerializedObject serializedObject, SerializedProperty serializedProperty, List<QuickNavItem> quickNavList, ModuleType moduleType)
        {
            this.editorWindow = editorWindow;
            this.serializedObject = serializedObject;
            this.serializedProperty = serializedProperty;
            this.quickNavList = quickNavList;

            this.moduleType = moduleType;

            // setup styles, icons etc
            SetupStyles();
        }

        /// <summary>
        /// Setup styles, icons, etc
        /// </summary>
        private void SetupStyles()
        {
            switch (moduleType)
            {
                case ModuleType.History:
                    previousIcon = GUIStyles.LeftIcon;
                    nextIcon = GUIStyles.RightIcon;
                    break;

                case ModuleType.Favorites:
                    previousIcon = GUIStyles.DownIcon;
                    nextIcon = GUIStyles.UpIcon; 
                    break;

                default: throw new System.Exception($"Unsupported module type: {moduleType}");
            }
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

                    var contextProperty = element.FindPropertyRelative("context");
                    var unityObjectProperty = element.FindPropertyRelative("unityObject");
                    var objectGuidProperty = element.FindPropertyRelative("objectGuid");

                    float margin = 3;

                    float left = 0;
                    float width = 0;
                    float right = 0;

                    // float objectIconWidth = 16;
                    // float pingButtonWidth = 30;
                    float jumpButtonWidth = 30;
                    float favoriteButtonWidth = 30;
                    float deleteButtonWidth = 30;
                    float sourceIconWidth = 16;

                    // EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), instanceIdProperty, GUIContent.none);

                    // get the context and the object
                    bool isProjectContext = contextProperty.enumValueIndex == (int)QuickNavItem.Context.Project;
                    UnityEngine.Object currentObject = unityObjectProperty.objectReferenceValue;


                    // context icon: scene or project
                    {
                        width = sourceIconWidth;
                        left = right + margin; right = left + width;

                        // create guicontent, but remove the text; we only want the icon
                        GUIContent gc = isProjectContext ? GUIStyles.ProjectIcon : GUIStyles.SceneIcon;
                        gc.text = null;

                        // show icon
                        EditorGUI.LabelField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), gc);
                    }

                    // jump button
                    {
                        width = jumpButtonWidth;
                        left = right + margin; right = left + width;
                        if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), GUIStyles.JumpIcon))
                        {
                            currentSelectionIndex = index;
                            JumpToQuickNavItem(false);
                        }
                    }

                    // object icon
                    /*
                    {
                        width = objectIconWidth;
                        left = right + margin; right = left + width;


                        // create guicontent, but remove the text; we only want the icon
                        GUIContent gc = EditorGUIUtility.ObjectContent(currentObject, typeof(object));
                        gc.text = null;

                        // show icon
                        EditorGUI.LabelField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), gc);
                    }
                    */

                    // object name
                    {
                        //width = 128;
                        width = EditorGUIUtility.currentViewWidth - (right + margin) - jumpButtonWidth - margin * 3 - 22;
                        width -= moduleType == ModuleType.History ? favoriteButtonWidth : 10; // favorites button isn't shown in favorites tab; however there's a drag handle and we don't want the delete button cut off when the scrollbars appear => use arbitrary value (need to find out scrollbar width later)
                        left = right + margin; right = left + width;

                        string displayName = unityObjectProperty.objectReferenceValue != null ? unityObjectProperty.objectReferenceValue.name : "<invalid>";

                        //EditorGUI.LabelField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), displayName);
                        GUIContent objectContent = EditorGUIUtility.ObjectContent(currentObject, typeof(object));

                        if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), objectContent, GUIStyles.ObjectButtonStyle))
                        {
                            currentSelectionIndex = index;
                            JumpToQuickNavItem(true);
                        }
                    }

                    /*
                    // object property
                    {
                        // textfield is stretched => calculate it from total length - left position - all the buttons to the right - number of margins ... and the fixed number is just arbitrary
                        width = EditorGUIUtility.currentViewWidth - (right + margin) - jumpButtonWidth - favoriteButtonWidth - margin * 3 - 22;
                        left = right + margin; right = left + width;
                        
                        EditorGUI.PropertyField(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), unityObjectProperty, GUIContent.none);
                    }
                    */

                    // favorite button
                    if( moduleType == ModuleType.History)
                    {
                        bool isFavoritesItem = editorWindow.quickNavData.IsFavoritesItem(currentObject);

                        bool guiEnabledPrev = GUI.enabled;
                        {
                            // disable the button in case it is already a favorite
                            GUI.enabled = !isFavoritesItem;

                            width = favoriteButtonWidth;
                            left = right + margin; right = left + width;
                            if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), GUIStyles.FavoriteIcon))
                            {

                                QuickNavItem navItem = new QuickNavItem(currentObject, isProjectContext);

                                editorWindow.AddToFavorites(navItem);

                                EditorUtility.SetDirty(serializedObject.targetObject);

                            }
                        }
                        GUI.enabled = guiEnabledPrev;
                    }

                    // delete button
                    {
                        width = deleteButtonWidth;
                        left = right + margin; right = left + width;
                        if (GUI.Button(new Rect(rect.x + left, rect.y + margin, width, EditorGUIUtility.singleLineHeight), GUIStyles.DeleteIcon))
                        {
                            GetQuickNavItemList().RemoveAt(index);

                            EditorUtility.SetDirty(serializedObject.targetObject);

                        }
                    }

                    // instance id; not relevant to show for now
                    /*
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

                    JumpToQuickNavItem( true);
                }

                if (GUILayout.Button(nextIcon, GUILayout.Width(80), GUILayout.Height(buttonHeight)))
                {
                    currentSelectionIndex--;
                    if (currentSelectionIndex < 0)
                        currentSelectionIndex = 0;

                    JumpToQuickNavItem( true);


                }

                GUILayout.FlexibleSpace();

                if(addSelectedEnabled)
                {

                    if (GUILayout.Button(GUIStyles.AddIcon, GUILayout.Width(60), GUILayout.Height(buttonHeight)))
                    {
                        editorWindow.AddSelectedToFavorites();
                    }

                }

                if (GUILayout.Button(GUIStyles.ClearIcon, GUILayout.Height(buttonHeight)))
                {
                    GetQuickNavItemList().Clear();
                    currentSelectionIndex = 0;

                    EditorUtility.SetDirty(serializedObject.targetObject);
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
        void JumpToQuickNavItem( bool openInInspector)
        {
            QuickNavItem quickNavItem = GetCurrentQuickNavItem();

            if (quickNavItem == null)
                return;

            // select in reorderable list
            reorderableList.index = currentSelectionIndex;
            //reorderableList.Select(currentSelectionIndex);

            // select the object and open it in the inspector
            if (openInInspector)
            {
                // selection objects
                UnityEngine.Object[] objects = new UnityEngine.Object[] { quickNavItem.unityObject };

                // select objects
                Selection.objects = objects;
            }
            // just select the object, don't open it in the inspector
            else
            {
                
                EditorGUIUtility.PingObject(quickNavItem.unityObject);

                // alternative: open in application (eg doubleclick)
                // AssetDatabase.OpenAsset(quickNavItem.unityObject);

            }

        }
    }
}

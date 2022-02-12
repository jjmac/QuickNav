using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rowlan.QuickNav
{
    public class QuickNavEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Rowlan/Quick Nav %h")]
        static void CreateWindow()
        {
            QuickNavEditorWindow wnd = EditorWindow.GetWindow<QuickNavEditorWindow>();
            wnd.titleContent.text = "Quick Nav";
        }

        private enum QuickNavTab
        {
            History,
            Favorites
        }

        private GUIContent[] quickNavTabs;
        private int selectedQuickNavTabIndex = 0;

        private QuickNavEditorWindow editorWindow;

        [SerializeField]
        public QuickNavData quickNavData = new QuickNavData();

        private ScriptableObject scriptableObject;
        private SerializedObject serializedObject;

        private QuickNavEditorModule historyModule;
        private QuickNavEditorModule favoritesModule;

        void OnEnable()
        {
            editorWindow = this;

            // initialize data
            scriptableObject = this;
            serializedObject = new SerializedObject(scriptableObject);

            // history
            SerializedProperty historyProperty = serializedObject.FindProperty("quickNavData.history");
            historyModule = new QuickNavEditorModule( this, serializedObject, historyProperty, "History", false);
            historyModule.OnEnable();

            // favorites
            SerializedProperty favoritesProperty = serializedObject.FindProperty("quickNavData.favorites");
            favoritesModule = new QuickNavEditorModule(this, serializedObject, favoritesProperty, "Favorites", true);
            favoritesModule.OnEnable();

            quickNavTabs = new GUIContent[]
            {
                new GUIContent( QuickNavTab.History.ToString()),
                new GUIContent( QuickNavTab.Favorites.ToString()),
            };

            // initialize selection
            OnSelectionChange();
        }


        void OnGUI()
        {
            serializedObject.Update();

            selectedQuickNavTabIndex = GUILayout.Toolbar(selectedQuickNavTabIndex, quickNavTabs);

            switch(selectedQuickNavTabIndex)
            {
                case ((int)QuickNavTab.History):
                    historyModule.OnGUI();
                    break;

                case ((int)QuickNavTab.Favorites):
                    favoritesModule.OnGUI();
                    break;

            }

            serializedObject.ApplyModifiedProperties();
        }

        void OnInspectorUpdate()
        {
            // Call Repaint on OnInspectorUpdate as it repaints the windows
            // less times as if it was OnGUI/Update
            Repaint();
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
                QuickNavItem quickNavItem = historyModule.GetCurrentQuickNavItem();
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

        public void AddToFavorites( QuickNavItem quickNavItem)
        {
            quickNavData.favorites.Add(quickNavItem);
        }
    }
}
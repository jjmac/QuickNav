using UnityEditor;
using UnityEngine;

namespace Rowlan.QuickNav
{
    public class QuickNavEditorWindow : EditorWindow
    {
        [MenuItem(ProjectSetup.MENU)]
        static void CreateWindow()
        {
            QuickNavEditorWindow wnd = EditorWindow.GetWindow<QuickNavEditorWindow>();
            wnd.titleContent.text = "Quick Nav";

            wnd.position = new Rect(0, 0, 600, 1000);
            //wnd.Close();
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
        public QuickNavData quickNavData;

        private SerializedObject serializedObject;

        private QuickNavEditorModule historyModule;
        private QuickNavEditorModule favoritesModule;

        void OnEnable()
        {
            editorWindow = this;

            // initialize data
            ScriptableObjectManager<QuickNavData> settingsManager = new ScriptableObjectManager<QuickNavData>(ProjectSetup.SETTINGS_FOLDER, ProjectSetup.SETTINGS_FILENAME);
            quickNavData = settingsManager.GetAsset();

            foreach (QuickNavItem qi in quickNavData.favorites)
            {
                if (qi.objectGuid == null)
                    continue;

                GlobalObjectId id;
                if (!GlobalObjectId.TryParse(qi.objectGuid, out id))
                {
                    Debug.Log("obj is null for " + qi.objectGuid);
                    continue;
                }

                Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);

                if (obj == null)
                    continue;

                qi.unityObject = obj;
            }

            serializedObject = new SerializedObject(quickNavData);

            // properties
            SerializedProperty historyProperty = serializedObject.FindProperty("history"); // history: might need the "quickNavData." prefix depending on what is the parent
            SerializedProperty favoritesProperty = serializedObject.FindProperty("favorites"); // favorites: might need the "quickNavData." prefix depending on what is the parent

            // unity startup, first access
            if ( !Startup.Instance.Initialized)
            {
                // check startup or play mode: don't do anything when the user switches to play mode in the editor
                bool isUnityStartup = EditorApplication.isPlayingOrWillChangePlaymode == false;

                if (isUnityStartup)
                {
                    // Debug.Log("Startup: Clearing history");

                    // clear history at startup
                    quickNavData.history.Clear();

                    EditorUtility.SetDirty(quickNavData);
                    //serializedObject.Update();
                }
            }

            #region Modules

            // history
            historyModule = new QuickNavEditorModule(this, serializedObject, historyProperty, editorWindow.quickNavData.history, QuickNavEditorModule.ModuleType.History)
            {
                headerText = "History",
                reorderEnabled = false,
                addSelectedEnabled = false,
            };
            historyModule.OnEnable();

            // favorites
            favoritesModule = new QuickNavEditorModule(this, serializedObject, favoritesProperty, editorWindow.quickNavData.favorites, QuickNavEditorModule.ModuleType.Favorites)
            {
                headerText = "Favorites",
                reorderEnabled = true,
                addSelectedEnabled = true,
            };
            favoritesModule.OnEnable();

            #endregion Modules

            quickNavTabs = new GUIContent[]
            {
                new GUIContent( QuickNavTab.History.ToString()),
                new GUIContent( QuickNavTab.Favorites.ToString()),
            };

            // initialize selection
            OnSelectionChange();

            Startup.Instance.Initialized = true;
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
                    if (quickNavItem.unityObject == Selection.objects[0])
                        return;
                }
            }

            // ensure collection doesn't exceed max size
            if (quickNavData.history.Count >= QuickNavData.LIST_ITEMS_MAX)
            {
                quickNavData.history.RemoveRange(QuickNavData.LIST_ITEMS_MAX - 1, quickNavData.history.Count - QuickNavData.LIST_ITEMS_MAX + 1);
            }

            // single item selection / navigation => add to history
            if (Selection.objects.Length == 1)
            {
                UnityEngine.Object selectedUnityObject = Selection.objects[0];

                QuickNavItem navItem = CreateQuickNavItem(selectedUnityObject);

                // insert new items first
                quickNavData.history.Insert(0, navItem);

                // persist the changes
                EditorUtility.SetDirty(quickNavData);
            }

        }

        public void AddSelectedToFavorites()
        {
            foreach( UnityEngine.Object unityObject in Selection.objects)
            {
                QuickNavItem navItem = CreateQuickNavItem(unityObject);

                quickNavData.favorites.Add( navItem);
            }

            // persist the changes
            EditorUtility.SetDirty(quickNavData);
        }

        public QuickNavItem CreateQuickNavItem( UnityEngine.Object unityObject)
        {
            string guid;
            long localId;

            bool isProjectContext = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(unityObject, out guid, out localId);

            QuickNavItem navItem = new QuickNavItem(unityObject, isProjectContext);

            return navItem;
        }

        public void AddToFavorites( QuickNavItem quickNavItem)
        {
            quickNavData.favorites.Add(quickNavItem);
        }
    }
}
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            serializedObject = new SerializedObject(quickNavData);

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

            // update history and favorites using the object guid
            // this may become necessary after a restart of the editor
            quickNavData.Refresh();

            // properties
            SerializedProperty historyProperty = serializedObject.FindProperty("history"); // history: might need the "quickNavData." prefix depending on what is the parent
            SerializedProperty favoritesProperty = serializedObject.FindProperty("favorites"); // favorites: might need the "quickNavData." prefix depending on what is the parent

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

            // hook into the scene change for refresh of the objects when another scene gets loaded
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= SceneOpenedCallback;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpenedCallback;
        }

        void OnDisable()
        {
            // remove scene change hook
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= SceneOpenedCallback;
        }

        /// <summary>
        /// Scene change handler
        /// </summary>
        /// <param name="_scene"></param>
        /// <param name="_mode"></param>
        void SceneOpenedCallback(Scene _scene, UnityEditor.SceneManagement.OpenSceneMode _mode)
        {
            // update history and favorites using the object guid
            // this may become necessary after a restart of the editor
            quickNavData.Refresh();
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
            if (Selection.objects.Length == 0)
                return;

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

            int itemsMax = quickNavData.historyItemsMax;

            // ensure collection doesn't exceed max size
            if (quickNavData.history.Count >= itemsMax)
            {
                quickNavData.history.RemoveRange(itemsMax - 1, quickNavData.history.Count - itemsMax + 1);
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rowlan.QuickNav
{
    /// <summary>
    /// Wrapper for ScriptableObject which allows optional persistence to an asset.
    /// 
    /// Can be used with persistence and without. Use the respective constructor.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScriptableObjectManager<T> where T :ScriptableObject
    {

        /// <summary>
        /// The root folder where the asset is stored. 
        /// </summary>
        private readonly string ROOT_FOLDER = "Assets";

        /// <summary>
        /// The instance of the scriptable object
        /// </summary>
        private T asset;

        /// <summary>
        /// Create a scriptable object without persistence.
        /// </summary>
        public ScriptableObjectManager()
        {
            // create the instance of the scriptable object
            CreateAsset();
        }

        /// <summary>
        /// Create a scriptable object with persistence.
        /// The asset folder must start with "Assets".
        /// </summary>
        /// <param name="assetFolder"></param>
        /// <param name="filename"></param>
        public ScriptableObjectManager( string assetFolder, string filename)
        {
            // ensure folder starts with 'Assets/'
            if (!assetFolder.StartsWith(ROOT_FOLDER))
                throw new UnityException($"Folder must start with '{ROOT_FOLDER}'");

            string assetFilePath = Path.Combine(assetFolder, filename);

            this.asset = AssetDatabase.LoadAssetAtPath<T>(assetFilePath);

            if( this.asset == null)
            {
                // create folders
                EnsureAssetPathExists(assetFolder);

                // create the instance of the scriptable object
                CreateAsset();

                AssetDatabase.CreateAsset(asset, assetFilePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Check if the asset folder path exists. If it doesn't exist,
        /// the path is created. The path can contain sub-folders.
        /// </summary>
        /// <param name="assetFolderPath"></param>
        private void EnsureAssetPathExists( string assetFolderPath)
        {
            // split by path separators. could be multiple, eg \\ and /
            List<string> folders = assetFolderPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToList();

            // extract Assets folder and make it the current parent
            string parentFolder = folders[0];
            folders.RemoveAt(0);

            foreach ( string folder in folders)
            {
                string currentFolder = Path.Combine(parentFolder, folder);

                bool folderExists = AssetDatabase.IsValidFolder(currentFolder);

                if (!folderExists)
                {
                    AssetDatabase.CreateFolder(parentFolder, folder);
                    AssetDatabase.Refresh();
                }

                parentFolder = currentFolder;
            }
        }

        private void CreateAsset()
        {
            this.asset = ScriptableObject.CreateInstance<T>();
        }

        public T GetAsset()
        {
            return asset;
        }

    }
}
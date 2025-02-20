using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class EditorDirectoryMenus
    {

        [MenuItem("File/Open Directory/Persistent Data", false, 100)]
        public static void OpenPersistentDataDirectory()
        {
            Process.Start(Application.persistentDataPath);
        }
        
        [MenuItem("File/Open Directory/Streaming Assets", false, 100)]
        public static void OpenStreamingAssetsDirectory()
        {
            Process.Start(Application.streamingAssetsPath);
        }
        
        [MenuItem("File/Open Directory/Temporary Cache", false, 100)]
        public static void OpenTemporaryCacheDirectory()
        {
            Process.Start(Application.temporaryCachePath);
        }

        [MenuItem("File/Open Directory/Packages", false, 111)]
        public static void OpenPackagesDirectory()
        {
            Process.Start("Packages");
        }
    }
}
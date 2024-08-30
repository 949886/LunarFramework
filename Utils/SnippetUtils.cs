// Created by LunarEclipse on 2024-08-30 14:50.

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = System.Object;

namespace Luna.Utils
{
    public class SnippetUtils
    {
        private void CreateScript(string directory)
        {
            var m_ClassName = "NewScript";
            var targetDir = Path.Combine("Assets", directory.Trim(new char[] {'/', '\\'}));
            var targetPath =  Path.Combine(targetDir, m_ClassName + ".cs");
            var templatePath = $"Assets/ScriptTemplates/Test.cs";
            CreateScriptAssetFromTemplate(targetPath, templatePath);
            AssetDatabase.Refresh();
        }
        
        internal static Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
        {
            string content = File.ReadAllText(resourceFile);
            return CreateScriptAssetWithContent(pathName, PreprocessScriptAssetTemplate(pathName, content));
        }
        
        public static Object CreateScriptAssetWithContent(string assetPath, string scriptContent)
        {
            // AssetModificationProcessorInternal.OnWillCreateAsset(assetPath);

            scriptContent = SetLineEndings(scriptContent, EditorSettings.lineEndingsForNewScripts);

            string fullPath = Path.GetFullPath(assetPath);
            File.WriteAllText(fullPath, scriptContent);

            // Import the asset
            AssetDatabase.ImportAsset(assetPath);

            return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
        }
        
        internal static string SetLineEndings(string content, LineEndingsMode lineEndingsMode)
        {
            const string windowsLineEndings = "\r\n";
            const string unixLineEndings = "\n";

            string preferredLineEndings;

            switch (lineEndingsMode)
            {
                case LineEndingsMode.OSNative:
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                        preferredLineEndings = windowsLineEndings;
                    else
                        preferredLineEndings = unixLineEndings;
                    break;
                case LineEndingsMode.Unix:
                    preferredLineEndings = unixLineEndings;
                    break;
                case LineEndingsMode.Windows:
                    preferredLineEndings = windowsLineEndings;
                    break;
                default:
                    preferredLineEndings = unixLineEndings;
                    break;
            }

            content = Regex.Replace(content, @"\r\n?|\n", preferredLineEndings);

            return content;
        }
        
        internal static string PreprocessScriptAssetTemplate(string pathName, string resourceContent)
        {
            string rootNamespace = null;

            if (Path.GetExtension(pathName) == ".cs")
            {
                rootNamespace = CompilationPipeline.GetAssemblyRootNamespaceFromScriptPath(pathName);
            }

            string content = resourceContent;

            // #NOTRIM# is a special marker that is used to mark the end of a line where we want to leave whitespace. prevent editors auto-stripping it by accident.
            content = content.Replace("#NOTRIM#", "");

            // macro replacement
            string baseFile = Path.GetFileNameWithoutExtension(pathName);

            content = content.Replace("#NAME#", baseFile);
            string baseFileNoSpaces = baseFile.Replace(" ", "");
            content = content.Replace("#SCRIPTNAME#", baseFileNoSpaces);

            // content = RemoveOrInsertNamespace(content, rootNamespace);

            // if the script name begins with an uppercase character we support a lowercase substitution variant
            if (char.IsUpper(baseFileNoSpaces, 0))
            {
                baseFileNoSpaces = char.ToLower(baseFileNoSpaces[0]) + baseFileNoSpaces.Substring(1);
                content = content.Replace("#SCRIPTNAME_LOWER#", baseFileNoSpaces);
            }
            else
            {
                // still allow the variant, but change the first character to upper and prefix with "my"
                baseFileNoSpaces = "my" + char.ToUpper(baseFileNoSpaces[0]) + baseFileNoSpaces.Substring(1);
                content = content.Replace("#SCRIPTNAME_LOWER#", baseFileNoSpaces);
            }

            return content;
        }
    }
}
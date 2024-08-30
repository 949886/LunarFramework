#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Luna.Utils.Nuget
{
    public class NugetPackageInitializer : UnityEditor.Editor
    {
        [MenuItem("Tools/LunarFramework/Initialize Nuget")]
        public static void InitializeNugetPackages()
        {
            CreateNugetConfig();
            CreateNugetPackage();
            EditorUtility.RequestScriptReload();
        }
        
        // Create Nuget.config file at the root of the project if it doesn't exist
        private static void CreateNugetConfig()
        {
            var nugetConfig = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
    <config>
        <add key=""repositoryPath"" value="".\Packages\Nuget"" />
    </config>
    <packageRestore>
        <add key=""enabled"" value=""true"" />
        <add key=""automatic"" value=""true"" />
      </packageRestore>
</configuration>";

            var path = Application.dataPath + "/../Nuget.config";
            if (!File.Exists(path))
            {
                File.WriteAllText(path, nugetConfig);
                Debug.Log("Nuget.config file created successfully.");
            }
            else Debug.Log("Nuget.config file already exists.");
        }
        
        private static void CreateNugetPackage()
        {
            var path = Application.dataPath + "/../Packages/Nuget";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            // Create a package.json file
            var packageJson =
@"{  
  ""name"": ""nuget"",
  ""displayName"": ""Nuget"",
  ""description"": ""Nuget packages."",
  ""version"": ""1.0.0"",
  ""unity"": ""2021.3""
}";
            var packageJsonPath = path + "/package.json";
            if (!File.Exists(packageJsonPath))
            {
                File.WriteAllText(packageJsonPath, packageJson);
                Debug.Log("Nuget package created successfully.");
            }
            else Debug.Log("Nuget package already exists.");
            
            
            // Create assembly definition file
            var assemblyDefinition =
@"{
	""name"": ""Nuget""
}";
            var assemblyDefinitionPath = path + "/Nuget.asmdef";
            if (!File.Exists(assemblyDefinitionPath))
            {
                File.WriteAllText(assemblyDefinitionPath, assemblyDefinition);
                Debug.Log("Nuget assembly definition file created successfully.");
            }
            else Debug.Log("Nuget.asmdef file already exists.");

            
            // Add reference in the manifest.json file
            var manifestPath = Application.dataPath + "/../Packages/manifest.json";
            var manifestJson = File.ReadAllText(manifestPath);
            if (!manifestJson.Contains("files:./Nuget"))
            {
                manifestJson = manifestJson.Replace("\"dependencies\": {", "\"dependencies\": {\n    \"nuget\": \"files:./Nuget\",");
                File.WriteAllText(manifestPath, manifestJson);
                Debug.Log("Nuget package added to the manifest.json file.");
            }
            else Debug.Log("Nuget package already exists in the manifest.json file.");
            
            
            // Not ignore Nuget.csproj file in the .gitignore file
            var textToAppend = "\n# Include Nuget.csproj file.\n!Nuget.csproj";
            
            var gitignorePath = Application.dataPath + "/../.gitignore";
            if (!File.Exists(gitignorePath)) return;
            
            if (!File.ReadAllText(gitignorePath).Contains("Nuget.csproj"))
            {
                using var writer = File.AppendText(gitignorePath);
                writer.Write(textToAppend);
                Debug.Log("Nuget.csproj file added to the .gitignore file.");
            }
            else Debug.Log("Nuget.csproj file already exists in the .gitignore file.");
        }
    }
}

#endif
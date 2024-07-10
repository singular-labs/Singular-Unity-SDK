using System;
using System.Xml;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Android;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Singular.Editor {
    
#if UNITY_IOS

public class SingularPostBuild
{
    [PostProcessBuildAttribute(1)]
    static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            Debug.Log("Start Xcode project related configuration of SDK......");
            AddiOSDependencies(pathToBuiltProject);
            HandleCustomAppDelegate(pathToBuiltProject);
        }
    }

    static void AddiOSDependencies(string pathToBuiltProject)
    {
        string projectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
        PBXProject pbxProject = new PBXProject();
        pbxProject.ReadFromFile(projectPath);

        #if UNITY_2019_3_OR_NEWER
        string targetGuid = pbxProject.GetUnityFrameworkTargetGuid();
        #else
        string targetGuid = pbxProject.TargetGuidByName(PBXProject.GetUnityTargetName());
        #endif
        
        // Add following frameworks to your project
        pbxProject.AddFrameworkToProject(targetGuid, "Security.framework", false);
        pbxProject.AddFrameworkToProject(targetGuid, "SystemConfiguration.framework", false);
        pbxProject.AddFrameworkToProject(targetGuid, "AdSupport.framework", false);
        pbxProject.AddFrameworkToProject(targetGuid, "Webkit.framework", false);
        pbxProject.AddFrameworkToProject(targetGuid, "StoreKit.framework", false);
        pbxProject.AddFrameworkToProject(targetGuid, "AdServices.framework", true); // optional=true
        
        // Add .dylib
        pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile("usr/lib/libsqlite3.0.tbd", "Frameworks/libsqlite3.0.tbd", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));

        // Save the changes to Xcode project file.
        pbxProject.WriteToFile(projectPath);
    }


    static void HandleCustomAppDelegate(string pathToBuiltProject)
    {
        if( !SingularEditorParams.IsIOSUseCustomAppDelegate )
            return;

        // get the path to SingularAppDelegate.m in built project
        var SingularAppDelegateFile            = $"{pathToBuiltProject}/Libraries/singular-unity-package/SingularSDK/Plugins/iOS/SingularAppDelegate.m";
        // get the content
        var SingularAppDelegateFileContent     = File.ReadAllText(SingularAppDelegateFile);
        // comment out the App delagate inplementation directive
        var SingularAppDelegateFileReplacement = SingularAppDelegateFileContent.Replace("IMPL_APP_CONTROLLER_SUBCLASS(SingularAppDelegate)", "//IMPL_APP_CONTROLLER_SUBCLASS(SingularAppDelegate)");
        // save the modified file
        File.WriteAllText( SingularAppDelegateFile, SingularAppDelegateFileReplacement);
    }
}

#endif

#if UNITY_ANDROID

public class SingularPostBuild: IPostGenerateGradleAndroidProject
{
    public int callbackOrder { get { return 1; } }

    public void OnPostGenerateGradleAndroidProject(string basePath)
    {
        ModifyAndroidManifestXmlFile(basePath);
    }
    
    private void ModifyAndroidManifestXmlFile(string basePath)
    {
        string appManifestPath = Path.Combine(basePath, "src/main/AndroidManifest.xml");

        // Let's open the app's AndroidManifest.xml file.
        XmlDocument manifestFile = new XmlDocument();
        manifestFile.Load(appManifestPath);

        // Add needed permissions if they are missing.
        AddPermissions(manifestFile);

        manifestFile.Save(appManifestPath);

        // Clean the manifest file.
        CleanManifestFile(appManifestPath);
    }

    static void AddPermissions(XmlDocument manifest)
    {
        List<string> existingPermissions = new List<string>();

        XmlElement manifestRoot = manifest.DocumentElement;

        string USES_PERMISSION_ELEMENT = "uses-permission";
        
        // Check if permissions are already there.
        foreach (XmlNode node in manifestRoot.ChildNodes)
        {
            if (node.Name == USES_PERMISSION_ELEMENT)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    existingPermissions.Add(attribute.Value);
                }
            }
        }

        string[] permissionsToAdd = new[]
        {
            "android.permission.ACCESS_NETWORK_STATE",
            "android.permission.INTERNET",
            "BIND_GET_INSTALL_REFERRER_SERVICE",
            "com.android.vending.CHECK_LICENSE",
            "com.google.android.gms.permission.AD_ID"
        };

        string ANDROID_NAME_ATTRIBUTE = "android__name"; // see doc inside below function: cleanManifestFile
        foreach (string permission in permissionsToAdd)
        {
            if (!existingPermissions.Contains(permission))
            {
                XmlElement element = manifest.CreateElement(USES_PERMISSION_ELEMENT);
                element.SetAttribute(ANDROID_NAME_ATTRIBUTE, permission);
                manifestRoot.AppendChild(element);
            }
        }
    }

    static void CleanManifestFile(String manifestPath)
    {
        // Due to XML writing issue with XmlElement methods which are unable
        // to write "android:[param]" string, we have wrote "android__[param]" string instead.
        // Now make the replacement: "android:[param]" -> "android__[param]"

        // Ex: If we set attribute as
        // XmlElement element = manifest.CreateElement("uses-permission");
        // element.SetAttribute("android:name", Constants.INTERNET);
        // When we run script, it becomes <uses-permission name=INTERNET> in AndroidManifest.xml
        // It should be as <uses-permission android:name=INTERNET>
        // So we write as android__name and replace it later

        TextReader manifestReader = new StreamReader(manifestPath);
        string manifestContent = manifestReader.ReadToEnd();
        manifestReader.Close();

        Regex regex = new Regex("android__");
        manifestContent = regex.Replace(manifestContent, "android:");

        TextWriter manifestWriter = new StreamWriter(manifestPath);
        manifestWriter.Write(manifestContent);
        manifestWriter.Close();
    }
}

#endif

}
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
        pbxProject.AddFrameworkToProject(targetGuid, "AdServices.framework", true);


        // Add .dylib
        pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile("usr/lib/libsqlite3.0.tbd", "Frameworks/libsqlite3.0.tbd", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));

        // Save the changes to Xcode project file.
        pbxProject.WriteToFile(projectPath);
    }
}

#endif

#if UNITY_ANDROID

public class SingularPostBuild: IPostGenerateGradleAndroidProject
{
    public const string ACCESS_NETWORK_STATE = "android.permission.ACCESS_NETWORK_STATE";
    public const string INTERNET = "android.permission.INTERNET";

    public void OnPostGenerateGradleAndroidProject(string basePath)
    {
        string appManifestPath = Path.Combine(basePath, "src/main/AndroidManifest.xml");

        // Let's open the app's AndroidManifest.xml file.
        XmlDocument manifestFile = new XmlDocument();
        manifestFile.Load(appManifestPath);

        // Add needed permissions if they are missing.
        addPermissions(manifestFile);

        manifestFile.Save(appManifestPath);

        // Clean the manifest file.
        cleanManifestFile(appManifestPath);
    }

    public int callbackOrder { get { return 1; } }

    static void addPermissions(XmlDocument manifest)
    {
        List<string> existingPermissions = new List<string>();

        XmlElement manifestRoot = manifest.DocumentElement;

        // Check if permissions are already there.
        foreach (XmlNode node in manifestRoot.ChildNodes)
        {
            if (node.Name == "uses-permission")
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    existingPermissions.Add(attribute.Value);
                }
            }
        }

        if (!existingPermissions.Contains(INTERNET))
        {
            XmlElement element = manifest.CreateElement("uses-permission");
            element.SetAttribute("android__name", INTERNET);
            manifestRoot.AppendChild(element);
        }

        if (!existingPermissions.Contains(ACCESS_NETWORK_STATE))
        {
            XmlElement element = manifest.CreateElement("uses-permission");
            element.SetAttribute("android__name", ACCESS_NETWORK_STATE);
            manifestRoot.AppendChild(element);
        }
    }

    static void cleanManifestFile(String manifestPath)
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
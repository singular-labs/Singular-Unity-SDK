using System;
using System.Xml;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Android;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
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
        pbxProject.AddFrameworkToProject(targetGuid, "AdServices.framework", true); // optional=true
        
        // Add .dylib
        pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile("usr/lib/libsqlite3.0.tbd", "Frameworks/libsqlite3.0.tbd", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));

        // Save the changes to Xcode project file.
        pbxProject.WriteToFile(projectPath);
    }





    private const string TargetUnityIphonePodfileLine    = "target 'Unity-iPhone' do";
    private const string UseFrameworksPodfileLine        = "use_frameworks!";
    private const string UseFrameworksDynamicPodfileLine = "use_frameworks! :linkage => :dynamic";
    private const string UseFrameworksStaticPodfileLine  = "use_frameworks! :linkage => :static";
    private const string SingularSDKFramework            = "Singular.xcframework";
    
    [PostProcessBuild(int.MaxValue)] // at the very end of the build, pod install will be complete
    private static void EndOfBuild(BuildTarget buildTarget, string buildPath)
    {
        // Check that the Pods directory exists (it might not if a publisher is building with Generate Podfile setting disabled in EDM).
        var podsDirectory = Path.Combine(buildPath, "Pods");
        if( !Directory.Exists(podsDirectory) || !ShouldEmbedDynamicLibraries( buildPath ) ) 
            return;
        
        //Debug.Log( $"[BuildProcessor] try embedding {SingularSDKFramework} to UnityMainTarget" );
        
        // find the SingularSDKFramework framework into Pods directory
        
        // both .framework and .xcframework are directories, not files
        var directories = Directory.GetDirectories(podsDirectory, SingularSDKFramework, SearchOption.AllDirectories);
        if( directories.Length <= 0 )
        {
            //Debug.LogError( $"[BuildProcessor] Framework:{SingularSDKFramework} not found in Pods directory:{podsDirectory}" );
            return;
        }
        
        var projectPath = PBXProject.GetPBXProjectPath(buildPath);
        var project     = new PBXProject();
        project.ReadFromFile(projectPath);
        
        var unityMainTargetGuid = project.GetUnityMainTargetGuid();
        
        var dynamicLibraryAbsolutePath       = directories[0];
        var index                            = dynamicLibraryAbsolutePath.LastIndexOf("Pods", StringComparison.Ordinal );
        var SingularSDKFrameworkRelativePath = dynamicLibraryAbsolutePath[index..];
        
        var fileGuid = project.AddFile(SingularSDKFrameworkRelativePath, SingularSDKFrameworkRelativePath);
        project.AddFileToEmbedFrameworks(unityMainTargetGuid, fileGuid);
        
        // save edited project
        project.WriteToFile(projectPath);
        
        //Debug.Log( $"[BuildProcessor] file:{SingularSDKFrameworkRelativePath} added with GUID:{fileGuid}" );
    }

    /// <summary>
    /// |-----------------------------------------------------------------------------------------------------------------------------------------------------|
    /// |         embed             |  use_frameworks! (:linkage => :dynamic)  |  use_frameworks! :linkage => :static  |  `use_frameworks!` line not present  |
    /// |---------------------------|------------------------------------------|---------------------------------------|--------------------------------------|
    /// | Unity-iPhone present      | Do not embed dynamic libraries           | Embed dynamic libraries               | Do not embed dynamic libraries       |
    /// | Unity-iPhone not present  | Embed dynamic libraries                  | Embed dynamic libraries               | Embed dynamic libraries              |
    /// |-----------------------------------------------------------------------------------------------------------------------------------------------------|
    /// </summary>
    /// <param name="buildPath">An iOS build path</param>
    /// <returns>Whether or not the dynamic libraries should be embedded.</returns>
    private static bool ShouldEmbedDynamicLibraries( string buildPath )
    {
        var podfilePath = Path.Combine( buildPath, "Podfile" );
        if( !File.Exists( podfilePath ) )
            return false;

        // If the Podfile doesn't have a `Unity-iPhone` target, we should embed the dynamic libraries.
        var lines                     = File.ReadAllLines( podfilePath );
        var containsUnityIphoneTarget = lines.Any( line => line.Contains( TargetUnityIphonePodfileLine ) );
        if( !containsUnityIphoneTarget )
            return true;

        // If the Podfile does not have a `use_frameworks! :linkage => static` line, we should not embed the dynamic libraries.
        var useFrameworksStaticLineIndex = Array.FindIndex( lines, line => line.Contains( UseFrameworksStaticPodfileLine ) );
        if( useFrameworksStaticLineIndex == -1 ) 
            return false;

        // If more than one of the `use_frameworks!` lines are present, CocoaPods will use the last one.
        var useFrameworksLineIndex        = Array.FindIndex( lines, line => line.Trim() == UseFrameworksPodfileLine ); // Check for exact line to avoid matching `use_frameworks! :linkage => static/dynamic`
        var useFrameworksDynamicLineIndex = Array.FindIndex( lines, line => line.Contains( UseFrameworksDynamicPodfileLine ) );

        // Check if `use_frameworks! :linkage => :static` is the last line of the three. If it is, we should embed the dynamic libraries.
        return useFrameworksLineIndex        < useFrameworksStaticLineIndex &&
               useFrameworksDynamicLineIndex < useFrameworksStaticLineIndex;
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
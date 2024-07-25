using UnityEditor;

namespace Singular.Editor
{
    public static class SingularEditorParams
    {
        private const string IOS_USE_CUSTOM_APP_DELEGATE_EDITOR_KEY = "SingularIsIOSUseCustomAppDelegate";
        internal static bool IsIOSUseCustomAppDelegate => EditorPrefs.GetInt( IOS_USE_CUSTOM_APP_DELEGATE_EDITOR_KEY, 0 ) == 1;

        // IOS APP USES CUSTOM APP DELEGATE

        [MenuItem( "Window/Singular/My iOS App use a custom AppDelegate", true )]
        private static bool _MenuItem_iOSUseCustomAppDelegate()
        {
            return !IsIOSUseCustomAppDelegate;
        }
        [MenuItem( "Window/Singular/My iOS App use a custom AppDelegate" )]
        private static void MenuItem_iOSUseCustomAppDelegate()
        {
            EditorPrefs.SetInt( IOS_USE_CUSTOM_APP_DELEGATE_EDITOR_KEY, 1 );
        }

        // IOS APP DON'T USES CUSTOM APP DELEGATE

        [MenuItem( "Window/Singular/My iOS App don't use a custom AppDelegate", true )]
        private static bool _MenuItem_iOSDontUseCustomAppDelegate()
        {
            return IsIOSUseCustomAppDelegate;
        }
        [MenuItem( "Window/Singular/My iOS App don't use a custom AppDelegate" )]
        private static void MenuItem_iOSDontUseCustomAppDelegate()
        {
            EditorPrefs.SetInt( IOS_USE_CUSTOM_APP_DELEGATE_EDITOR_KEY, 0 );
        }
    }
}
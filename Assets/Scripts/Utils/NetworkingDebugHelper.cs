using System;
using UnityEditor;

namespace Utils
{
#if UNITY_EDITOR
    public static class NetworkingDebugHelper
    {
        private const string TOGGLE_SERVER_DEBUG_MENU = "Dungen/Debug server";
        private const string DEBUG_DEFINE = "DUNGEN_NETWORK_DEBUG";

        private static bool debugEnabled;

        [MenuItem(TOGGLE_SERVER_DEBUG_MENU)]
        private static void ToggleServerDebug()
        {
            debugEnabled = !debugEnabled;

            if (debugEnabled)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, DEBUG_DEFINE);
            }
            else
            {
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

                var index = defines.IndexOf(DEBUG_DEFINE);
                if (index > 0) index -= 1;

                var lengthToRemove = Math.Min(DEBUG_DEFINE.Length + 1, defines.Length - index);

                defines = defines.Remove(index, lengthToRemove);

                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
            }
        }

        [MenuItem(TOGGLE_SERVER_DEBUG_MENU, true)]
        private static bool ValidateToggleServerDebug()
        {
            Menu.SetChecked(TOGGLE_SERVER_DEBUG_MENU, debugEnabled);
            return true;
        }
    }
#endif
}

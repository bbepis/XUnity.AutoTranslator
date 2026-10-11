#if MANAGED

using System.Reflection;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class IMGUIWindowFunctions
   {
      private static FieldInfo _layoutedWindowFunc;
      private static bool _layoutedWindowFuncLookedUp;

      /// <summary>
      /// GUILayout.Window does not pass the plugin's window function to GUI.Window. It wraps it in GUILayout.LayoutedWindow
      /// and passes LayoutedWindow.DoWindow instead, a method of UnityEngine.IMGUIModule. Anything that identifies the
      /// plugin by the window function (plugin-specific translations, BlacklistedIMGUIPlugins) must look behind that wrapper,
      /// where the plugin's function is kept in the m_Func field. Any other window function is returned as is.
      /// </summary>
      public static GUI.WindowFunction GetPluginFunction( GUI.WindowFunction function )
      {
         var target = function?.Target;
         if( target == null ) return function;

         var type = target.GetType();
         if( type.Name != "LayoutedWindow" || type.DeclaringType == null || type.DeclaringType.Name != "GUILayout" ) return function;

         if( !_layoutedWindowFuncLookedUp )
         {
            _layoutedWindowFuncLookedUp = true;
            _layoutedWindowFunc = type.GetField( "m_Func", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
         }

         return _layoutedWindowFunc?.GetValue( target ) as GUI.WindowFunction ?? function;
      }
   }
}

#endif

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Shims;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core
{

   /// <summary>
   /// Class that allows you to check which features are availble of the Unity version that is used.
   /// </summary>
   public static class UnityFeatures
   {
      private static readonly BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

      internal static bool SupportsMouseScrollDelta { get; } = false;
      internal static bool SupportsClipboard { get; } = false;
      internal static bool SupportsCustomYieldInstruction { get; } = false;
      internal static bool SupportsSceneManager { get; } = false;
      internal static bool SupportsWaitForSecondsRealtime { get; } = false;

      static UnityFeatures()
      {
         try
         {
            SupportsClipboard = UnityTypes.TextEditor?.GetProperty( "text" )?.GetSetMethod() != null;
         }
         catch( Exception )
         {
            
         }

         try
         {
            SupportsCustomYieldInstruction = UnityTypes.CustomYieldInstruction != null;
         }
         catch( Exception )
         {
            
         }

         try
         {
            SupportsSceneManager = UnityTypes.Scene != null
               && UnityTypes.SceneManager != null
               && UnityTypes.SceneManager.GetMethod("add_sceneLoaded", All) != null;
         }
         catch( Exception )
         {

         }

         try
         {
            SupportsMouseScrollDelta = UnityTypes.Input.GetProperty( "mouseScrollDelta" ) != null;
         }
         catch( Exception )
         {

         }

         try
         {
            SupportsWaitForSecondsRealtime = UnityTypes.WaitForSecondsRealtime != null;
         }
         catch( Exception )
         {

         }
      }
   }
}

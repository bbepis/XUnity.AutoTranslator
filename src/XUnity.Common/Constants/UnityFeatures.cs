using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace XUnity.Common.Constants
{

   /// <summary>
   /// Class that allows you to check which features are availble of the Unity version that is used.
   /// </summary>
   public static class UnityFeatures
   {
      private static readonly BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

      public static bool SupportsMouseScrollDelta { get; } = false;
      public static bool SupportsClipboard { get; } = false;
      public static bool SupportsCustomYieldInstruction { get; } = false;
      public static bool SupportsSceneManager { get; } = false;
      public static bool SupportsWaitForSecondsRealtime { get; set; } = false;

      static UnityFeatures()
      {
         try
         {
            SupportsClipboard = UnityTypes.TextEditor?.ClrType.GetProperty( "text" )?.GetSetMethod() != null;
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
#if MANAGED
               && UnityTypes.SceneManager.ClrType.GetMethod("add_sceneLoaded", All) != null;
#else
               && UnityTypes.SceneManager_Methods.add_sceneLoaded != null;
#endif
         }
         catch( Exception )
         {

         }

         try
         {
            SupportsMouseScrollDelta = UnityTypes.Input?.ClrType.GetProperty( "mouseScrollDelta" ) != null;
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

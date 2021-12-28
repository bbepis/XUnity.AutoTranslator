#if MANAGED

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class PluginTranslationHooks
   {
      public static readonly Type[] All = new[] {
         typeof( Transform_SetParent_Hook ),
         typeof( GameObject_AddComponent_Hook ),
         typeof( Object_InstantiateSingle_Hook ),
         typeof( Object_InstantiateSingleWithParent_Hook ),
         typeof( Object_CloneSingle_Hook ),
         typeof( Object_CloneSingleWithParent_Hook ),
      };
   }

   internal static class Transform_SetParent_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Transform != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.Transform.ClrType, "SetParent", new Type[] { UnityTypes.Transform.ClrType, typeof( bool ) } );
      }

      static Action<Transform, Transform, bool> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Transform, Transform, bool>>();
      }

      static void MM_Detour( Transform __instance, Transform parent, bool worldPositionStays )
      {
         if( parent != null )
         {
            var info = parent.GetExtensionData<TransformInfo>();
            if( info?.TextCache != null )
            {
               var prev = CallOrigin.TextCache;
               CallOrigin.TextCache = info.TextCache;
               try
               {
                  CallOrigin.AssociateSubHierarchyWithTransformInfo( __instance, info );
               }
               finally
               {
                  CallOrigin.TextCache = prev;
               }
            }
         }

         _original( __instance, parent, worldPositionStays );
      }
   }

   internal static class GameObject_AddComponent_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GameObject != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GameObject.ClrType, "Internal_AddComponentWithType", new Type[] { typeof( Type ) } );
      }

      static Func<GameObject, Type, Component> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<GameObject, Type, Component>>();
      }

      static Component MM_Detour( GameObject __instance, Type componentType )
      {
         var result = _original( __instance, componentType );

         if( result.IsKnownTextType() )
         {
            var cache = CallOrigin.CalculateTextCacheFromStackTrace( null );
            if( cache != null )
            {
               var info = result.GetOrCreateTextTranslationInfo();
               info.TextCache = cache;
            }
         }

         return result;
      }
   }

   internal static class Object_InstantiateSingle_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Object != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.Object.ClrType, "Internal_InstantiateSingle", new Type[] { typeof( UnityEngine.Object ), typeof( Vector3 ), typeof( Quaternion ) } );
      }

      static Func<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object>>();
      }

      static UnityEngine.Object MM_Detour( UnityEngine.Object data, Vector3 pos, Quaternion rot )
      {
         var prev = CallOrigin.TextCache;
         try
         {
            CallOrigin.TextCache = CallOrigin.CalculateTextCacheFromStackTrace( null );

            var result = _original( data, pos, rot );

            if( CallOrigin.TextCache != null && result is GameObject go && !go.activeInHierarchy )
            {
               CallOrigin.SetTextCacheForAllObjectsInHierachy( go, CallOrigin.TextCache );
            }

            return result;
         }
         finally
         {
            CallOrigin.TextCache = prev;
         }
      }
   }

   internal static class Object_InstantiateSingleWithParent_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Object != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.Object.ClrType, "Internal_InstantiateSingleWithParent", new Type[] { typeof( UnityEngine.Object ), typeof( Transform ), typeof( Vector3 ), typeof( Quaternion ) } );
      }

      static Func<UnityEngine.Object, Transform, Vector3, Quaternion, UnityEngine.Object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<UnityEngine.Object, Transform, Vector3, Quaternion, UnityEngine.Object>>();
      }

      static UnityEngine.Object MM_Detour( UnityEngine.Object data, Transform parent, Vector3 pos, Quaternion rot )
      {
         var prev = CallOrigin.TextCache;
         try
         {
            CallOrigin.TextCache = CallOrigin.CalculateTextCacheFromStackTrace( parent?.gameObject );

            var result = _original( data, parent, pos, rot );

            if( CallOrigin.TextCache != null && result is GameObject go && !go.activeInHierarchy )
            {
               CallOrigin.SetTextCacheForAllObjectsInHierachy( go, CallOrigin.TextCache );
            }

            return result;
         }
         finally
         {
            CallOrigin.TextCache = prev;
         }
      }
   }

   internal static class Object_CloneSingle_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Object != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.Object.ClrType, "Internal_CloneSingle", new Type[] { typeof( UnityEngine.Object ) } );
      }

      static Func<UnityEngine.Object, UnityEngine.Object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<UnityEngine.Object, UnityEngine.Object>>();
      }

      static UnityEngine.Object MM_Detour( UnityEngine.Object data )
      {
         var prev = CallOrigin.TextCache;
         try
         {
            CallOrigin.TextCache = CallOrigin.CalculateTextCacheFromStackTrace( null );

            var result = _original( data );

            if( CallOrigin.TextCache != null && result is GameObject go && !go.activeInHierarchy )
            {
               CallOrigin.SetTextCacheForAllObjectsInHierachy( go, CallOrigin.TextCache );
            }

            return result;
         }
         finally
         {
            CallOrigin.TextCache = prev;
         }
      }
   }

   internal static class Object_CloneSingleWithParent_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Object != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.Object.ClrType, "Internal_CloneSingleWithParent", new Type[] { typeof( UnityEngine.Object ), typeof( Transform ), typeof( bool ) } );
      }

      static Func<UnityEngine.Object, Transform, bool, UnityEngine.Object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<UnityEngine.Object, Transform, bool, UnityEngine.Object>>();
      }

      static UnityEngine.Object MM_Detour( UnityEngine.Object data, Transform parent, bool worldPositionStays )
      {
         var prev = CallOrigin.TextCache;
         try
         {
            CallOrigin.TextCache = CallOrigin.CalculateTextCacheFromStackTrace( parent?.gameObject );

            var result = _original( data, parent, worldPositionStays );

            if( CallOrigin.TextCache != null && result is GameObject go && !go.activeInHierarchy )
            {
               CallOrigin.SetTextCacheForAllObjectsInHierachy( go, CallOrigin.TextCache );
            }

            return result;
         }
         finally
         {
            CallOrigin.TextCache = prev;
         }
      }
   }
}

#endif

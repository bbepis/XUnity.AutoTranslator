using System.Collections.Generic;
using System.Runtime.CompilerServices;
using XUnity.Common.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using UnityEngine;
using XUnity.Common.Extensions;

#if IL2CPP
using Il2CppInterop.Runtime;
#endif

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class ComponentHelper
   {
      public static T[] FindObjectsOfType<T>()
         where T : UnityEngine.Object
      {
#if IL2CPP
         var il2cppType = Il2CppSystem.Type.internal_from_handle( IL2CPP.il2cpp_class_get_type( Il2CppClassPointerStore<T>.NativeClassPtr ) );
         var objects = UnityEngine.Object.FindObjectsOfType( il2cppType );
#else
         var objects = UnityEngine.Object.FindObjectsOfType( typeof( T ) );
#endif
         if( objects == null ) return null;

         var typedArr = new T[ objects.Length ];
         for( int i = 0 ; i < typedArr.Length ; i++ )
         {
            objects[ i ].TryCastTo<T>( out var castedObj );
            typedArr[ i ] = castedObj;
         }

         return typedArr;
      }

      public static Texture2D CreateEmptyTexture2D( TextureFormat originalTextureFormat )
      {
         TextureFormat newFormat;
         switch( originalTextureFormat )
         {
            case TextureFormat.RGB24:
               newFormat = TextureFormat.RGB24;
               break;
            case TextureFormat.DXT1:
               newFormat = TextureFormat.RGB24;
               break;
            case TextureFormat.DXT5:
               newFormat = TextureFormat.ARGB32;
               break;
            default:
               newFormat = TextureFormat.ARGB32;
               break;
         }

         return new Texture2D( 2, 2, newFormat, false );
      }
   }
}

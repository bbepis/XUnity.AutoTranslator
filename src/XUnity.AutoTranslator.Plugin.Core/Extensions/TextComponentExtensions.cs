using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextComponentExtensions
   {
      private static readonly IComponentHelper Helper = ComponentHelper.Instance;

      public static bool ShouldTranslateTextComponent( this object ui, bool ignoreComponentState )
      {
         return Helper.ShouldTranslateTextComponent( ui, ignoreComponentState );
      }

      public static bool IsComponentActive( this object ui )
      {
         return Helper.IsComponentActive( ui );
      }

      public static bool IsKnownTextType( this object ui )
      {
         return Helper.IsKnownTextType( ui );
      }

      public static bool SupportsRichText( this object ui )
      {
         return Helper.SupportsRichText( ui );
      }

      public static bool SupportsStabilization( this object ui )
      {
         return Helper.SupportsStabilization( ui );
      }

      public static bool IsSpammingComponent( this object ui )
      {
         return Helper.IsSpammingComponent( ui );
      }

      public static bool SupportsLineParser( this object ui )
      {
         return Helper.SupportsLineParser( ui );
      }

      public static bool IsNGUI( this object ui )
      {
         return Helper.IsNGUI( ui );
      }

      public static string GetText( this object ui )
      {
         if( ui == null ) return null;

         TextGetterCompatModeHelper.IsGettingText = true;
         try
         {
            return Helper.GetText( ui ) ?? string.Empty;
         }
         finally
         {
            TextGetterCompatModeHelper.IsGettingText = false;
         }
      }

      public static void SetText( this object ui, string text )
      {
         if( ui == null ) return;

         Helper.SetText( ui, text );
      }

      public static TextTranslationInfo GetOrCreateTextTranslationInfo( this object ui )
      {
         if( !ui.IsSpammingComponent() )
         {
            return Helper.GetOrCreateTextTranslationInfo( ui );
         }

         return null;
      }

      public static TextTranslationInfo GetTextTranslationInfo( this object ui )
      {
         if( !ui.IsSpammingComponent() )
         {
            return Helper.GetTextTranslationInfo( ui );
         }

         return null;
      }

      public static object CreateWrapperTextComponentIfRequiredAndPossible( this object ui )
      {
         return Helper.CreateWrapperTextComponentIfRequiredAndPossible( ui );
      }

      public static IEnumerable<object> GetAllTextComponentsInChildren( this object go )
      {
         return Helper.GetAllTextComponentsInChildren( go );
      }

      public static string[] GetPathSegments( this object ui )
      {
         return Helper.GetPathSegments( ui );
      }

      public static string GetPath( this object ui )
      {
         return Helper.GetPath( ui );
      }

      public static bool HasIgnoredName( this object ui )
      {
         return Helper.HasIgnoredName( ui );
      }

      public static object GetTexture( this object ui )
      {
         return Helper.GetTexture( ui );
      }

      public static void SetTexture( this object ui, object texture )
      {
         Helper.SetTexture( ui, texture );
      }

      public static void SetAllDirtyEx( this object ui )
      {
         Helper.SetAllDirtyEx( ui );
      }

      public static bool IsKnownImageType( this object ui )
      {
         return Helper.IsKnownImageType( ui );
      }

      public static string GetTextureName( this object texture, string fallbackName )
      {
         return Helper.GetTextureName( texture, fallbackName );
      }

      public static void LoadImageEx( this object texture, byte[] data, object originalTexture )
      {
         Helper.LoadImageEx( texture, data, originalTexture );
      }

      public static TextureDataResult GetTextureData( this object texture )
      {
         return Helper.GetTextureData( texture );
      }
   }
}

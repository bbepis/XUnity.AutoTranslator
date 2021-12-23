using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.AutoTranslator.Plugin.Core.Textures;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class ComponentExtensions
   {
      private interface IPropertyMover
      {
         void MoveProperty( object source, object destination );
      }

      private class PropertyMover<T, TPropertyType> : IPropertyMover
      {
         private readonly Func<T, TPropertyType> _get;
         private readonly Action<T, TPropertyType> _set;

         public PropertyMover( PropertyInfo propertyInfo )
         {
            var getter = propertyInfo.GetGetMethod();
            var setter = propertyInfo.GetSetMethod();

            _get = (Func<T, TPropertyType>)ExpressionHelper.CreateTypedFastInvoke( getter );
            _set = (Action<T, TPropertyType>)ExpressionHelper.CreateTypedFastInvoke( setter );
         }

         public void MoveProperty( object source, object destination )
         {
            var value = _get( (T)source );
            _set( (T)destination, value );
         }
      }

      private static readonly Color Transparent = new Color( 0, 0, 0, 0 );
      private static readonly string SetAllDirtyMethodName = "SetAllDirty";
      private static readonly string TexturePropertyName = "texture";
      private static readonly string MainTexturePropertyName = "mainTexture";
      private static readonly string CapitalMainTexturePropertyName = "MainTexture";
      private static readonly string MarkAsChangedMethodName = "MarkAsChanged";
      private static readonly string SupportRichTextPropertyName = "supportRichText";
      private static readonly string RichTextPropertyName = "richText";
      private static GameObject[] _objects = new GameObject[ 128 ];
      private static readonly string XuaIgnore = "XUAIGNORE";
      private static readonly Dictionary<Type, ITextComponentManipulator> Manipulators = new Dictionary<Type, ITextComponentManipulator>();
      private static List<IPropertyMover> TexturePropertyMovers;

      static ComponentExtensions()
      {
         TexturePropertyMovers = new List<IPropertyMover>();
         LoadProperty<string>( "name" );
         LoadProperty<int>( "anisoLevel" );
         LoadProperty<FilterMode>( "filterMode" );
         LoadProperty<float>( "mipMapBias" );
         LoadProperty<TextureWrapMode>( "wrapMode" );
      }

      private static void LoadProperty<TPropertyType>( string propertyName )
      {
         var property = typeof( Texture2D ).GetProperty( propertyName );
         if( property.CanWrite && property.CanRead )
         {
            TexturePropertyMovers.Add( new PropertyMover<Texture, TPropertyType>( property ) );
         }
      }

      private static ITextComponentManipulator GetTextManipulator( object ui )
      {
         var type = ui.GetType();
         if( type == null )
         {
            return null;
         }

         if( !Manipulators.TryGetValue( type, out var manipulator ) )
         {
            if( ui is ITextComponent )
            {
               manipulator = new TextComponentManipulator();
            }
#if MANAGED
            else if( type == UnityTypes.TextField )
            {
               manipulator = new FairyGUITextComponentManipulator();
            }
            else if( type == UnityTypes.TextArea2D )
            {
               manipulator = new TextArea2DComponentManipulator();
            }
#endif
            else
            {
               manipulator = new DefaultTextComponentManipulator( type );
            }
            Manipulators[ type ] = manipulator;
         }

         return manipulator;
      }

      public static bool IsComponentActive( this object ui )
      {
         if( ui is Component component )
         {
            return component.gameObject?.activeInHierarchy ?? false;
         }
         else if( ui is ITextComponent tc )
         {
            return tc.Component.gameObject?.activeInHierarchy ?? false;
         }
         return true;
      }

      public static bool IsKnownTextType( this object ui )
      {
         if( ui == null ) return false;

#if MANAGED
         var type = ui.GetType();
#else
         var isIl2CppType = ui is UnhollowerBaseLib.Il2CppObjectBase;
         var type = isIl2CppType ? ui.GetIl2CppTypeSafe() : null;
#endif


         return ( Settings.EnableIMGUI && !_guiContentCheckFailed && IsGUIContentSafe( ui ) )
            || ( ui is ITextComponent tc && tc.IsEnabledInSettings() )
#if MANAGED
            || ( Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( type ) )
            || ( Settings.EnableNGUI && UnityTypes.UILabel != null && UnityTypes.UILabel.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.IsAssignableFrom( type ) )
            || ( Settings.EnableFairyGUI && UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMeshPro && IsKnownTextMeshProType( type ) )
#else
            || ( isIl2CppType && Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.Il2CppType.IsAssignableFrom( type ) )
            || ( isIl2CppType && Settings.EnableNGUI && UnityTypes.UILabel != null && UnityTypes.UILabel.Il2CppType.IsAssignableFrom( type ) )
            || ( isIl2CppType && Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.Il2CppType.IsAssignableFrom( type ) )
            || ( isIl2CppType && Settings.EnableTextMeshPro && IsKnownTextMeshProType( type ) )
#endif
            ;
      }

#if MANAGED
      public static bool IsKnownTextMeshProType( Type type )
      {
         if( UnityTypes.TMP_Text != null )
         {
            return UnityTypes.TMP_Text.IsAssignableFrom( type );
         }
         else
         {
            return UnityTypes.TextMeshProUGUI?.IsAssignableFrom( type ) == true
               || UnityTypes.TextMeshPro?.IsAssignableFrom( type ) == true;
         }
      }
#else
      public static bool IsKnownTextMeshProType( Il2CppSystem.Type type )
      {
         if( UnityTypes.TMP_Text != null )
         {
            return UnityTypes.TMP_Text.Il2CppType.IsAssignableFrom( type );
         }
         else
         {
            return UnityTypes.TextMeshProUGUI?.Il2CppType.IsAssignableFrom( type ) == true
               || UnityTypes.TextMeshPro?.Il2CppType.IsAssignableFrom( type ) == true;
         }
      }
#endif

      public static bool SupportsRichText( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( ui is ITextComponent tc && tc.SupportsRichText() )
#if MANAGED
            || ( UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( type ) && Equals( type.CachedProperty( SupportRichTextPropertyName )?.Get( ui ), true ) )
            || ( UnityTypes.TextMesh != null && UnityTypes.TextMesh.IsAssignableFrom( type ) && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
            || DoesTextMeshProSupportRichText( ui, type )
            || ( UnityTypes.UguiNovelText != null && UnityTypes.UguiNovelText.IsAssignableFrom( type ) )
            || ( UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom( type ) )
#endif
            ;
      }

#if MANAGED
      public static bool DoesTextMeshProSupportRichText( object ui, Type type )
      {
         if( UnityTypes.TMP_Text != null )
         {
            return UnityTypes.TMP_Text.IsAssignableFrom( type ) && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true );
         }
         else
         {
            return ( UnityTypes.TextMeshPro?.IsAssignableFrom( type ) == true && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
               || ( UnityTypes.TextMeshProUGUI?.IsAssignableFrom( type ) == true && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) );
         }
      }
#endif

      public static bool SupportsStabilization( this object ui )
      {
         if( ui == null ) return false;

         return _guiContentCheckFailed || !IsGUIContentSafe( ui );
      }

      public static bool IsSpammingComponent( this object ui )
      {
         return ui == null
            || ( !_guiContentCheckFailed && IsGUIContentSafe( ui ) )
            || ( ui is ITextComponent tc && tc.IsSpammingComponent() );
      }

      private static bool _guiContentCheckFailed;
      private static bool IsGUIContentSafe( object ui )
      {
         try
         {
            return IsGUIContentUnsafe( ui );
         }
         catch
         {
            _guiContentCheckFailed = true;
         }
         return false;
      }

      private static bool IsGUIContentUnsafe( object ui ) => ui.TryCastTo<GUIContent>( out _ );

      private static bool SetTextOnGUIContentSafe( object ui, string text )
      {
         try
         {
            return SetTextOnGUIContentUnsafe( ui, text );
         }
         catch
         {
            _guiContentCheckFailed = true;
         }
         return false;
      }

      private static bool SetTextOnGUIContentUnsafe( object ui, string text )
      {
         if( ui.TryCastTo<GUIContent>( out var gui ) )
         {
            gui.text = text;
            return true;
         }
         return false;
      }

      private static bool TryGetTextFromGUIContentSafe( object ui, out string text )
      {
         try
         {
            return TryGetTextFromGUIContentUnsafe( ui, out text );
         }
         catch
         {
            _guiContentCheckFailed = false;
         }
         text = null;
         return false;
      }

      private static bool TryGetTextFromGUIContentUnsafe( object ui, out string text )
      {
         if( ui.TryCastTo<GUIContent>( out var gui ) )
         {
            text = gui.text;
            return true;
         }
         text = null;
         return false;
      }

      public static bool SupportsLineParser( this object ui )
      {
         if( Settings.GameLogTextPaths.Count > 0 )
         {
            if( ui is Component component )
            {
               return Settings.GameLogTextPaths.Contains( component.gameObject.GetPath() );
            }
            else if( ui is ITextComponent tc )
            {
               return Settings.GameLogTextPaths.Contains( tc.Component.gameObject.GetPath() );
            }
         }
         return false;
      }

      public static string GetText( this object ui )
      {
         if( ui == null ) return null;

         TextGetterCompatModeHelper.IsGettingText = true;
         try
         {
            if( _guiContentCheckFailed || !TryGetTextFromGUIContentSafe( ui, out var text ) )
            {
               return GetTextManipulator( ui )?.GetText( ui );
            }
            return text;
         }
         finally
         {
            TextGetterCompatModeHelper.IsGettingText = false;
         }
      }

      public static void SetText( this object ui, string text )
      {
         if( ui == null ) return;

         if( _guiContentCheckFailed || !SetTextOnGUIContentSafe( ui, text ) )
         {
            GetTextManipulator( ui )?.SetText( ui, text );
         }
      }

      public static TextTranslationInfo GetOrCreateTextTranslationInfo( this object ui )
      {
         if( !ui.IsSpammingComponent() )
         {
            var info = ui.GetOrCreateExtensionData<TextTranslationInfo>();
            info.Initialize( ui );
            return info;
         }

         return null;
      }

      public static TextTranslationInfo GetTextTranslationInfo( this object ui )
      {
         if( !ui.IsSpammingComponent() )
         {
            var info = ui.GetExtensionData<TextTranslationInfo>();
            return info;
         }

         return null;
      }

      public static object CreateWrapperTextComponentIfRequiredAndPossible( this object ui )
      {
         if( IsKnownTextType( ui ) )
         {
#if MANAGED
            return ui;
#else
            if( ui.TryCastTo<Component>( out var component ) )
            {
               var type = component.GetIl2CppTypeSafe();

               if( Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.Il2CppType.IsAssignableFrom( type ) )
               {
                  return new TextComponent( component );
               }
               else if( Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.Il2CppType.IsAssignableFrom( type ) )
               {
                  return new TextMeshComponent( component );
               }
               else if( Settings.EnableTextMeshPro && UnityTypes.TMP_Text != null && UnityTypes.TMP_Text.Il2CppType.IsAssignableFrom( type ) )
               {
                  return new TMP_TextComponent( component );
               }
            }
#endif
         }

         return null;
      }

      public static IEnumerable<object> GetAllTextComponentsInChildren( this object go )
      {
         // Only used for plugin specific hooks
#if MANAGED
         var gameObject = (GameObject)go;

         if( Settings.EnableTextMeshPro && UnityTypes.TMP_Text != null )
         {
            foreach( var comp in gameObject.GetComponentsInChildren( UnityTypes.TMP_Text, true ) )
            {
               yield return comp;
            }
         }
         if( Settings.EnableUGUI && UnityTypes.Text != null )
         {
            foreach( var comp in gameObject.GetComponentsInChildren( UnityTypes.Text, true ) )
            {
               yield return comp;
            }
         }
         if( Settings.EnableTextMesh && UnityTypes.TextMesh != null )
         {
            foreach( var comp in gameObject.GetComponentsInChildren( UnityTypes.TextMesh, true ) )
            {
               yield return comp;
            }
         }
         if( Settings.EnableNGUI && UnityTypes.UILabel != null )
         {
            foreach( var comp in gameObject.GetComponentsInChildren( UnityTypes.UILabel, true ) )
            {
               yield return comp;
            }
         }
#else
         yield break;
#endif
      }

      private static GameObject GetAssociatedGameObject( object obj )
      {
         if( obj is GameObject go )
         {

         }
         else if( obj is ITextComponent tc )
         {
            go = tc.Component.gameObject;
         }
         else if( obj is Component comp )
         {
            go = comp.gameObject;
         }
         else
         {
            throw new ArgumentException( "Expected object to be a GameObject or component.", "obj" );
         }

         return go;
      }

      public static string[] GetPathSegments( this object obj )
      {
         var go = GetAssociatedGameObject( obj );

         int i = 0;
         int j = 0;

         _objects[ i++ ] = go;
         while( go.transform.parent != null )
         {
            go = go.transform.parent.gameObject;
            _objects[ i++ ] = go;
         }

         var result = new string[ i ];
         while( --i >= 0 )
         {
            result[ j++ ] = _objects[ i ].name;
            _objects[ i ] = null;
         }

         return result;
      }

      public static string GetPath( this object obj )
      {
         StringBuilder path = new StringBuilder();
         var segments = GetPathSegments( obj );
         for( int i = 0 ; i < segments.Length ; i++ )
         {
            path.Append( "/" ).Append( segments[ i ] );
         }

         return path.ToString();
      }

      public static bool HasIgnoredName( this object ui )
      {
         var go = GetAssociatedGameObject( ui );
         return go.name.Contains( XuaIgnore );
      }

      public static object GetTexture( this object ui )
      {
#warning NOT TESTED FOR IL2CPP, but may work
         if( ui == null ) return null;

         if( ui is SpriteRenderer spriteRenderer )
         {
            return spriteRenderer.sprite?.texture;
         }
         else
         {
            // lets attempt some reflection for several known types
            var type = ui.GetType();
            var texture = type.CachedProperty( MainTexturePropertyName )?.Get( ui )
               ?? type.CachedProperty( TexturePropertyName )?.Get( ui )
               ?? type.CachedProperty( CapitalMainTexturePropertyName )?.Get( ui );

            return texture as Texture2D;
         }
      }

      public static object SetTexture( this object ui, object textureObj, object spriteObj, bool isPrefixHooked )
      {
#warning NOT TESTED FOR IL2CPP, but may work
         if( ui == null ) return null;
         var sprite = (Sprite)spriteObj;
         var texture = (Texture2D)textureObj;

         var currentTexture = ui.GetTexture();

         if( !Equals( currentTexture, texture ) )
         {
            if( Settings.EnableSpriteRendererHooking && ui is SpriteRenderer sr )
            {
               if( isPrefixHooked )
               {
                  return SafeCreateSprite( sr, sprite, texture );
               }
               else
               {
                  return SafeSetSprite( sr, sprite, texture );
               }
            }
            else
            {
               // This logic is only used in legacy mode and is not verified with NGUI
               var type = ui.GetType();
               type.CachedProperty( MainTexturePropertyName )?.Set( ui, texture );
               type.CachedProperty( TexturePropertyName )?.Set( ui, texture );
               type.CachedProperty( CapitalMainTexturePropertyName )?.Set( ui, texture );

               // special handling for UnityEngine.UI.Image
               var material = type.CachedProperty( "material" )?.Get( ui );
               if( material != null )
               {
                  var mainTextureProperty = material.GetType().CachedProperty( MainTexturePropertyName );
                  var materialTexture = mainTextureProperty?.Get( material );
                  if( ReferenceEquals( materialTexture, currentTexture ) )
                  {
                     mainTextureProperty?.Set( material, texture );
                  }
               }
            }
         }

         return null;
      }

      private static Sprite SafeSetSprite( SpriteRenderer sr, Sprite sprite, Texture2D texture )
      {
         var newSprite = Sprite.Create( texture, sprite != null ? sprite.rect : sr.sprite.rect, Vector2.zero );
         sr.sprite = newSprite;
         return newSprite;
      }

      private static Sprite SafeCreateSprite( SpriteRenderer sr, Sprite sprite, Texture2D texture )
      {
         var newSprite = Sprite.Create( texture, sprite != null ? sprite.rect : sr.sprite.rect, Vector2.zero );
         return newSprite;
      }

      public static void SetAllDirtyEx( this object ui )
      {
#if MANAGED
         if( ui == null ) return;

         var type = ui.GetType();

         if( UnityTypes.Graphic != null && UnityTypes.Graphic.IsAssignableFrom( type ) )
         {
            UnityTypes.Graphic.CachedMethod( SetAllDirtyMethodName ).Invoke( ui );
         }
         else if( !( ui is SpriteRenderer ) )
         {
            AccessToolsShim.Method( type, MarkAsChangedMethodName )?.Invoke( ui, null );
         }
#endif
      }

      public static bool IsKnownImageType( this object ui )
      {
#if MANAGED
         var type = ui.GetType();

         return ( ui is Material || ui is SpriteRenderer )
            || ( UnityTypes.Image != null && UnityTypes.Image.IsAssignableFrom( type ) )
            || ( UnityTypes.RawImage != null && UnityTypes.RawImage.IsAssignableFrom( type ) )
            || ( UnityTypes.CubismRenderer != null && UnityTypes.CubismRenderer.IsAssignableFrom( type ) )
            || ( UnityTypes.UIWidget != null && type != UnityTypes.UILabel && UnityTypes.UIWidget.IsAssignableFrom( type ) )
            || ( UnityTypes.UIAtlas != null && UnityTypes.UIAtlas.IsAssignableFrom( type ) )
            || ( UnityTypes.UITexture != null && UnityTypes.UITexture.IsAssignableFrom( type ) )
            || ( UnityTypes.UIPanel != null && UnityTypes.UIPanel.IsAssignableFrom( type ) );
#else
         return false;
#endif
      }

      public static string GetTextureName( this object texture, string fallbackName )
      {
         if( texture.TryCastTo<Texture2D>( out var texture2d ) )
         {
            var name = texture2d.name;
            if( !string.IsNullOrEmpty( name ) )
            {
               return name;
            }
         }
         return fallbackName;
      }

      public static void LoadImageEx( this Texture2D texture, byte[] data, ImageFormat dataType, object originalTexture )
      {
         TextureLoader.Load( texture, data, dataType );

         if( texture.TryCastTo<Texture2D>( out var texture2D ) && originalTexture.TryCastTo<Texture2D>( out var originalTexture2D ) )
         {
            foreach( var prop in TexturePropertyMovers )
            {
               prop.MoveProperty( originalTexture2D, texture2D );
            }
            //texture2D.name = originalTexture2D.name;
            //texture2D.anisoLevel = originalTexture2D.anisoLevel;
            //texture2D.filterMode = originalTexture2D.filterMode;
            //texture2D.mipMapBias = originalTexture2D.mipMapBias;
            //texture2D.wrapMode = originalTexture2D.wrapMode;
         }
      }

      private static byte[] EncodeToPNGEx( Texture2D texture )
      {
         if( UnityTypes.ImageConversions_Methods.EncodeToPNG != null )
         {
            return UnityTypes.ImageConversions_Methods.EncodeToPNG( texture );
         }
         else if( UnityTypes.Texture2D_Methods.EncodeToPNG != null )
         {
            return UnityTypes.Texture2D_Methods.EncodeToPNG( texture );
         }
         else
         {
            throw new NotSupportedException( "No way to encode the texture to PNG." );
         }
      }

      public static TextureDataResult GetTextureData( this object texture )
      {
#warning Probably wont work with IL2CPP

         var tex = (Texture2D)texture;
         var width = tex.width;
         var height = tex.height;

         var start = Time.realtimeSinceStartup;

         byte[] data = null;
         //bool nonReadable = texture.IsNonReadable();

         //if( !nonReadable )
         //{
         //   data = texture.EncodeToPNGEx();
         //}

         if( data == null )
         {
            var tmp = RenderTexture.GetTemporary( width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default );
            GL.Clear( false, true, Transparent );
            Graphics.Blit( tex, tmp );
            var previousRenderTexture = RenderTexture.active;
            RenderTexture.active = tmp;

            var texture2d = new Texture2D( width, height );
            texture2d.ReadPixels( new Rect( 0, 0, tmp.width, tmp.height ), 0, 0 );
            data = EncodeToPNGEx( texture2d );
            UnityEngine.Object.DestroyImmediate( texture2d );

            //Graphics.Blit( tex, tmp );
            //var texture2d = GetTextureFromRenderTexture( tmp );
            //var data = texture2d.EncodeToPNG();
            //UnityEngine.Object.DestroyImmediate( texture2d );

            RenderTexture.active = previousRenderTexture == tmp ? null : previousRenderTexture;
            RenderTexture.ReleaseTemporary( tmp );
         }

         var end = Time.realtimeSinceStartup;

         return new TextureDataResult( data, false, end - start );
      }

      public static bool IsCompatible( this object texture, ImageFormat dataType )
      {
         // .png => Don't really care about which format it is in. If it is DXT1 or DXT5 could be used to force creation of new texture
         //  => Because we use LoadImage, which works for any texture but causes bad quality if used on DXT1 or DXT5

         // .tga => Require that the format is RGBA32 or RGB24. If not, we must create a new one no matter what
         //  => Because we use SetPixels. This function works only on RGBA32, ARGB32, RGB24 and Alpha8 texture formats.

         var texture2d = (Texture2D)texture;

         var format = texture2d.format;
         return dataType == ImageFormat.PNG
            || ( dataType == ImageFormat.TGA && ( format == TextureFormat.ARGB32 || format == TextureFormat.RGBA32 || format == TextureFormat.RGB24 ) );
      }
   }
}

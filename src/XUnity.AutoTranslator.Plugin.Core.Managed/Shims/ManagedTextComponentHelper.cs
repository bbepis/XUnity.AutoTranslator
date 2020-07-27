using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   internal class ManagedTextComponentHelper : ITextComponentHelper
   {
      private static readonly MethodInfo LoadImage = UnityTypes.ImageConversion != null ? AccessToolsShim.Method( UnityTypes.ImageConversion, "LoadImage", new[] { typeof( Texture2D ), typeof( byte[] ), typeof( bool ) } ) : null;
      private static readonly MethodInfo EncodeToPNG = UnityTypes.ImageConversion != null ? AccessToolsShim.Method( UnityTypes.ImageConversion, "EncodeToPNG", new[] { typeof( Texture2D ) } ) : null;

      private static readonly Color Transparent = new Color( 0, 0, 0, 0 );
      private static readonly string SetAllDirtyMethodName = "SetAllDirty";
      private static readonly string TexturePropertyName = "texture";
      private static readonly string MainTexturePropertyName = "mainTexture";
      private static readonly string CapitalMainTexturePropertyName = "MainTexture";
      private static readonly string MarkAsChangedMethodName = "MarkAsChanged";
      private static readonly string SupportRichTextPropertyName = "supportRichText";
      private static readonly string RichTextPropertyName = "richText";
      private static readonly Dictionary<Type, ITextComponentManipulator> Manipulators = new Dictionary<Type, ITextComponentManipulator>();
      private static GameObject[] _objects = new GameObject[ 128 ];
      private static readonly string XuaIgnore = "XUAIGNORE";

      private static ITextComponentManipulator GetTextManipulator( object ui )
      {
         var type = ui.GetType();
         if( type == null )
         {
            return null;
         }

         if( !Manipulators.TryGetValue( type, out var manipulator ) )
         {
            if( type == UnityTypes.TextField )
            {
               manipulator = new FairyGUITextComponentManipulator();
            }
            else
            {
               manipulator = new DefaultTextComponentManipulator( type );
            }
            Manipulators[ type ] = manipulator;
         }

         return manipulator;
      }

      public bool IsKnownTextType( object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( Settings.EnableIMGUI && ui is GUIContent )
            || ( Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( type ) )
            || ( Settings.EnableNGUI && UnityTypes.UILabel != null && UnityTypes.UILabel.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.IsAssignableFrom( type ) )
            || ( Settings.EnableFairyGUI && UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMeshPro && IsKnownTextMeshProType( type ) );
      }

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

      public bool IsSpammingComponent( object ui )
      {
         return ui == null || ui is GUIContent;
      }

      public bool SupportsLineParser( object ui )
      {
         return Settings.GameLogTextPaths.Count > 0 && ui is Component comp && Settings.GameLogTextPaths.Contains( comp.gameObject.GetPath() );
      }

      public bool SupportsRichText( object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( type ) && Equals( type.CachedProperty( SupportRichTextPropertyName )?.Get( ui ), true ) )
            || ( UnityTypes.TextMesh != null && UnityTypes.TextMesh.IsAssignableFrom( type ) && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
            || DoesTextMeshProSupportRichText( ui, type )
            || ( UnityTypes.UguiNovelText != null && UnityTypes.UguiNovelText.IsAssignableFrom( type ) )
            || ( UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom( type ) );
      }

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

      public bool SupportsStabilization( object ui )
      {
         if( ui == null ) return false;

         return !( ui is GUIContent );
      }

      public bool IsNGUI( object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return UnityTypes.UILabel != null && UnityTypes.UILabel.IsAssignableFrom( type );
      }

      public void SetText( object ui, string text )
      {
         if( ui is GUIContent gui )
         {
            gui.text = text;
         }
         else
         {
            // fallback to reflective approach
            GetTextManipulator( ui )?.SetText( ui, text );
         }
      }

      public string GetText( object ui )
      {
         if( ui is GUIContent gui )
         {
            return gui.text;
         }
         else
         {
            // fallback to reflective approach
            return GetTextManipulator( ui )?.GetText( ui );
         }
      }

      public bool ShouldTranslateTextComponent( object ui, bool ignoreComponentState )
      {
         var component = ui as Component;
         if( component != null )
         {
            // dummy check
            var go = component.gameObject;
            var ignore = go.HasIgnoredName();
            if( ignore )
            {
               return false;
            }

            if( !ignoreComponentState )
            {
               var behaviour = component as Behaviour;
               if( !go.activeInHierarchy || behaviour?.enabled == false ) // legacy "isActiveAndEnabled"
               {
                  return false;
               }
            }

            var inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.InputField );
            if( inputField != null )
            {
               if( UnityTypes.InputField_Properties.Placeholder != null )
               {
                  var placeholder = UnityTypes.InputField_Properties.Placeholder.Get( inputField );
                  return ReferenceEquals( placeholder, ui );
               }
            }

            inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.TMP_InputField );
            if( inputField != null )
            {
               if( UnityTypes.TMP_InputField_Properties.Placeholder != null )
               {
                  var placeholder = UnityTypes.TMP_InputField_Properties.Placeholder.Get( inputField );
                  return ReferenceEquals( placeholder, ui );
               }
            }

            inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.UIInput );

            return inputField == null;
         }

         return true;
      }

      public bool IsComponentActive( object ui )
      {
         if( ui is Component component )
         {
            return component.gameObject?.activeSelf ?? false;
         }
         return true;
      }

      public TextTranslationInfo GetTextTranslationInfo( object ui )
      {
         var info = ui.GetOrCreateExtensionData<ManagedTextTranslationInfo>();
         info.Initialize( ui );

         return info;
      }

      public TextTranslationInfo GetOrCreateTextTranslationInfo( object ui )
      {
         var info = ui.GetExtensionData<ManagedTextTranslationInfo>();
         return info;
      }

      public object CreateWrapperTextComponentIfRequiredAndPossible( object ui )
      {
         return IsKnownTextType( ui ) ? ui : null;
      }

      public IEnumerable<object> GetAllTextComponentsInChildren( object go )
      {
         var gameObject = (GameObject)go;

         if( Settings.EnableTextMeshPro && UnityTypes.TMP_Text != null )
         {
            foreach( var comp in gameObject.GetComponentsInChildren( UnityTypes.TMP_Text ) )
            {
               yield return comp;
            }
         }
         if( Settings.EnableUGUI && UnityTypes.Text != null )
         {
            foreach( var comp in gameObject.GetComponentsInChildren( UnityTypes.Text ) )
            {
               yield return comp;
            }
         }
         if( Settings.EnableTextMesh && UnityTypes.TextMesh != null )
         {
            foreach( var comp in gameObject.GetComponentsInChildren( UnityTypes.TextMesh ) )
            {
               yield return comp;
            }
         }
         if( Settings.EnableNGUI && UnityTypes.UILabel != null )
         {
            foreach( var comp in gameObject.GetComponentsInChildren( UnityTypes.UILabel ) )
            {
               yield return comp;
            }
         }
      }

      public string[] GetPathSegments( object obj )
      {
         if( obj is GameObject go )
         {

         }
         else if( obj is Component comp )
         {
            go = comp.gameObject;
         }
         else
         {
            throw new ArgumentException( "Expected object to be a GameObject or component.", "obj" );
         }

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

      public string GetPath( object obj )
      {
         if( obj is GameObject go )
         {

         }
         else if( obj is Component comp )
         {
            go = comp.gameObject;
         }
         else
         {
            throw new ArgumentException( "Expected object to be a GameObject or component.", "obj" );
         }

         StringBuilder path = new StringBuilder();
         var segments = GetPathSegments( go );
         for( int i = 0; i < segments.Length; i++ )
         {
            path.Append( "/" ).Append( segments[ i ] );
         }

         return path.ToString();
      }

      public bool HasIgnoredName( object obj )
      {
         if( obj is GameObject go )
         {

         }
         else if( obj is Component comp )
         {
            go = comp.gameObject;
         }
         else
         {
            throw new ArgumentException( "Expected object to be a GameObject or component.", "obj" );
         }

         return go.name.Contains( XuaIgnore );
      }

      public string GetTextureName( object texture, string fallbackName )
      {
         if( texture is Texture2D texture2d )
         {
            var name = texture2d.name;
            if( !string.IsNullOrEmpty( name ) )
            {
               return name;
            }
         }
         return fallbackName;
      }

      public void LoadImageEx( object texture, byte[] data, object originalTexture )
      {
         if( LoadImage != null )
         {
            LoadImage.Invoke( null, new object[] { texture, data, false } );
         }
         else
         {
            LoadImageSafe( (Texture2D)texture, data );
         }

         if( texture is Texture2D texture2D && originalTexture is Texture2D originalTexture2D )
         {
            texture2D.name = originalTexture2D.name;
            texture2D.anisoLevel = originalTexture2D.anisoLevel;
            texture2D.filterMode = originalTexture2D.filterMode;
            texture2D.mipMapBias = originalTexture2D.mipMapBias;
            texture2D.wrapMode = originalTexture2D.wrapMode;
         }
      }

      private static byte[] EncodeToPNGEx( object texture )
      {
         if( EncodeToPNG != null )
         {
            return (byte[])EncodeToPNG.Invoke( null, new object[] { texture } );
         }
         else
         {
            return EncodeToPNGSafe( (Texture2D)texture );
         }
      }

      public bool IsKnownImageType( object ui )
      {
         var type = ui.GetType();

         return ( ui is Material || ui is SpriteRenderer )
            || ( UnityTypes.Image != null && UnityTypes.Image.IsAssignableFrom( type ) )
            || ( UnityTypes.RawImage != null && UnityTypes.RawImage.IsAssignableFrom( type ) )
            || ( UnityTypes.CubismRenderer != null && UnityTypes.CubismRenderer.IsAssignableFrom( type ) )
            || ( UnityTypes.UIWidget != null && type != UnityTypes.UILabel && UnityTypes.UIWidget.IsAssignableFrom( type ) )
            || ( UnityTypes.UIAtlas != null && UnityTypes.UIAtlas.IsAssignableFrom( type ) )
            || ( UnityTypes.UITexture != null && UnityTypes.UITexture.IsAssignableFrom( type ) )
            || ( UnityTypes.UIPanel != null && UnityTypes.UIPanel.IsAssignableFrom( type ) );
      }

      public object GetTexture( object ui )
      {
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

      public void SetTexture( object ui, object texture )
      {
         if( ui == null ) return;

         var currentTexture = (Texture2D)ui.GetTexture();

         if( !Equals( currentTexture, texture ) )
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

      public void SetAllDirtyEx( object ui )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( UnityTypes.Graphic != null && UnityTypes.Graphic.IsAssignableFrom( type ) )
         {
            UnityTypes.Graphic.CachedMethod( SetAllDirtyMethodName ).Invoke( ui );
         }
         else
         {
            AccessToolsShim.Method( type, MarkAsChangedMethodName )?.Invoke( ui, null );
         }
      }

      private static void LoadImageSafe( Texture2D texture, byte[] data )
      {
         texture.LoadImage( data );
      }

      private static byte[] EncodeToPNGSafe( Texture2D texture )
      {
         return texture.EncodeToPNG();
      }

      public TextureDataResult GetTextureData( object texture )
      {
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

      public object CreateEmptyTexture2D()
      {
         return new Texture2D( 2, 2, TextureFormat.ARGB32, false );
      }
   }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.AutoTranslator.Plugin.Core.Textures;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   internal class Il2CppComponentHelper : IComponentHelper
   {
      private static GameObject[] _objects = new GameObject[ 128 ];
      private static readonly string XuaIgnore = "XUAIGNORE";

      public string GetText( object ui )
      {
         return ( ui as ITextComponent )?.Text;
      }

      public bool IsComponentActive( object ui )
      {
         return ( ( ui as ITextComponent )?.Component ).gameObject?.activeInHierarchy ?? false;
      }

      public bool IsKnownTextType( object ui )
      {
         return ui is ITextComponent;
      }

      public bool IsNGUI( object ui )
      {
         return false;
      }

      public bool IsSpammingComponent( object ui )
      {
         return ui is ITextComponent tc && tc.IsSpammingComponent();
      }

      public void SetText( object ui, string text )
      {
         if( ui is ITextComponent tc )
         {
            tc.Text = text;
         }
      }

      public bool ShouldTranslateTextComponent( object ui, bool ignoreComponentState )
      {
         if( ui is ITextComponent tc && tc.Component != null )
         {
            var component = tc.Component;

            // dummy check
            var go = component.gameObject;
            var ignore = go.HasIgnoredName();
            if( ignore )
            {
               return false;
            }

            if( !ignoreComponentState )
            {
               component.TryCastTo<Behaviour>( out var behaviour);
               if( !go.activeInHierarchy || behaviour?.enabled == false ) // legacy "isActiveAndEnabled"
               {
                  return false;
               }
            }

            return !tc.IsPlaceholder();
         }

         return true;
      }

      public bool SupportsLineParser( object ui )
      {
         return Settings.GameLogTextPaths.Count > 0 && Settings.GameLogTextPaths.Contains( ( ( ui as ITextComponent )?.Component ).gameObject.GetPath() );
      }

      public bool SupportsRichText( object ui )
      {
         return ui is ITextComponent tc && tc.SupportsRichText();
      }

      public bool SupportsStabilization( object ui )
      {
         if( ui == null ) return false;

         return true;
      }

      public TextTranslationInfo GetOrCreateTextTranslationInfo( object ui )
      {
         var info = ui.GetOrCreateExtensionData<Il2CppTextTranslationInfo>();
         info.Initialize( ui );

         return info;
      }

      public TextTranslationInfo GetTextTranslationInfo( object ui )
      {
         var info = ui.GetExtensionData<Il2CppTextTranslationInfo>();

         return info;
      }

      public object CreateWrapperTextComponentIfRequiredAndPossible( object ui )
      {
         if( ui is Component tc )
         {
            var type = tc.GetIl2CppTypeSafe();

            if( Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.Il2CppType.IsAssignableFrom( type ) )
            {
               return new TextComponent( tc );
            }
            else if( Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.Il2CppType.IsAssignableFrom( type ) )
            {
               return new TextMeshComponent( tc );
            }
            else if( Settings.EnableTextMeshPro && UnityTypes.TMP_Text != null && UnityTypes.TMP_Text.Il2CppType.IsAssignableFrom( type ) )
            {
               return new TMP_TextComponent( tc );
            }
         }
         return null;
      }

      public IEnumerable<object> GetAllTextComponentsInChildren( object go )
      {
         yield break;
      }

      public string[] GetPathSegments( object obj )
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

      private List<IPropertyMover> _texturePropertyMovers;

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

      private void LoadProperty<TPropertyType>( string propertyName )
      {
         var property = typeof( Texture2D ).GetProperty( propertyName );
         if( property.CanWrite && property.CanRead )
         {
            _texturePropertyMovers.Add( new PropertyMover<Texture, TPropertyType>( property ) );
         }
      }

      public void LoadImageEx( object texture, byte[] data, ImageFormat dataType, object originalTexture )
      {
         TextureLoader.Load( texture, data, dataType );

         if( texture is Texture2D texture2D && originalTexture is Texture2D originalTexture2D )
         {
            if( _texturePropertyMovers == null )
            {
               _texturePropertyMovers = new List<IPropertyMover>();
               LoadProperty<string>( "name" );
               LoadProperty<int>( "anisoLevel" );
               LoadProperty<FilterMode>( "filterMode" );
               LoadProperty<float>( "mipMapBias" );
               LoadProperty<bool>( "wrapMode" );
            }

            foreach( var prop in _texturePropertyMovers )
            {
               prop.MoveProperty( originalTexture2D, texture2D );
            }
         }
      }

      public TextureDataResult GetTextureData( object texture )
      {
         // why no Image Conversion?
         throw new NotImplementedException();
      }

      public bool IsKnownImageType( object ui )
      {
         return false;
      }

      public object GetTexture( object ui )
      {
         return null;
      }

      public object SetTexture( object ui, object texture, bool isPrefixedHooked )
      {
         return null;
      }

      public void SetAllDirtyEx( object ui )
      {
      }

      public object CreateEmptyTexture2D( int originalTextureFormat )
      {
         var format = (TextureFormat)originalTextureFormat;

         TextureFormat newFormat;
         switch( format )
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

      public bool IsCompatible( object texture, ImageFormat dataType )
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

      public object SetTexture( object ui, object textureObj, object spriteObj, bool isPrefixHooked )
      {
         throw new NotImplementedException();
      }

      public int GetScreenWidth()
      {
         return Screen.width;
      }

      public int GetScreenHeight()
      {
         return Screen.height;
      }
   }
}

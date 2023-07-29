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
      private static List<IPropertyMover> TexturePropertyMovers;
#if MANAGED
      private static readonly Dictionary<Type, ITextComponentManipulator> Manipulators = new Dictionary<Type, ITextComponentManipulator>();
#else
      private static readonly Dictionary<Il2CppSystem.Type, ITextComponentManipulator> Manipulators = new Dictionary<Il2CppSystem.Type, ITextComponentManipulator>();
#endif

      static ComponentExtensions()
      {
         TexturePropertyMovers = new List<IPropertyMover>();
         LoadProperty<UnityEngine.Object, string>( "name" );
         LoadProperty<Texture, int>( "anisoLevel" );
         LoadProperty<Texture, FilterMode>( "filterMode" );
         LoadProperty<Texture, float>( "mipMapBias" );
         LoadProperty<Texture, TextureWrapMode>( "wrapMode" );
      }

      private static void LoadProperty<TObject, TPropertyType>( string propertyName )
      {
         var property = typeof( TObject ).GetProperty( propertyName );
         if( property != null && property.CanWrite && property.CanRead )
         {
            TexturePropertyMovers.Add( new PropertyMover<TObject, TPropertyType>( property ) );
         }
      }

      public static ITextComponentManipulator GetTextManipulator( this object ui )
      {
         if( ui == null )
         {
            return null;
         }

         var unityType = ui.GetUnityType();

         if( !Manipulators.TryGetValue( unityType, out var manipulator ) )
         {
            if( UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom( unityType ) )
            {
               manipulator = new FairyGUITextComponentManipulator();
            }
#if MANAGED
            else if( UnityTypes.TextArea2D != null && UnityTypes.TextArea2D.IsAssignableFrom( unityType ) )
            {
               manipulator = new TextArea2DComponentManipulator();
            }
#endif
            else if( UnityTypes.UguiNovelText != null && UnityTypes.UguiNovelText.IsAssignableFrom( unityType ) )
            {
               manipulator = new UguiNovelTextComponentManipulator( ui.GetType() );
            }
            else
            {
               manipulator = new DefaultTextComponentManipulator( ui.GetType() );
            }
            Manipulators[ unityType ] = manipulator;
         }

         return manipulator;
      }

      public static bool ShouldIgnoreTextComponent( this object ui )
      {
         if( ui is Component component && component )
         {
            var go = component.gameObject;
            var ignore = go.HasIgnoredName();
            if( ignore )
            {
               return true;
            }

            Component inputField = null;
            if( UnityTypes.InputField != null )
            {
               inputField = component.gameObject.GetFirstComponentInSelfOrAncestor( UnityTypes.InputField?.UnityType );
               if( inputField != null )
               {
                  if( UnityTypes.InputField_Properties.Placeholder != null )
                  {
                     var placeholder = (Component)UnityTypes.InputField_Properties.Placeholder.Get( inputField );
                     return !UnityObjectReferenceComparer.Default.Equals( placeholder, component );
                  }
               }
            }

            if( UnityTypes.TMP_InputField != null )
            {
               inputField = component.gameObject.GetFirstComponentInSelfOrAncestor( UnityTypes.TMP_InputField?.UnityType );
               if( inputField != null )
               {
                  if( UnityTypes.TMP_InputField_Properties.Placeholder != null )
                  {
                     var placeholder = (Component)UnityTypes.TMP_InputField_Properties.Placeholder.Get( inputField );
                     return !UnityObjectReferenceComparer.Default.Equals( placeholder, component );
                  }
               }
            }

            inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.UIInput?.UnityType );

            return inputField != null;
         }

         return false;
      }

      public static bool IsComponentActive( this object ui )
      {
         if( ui is Component component && component )
         {
            var go = component.gameObject;
            if( go )
            {
               if( component is Behaviour be )
               {
                  return go.activeInHierarchy && be.enabled;
               }
               else
               {
                  return go.activeInHierarchy;
               }
            }
         }
         return true;
      }

      public static bool IsKnownTextType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetUnityType();

         return ( Settings.EnableIMGUI && !_guiContentCheckFailed && IsGUIContentSafe( ui ) )
            || ( Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( type ) )
            || ( Settings.EnableNGUI && UnityTypes.UILabel != null && UnityTypes.UILabel.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.IsAssignableFrom( type ) )
            || ( Settings.EnableFairyGUI && UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMeshPro && IsKnownTextMeshProType( type ) );
      }

#if MANAGED
      public static bool IsKnownTextMeshProType( Type type )
#else
      public static bool IsKnownTextMeshProType( Il2CppSystem.Type type )
#endif
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

      public static bool SupportsRichText( this object ui )
      {
         if( ui == null ) return false;

         var clrType = ui.GetType();
         var unityType = ui.GetUnityType();

         return
            ( UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( unityType ) && Equals( clrType.CachedProperty( SupportRichTextPropertyName )?.Get( ui ), true ) )
            || ( UnityTypes.TextMesh != null && UnityTypes.TextMesh.IsAssignableFrom( unityType ) && Equals( clrType.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
            || DoesTextMeshProSupportRichText( ui, clrType, unityType )
            || ( UnityTypes.UguiNovelText != null && UnityTypes.UguiNovelText.IsAssignableFrom( unityType ) )
            || ( UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom( unityType ) );
      }

#if MANAGED
      public static bool DoesTextMeshProSupportRichText( object ui, Type clrType, Type unityType )
#else
      public static bool DoesTextMeshProSupportRichText( object ui, Type clrType, Il2CppSystem.Type unityType )
#endif
      {
         if( UnityTypes.TMP_Text != null )
         {
            return UnityTypes.TMP_Text.IsAssignableFrom( unityType ) && Equals( clrType.CachedProperty( RichTextPropertyName )?.Get( ui ), true );
         }
         else
         {
            return ( UnityTypes.TextMeshPro?.IsAssignableFrom( unityType ) == true && Equals( clrType.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
               || ( UnityTypes.TextMeshProUGUI?.IsAssignableFrom( unityType ) == true && Equals( clrType.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) );
         }
      }

      public static bool SupportsStabilization( this object ui )
      {
         if( ui == null ) return false;

         return _guiContentCheckFailed || !IsGUIContentSafe( ui );
      }

      public static bool IsSpammingComponent( this object ui )
      {
         return ui == null
            || ( !_guiContentCheckFailed && IsGUIContentSafe( ui ) );
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

      private static bool IsGUIContentUnsafe( object ui ) => ui is GUIContent;

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
         if( ui is GUIContent gui )
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
         if( ui is GUIContent gui )
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
         }
         return false;
      }

      public static string GetText( this object ui, TextTranslationInfo info )
      {
         if( ui == null ) return null;

         TextGetterCompatModeHelper.IsGettingText = true;
         try
         {
            string text = null;
            if( ( _guiContentCheckFailed || !TryGetTextFromGUIContentSafe( ui, out text ) ) && info != null )
            {
               return info.TextManipulator.GetText( ui );
            }
            return text;
         }
         finally
         {
            TextGetterCompatModeHelper.IsGettingText = false;
         }
      }

      public static void SetText( this object ui, string text, TextTranslationInfo info )
      {
         if( ui == null ) return;

         if( ( _guiContentCheckFailed || !SetTextOnGUIContentSafe( ui, text ) ) && info != null )
         {
            info.TextManipulator.SetText( ui, text );
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
      public static ImageTranslationInfo GetOrCreateImageTranslationInfo( this object obj, Texture2D originalTexture )
      {
         if( obj == null ) return null;

         var iti = obj.GetOrCreateExtensionData<ImageTranslationInfo>();
         if( iti.Original == null ) iti.Initialize( originalTexture );

         return iti;
      }

      public static TextureTranslationInfo GetOrCreateTextureTranslationInfo( this Texture2D texture )
      {
         var tti = texture.GetOrCreateExtensionData<TextureTranslationInfo>();
         tti.Initialize( texture );

         return tti;
      }

      public static object CreateDerivedProxyIfRequiredAndPossible( this Component ui )
      {
         if( ui.IsKnownTextType() )
         {
#if MANAGED
            return ui;
#else
            var unityType = ui.GetUnityType();
            if( Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( unityType ) )
            {
               return Il2CppUtilities.CreateProxyComponentWithDerivedType( ui.Pointer, UnityTypes.Text.ClrType );
            }
            else if( Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.IsAssignableFrom( unityType ) )
            {
               return Il2CppUtilities.CreateProxyComponentWithDerivedType( ui.Pointer, UnityTypes.TextMesh.ClrType );
            }
            else if( Settings.EnableTextMeshPro && UnityTypes.TextMeshProUGUI != null && UnityTypes.TextMeshProUGUI.IsAssignableFrom( unityType ) )
            {
               return Il2CppUtilities.CreateProxyComponentWithDerivedType( ui.Pointer, UnityTypes.TextMeshProUGUI.ClrType );
            }
            else if( Settings.EnableTextMeshPro && UnityTypes.TextMeshPro != null && UnityTypes.TextMeshPro.IsAssignableFrom( unityType ) )
            {
               return Il2CppUtilities.CreateProxyComponentWithDerivedType( ui.Pointer, UnityTypes.TextMeshPro.ClrType );
            }
            else if( Settings.EnableTextMeshPro && UnityTypes.TMP_Text != null && UnityTypes.TMP_Text.IsAssignableFrom( unityType ) )
            {
               return Il2CppUtilities.CreateProxyComponentWithDerivedType( ui.Pointer, UnityTypes.TMP_Text.ClrType );
            }
#endif
         }

         return null;
      }

#if IL2CPP
      public static Component CreateTextMeshProDerivedProxy( this Component ui )
      {
         var unityType = ui.GetUnityType();
         if( UnityTypes.TextMeshProUGUI != null && UnityTypes.TextMeshProUGUI.IsAssignableFrom( unityType ) )
         {
            return (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( ui.Pointer, UnityTypes.TextMeshProUGUI.ClrType );
         }
         else if( UnityTypes.TextMeshPro != null && UnityTypes.TextMeshPro.IsAssignableFrom( unityType ) )
         {
            return (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( ui.Pointer, UnityTypes.TextMeshPro.ClrType );
         }
         else if( UnityTypes.TMP_Text != null && UnityTypes.TMP_Text.IsAssignableFrom( unityType ) )
         {
            return (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( ui.Pointer, UnityTypes.TMP_Text.ClrType );
         }
         return null;
      }

      public static Component CreateTextMeshProDerivedProxy( this IntPtr ui )
      {
         if( UnityTypes.TextMeshProUGUI != null && ui.IsInstancePointerAssignableFrom( UnityTypes.TextMeshProUGUI.ClassPointer ) )
         {
            return (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( ui, UnityTypes.TextMeshProUGUI.ClrType );
         }
         else if( UnityTypes.TextMeshPro != null && ui.IsInstancePointerAssignableFrom( UnityTypes.TextMeshPro.ClassPointer ) )
         {
            return (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( ui, UnityTypes.TextMeshPro.ClrType );
         }
         else if( UnityTypes.TMP_Text != null && ui.IsInstancePointerAssignableFrom( UnityTypes.TMP_Text.ClassPointer ) )
         {
            return (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( ui, UnityTypes.TMP_Text.ClrType );
         }
         return null;
      }

      public static Component CreateNGUIDerivedProxy( this IntPtr ui )
      {
         if( UnityTypes.UILabel != null && ui.IsInstancePointerAssignableFrom( UnityTypes.UILabel.ClassPointer ) )
         {
            return (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( ui, UnityTypes.UILabel.ClrType );
         }
         return null;
      }
#endif

      public static Component GetOrCreateNGUIDerivedProxy( this Component ui )
      {
         var unityType = ui.GetUnityType();
         if( UnityTypes.UILabel != null && UnityTypes.UILabel.IsAssignableFrom( unityType ) )
         {
#if IL2CPP
            return (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( ui.Pointer, UnityTypes.UILabel.ClrType );
#else
            return ui;
#endif
         }
         return null;
      }

#if IL2CPP
      public static Component GetFirstComponentInSelfOrAncestor( this GameObject go, Il2CppSystem.Type type )
#else
      public static Component GetFirstComponentInSelfOrAncestor( this GameObject go, Type type )
#endif
      {
         if( type == null ) return null;

         var current = go;

         while( current != null )
         {
            var foundComponent = current.GetComponent( type );
            if( foundComponent != null )
            {
               return foundComponent;
            }

            current = current.transform?.parent?.gameObject;
         }

         return null;
      }

      public static IEnumerable<Component> GetAllTextComponentsInChildren( this GameObject go )
      {
         // Only used for plugin specific hooks
         if( Settings.EnableTextMeshPro && UnityTypes.TMP_Text != null )
         {
            foreach( var comp in go.GetComponentsInChildren( UnityTypes.TMP_Text.UnityType, true ) )
            {
               yield return comp;
            }
         }
         if( Settings.EnableUGUI && UnityTypes.Text != null )
         {
            foreach( var comp in go.GetComponentsInChildren( UnityTypes.Text.UnityType, true ) )
            {
               yield return comp;
            }
         }
         if( Settings.EnableTextMesh && UnityTypes.TextMesh != null )
         {
            foreach( var comp in go.GetComponentsInChildren( UnityTypes.TextMesh.UnityType, true ) )
            {
               yield return comp;
            }
         }
         if( Settings.EnableNGUI && UnityTypes.UILabel != null )
         {
            foreach( var comp in go.GetComponentsInChildren( UnityTypes.UILabel.UnityType, true ) )
            {
               yield return comp;
            }
         }
      }

      private static GameObject GetAssociatedGameObject( object obj )
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
         for( int i = 0; i < segments.Length; i++ )
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

      public static Texture2D GetTexture( this object ui )
      {
#warning NOT TESTED FOR IL2CPP, but may work
         if( ui == null ) return null;

         if( ui.TryCastTo<SpriteRenderer>( out var spriteRenderer ) )
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

      public static Sprite SetTexture( this object ui, Texture2D texture, Sprite sprite, bool isPrefixHooked )
      {
#warning NOT TESTED FOR IL2CPP, but may work
         if( ui == null ) return null;

         var currentTexture = ui.GetTexture();

         if( !UnityObjectReferenceComparer.Default.Equals( currentTexture, texture ) )
         {
            if( Settings.EnableSpriteRendererHooking && ui.TryCastTo<SpriteRenderer>( out var sr ) )
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
                  var materialTexture = (Texture2D)mainTextureProperty?.Get( material );
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
         if( ui == null ) return;

         var type = ui.GetUnityType();

         if( UnityTypes.Graphic != null && UnityTypes.Graphic.IsAssignableFrom( type ) )
         {
            UnityTypes.Graphic.ClrType.CachedMethod( SetAllDirtyMethodName ).Invoke( ui );
         }
         else if( !ui.TryCastTo<SpriteRenderer>( out _ ) )
         {
            var clrType = ui.GetType();
            AccessToolsShim.Method( clrType, MarkAsChangedMethodName )?.Invoke( ui, null );
         }
      }

      public static bool IsKnownImageType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetUnityType();

         return ( ui.TryCastTo<Material>( out _ ) || ui.TryCastTo<SpriteRenderer>( out _ ) )
            || ( UnityTypes.Image != null && UnityTypes.Image.IsAssignableFrom( type ) )
            || ( UnityTypes.RawImage != null && UnityTypes.RawImage.IsAssignableFrom( type ) )
            || ( UnityTypes.CubismRenderer != null && UnityTypes.CubismRenderer.IsAssignableFrom( type ) )
            || ( UnityTypes.UIWidget != null && !Equals( type, UnityTypes.UILabel?.UnityType ) && UnityTypes.UIWidget.IsAssignableFrom( type ) )
            || ( UnityTypes.UIAtlas != null && UnityTypes.UIAtlas.IsAssignableFrom( type ) )
            || ( UnityTypes.UITexture != null && UnityTypes.UITexture.IsAssignableFrom( type ) )
            || ( UnityTypes.UIPanel != null && UnityTypes.UIPanel.IsAssignableFrom( type ) );
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

      public static void LoadImageEx( this Texture2D texture, byte[] data, ImageFormat dataType, Texture2D originalTexture )
      {
         TextureLoader.Load( texture, data, dataType );

         if( originalTexture != null )
         {
            foreach( var prop in TexturePropertyMovers )
            {
               prop.MoveProperty( originalTexture, texture );
            }
         }
      }

      private static byte[] EncodeToPNGEx( Texture2D texture )
      {
         if( UnityTypes.ImageConversion_Methods.EncodeToPNG != null )
         {
            return UnityTypes.ImageConversion_Methods.EncodeToPNG( texture );
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

      public static TextureDataResult GetTextureData( this Texture2D texture )
      {
#warning Probably wont work with IL2CPP
         var start = Time.realtimeSinceStartup;

         var width = texture.width;
         var height = texture.height;

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
            Graphics.Blit( texture, tmp );
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

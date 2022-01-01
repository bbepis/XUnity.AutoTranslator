using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text
{
   internal class DefaultTextComponentManipulator : ITextComponentManipulator
   {
      private static readonly string TextPropertyName = "text";

      private readonly Type _type;
      private readonly CachedProperty _property;

      public DefaultTextComponentManipulator( Type type )
      {
         _type = type;
         _property = type.CachedProperty( TextPropertyName );
      }

      public string GetText( object ui )
      {
         return (string)_property?.Get( ui );
      }

      public void SetText( object ui, string text )
      {
         var type = _type;

#if MANAGED
         if( UnityTypes.TextWindow != null && UnityTypes.TextMeshPro?.ClrType.IsAssignableFrom( type ) == true )
         {
            var textWindow = GameObject.FindObjectOfType( UnityTypes.TextWindow.ClrType );
            if( textWindow != null )
            {
               var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
               var textMesh = textWindow.GetType().GetField( "TextMesh", flags )?.GetValue( textWindow );
               if( textMesh != null )
               {
                  if( Equals( textMesh, ui ) )
                  {
                     var frames = new StackTrace().GetFrames();
                     if( frames.Any( x => x.GetMethod().DeclaringType == UnityTypes.TextWindow.ClrType ) )
                     {
                        // If inline (sync)

                        var previousCurText = textWindow.GetType().GetField( "curText", flags ).GetValue( textWindow );
                        textWindow.GetType().GetField( "curText", flags ).SetValue( textWindow, text );

                        Settings.SetCurText = textWindowInner =>
                        {
                           var translatedText = textWindow.GetType().GetField( "curText", flags ).GetValue( textWindow );
                           if( Equals( text, translatedText ) )
                           {
                              textWindowInner.GetType().GetMethod( "FinishTyping", flags ).Invoke( textWindowInner, null );

                              textWindowInner.GetType().GetField( "curText", flags ).SetValue( textWindowInner, previousCurText );

                              var textMeshInner = textWindowInner.GetType().GetField( "TextMesh", flags ).GetValue( textWindowInner );
                              var keyword = textWindowInner.GetType().GetField( "Keyword", flags ).GetValue( textWindowInner );
                              keyword.GetType().GetMethod( "UpdateTextMesh", flags ).Invoke( keyword, new object[] { textMeshInner, true } );
                           }

                           Settings.SetCurText = null;
                        };
                     }
                     else
                     {
                        // If delayed by plugin (async)

                        // update the actual text in the text box
                        type.CachedProperty( TextPropertyName )?.Set( ui, text );

                        // obtain the curText in the text window
                        var previousCurText = textWindow.GetType().GetField( "curText", flags ).GetValue( textWindow );

                        // set curText to OUR text
                        textWindow.GetType().GetField( "curText", flags ).SetValue( textWindow, text );

                        // call FinishTyping
                        textWindow.GetType().GetMethod( "FinishTyping", flags ).Invoke( textWindow, null );

                        // reset curText
                        textWindow.GetType().GetField( "curText", flags ).SetValue( textWindow, previousCurText );

                        var keyword = textWindow.GetType().GetField( "Keyword", flags ).GetValue( textWindow );
                        keyword.GetType().GetMethod( "UpdateTextMesh", flags ).Invoke( keyword, new object[] { textMesh, true } );
                     }

                     return;
                  }
               }
            }
         }
#endif

         // fallback to reflective approach
         var property = _property;
         if( property != null )
         {
            property.Set( ui, text );

#if MANAGED
            if( Settings.IgnoreVirtualTextSetterCallingRules )
            {
               var newText = (string)property.Get( ui );
               Type currentType = type;
               while( text != newText && currentType != null )
               {
                  var typeAndMethod = GetTextPropertySetterInParent( currentType );
                  if( typeAndMethod != null )
                  {
                     currentType = typeAndMethod.Type;

                     typeAndMethod.SetterInvoker( ui, text );
                     newText = (string)property.Get( ui );
                  }
                  else
                  {
                     currentType = null;
                  }
               }
            }
#endif
         }

         // TMPro
         var maxVisibleCharactersProperty = type.CachedProperty( "maxVisibleCharacters" );
         if( maxVisibleCharactersProperty != null && maxVisibleCharactersProperty.PropertyType == typeof( int ) )
         {
            var value = (int)maxVisibleCharactersProperty.Get( ui );
            if( 0 < value && value < 99999 )
            {
               maxVisibleCharactersProperty.Set( ui, 99999 );
            }
         }

#if MANAGED
         if( UnityTypes.TextExpansion_Methods.SetMessageType != null && UnityTypes.TextExpansion_Methods.SkipTypeWriter != null )
         {
            if( UnityTypes.TextExpansion.ClrType.IsAssignableFrom( type ) )
            {
               UnityTypes.TextExpansion_Methods.SetMessageType.Invoke( ui, 1 );
               UnityTypes.TextExpansion_Methods.SkipTypeWriter.Invoke( ui );
            }
         }
#endif
      }

      private static Dictionary<Type, TypeAndMethod> _textSetters = new Dictionary<Type, TypeAndMethod>();
      private static TypeAndMethod GetTextPropertySetterInParent( Type type )
      {
         var current = type.BaseType;
         while( current != null )
         {
            if( _textSetters.TryGetValue( type, out var typeAndMethod ) )
            {
               return typeAndMethod;
            }
            else
            {
               var property = current.GetProperty( TextPropertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

               if( property != null && property.CanWrite )
               {
                  var tam = new TypeAndMethod( current, property.GetSetMethod() );
                  _textSetters[ current ] = tam;
                  return tam;
               }

               current = current.BaseType;
            }
         }

         return null;
      }

      private class TypeAndMethod
      {
         FastReflectionDelegate _setterInvoker;

         public TypeAndMethod( Type type, MethodBase method )
         {
            Type = type;
            SetterMethod = method;
         }

         public Type Type { get; }
         public MethodBase SetterMethod { get; }
         public FastReflectionDelegate SetterInvoker
         {
            get
            {
               return _setterInvoker ?? ( _setterInvoker = CustomFastReflectionHelper.CreateFastDelegate( SetterMethod, true, true ) );
            }
         }
      }
   }

}

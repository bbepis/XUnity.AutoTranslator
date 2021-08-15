using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
   [StructLayout( LayoutKind.Sequential )]
   public sealed class Event
   {
      [NonSerialized]
      internal IntPtr m_Ptr;

      private static Event s_Current;

      private static Event s_MasterEvent;

      public Vector2 mousePosition
      {
         get
         {
            Internal_GetMousePosition( out Vector2 value );
            return value;
         }
         set
         {
            Internal_SetMousePosition( value );
         }
      }

      public Vector2 delta
      {
         get
         {
            Internal_GetMouseDelta( out Vector2 value );
            return value;
         }
         set
         {
            Internal_SetMouseDelta( value );
         }
      }

      [Obsolete( "Use HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);", true )]
      public Ray mouseRay
      {
         get
         {
            return new Ray( Vector3.up, Vector3.up );
         }
         set
         {
         }
      }

      public bool shift
      {
         get
         {
            return ( modifiers & EventModifiers.Shift ) != 0;
         }
         set
         {
            if( !value )
            {
               modifiers &= ~EventModifiers.Shift;
            }
            else
            {
               modifiers |= EventModifiers.Shift;
            }
         }
      }

      public bool control
      {
         get
         {
            return ( modifiers & EventModifiers.Control ) != 0;
         }
         set
         {
            if( !value )
            {
               modifiers &= ~EventModifiers.Control;
            }
            else
            {
               modifiers |= EventModifiers.Control;
            }
         }
      }

      public bool alt
      {
         get
         {
            return ( modifiers & EventModifiers.Alt ) != 0;
         }
         set
         {
            if( !value )
            {
               modifiers &= ~EventModifiers.Alt;
            }
            else
            {
               modifiers |= EventModifiers.Alt;
            }
         }
      }

      public bool command
      {
         get
         {
            return ( modifiers & EventModifiers.Command ) != 0;
         }
         set
         {
            if( !value )
            {
               modifiers &= ~EventModifiers.Command;
            }
            else
            {
               modifiers |= EventModifiers.Command;
            }
         }
      }

      public bool capsLock
      {
         get
         {
            return ( modifiers & EventModifiers.CapsLock ) != 0;
         }
         set
         {
            if( !value )
            {
               modifiers &= ~EventModifiers.CapsLock;
            }
            else
            {
               modifiers |= EventModifiers.CapsLock;
            }
         }
      }

      public bool numeric
      {
         get
         {
            return ( modifiers & EventModifiers.Numeric ) != 0;
         }
         set
         {
            if( !value )
            {
               modifiers &= ~EventModifiers.Shift;
            }
            else
            {
               modifiers |= EventModifiers.Shift;
            }
         }
      }

      public bool functionKey => ( modifiers & EventModifiers.FunctionKey ) != 0;

      public static Event current
      {
         get
         {
            return s_Current;
         }
         set
         {
            if( value != null )
            {
               s_Current = value;
            }
            else
            {
               s_Current = s_MasterEvent;
            }

            Internal_SetNativeEvent( s_Current.m_Ptr );
         }
      }

      public bool isKey
      {
         get
         {
            EventType type = this.type;
            return type == EventType.KeyDown || type == EventType.KeyUp;
         }
      }

      public bool isMouse
      {
         get
         {
            EventType type = this.type;
            return type == EventType.MouseMove || type == EventType.MouseDown || type == EventType.MouseUp || type == EventType.MouseDrag || type == EventType.ContextClick || type == EventType.MouseEnterWindow || type == EventType.MouseLeaveWindow;
         }
      }

      public bool isScrollWheel
      {
         get
         {
            EventType type = this.type;
            return type == EventType.ScrollWheel || type == EventType.ScrollWheel;
         }
      }

      public EventType rawType
      {
   
   
         get;
      }

      public EventType type
      {
   
   
         get;
   
   
         set;
      }

      public int button
      {
   
   
         get;
   
   
         set;
      }

      public EventModifiers modifiers
      {
   
   
         get;
   
   
         set;
      }

      public float pressure
      {
   
   
         get;
   
   
         set;
      }

      public int clickCount
      {
   
   
         get;
   
   
         set;
      }

      public char character
      {
   
   
         get;
   
   
         set;
      }

      public string commandName
      {
   
   
         get;
   
   
         set;
      }

      public KeyCode keyCode
      {
   
   
         get;
   
   
         set;
      }

      public int displayIndex
      {
   
   
         get;
   
   
         set;
      }

      public Event()
      {
         Init( 0 );
      }

      public Event( int displayIndex )
      {
         Init( displayIndex );
      }

      public Event( Event other )
      {
         if( other == null )
         {
            throw new ArgumentException( "Event to copy from is null." );
         }

         InitCopy( other );
      }

      private Event( IntPtr ptr )
      {
         InitPtr( ptr );
      }

      ~Event()
      {
         Cleanup();
      }

      internal static void CleanupRoots()
      {
         s_Current = null;
         s_MasterEvent = null;
      }

      private static void Internal_MakeMasterEventCurrent( int displayIndex )
      {
         if( s_MasterEvent == null )
         {
            s_MasterEvent = new Event( displayIndex );
         }

         s_MasterEvent.displayIndex = displayIndex;
         s_Current = s_MasterEvent;
         Internal_SetNativeEvent( s_MasterEvent.m_Ptr );
      }

      public static Event KeyboardEvent( string key )
      {
         Event @event = new Event( 0 );
         @event.type = EventType.KeyDown;
         if( string.IsNullOrEmpty( key ) )
         {
            return @event;
         }

         int num = 0;
         bool flag = false;
         do
         {
            flag = true;
            if( num >= key.Length )
            {
               flag = false;
               break;
            }

            switch( key[ num ] )
            {
               case '&':
                  @event.modifiers |= EventModifiers.Alt;
                  num++;
                  break;
               case '^':
                  @event.modifiers |= EventModifiers.Control;
                  num++;
                  break;
               case '%':
                  @event.modifiers |= EventModifiers.Command;
                  num++;
                  break;
               case '#':
                  @event.modifiers |= EventModifiers.Shift;
                  num++;
                  break;
               default:
                  flag = false;
                  break;
            }
         }
         while( flag );
         string text = key.Substring( num, key.Length - num ).ToLower();
         switch( text )
         {
            case "[0]":
               @event.character = '0';
               @event.keyCode = KeyCode.Keypad0;
               break;
            case "[1]":
               @event.character = '1';
               @event.keyCode = KeyCode.Keypad1;
               break;
            case "[2]":
               @event.character = '2';
               @event.keyCode = KeyCode.Keypad2;
               break;
            case "[3]":
               @event.character = '3';
               @event.keyCode = KeyCode.Keypad3;
               break;
            case "[4]":
               @event.character = '4';
               @event.keyCode = KeyCode.Keypad4;
               break;
            case "[5]":
               @event.character = '5';
               @event.keyCode = KeyCode.Keypad5;
               break;
            case "[6]":
               @event.character = '6';
               @event.keyCode = KeyCode.Keypad6;
               break;
            case "[7]":
               @event.character = '7';
               @event.keyCode = KeyCode.Keypad7;
               break;
            case "[8]":
               @event.character = '8';
               @event.keyCode = KeyCode.Keypad8;
               break;
            case "[9]":
               @event.character = '9';
               @event.keyCode = KeyCode.Keypad9;
               break;
            case "[.]":
               @event.character = '.';
               @event.keyCode = KeyCode.KeypadPeriod;
               break;
            case "[/]":
               @event.character = '/';
               @event.keyCode = KeyCode.KeypadDivide;
               break;
            case "[-]":
               @event.character = '-';
               @event.keyCode = KeyCode.KeypadMinus;
               break;
            case "[+]":
               @event.character = '+';
               @event.keyCode = KeyCode.KeypadPlus;
               break;
            case "[=]":
               @event.character = '=';
               @event.keyCode = KeyCode.KeypadEquals;
               break;
            case "[equals]":
               @event.character = '=';
               @event.keyCode = KeyCode.KeypadEquals;
               break;
            case "[enter]":
               @event.character = '\n';
               @event.keyCode = KeyCode.KeypadEnter;
               break;
            case "up":
               @event.keyCode = KeyCode.UpArrow;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "down":
               @event.keyCode = KeyCode.DownArrow;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "left":
               @event.keyCode = KeyCode.LeftArrow;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "right":
               @event.keyCode = KeyCode.RightArrow;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "insert":
               @event.keyCode = KeyCode.Insert;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "home":
               @event.keyCode = KeyCode.Home;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "end":
               @event.keyCode = KeyCode.End;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "pgup":
               @event.keyCode = KeyCode.PageDown;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "page up":
               @event.keyCode = KeyCode.PageUp;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "pgdown":
               @event.keyCode = KeyCode.PageUp;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "page down":
               @event.keyCode = KeyCode.PageDown;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "backspace":
               @event.keyCode = KeyCode.Backspace;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "delete":
               @event.keyCode = KeyCode.Delete;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "tab":
               @event.keyCode = KeyCode.Tab;
               break;
            case "f1":
               @event.keyCode = KeyCode.F1;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f2":
               @event.keyCode = KeyCode.F2;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f3":
               @event.keyCode = KeyCode.F3;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f4":
               @event.keyCode = KeyCode.F4;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f5":
               @event.keyCode = KeyCode.F5;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f6":
               @event.keyCode = KeyCode.F6;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f7":
               @event.keyCode = KeyCode.F7;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f8":
               @event.keyCode = KeyCode.F8;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f9":
               @event.keyCode = KeyCode.F9;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f10":
               @event.keyCode = KeyCode.F10;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f11":
               @event.keyCode = KeyCode.F11;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f12":
               @event.keyCode = KeyCode.F12;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f13":
               @event.keyCode = KeyCode.F13;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f14":
               @event.keyCode = KeyCode.F14;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "f15":
               @event.keyCode = KeyCode.F15;
               @event.modifiers |= EventModifiers.FunctionKey;
               break;
            case "[esc]":
               @event.keyCode = KeyCode.Escape;
               break;
            case "return":
               @event.character = '\n';
               @event.keyCode = KeyCode.Return;
               @event.modifiers &= ~EventModifiers.FunctionKey;
               break;
            case "space":
               @event.keyCode = KeyCode.Space;
               @event.character = ' ';
               @event.modifiers &= ~EventModifiers.FunctionKey;
               break;
            default:
               if( text.Length != 1 )
               {
                  try
                  {
                     @event.keyCode = (KeyCode)Enum.Parse( typeof( KeyCode ), text, ignoreCase: true );
                  }
                  catch( ArgumentException )
                  {
                  }
               }
               else
               {
                  @event.character = text.ToLower()[ 0 ];
                  @event.keyCode = (KeyCode)@event.character;
                  if( @event.modifiers != 0 )
                  {
                     @event.character = '\0';
                  }
               }

               break;
         }

         return @event;
      }

      public override int GetHashCode()
      {
         int num = 1;
         if( isKey )
         {
            num = (ushort)keyCode;
         }

         if( isMouse )
         {
            num = mousePosition.GetHashCode();
         }

         return ( num * 37 ) | (int)modifiers;
      }

      public override bool Equals( object obj )
      {
         if( obj == null )
         {
            return false;
         }

         if( object.ReferenceEquals( this, obj ) )
         {
            return true;
         }

         if( obj.GetType() != GetType() )
         {
            return false;
         }

         Event @event = (Event)obj;
         if( type != @event.type || ( modifiers & ~EventModifiers.CapsLock ) != ( @event.modifiers & ~EventModifiers.CapsLock ) )
         {
            return false;
         }

         if( isKey )
         {
            return keyCode == @event.keyCode;
         }

         if( isMouse )
         {
            return mousePosition == @event.mousePosition;
         }

         return false;
      }

      public override string ToString()
      {
         if( isKey )
         {
            if( character == '\0' )
            {
            }

            return string.Concat( "Event:", type, "   Character:", (int)character, "   Modifiers:", modifiers, "   KeyCode:", keyCode );
         }

         if( isMouse )
         {
         }

         if( type == EventType.ExecuteCommand || type == EventType.ValidateCommand )
         {
         }

         return "" + type;
      }

      public void Use()
      {
         if( type == EventType.Repaint || type == EventType.Layout )
         {
         }

         Internal_Use();
      }




      private extern void Init( int displayIndex );




      private extern void Cleanup();




      private extern void InitCopy( Event other );




      private extern void InitPtr( IntPtr ptr );




      internal extern void CopyFromPtr( IntPtr ptr );



      public extern EventType GetTypeForControl( int controlID );

      private void Internal_SetMousePosition( Vector2 value )
      {
         INTERNAL_CALL_Internal_SetMousePosition( this, ref value );
      }



      private static extern void INTERNAL_CALL_Internal_SetMousePosition( Event self, ref Vector2 value );



      private extern void Internal_GetMousePosition( out Vector2 value );

      private void Internal_SetMouseDelta( Vector2 value )
      {
         INTERNAL_CALL_Internal_SetMouseDelta( this, ref value );
      }



      private static extern void INTERNAL_CALL_Internal_SetMouseDelta( Event self, ref Vector2 value );



      private extern void Internal_GetMouseDelta( out Vector2 value );



      private static extern void Internal_SetNativeEvent( IntPtr ptr );



      private extern void Internal_Use();



      public static extern bool PopEvent( Event outEvent );



      public static extern int GetEventCount();
   }
}

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
   public sealed class Input
   {
      public static bool compensateSensors
      {
         get;
         set;
      }

      [Obsolete( "isGyroAvailable property is deprecated. Please use SystemInfo.supportsGyroscope instead." )]
      public static bool isGyroAvailable
      {
         get;
      }

      public static Vector3 mousePosition
      {
         get
         {
            INTERNAL_get_mousePosition( out Vector3 value );
            return value;
         }
      }

      public static Vector2 mouseScrollDelta
      {
         get
         {
            INTERNAL_get_mouseScrollDelta( out Vector2 value );
            return value;
         }
      }

      public static bool mousePresent
      {
         get;
      }

      public static bool simulateMouseWithTouches
      {
         get;
         set;
      }

      public static bool anyKey
      {
         get;
      }

      public static bool anyKeyDown
      {
         get;
      }

      public static string inputString
      {
         get;
      }

      public static Vector3 acceleration
      {
         get
         {
            INTERNAL_get_acceleration( out Vector3 value );
            return value;
         }
      }

      public static AccelerationEvent[] accelerationEvents
      {
         get
         {
            int accelerationEventCount = Input.accelerationEventCount;
            AccelerationEvent[] array = new AccelerationEvent[ accelerationEventCount ];
            for( int i = 0 ; i < accelerationEventCount ; i++ )
            {
               ref AccelerationEvent reference = ref array[ i ];
               reference = GetAccelerationEvent( i );
            }

            return array;
         }
      }

      public static int accelerationEventCount
      {
         get;
      }

      public static Touch[] touches
      {
         get
         {
            int touchCount = Input.touchCount;
            Touch[] array = new Touch[ touchCount ];
            for( int i = 0 ; i < touchCount ; i++ )
            {
               ref Touch reference = ref array[ i ];
               reference = GetTouch( i );
            }

            return array;
         }
      }

      public static int touchCount
      {
         get;
      }

      [Obsolete( "eatKeyPressOnTextFieldFocus property is deprecated, and only provided to support legacy behavior." )]
      public static bool eatKeyPressOnTextFieldFocus
      {
         get;
         set;
      }

      public static bool touchPressureSupported
      {
         get;
      }

      public static bool stylusTouchSupported
      {
         get;
      }

      public static bool touchSupported
      {
         get;
      }

      public static bool multiTouchEnabled
      {
         get;
         set;
      }

      public static DeviceOrientation deviceOrientation
      {
         get;
      }

      public static IMECompositionMode imeCompositionMode
      {
         get;
         set;
      }

      public static string compositionString
      {
         get;
      }

      public static bool imeIsSelected
      {
         get;
      }

      public static Vector2 compositionCursorPos
      {
         get
         {
            INTERNAL_get_compositionCursorPos( out Vector2 value );
            return value;
         }
         set
         {
            INTERNAL_set_compositionCursorPos( ref value );
         }
      }

      public static bool backButtonLeavesApp
      {
         get;
         set;
      }

      private static extern int mainGyroIndex_Internal();

      private static extern bool GetKeyInt( int key );

      private static extern bool GetKeyString( string name );

      private static extern bool GetKeyUpInt( int key );

      private static extern bool GetKeyUpString( string name );

      private static extern bool GetKeyDownInt( int key );

      private static extern bool GetKeyDownString( string name );

      public static extern float GetAxis( string axisName );

      public static extern float GetAxisRaw( string axisName );

      public static extern bool GetButton( string buttonName );

      public static extern bool GetButtonDown( string buttonName );

      public static extern bool GetButtonUp( string buttonName );

      public static bool GetKey( string name )
      {
         return GetKeyString( name );
      }

      public static bool GetKey( KeyCode key )
      {
         return GetKeyInt( (int)key );
      }

      public static bool GetKeyDown( string name )
      {
         return GetKeyDownString( name );
      }

      public static bool GetKeyDown( KeyCode key )
      {
         return GetKeyDownInt( (int)key );
      }

      public static bool GetKeyUp( string name )
      {
         return GetKeyUpString( name );
      }

      public static bool GetKeyUp( KeyCode key )
      {
         return GetKeyUpInt( (int)key );
      }

      public static extern string[] GetJoystickNames();

      public static extern bool GetMouseButton( int button );

      public static extern bool GetMouseButtonDown( int button );

      public static extern bool GetMouseButtonUp( int button );

      public static extern void ResetInputAxes();

      private static extern void INTERNAL_get_mousePosition( out Vector3 value );

      private static extern void INTERNAL_get_mouseScrollDelta( out Vector2 value );

      private static extern void INTERNAL_get_acceleration( out Vector3 value );

      public static AccelerationEvent GetAccelerationEvent( int index )
      {
         INTERNAL_CALL_GetAccelerationEvent( index, out AccelerationEvent value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetAccelerationEvent( int index, out AccelerationEvent value );

      public static Touch GetTouch( int index )
      {
         INTERNAL_CALL_GetTouch( index, out Touch value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetTouch( int index, out Touch value );

      private static extern void INTERNAL_get_compositionCursorPos( out Vector2 value );

      private static extern void INTERNAL_set_compositionCursorPos( ref Vector2 value );
   }
}

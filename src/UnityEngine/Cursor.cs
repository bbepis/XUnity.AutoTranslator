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
   public sealed class Cursor
   {
      public static bool visible
      {
         get;
         set;
      }

      public static CursorLockMode lockState
      {
         get;
         set;
      }

      private static void SetCursor( Texture2D texture, CursorMode cursorMode )
      {
         SetCursor( texture, Vector2.zero, cursorMode );
      }

      public static void SetCursor( Texture2D texture, Vector2 hotspot, CursorMode cursorMode )
      {
         INTERNAL_CALL_SetCursor( texture, ref hotspot, cursorMode );
      }

      private static extern void INTERNAL_CALL_SetCursor( Texture2D texture, ref Vector2 hotspot, CursorMode cursorMode );
   }
}

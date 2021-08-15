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
   public sealed class Screen
   {
      public static Resolution[] resolutions
      {
         get;
      }

      public static Resolution currentResolution
      {
         get;
      }

      public static int width
      {
         get;
      }

      public static int height
      {
         get;
      }

      public static float dpi
      {
         get;
      }

      public static bool fullScreen
      {
         get;
         set;
      }

      public static bool autorotateToPortrait
      {
         get;
         set;
      }

      public static bool autorotateToPortraitUpsideDown
      {
         get;
         set;
      }

      public static bool autorotateToLandscapeLeft
      {
         get;
         set;
      }

      public static bool autorotateToLandscapeRight
      {
         get;
         set;
      }

      public static ScreenOrientation orientation
      {
         get;
         set;
      }

      public static int sleepTimeout
      {
         get;
         set;
      }

      public static bool lockCursor => throw new NotImplementedException();

      public static extern void SetResolution( int width, int height, bool fullscreen, int preferredRefreshRate );

      public static void SetResolution( int width, int height, bool fullscreen )
      {
         int preferredRefreshRate = 0;
         SetResolution( width, height, fullscreen, preferredRefreshRate );
      }
   }
}

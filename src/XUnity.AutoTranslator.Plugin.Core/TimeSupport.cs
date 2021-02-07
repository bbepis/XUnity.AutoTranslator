using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public class UnityEngineTime : ITime
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public float realtimeSinceStartup => Time.realtimeSinceStartup;

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public int frameCount => Time.frameCount;
   }

   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public class TimeSupport
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static ITime Time { get; set; } = new UnityEngineTime();
   }

   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public interface ITime
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      float realtimeSinceStartup { get; }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      int frameCount { get; }
   }
}

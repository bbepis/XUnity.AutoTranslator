using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class CoroutineHelper
   {
      public static Coroutine Start( IEnumerator coroutine ) => AutoTranslationPlugin.Current.StartCoroutine( coroutine );
   }
}

#if MANAGED

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class UtageHelper
   {
      private static object AdvManager;
      private static HashSet<string> Labels = new HashSet<string>();

      public static void FixLabel( ref string label )
      {
         var empty = new object[ 0 ];
         if( AdvManager == null && UnityTypes.AdvDataManager != null )
         {
            try
            {
               AdvManager = GameObject.FindObjectOfType( UnityTypes.AdvDataManager.UnityType );
               var ScenarioDataTblProperty = UnityTypes.AdvDataManager.ClrType.GetProperty( "ScenarioDataTbl" );
               var ScenarioDataTbl = ScenarioDataTblProperty.GetValue( AdvManager, empty );
               foreach( object labelToAdvScenarioDataKeyValuePair in (IEnumerable)ScenarioDataTbl )
               {
                  var labelToAdvScenarioDataKeyValuePairType = typeof( KeyValuePair<,> )
                     .MakeGenericType( new Type[] { typeof( string ), UnityTypes.AdvScenarioData.ClrType } );

                  var AdvScenarioDataKey = (string)labelToAdvScenarioDataKeyValuePairType.GetProperty( "Key" )
                     .GetValue( labelToAdvScenarioDataKeyValuePair, empty );

                  Labels.Add( AdvScenarioDataKey );

                  var AdvScenarioData = labelToAdvScenarioDataKeyValuePairType.GetProperty( "Value" )
                     .GetValue( labelToAdvScenarioDataKeyValuePair, empty );

                  if( AdvScenarioData != null )
                  {
                     var ScenarioLabelsProperty = AdvScenarioData.GetType().GetProperty( "ScenarioLabels" );

                     var labelToAdvScenarioLabelData = ScenarioLabelsProperty.GetValue( AdvScenarioData, empty );

                     foreach( object labelToAdvScenarioLabelDataKeyValuePair in (IEnumerable)labelToAdvScenarioLabelData )
                     {
                        var labelToAdvScenarioLabelDataKeyValuePairType = typeof( KeyValuePair<,> )
                           .MakeGenericType( new Type[] { typeof( string ), UnityTypes.AdvScenarioLabelData.ClrType } );

                        var AdvScenarioLabelDataKey = (string)labelToAdvScenarioLabelDataKeyValuePairType.GetProperty( "Key" )
                           .GetValue( labelToAdvScenarioLabelDataKeyValuePair, empty );

                        Labels.Add( AdvScenarioLabelDataKey );
                     }
                  }
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Warn( e, "An error occurred while setting up scenario set." );
            }
         }

         if( !Labels.Contains( label ) )
         {
            var scope = TranslationScopeHelper.GetScope( null );
            if( AutoTranslationPlugin.Current.TextCache.TryGetReverseTranslation( label, scope, out string key ) )
            {
               label = key;
            }
         }
      }
   }
}

#endif

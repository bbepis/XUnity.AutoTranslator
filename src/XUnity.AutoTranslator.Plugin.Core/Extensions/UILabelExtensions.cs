namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class UILabelExtensions
   {
      private static readonly string SpacingYPropertyName = "spacingY";
      private static readonly string FloatSpacingYPropertyName = "floatSpacingY";
      private static readonly string UseFloatSpacingPropertyName = "useFloatSpacing";

      public static object GetSpacingY( this object uiLabel )
      {
         var type = uiLabel.GetType();
         var useFloatSpacing = (bool)type.GetProperty( UseFloatSpacingPropertyName )?.GetGetMethod()?.Invoke( uiLabel, null );
         if( useFloatSpacing )
         {
            return type.GetProperty( FloatSpacingYPropertyName )?.GetGetMethod()?.Invoke( uiLabel, null );
         }
         else
         {
            return type.GetProperty( SpacingYPropertyName )?.GetGetMethod()?.Invoke( uiLabel, null );
         }
      }

      public static void SetSpacingY( this object uiLabel, object spacing )
      {
         var type = uiLabel.GetType();
         if( spacing is float )
         {
            type.GetProperty( FloatSpacingYPropertyName )?.GetSetMethod()?.Invoke( uiLabel, new[] { spacing } );
         }
         else
         {
            type.GetProperty( SpacingYPropertyName )?.GetSetMethod()?.Invoke( uiLabel, new[] { spacing } );
         }
      }

      public static object Multiply( this object numeric, float scale )
      {
         if( numeric is float )
         {
            return (float)numeric * scale;
         }
         else
         {
            return (int)( (int)numeric * scale );
         }
      }
   }
}

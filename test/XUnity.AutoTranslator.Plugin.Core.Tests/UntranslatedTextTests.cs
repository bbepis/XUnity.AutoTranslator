using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public class UntranslatedTextTests
   {
      [Theory( DisplayName = "Can_Substitute" )]
      [InlineData( "がんばれ、遥", "がんばれ、{{A}}" )]
      [InlineData( "がんばれ、Haruka", "がんばれ、{{A}}" )]
      [InlineData( "がんばれ、Haruka... Here's an arbitrary number: 0.423 isn't that nice?", "がんばれ、{{A}}... Here's an arbitrary number: {{B}} isn't that nice?" )]
      [InlineData( "HS2_BetterHScenes 2.6.2", "HS{{A}}_BetterHScenes {{B}}" )]
      [InlineData( "Slope: 38,6°", "Slope: {{A}}°" )]
      [InlineData( "Here's a date and time: 2021/05-06 11:43**", "Here's a date and time: {{A}} {{B}}**" )]
      [InlineData( "Version 1.0.*-beta ::123::", "Version {{A}}.*-beta ::{{B}}::" )]
      [InlineData( "Version 1.0.*-beta 456::123::", "Version {{A}}.*-beta {{B}}::" )]
      [InlineData( "3::", "{{A}}::" )]
      [InlineData( "::3::", "::{{A}}::" )]
      [InlineData( "::3", "::{{A}}" )]
      [InlineData( "::3abc", "::{{A}}abc" )]
      [InlineData( "ABCDEF 3", "ABCDEF {{A}}" )]
      [InlineData( "ABCDEF-3", "ABCDEF-{{A}}" )]
      [InlineData( "3 ABCDEF", "{{A}} ABCDEF" )]
      [InlineData( "3-ABCDEF", "{{A}}-ABCDEF" )]
      public void Can_Substitute( string input, string expectedTemplate )
      {
         Settings.Replacements[ "Haruka" ] = "Haruka";
         Settings.Replacements[ "遥" ] = "Haruka";

         var untranslatedText = new UntranslatedText( input, false, true, false, true, true );

         Assert.Equal( expectedTemplate, untranslatedText.TemplatedOriginal_Text );
         Assert.False( untranslatedText.IsOnlyTemplate );
      }

      [Theory( DisplayName = "Can_Substitute_Fully" )]
      [InlineData( "遥 Haruka", "{{B}} {{A}}" )]
      [InlineData( "遥", "{{A}}" )]
      public void Can_Substitute_Fully( string input, string expectedTemplate )
      {
         Settings.Replacements[ "Haruka" ] = "Haruka";
         Settings.Replacements[ "遥" ] = "Haruka";

         var untranslatedText = new UntranslatedText( input, false, true, false, true, true );

         Assert.Equal( expectedTemplate, untranslatedText.TemplatedOriginal_Text );
         Assert.True( untranslatedText.IsOnlyTemplate );
      }

      [Theory( DisplayName = "Can_Trim_Surrounding_Whitespace" )]
      [InlineData( "  Hello  ", "Hello", "  ", "  " )]
      [InlineData( " Hello", "Hello", " ", null )]
      [InlineData( "Hello", "Hello", null, null )]
      [InlineData( "\r\n \r\nHello", "Hello", "\r\n \r\n", null )]
      [InlineData( "\r\n \r\nHello\n", "Hello", "\r\n \r\n", "\n" )]
      [InlineData( "\r\r\r\r\n \n　Hello  \r\n", "Hello", "\r\r\r\r\n \n　", "  \r\n" )]
      public void Can_Trim_Surrounding_Whitespace( string input, string expectedTrimmedText, string expectedLeadingWhitespace, string expectedTrailingWhitespace )
      {
         var untranslatedText = new UntranslatedText( input, false, false, false, true, true );

         Assert.Equal( input, untranslatedText.TemplatedOriginal_Text_InternallyTrimmed );
         Assert.Equal( expectedTrimmedText, untranslatedText.TemplatedOriginal_Text_FullyTrimmed );
         Assert.Equal( expectedLeadingWhitespace, untranslatedText.LeadingWhitespace );
         Assert.Equal( expectedTrailingWhitespace, untranslatedText.TrailingWhitespace );
      }

      [Theory( DisplayName = "Can_Trim_Internal_Whitespace_English" )]
      [InlineData( "What are you doing?", "What are you doing?" )]
      [InlineData( "What are\nyou doing?", "What are you doing?" )]
      [InlineData( "What are\n\nyou doing?", "What are\n\nyou doing?" )]
      [InlineData( "What are\n\n \n\nyou doing?", "What are\n\n\n\nyou doing?" )]
      [InlineData( "What are\n\n  \n\nyou doing?", "What are\n\n  \n\nyou doing?" )]
      [InlineData( "What are\n  \nyou doing?", "What are  you doing?" )]
      public void Can_Trim_Internal_Whitespace_English( string input, string expectedTrimmedText )
      {
         var untranslatedText = new UntranslatedText( input, false, true, true, true, true );

         Assert.Equal( expectedTrimmedText, untranslatedText.TemplatedOriginal_Text_FullyTrimmed );
      }

      [Theory( DisplayName = "Can_Trim_Internal_Whitespace" )]
      [InlineData( "Hel lo", "Hel lo" )]
      [InlineData( "Hel\r\n lo", "Hello" )]
      [InlineData( "Hel\n\nlo", "Hel\n\nlo" )]
      [InlineData( "Hello\nWhat\nYou", "HelloWhatYou" )]
      [InlineData( "Hello\n\nWhat\nYou", "Hello\n\nWhatYou" )]
      public void Can_Trim_Internal_Whitespace( string input, string expectedTrimmedText )
      {
         var untranslatedText = new UntranslatedText( input, false, true, false, true, true );

         Assert.Equal( expectedTrimmedText, untranslatedText.TemplatedOriginal_Text_FullyTrimmed );
      }

      [Theory( DisplayName = "Can_Trim_Internal_And_Surrounding_Whitespace" )]
      [InlineData( "\r\n \r\nHe llo", "He llo", "\r\n \r\n", null )]
      [InlineData( "\r\n \r\nHe   llo", "He   llo", "\r\n \r\n", null )]
      [InlineData( "\r\n \r\nHel\r\nlo\n", "Hello", "\r\n \r\n", "\n" )]
      [InlineData( "\r\n \r\nHel\n\nlo\n", "Hel\n\nlo", "\r\n \r\n", "\n" )]
      [InlineData( "\r\n \r\nHel\n\n lo\n", "Hel\n\nlo", "\r\n \r\n", "\n" )]
      [InlineData( "\r\n \r\nHel\n\n  lo\n", "Hel\n\n  lo", "\r\n \r\n", "\n" )]
      [InlineData( "\r\n \r\nHel  \n\n  lo\n", "Hel  \n\n  lo", "\r\n \r\n", "\n" )]
      [InlineData( "\r\n \r\nHel  \n  lo\n", "Hel    lo", "\r\n \r\n", "\n" )]
      [InlineData( "\r\n \r\nHel \n lo\n", "Hello", "\r\n \r\n", "\n" )]
      [InlineData( "\r\r\r\r\n \n　Hell\no  \r\n", "Hello", "\r\r\r\r\n \n　", "  \r\n" )]
      public void Can_Trim_Internal_And_Surrounding_Whitespace( string input, string expectedTrimmedText, string expectedLeadingWhitespace, string expectedTrailingWhitespace )
      {
         var untranslatedText = new UntranslatedText( input, false, true, false, true, true );

         Assert.Equal( expectedTrimmedText, untranslatedText.TemplatedOriginal_Text_FullyTrimmed );
         Assert.Equal( expectedLeadingWhitespace, untranslatedText.LeadingWhitespace );
         Assert.Equal( expectedTrailingWhitespace, untranslatedText.TrailingWhitespace );
      }

      [Theory( DisplayName = "Can_Trim_Internal_And_Surrounding_Whitespace_And_Template" )]
      [InlineData( "\r\n \r\nFPS: 60.53", "FPS: {{A}}", "\r\n \r\n", null )]
      [InlineData( "\r\n \r\nFPS:   60.53", "FPS:   {{A}}", "\r\n \r\n", null )]
      [InlineData( "\r\n \r\nFPS:\n 60.53", "FPS:{{A}}", "\r\n \r\n", null )]
      [InlineData( "\r\n \r\nFPS:\n  60.53", "FPS:  {{A}}", "\r\n \r\n", null )]
      [InlineData( "\r\n \r\nFPS:\n \n 60.53", "FPS:{{A}}", "\r\n \r\n", null )]
      [InlineData( "\r\n \r\nFPS:\n  \n 60.53", "FPS:  {{A}}", "\r\n \r\n", null )]
      public void Can_Trim_Internal_And_Surrounding_Whitespace_And_Template( string input, string expectedTrimmedText, string expectedLeadingWhitespace, string expectedTrailingWhitespace )
      {
         var untranslatedText = new UntranslatedText( input, true, true, false, true, true );

         Assert.Equal( expectedTrimmedText, untranslatedText.TemplatedOriginal_Text_FullyTrimmed );
         Assert.Equal( expectedLeadingWhitespace, untranslatedText.LeadingWhitespace );
         Assert.Equal( expectedTrailingWhitespace, untranslatedText.TrailingWhitespace );
      }

      [Theory( DisplayName = "Are_References_The_Same" )]
      [InlineData( "Hello" )]
      [InlineData( "He  llo" )]
      [InlineData( "He  \n\nllo" )]
      public void Are_References_The_Same( string input )
      {
         var untranslatedText = new UntranslatedText( input, false, true, false, true, true );

         Assert.True( ReferenceEquals( input, untranslatedText.Original_Text ) );
         Assert.True( ReferenceEquals( input, untranslatedText.Original_Text_ExternallyTrimmed ) );
         Assert.True( ReferenceEquals( input, untranslatedText.Original_Text_InternallyTrimmed ) );
         Assert.True( ReferenceEquals( input, untranslatedText.Original_Text_FullyTrimmed ) );

         Assert.True( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text ) );
         Assert.True( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text_ExternallyTrimmed ) );
         Assert.True( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text_InternallyTrimmed ) );
         Assert.True( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text_FullyTrimmed ) );
      }

      [Theory( DisplayName = "Are_References_The_Correct_With_Internal_Whitespace" )]
      [InlineData( "He \nllo" )]
      [InlineData( "He  \r\t\t\nllo" )]
      public void Are_References_The_Correct_With_Internal_Whitespace( string input )
      {
         var untranslatedText = new UntranslatedText( input, false, true, false, true, true );

         Assert.True( ReferenceEquals( input, untranslatedText.Original_Text ) );
         Assert.True( ReferenceEquals( input, untranslatedText.Original_Text_ExternallyTrimmed ) );
         Assert.False( ReferenceEquals( input, untranslatedText.Original_Text_InternallyTrimmed ) );
         Assert.False( ReferenceEquals( input, untranslatedText.Original_Text_FullyTrimmed ) );

         Assert.True( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text ) );
         Assert.True( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text_ExternallyTrimmed ) );
         Assert.False( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text_InternallyTrimmed ) );
         Assert.False( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text_FullyTrimmed ) );
      }

      [Theory( DisplayName = "Are_References_The_Correct_With_External_Whitespace" )]
      [InlineData( "   Hello   " )]
      [InlineData( "   \r\t\t\n\nHello   " )]
      public void Are_References_The_Correct_With_External_Whitespace( string input )
      {
         var untranslatedText = new UntranslatedText( input, false, true, false, true, true );

         Assert.True( ReferenceEquals( input, untranslatedText.Original_Text ) );
         Assert.False( ReferenceEquals( input, untranslatedText.Original_Text_ExternallyTrimmed ) );
         Assert.True( ReferenceEquals( input, untranslatedText.Original_Text_InternallyTrimmed ) );
         Assert.False( ReferenceEquals( input, untranslatedText.Original_Text_FullyTrimmed ) );

         Assert.True( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text ) );
         Assert.False( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text_ExternallyTrimmed ) );
         Assert.True( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text_InternallyTrimmed ) );
         Assert.False( ReferenceEquals( input, untranslatedText.TemplatedOriginal_Text_FullyTrimmed ) );
      }

      [Theory( DisplayName = "All_Expectations" )]
      [InlineData( "  私は遥\nあんたがだれ？   ", "  私は{{A}}\nあんたがだれ？   ", "私は{{A}}\nあんたがだれ？", "  私は{{A}}あんたがだれ？   ", "私は{{A}}あんたがだれ？", "  私は遥\nあんたがだれ？   ", "私は遥\nあんたがだれ？", "  私は遥あんたがだれ？   ", "私は遥あんたがだれ？", "  ", "   ", false )]
      [InlineData( "He  \r\t\t\nllo", "He  \r\t\t\nllo", "He  \r\t\t\nllo", "He  \t\tllo", "He  \t\tllo", "He  \r\t\t\nllo", "He  \r\t\t\nllo", "He  \t\tllo", "He  \t\tllo", null, null, true )]
      [InlineData( "Who\nare you?", "Who\nare you?", "Who\nare you?", "Who are you?", "Who are you?", "Who\nare you?", "Who\nare you?", "Who are you?", "Who are you?", null, null, true )]
      public void All_Expectations(
         string input,
         string TemplatedOriginal_Text,
         string TemplatedOriginal_Text_ExternallyTrimmed,
         string TemplatedOriginal_Text_InternallyTrimmed,
         string TemplatedOriginal_Text_FullyTrimmed,
         string Original_Text,
         string Original_Text_ExternallyTrimmed,
         string Original_Text_InternallyTrimmed,
         string Original_Text_FullyTrimmed,
         string leading,
         string trailing,
         bool whitespaceBetweenWords )
      {
         Settings.Replacements[ "遥" ] = "Haruka";

         var untranslatedText = new UntranslatedText( input, false, true, whitespaceBetweenWords, true, true );

         Assert.Equal( TemplatedOriginal_Text, untranslatedText.TemplatedOriginal_Text );
         Assert.Equal( TemplatedOriginal_Text_ExternallyTrimmed, untranslatedText.TemplatedOriginal_Text_ExternallyTrimmed );
         Assert.Equal( TemplatedOriginal_Text_InternallyTrimmed, untranslatedText.TemplatedOriginal_Text_InternallyTrimmed );
         Assert.Equal( TemplatedOriginal_Text_FullyTrimmed, untranslatedText.TemplatedOriginal_Text_FullyTrimmed );

         Assert.Equal( Original_Text, untranslatedText.Original_Text );
         Assert.Equal( Original_Text_ExternallyTrimmed, untranslatedText.Original_Text_ExternallyTrimmed );
         Assert.Equal( Original_Text_InternallyTrimmed, untranslatedText.Original_Text_InternallyTrimmed );
         Assert.Equal( Original_Text_FullyTrimmed, untranslatedText.Original_Text_FullyTrimmed );

         Assert.Equal( leading, untranslatedText.LeadingWhitespace );
         Assert.Equal( trailing, untranslatedText.TrailingWhitespace );

      }

      // FIXME: Make thorough text with proper 'language'
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class TranslationAggregatorViewModel
   {
      private LinkedList<AggregatedTranslationViewModel> _translations;
      private LinkedListNode<AggregatedTranslationViewModel> _current;
      private List<Translation> _translationsToAggregate = new List<Translation>();
      private HashSet<string> _textsToAggregate = new HashSet<string>();
      private float _lastUpdate = 0.0f;

      public TranslationAggregatorViewModel( TranslationManager translationManager )
      {
         _translations = new LinkedList<AggregatedTranslationViewModel>();

         Manager = translationManager;
         Height = 100; // TODO: Get from config
         Width = 400; // TODO: Get from config

         AllTranslators = translationManager.AllEndpoints
            .Select( x => new TranslatorViewModel( x ) )
            .ToList();

         AvailableTranslators = AllTranslators
            .Where( x => x.Endpoint.Error == null )
            .ToList();
      }

      public bool IsShown { get; set; }

      public bool IsShowingOptions { get; set; }

      public float Height { get; set; }

      public float Width { get; set; }

      public List<TranslatorViewModel> AvailableTranslators { get; }

      public List<TranslatorViewModel> AllTranslators { get; }

      public TranslationManager Manager { get; set; }

      public AggregatedTranslationViewModel Current => _current?.Value;

      public void OnNewTranslationAdded( TextTranslationInfo info )
      {
         if( !_textsToAggregate.Contains( info.OriginalText ) )
         {
            var vm = new Translation( info.OriginalText, info.TranslatedText );

            _textsToAggregate.Add( info.OriginalText );
            _translationsToAggregate.Add( vm );

            _lastUpdate = Time.realtimeSinceStartup;

            // never add more than 10 translations to a single window...
            if( _translationsToAggregate.Count >= 10 )
            {
               CreateNewAggregatedTranslation();
            }
         }
      }

      private void CreateNewAggregatedTranslation()
      {
         try
         {
            var translations = _translationsToAggregate.ToList();

            var vm = new AggregatedTranslationViewModel( this, translations );

            var previousLast = _translations.Last;

            _translations.AddLast( vm );
            if( _current == null )
            {
               _current = _translations.Last;
            }
            else
            {
               if( _current == previousLast )
               {
                  _current = _translations.Last;
               }
            }

            // ensure we never have more than 100
            if( _translations.Count >= 100 )
            {
               var first = _translations.First;
               _translations.RemoveFirst();

               if( _current == first )
               {
                  _current = _translations.First;
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error while copying text to clipboard." );
         }
         finally
         {
            _textsToAggregate.Clear();
            _translationsToAggregate.Clear();
         }
      }

      public void Update()
      {
         if( _translationsToAggregate.Count > 0 && Time.realtimeSinceStartup - _lastUpdate > Settings.ClipboardDebounceTime )
         {
            CreateNewAggregatedTranslation();
         }

         if( _current != null )
         {
            _current.Value.Update();
         }
      }

      public bool HasPrevious()
      {
         return _current?.Previous != null;
      }

      public void MovePrevious()
      {
         _current = _current.Previous;
      }

      public bool HasNext()
      {
         return _current?.Next != null;
      }

      public void MoveNext()
      {
         _current = _current.Next;
      }

      public void MoveLatest()
      {
         _current = _translations.Last;
      }
   }
}

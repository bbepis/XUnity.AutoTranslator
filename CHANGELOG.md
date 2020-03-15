### 4.10.0
 * FEATURE - Native ezTransXP support through Ehnd - Thanks to Jiwon-Pack on Github
 * MISC - Improved linux support. Plugin should now function as expected when used with BepInEx 5.x on linux (resource redirection may still have problems with case-sensitivity and path separators). Other installation methods may not yield similar results
 * MISC - Fixed .zip release files so they use correct directory separators

### 4.9.0
 * FEATURE - API to support easier manual translations of TextAsset resources
 * FEATURE - Support for not loading every translated image into memory permanently, but instead loading them on demand (new configuration option)
 * FEATURE - Better support for variations of Chinese as output language. 'zh-CN', 'zh-TW', 'zh-Hans', 'zh-Hant', 'zh' can now all be used by all translators if supported
 * FEATURE - Better support for variations of Chinese as input language. Plugin wont complain if 'zh-Hans', 'zh-Hant' or 'zh' is used
 * MISC - Support for older versions of UTAGE
 * MISC - New option that enables ignoring rules for calling virtual methods when setting the text of a text component: 'IgnoreVirtualTextSetterCallingRules'
 * MISC - Updated version of MonoMod distributed with the plugin
 * BUG FIX - Potential errors caused by using weak references incorrectly causing a race condition
 * BUG FIX - Potential error during initialization if Resource Redirector is not present

### 4.8.3
 * MISC - Added support for ShimejiEngine.dll
 * BUG FIX - Fixed a NullReferenceException that could cause the plugin to stop working

### 4.8.2
 * MISC - Bundle Harmony 2.0-beta (custom build) with UnityInjector package instead of 1.2.0.1 to be more compatible with i18nEx
 * BUG FIX - Fixed bug that could cause out-of-process requests to stop working in certain situations
 * BUG FIX - Fixed a "performance" bug in TextMesh hooks

### 4.8.1
 * MISC - Experimental Google! Translate compatibility endpoint that services requests out-of-process
 * MISC - Updated default user agents
 * MISC - Minor changes to Google! translate TKK timing logic
 * MISC - Allow translation of placeholder text with UGUI and TextMesh Pro texts
 * BUG FIX - Disallow outputting of IMGUI templated texts that are not considered translatable

### 4.8.0
 * FEATURE - Changed the way TextMesh Pro fonts can be changed. Fonts can now be loaded through external asset bundles
 * CHANGE - Removed BepInEx 4.x package to remove confusion about which package to download
 * BUG FIX - Fixed a bug that could cause a crash in Unity games using a very old version of the engine
 * BUG FIX - Fixed a bug where substitutions could sometimes cause texts not to be translated
 * BUG FIX - Fixed a bug that could cause redirected resources not to be loaded if dumping was enabled

### 4.7.1
 * BUG FIX - Development-time fix to the nuget package
 * BUG FIX - The 'Translators' directory must now be placed in the same directory that the XUnity.AutoTranslator.Plugin.Core.dll is placed in. This allows moving around the plugin as you see fit in BepInEx 5.0. For the ReiPatcher installer, the Translators are now found in the Managed directory of the game. For UnityInjector the Translators directory has been moved out of the Config directory
 * BUG FIX - Minor bug fix where in some cases the plugin could not create the initial translation files on startup

### 4.7.0
 * FEATURE - Text preprocessors that allows applying text replacements before a text is sent for translation
 * MISC - Changed a lot of logging levels and changed SilentMode to default to true

### 4.6.4
 * BUG FIX - Fixed bug where the OutputFile was not always loaded with lowest priority during initialization
 * BUG FIX - Fixed a bug where redirected resources could not be located under certain circumstances (again)

### 4.6.3
 * MISC - Disable all translator endpoints if output language is the same as the source language

### 4.6.2
 * BUG FIX - Fixed a bug where redirected resources could not be located under certain circumstances
 * BUG FIX - Added throttling logic to Baidu translate to ensure the plugin never exceeds one query per second

### 4.6.1
 * BUG FIX - Fixed a bug where in some situations a line may be sent for translation twice due to whitespace differences

### 4.6.0
 * FEATURE - Support for ZIP files in redirected resource directory
 * BUG FIX - Fixed bug where you somtimes had to close dialogues before translations would appear
 * BUG FIX - Fixed bug where plugin would not function if a game is launched through a custom launcher not located in the game directory
 * MISC - Automatically enable texture translations if a texture directory is present during initial startup

### 4.5.1
 * MISC - Improved ZIP archive performance (probably)
 * MISC - Disabled HTTPS certificate checks by default for all runtimes because default value generation was incorrect
 * MISC - Added built-in 'Passthrough' endpoint that does not perform any translations, but allows dumping texts and process texts through substitutions, etc.
 * MISC - Increased max value of 'MaxCharactersPerTranslation' from 500 to 1000

### 4.5.0
 * CHANGE - Warning! All default directory paths have been changed in new configuration files
 * FEATURE - {Lang} / {lang} parameter can now be used in any path specified in the configuration file. This will allow more clean seperation between translations to different languages without needing to modify/move any files
 * FEATURE - Support ZIP files for translation, resize and texture files
 * MISC - Reduced number of static translations significantly

### 4.4.0
 * FEATURE - Allow translation of any found texts. Whether or not text considered to be untranslatable should be output is controlled through configuration
 * BUG FIX - Fixed a bug in the 'ReiPatcher' standalone installer that caused installation to fail on Windows 7 and below
 * MISC - Reduced batch error re-enable cooldown from 240 seconds to 60 seconds

### 4.3.1
 * BUG FIX - Fixed rich text handling bug introduced in 4.3.0

### 4.3.0
 * FEATURE - Added UI resize functionality that enables resizing the font size and more of translated text components manually
 * MISC - Obsoleted AssetLoadedHandlerBase<T>.IsDumpingEnabled. Replaced with AutoTranslatorSettings.IsDumpingRedirectedResourcesEnabled

### 4.2.0
 * CHANGE - Changed how splitting regexes are configured to supports scopes + more.

### 4.1.0
 * FEATURE - Added new concept that allows splitting texts to be translated with a regex before trying to translate them
 * MISC - Improved NGUI text resizing behaviour

### 4.0.1
 * MISC - Added option to convert japanese wide-width numerics to standard ASCII numerics during translation postprocessing

### 4.0.0
 * BREAKING CHANGE - Restructuring of internal and some public APIs - ITranslateEndpoint API not affected
 * BREAKING CHANGE - Changed file layout in Translators directory - should replace folder entirely upon upgrade to avoid redundant files that may be picked up unintentionally by mod loaders
 * FEATURE - Resource Redirection API
 * FEATURE - Support for redirecting translation processing to an external process
 * MISC - Updated distributed MonoMod version

### 3.8.1
 * BUG FIX - Minor bug in relation to harmony initialization
 * BUG FIX - Fixed bug in MonoMod hooking that would cause it to always use native hooks over managed hooks
 * BUG FIX - Fixed bug in 'EnableLegacyTextureLoading' which should now work consistently
 * MISC - Option to enable/disable success translation logs through option 'EnableSilentMode'
 * MISC - IMGUI window black listing option

### 3.8.0
 * FEATURE - Support 'TARC' directives '#set exe', '#unset exe', '#set level' and '#unset level'. Must be explicitly enabled through config option 'EnableTranslationScoping=True'
 * CHANGE - Completely reworked whitespace handling to better support manual translations (still backwards compatible)
 * BUG FIX - Fixed bug in 'TextGetterCompatibilityMode'
 * BUG FIX - Corrected whitespace handling when constructing alternate translation-pairs for languages requiring whitespace
 * MISC - Removed 'Delay' config option due to feature bloat
 * MISC - Removed 'WhitespaceRemovalStrategy' config option due to feature bloat
 * MISC - Changed default value of 'CacheWhitespaceDifferences' from True to False

### 3.7.0
 * FEATURE - Support for hooking 3D TextMesh (disabled by default)
 * FEATURE - Public API surface to query the plugin for translations
 * CHANGE - Replaced 'Experimental' hooks with MonoMod hooks, which are now always enabled in supported environments
 * MISC - Allow external plugins to pass in unknown components when overriding hooks
 * BUG FIX - Fixed bug in relation to rich text handling in Translation Aggregator-view

### 3.6.2
 * BUG FIX - Fixed a bug with escape/unescape logic in translation text files
 * BUG FIX - Plugin now loads all translations provided in the translation text files - even if they are not considered a candidate for machine translation
 * BUG FIX - Fixed a bug where reloading or toggling the translations would not always show the correct text
 * BUG FIX - Fixed a bug where substitution file was being loaded twice - this should have no actual effect though
 * MISC - Improved rich text handling logic
 * MISC - Support generating partial translations through config option `GeneratePartialTranslations`
 * MISC - Support for overriding TextMeshPro fonts through config options `OverrideFontTextMeshPro`, though this requires the game to be build with the specified resource
 * MISC - Support for generating partial translations to better support text that is scrolling in
 * MISC - Support for for older versions of the Unity engine (tested down to 4.5)
 * MISC - Updated Bing API plugin to new API version

### 3.6.1
 * BUG FIX - Substitution would sometimes causes some translations not to be shown the first time around
 * BUG FIX - In the Translation Aggregator-view, sometimes texts would not be translated because the original text could not found
 * MISC - Option to only let the plugin generate translations without variables (GenerateStaticSubstitutionTranslations)

### 3.6.0
 * FEATURE - 'Translation Aggregator'-like view that enables viewing translations for displayed texts from multiple different translators (press ALT+1)
 * FEATURE - Substitution support. Enable dictionary lookup for strings (usually proper nouns) embedded in text to replace them with a manual translation
 * FEATURE - Papago translator support
 * MISC - Removed hard dependency on UnityEngine.UI to support older versions of the Unity engine
 * MISC - Automatically initialize LEC installation path if installed when creating configuration file
 * MISC - Automatically enable experimental hooks when installed in single mod scenario (ReiPatcher) and it is required by the runtime
 * BUG FIX - Fixed bug where LEC was not working when run in a .NET 4.x equivalent runtime

### 3.5.0
 * FEATURE - Harmony 2.0-prerelease support (in order to support BepInEx 5.0.0-RC1)
 * BUG FIX - Fixed a bug where the plugin would sometimes dump textures if 'DetectDuplicateTextureNames' was turned on, even though 'EnableTextureDumping' was turned off
 * BUG FIX - Correct whitespace handling of source languages requiring whitelines between words

### 3.4.0
 * FEATURE - Added capability of plugin to detect textures that shares the same resource name and identify these resources in an alternative way
 * BUG FIX - Fixed an issue with TextMeshPro that could cause text to glitch in certain situations

### 3.3.1
 * MISC - Options to cache results of regex lookups and whitespace differences
 * BUG FIX - Fixed 'WhitespaceRemovalStrategy.TrimPerNewline' which was broken to remove all non-repeating whitespace, rather than only the non-repeating whitespace surrounding newlines
 * BUG FIX - Fully clear translations before reloading (ALT+R)

### 3.3.0
 * FEATURE - Support 'TARC' regex formatting in translation files
 * FEATURE - Much improved handling of whitespace and newlines. Option 'TrimAllText' removed and options for 'WhitespaceRemovalStrategy' changed
 * BUG FIX - Allow hooking of text with components named 'Dummy'

### 3.2.0
 * FEATURE - BepInEx 5.x plugin support
 * CHANGE - Restructured large portions of the internal code to support more features going forward
 * BUG FIX - Interacting with UI now blocks input to game
 * BUG FIX - Better handling of error'ed translations in relation to rich text
 * BUG FIX - Minor fixes to 'copy to clipboard' to disable IMGUI spam
 * BUG FIX - Fixed potential NullReferenceException in GoogleTranslate and BingTranslate during timeout errors
 * MISC - Removed 'Dump Untranslated Texts' hotkey due to feature bloat
 * MISC - Allow unselecting translation endpoint in UI
 * MISC - Increased request timeout from 50 to 150 seconds to ensure better error logging of failed requests

### 3.1.0
 * FEATURE - Support for games with 'netstandard2.0' API surface through config option 'EnableExperimentalHooks'
 * BUG FIX - Bug fixes and improvements to Utage hooking implementation - EnableUtage config option also removed (always on now)
 * BUG FIX - Rich text parser bug fixes when only a single tag with no ending was used
 * BUG FIX - Fixed potential NullReferenceException in TextGetterCompatibilityMode
 * BUG FIX - Load translator assemblies even if a '#' is present in file path
 * MISC - Determine whether to disable certificate checks at config initialization based on scripting backend and unity version

### 3.0.2
 * BUG FIX - UnityInjector installer package now uses correct folder structure (Translators has been moved into Config folder) and ExIni is no longer distributed
 * BUG FIX - Fixed harmony priority usage, which was incorrectly used in 3.0.1
 * MISC - Plugin should no longer translate text input fields for NGUI
 * MISC - Added config option to 'DisableCertificateValidation' for all hosts under all circumstances in case the plugin locks up. This option is only required by very few games
 * MISC - Experimental hooking support for methods with no body (configured through 'EnableExperimentalHooks' setting)
 * MISC - Restructured README file. New order: 1. Installation, 2. Usage, 3. Configuration, 4. Integration 

### 3.0.1
 * BUG FIX - Fixed bug that could in certain situation cause IMGUI translation to drain on performance
 * BUG FIX - Never close a service point while a request is ongoing. Previously this could cause the plugin to lockup
 * BUG FIX - Only disable certificate checks if the .NET version is at or below 3.5
 * BUG FIX - Improved cleanup of object references
 * BUG FIX - Improved 'text stagger' check. Sometimes the plugin was identifying text as scrolling in, even though it was not
 * MISC - Proper test and support for .NET 4.x equivalent scripting backend for Unity
 * MISC - Timeout handling if an endpoint becomes unresponsive
 * MISC - Support post processing for normal text translations
 * MISC - Change harmony text hook priority to 'Last' instead of simply be executed 'after DTL'
 * MISC - More resilient harmony hook implementation to support potentially different versions of harmony being used
 * MISC - Updated harmony version deployed with the plugin (for IPA, ReiPatcher and UnityInjector), so it is no longer the homebrew version that was distributed with BepInEx
 * MISC - Made UI more readable by using a solid background
 * MISC - Changed max queued translations from 3500 to 4000

### 3.0.0
 * FEATURE - UI to control plugin more conveniently (press ALT + 0 (that's a zero))
 * FEATURE - Dynamic selection of translator during game session
 * FEATURE - Support BingTranslate API
 * FEATURE - Support LEC Offline Power Translator 15
 * FEATURE - Enable custom implementations of translators
 * FEATURE - Removed support for Excite translate because it only support the 'WWW' API in Unity due to missing Tls1.2 support
 * FEATURE - Updated Watson translate to v3
 * FEATURE - Support for 'romaji' as output language. Only google supports this at the moment
 * FEATURE - Batching support for all endpoints where the API supports it
 * BUG FIX - Too many small fixes to mention
 * MISC - {GameExeName} variable can now be used in configuration of directories and files
 * MISC - Changed the way the 'Custom' endpoint works. See README for more info
 * MISC - Added new configuration 'GameLogTextPaths' to enable special handling of text components that text is being appended to continuously (requires export knowledge to setup)

### 2.18.0
 * FEATURE - Text Getter Compatibility Mode. Fools the game into thinking that it is not actually translated
 * FEATURE - Textures - Live2D component support
 * FEATURE - Textures - SpriteRenderer component support
 * FEATURE - Hotkey to reboot plugin in certain situations
 * BUG FIX - No longer trim translated text (if configured) when loading translation files
 * MISC - Support legacy OnLevelWasLoaded
 * MISC - Avoid harmless 'log errors' in relation to texture translation
 * MISC - Set max value of MaxCharactersPerTranslation to 500
 * MISC - Documentation update to describe 'Behaviour' configuration section
 * MISC - Removed "FromImageNameThenData" as hash source on textures
 * MISC - Added "FromImageNameAndScene" as hash source on textures
 * MISC - Inline comment handling when using '//' to indicate a comment
 * MISC - Improved default configuration for Utage-related games to improve translation when newlines are involved

### 2.17.0
 * FEATURE - Support legitimate Bing translate API (requires key)
 * FEATURE - Documented custom endpoint
 * BUG FIX - Fixed bug in older versions of the Unity engine where the plugin would crash on startup due to missing APIs in relation to the SceneManagement namespace
 * BUG FIX - Fixed bug that could happen in Utage-based games, that would cause dialogue to sometimes be cut off mid-sentence
 * BUG FIX - Incorrect handling of 'null' default values in configuration
 * BUG FIX - Various other minor fixes
 * MISC - Added configuration option to support ignoring texts starting with certain characters, IgnoreTextStartingWith
 * MISC - Template reparation for IMGUI translations
 * MISC - Big update to README file to fully describe the features of the plugin

### 2.16.0
 * FEATURE - Support image dumping and loading (not automatic!). Disabled by default
 * BUG FIX - Fixed toggle translation which was broken in 2.15.4
 * BUG FIX - Updated TKK retrieval logic
 * MISC - Removed Jurassic dependency as it is no longer required

### 2.15.4
 * MISC - Added configuration option to apply 'UI resize behaviour' to all components regardless of them being translated: ForceUIResizing
 * MISC - Added configuration option to change line spacing for UGUI within the 'UI resize behaviour': ResizeUILineSpacingScale

### 2.15.3
 * BUG FIX - Potential crash during startup where potentially 'illegal' property was being read.
 * MISC - Support 'scrolling text' for immediate components such as IMGUI. Previously such behaviour would shut down the plugin.
 * MISC - Changed behaviour of font overriding. All found Text instances will now have their fonts changed.

### 2.15.2
 * BUG FIX - Fixed bug that could cause hooking not to work when hooks were overriden by external plugin

### 2.15.1
 * BUG FIX - Updated TKK retrieval logic. Improved error message if it cannot be retrieved.

### 2.15.0
 * FEATURE - Manual hooking - press ALT + U. This will lookup all game objects and attempt translation immediately
 * FEATURE - Capability for other plugins to tell the Auto Translator not to translate texts
 * BUG FIX - Initialization hooking that will attempt to hook any game object that was missed during game initialization
 * BUG FIX - Minor fixes to handling of rich text to better support tags that should be ignored, such as ruby, group
 * MISC - Generally better hooking capabilities

### 2.14.1
 * BUG FIX - Never allow text to be queued for translation before stabilization check for rich text 
 * MISC - Improved a spam detection check

### 2.14.0
 * FEATURE - Dramatically improved the text hooking capability for NGUI to much better handle static elements

### 2.13.1
 * BUG FIX - Minor bug fix in rich text parser
 * MISC - Enable Rich Text for TextMeshPro
 * MISC - Improved whitespace handling with additional configuration option

### 2.13.0
 * FEATURE - Support for older Unity Engine versions
 * BUG FIX - Respect BepInEx logger config over own config
 * BUG FIX - Fix exception that could occur in relation to NGUI
 * MISC - Less leniency in what constitutes an error when translating

### 2.12.0
 * FEATURE - General support for rich text in relation to UGUI and Utage
 * FEATURE - Experimental support for custom fonts for UGUI
 * CHANGE - Support only source languages with predefined character checks - for now (ja, zh-CN, zh-TW, ru, ko, en)
 * CHANGE - Slightly different translation load priority from files
 * BUG FIX - Dramatically improved resize behaviour for NGUI
 * BUG FIX - Fixed a bug where hook overrides would not always be honored depending on mod load order
 * MISC - 3 additional spam prevention checks
 * MISC - Uses BepInLogger for BepInEx implementation (requires 4.0.0+)
 * MISC - Redirect "GoogleTranslateHack" to "GoogleTranslate" because instructions were being distributed to use this, and it is not very friendly towards their APIs

### 2.11.0
 * FEATURE - Support for legitimate Google Cloud Translate API (requires key)
 * BUG FIX - Fixed situations where a text would not be translated on a component if a operation is ongoing on the component
 * BUG FIX - Less delay on translation in certain situations
 * MISC - Plugin seeded with ~10000 manually translated texts for commonly used translations to avoid hitting the configured endpoint too much (enable or disable with "UseStaticTranslations")
   * Only applies when configured for Japanese to English (default)

### 2.10.1
 * BUG FIX - Fix to prevent text overflow for large component for UGUI

### 2.10.0
 * FEATURE - Support Yandex translate (requires key)
 * FEATURE - Support Watson translate (requires key)
 * FEATURE - Batching support for selected endpoints (makes translations much faster and requires lesser request)
 * FEATURE - Experimental Utage support
 * BUG FIX - Fixed minor bug during reading of text translation cache
 * BUG FIX - Now escapes the '='-sign in the translation file, so texts containing this character can be translated
 * BUG FIX - Fixed kana check when testing if a text is a candidate for translation
 * MISC - No longer creating a new thread for each translation
 * MISC - Proactive closing of unused TCP connections
 * CONFIG - TrimAllText, to indicate whether whitespace in front of and behind translation candidates should be removed
 * CONFIG - EnableBatching, to indicate whether batching should be enabled for supported endpoints
 * CONFIG - EnableUIResizing, to indicate whether the plugin should make a "best attempt" at resizing the UI upon translation. Current only work for NGUI

### 2.9.1
 * MISC - Added automatic configuration migration support
   * Versions of this plugin were being distributed with predefined configuration to target "GoogleTranslateHack". The first time the plugin is run under this version, it will change this value back to the default.

### 2.9.0
 * FEATURE - Installation as UnityInjector plugin
 * FEATURE - Support Excite translate
 * MISC - Better debugging capabilities with extra config options

### 2.8.0
 * CHANGE - Whether SSL is enabled or not is now entirely based on chosen endpoint support
 * FEATURE - Support for IMGUI translation texts with numbers
 * FEATURE - Support for overwriting IMGUI hook events
 * BUG FIX - Improved fix for gtrans (23.07.2018) by supporting persistent HTTP connections and cookies and recalculation of TKK and SSL
 * BUG FIX - Fixed whitespace handling to honor configuration more appropriately
 * BUG FIX - User-interaction (hotkeys) now works when in shutdown mode
 * MISC - Prints out to console errors that occurrs during translation
 * MISC - IMGUI is still disabled by default. Often other mods UI are implemented in IMGUI. Enabling it will allow those UIs to be translated as well. 
   * Simply change the config, such that: EnableIMGUI=True

### 2.7.0
 * FEATURE - Additional installation instructions for standalone installation through ReiPatcher
 * BUG FIX - Fixed a bug with NGUI that caused those texts not to be translated
 * BUG FIX - Improved fix for gtrans (23.07.2018)

### 2.6.0
 * FEATURE - Support for newer versions of unity engine (those including UnityEngine.CoreModule, etc. in Managed folder)
 * BUG FIX - Fix for current issue with gtrans (23.07.2018)
 * BUG FIX - Keeps functioning if web services fails, but no requests will be sent in such scenario. Texts will simply be translated from cache
 * BUG FIX - Changed hooking, such that if text framework fails, the others wont also fail
 * MISC - Concurrency now based on which type of endpoint. For gtrans it is set to 1
 * MISC - Bit more leniency in translation queue spam detection to prevent shutdown of plugin under normal circumstances

### 2.5.0
 * FEATURE Copy to clipboard feature
 * BUG FIX - Various new rate limiting patterns to prevent spam to configured translate endpoint

### 2.4.1
 * BUG FIX - Disabled IMGUI hook due to bug

### 2.4.0
 * CHANGE - Completely reworked configuration for more organization
 * FEATURE - Added support for BaiduTranslate. User must provide AppId/AppSecret for API . Use "BaiduTranslate" as endpoint
 * FEATURE - Force splitting translated texts into newlines, if configured
 * BUG FIX - Fixed broken feature that allowed disabling the online Endpoint
 * BUG FIX - Eliminated potential concurrency issues that could cause a translated string to be retranslated.
 * BUG FIX - Better support for other 'from' languages than japanese, as the japanese symbol check has been replaced with a more generic one
 * BUG FIX - Fixed a bug where hot key actions (toggle translation, etc.) would often fail
 * BUG FIX - Multiline translations now partially supported. However, all texts considered dialogue will still be translated as a single line
 * BUG FIX - More leniency in allowing text formats (in translation files) to be included as translations

### 2.3.1
 * BUG FIX - Fixed bug that caused the application to quit if any hooks were overriden.

### 2.3.0
 * FEATURE - Allow usage of SSL
 * BUG FIX - Better dialogue caching handling. Often a dialogue might get translated multiple times because of small differences in the source text in regards to whitespace.

### 2.2.0
 * MISC - Added anti-spam safeguards to web requests that are sent. What it means: The plugin will no longer be able to attempt to translate a text it already considers translated.
 * MISC - Changed internal programmatic HTTP service provider from .NET WebClient to Unity WWW.

### 2.1.0
 * FEATURE - Added configuration options to control which text frameworks to translate
 * FEATURE - Added integration feature that allows other translation plugins to use this plugin as a fallback
 * BUG FIX - Fixed a bug that could cause a StackOverflowException in unfortunate scenarios, if other mods interfered.
 * BUG FIX - MUCH improved dialogue handling. Translations for dialogues should be significantly better than 2.0.1

### 2.0.1
 * BUG FIX - Changed configuration path so to not conflict with the configuration files that other mods uses, as it does not use the shared configuration system. The previous version could override configuration from other mods.
 * MISC - General performance improvements.

### 2.0.0
 * Initial release

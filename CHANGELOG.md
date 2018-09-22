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

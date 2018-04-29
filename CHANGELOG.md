### 2.3.0
 * Allow usage of SSL
 * Better dialogue caching handling. Often a dialogue might get translated multiple times because of small differences in the source text in regards to whitespace.

### 2.2.0
 * Added anti-spam safeguards to web requests that are sent. What it means: The plugin will no longer be able to attempt to translate a text it already considers translated.
 * Changed internal programmatic HTTP service provider from .NET WebClient to Unity WWW.

### 2.1.0
 * Fixed a bug that could cause a StackOverflowException in unfortunate scenarios, if other mods interfered.
 * Added configuration options to control which text frameworks to translate
 * Added integration feature that allows other translation plugins to use this plugin as a fallback
 * MUCH improved dialogue handling. Translations for dialogues should be significantly better than 2.0.1

### 2.0.1
 * Changed configuration path so to not conflict with the configuration files that other mods uses, as it does not use the shared configuration system. The previous version could override configuration from other mods.
 * General performance improvements.

### 2.0.0
 * The initial release

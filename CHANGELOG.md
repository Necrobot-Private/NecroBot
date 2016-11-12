# Change Log

## [v1.0.0.16](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.16) (2016-11-11)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.15...v1.0.0.16)

**Fixed bugs:**

- Support for 0.45 API

## [v1.0.0.15](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.15) (2016-11-08)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.14...v1.0.0.15)

**Fixed bugs:**

- Bug fix 2 for http://www.mypogosnipers.com/ and http://msniper.com pokemon disappearing too fast.

## [v1.0.0.14](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.14) (2016-11-08)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.13...v1.0.0.14)

**Fixed bugs:**

- Bug fix for http://www.mypogosnipers.com/ and http://msniper.com pokemon disappearing too fast.

## [v1.0.0.13](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.13) (2016-11-08)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.12...v1.0.0.13)

**Fixed bugs:**

- Change default settings to allow all pokemon to be caught (except for regional pokemon).

## [v1.0.0.12](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.12) (2016-11-07)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.11...v1.0.0.12)

**Implemented enhancements:**

- Only regenerate device info once if device package name is set to "random". 
- Regenerate iOS device info and save to auth.config if using Android device info.

## [v1.0.0.11](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.11) (2016-11-07)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.10...v1.0.0.11)

**Implemented enhancements:**

- Captcha checking - for now we are checking if there is a captcha being required and will display message to user and have the bot exit.
- Disabled human snipe online sources to avoid errors in console
- Allow common pokemon to be sniped again (to avoid getting questions about why they are being skipped).

## [v1.0.0.10](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.10) (2016-11-07)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.9...v1.0.0.10)

**Fixed bugs:**

- Generate random iOS device info for the API if Android info is read from auth.json.  Note that the iOS device settings are not saved.


## [v1.0.0.9](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.9) (2016-11-07)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.6...v1.0.0.9)

**Implemented enhancements:**

- Support for 0.43 API

## [v1.0.0.6](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.6) (2016-09-23)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.5...v1.0.0.6)

**Implemented enhancements:**

- \[Feature Request\] Catch Filter for CP, IVs, Level, Moves [\#239](https://github.com/Necrobot-Private/NecroBot/issues/239)
- \[Feature Request\] Sniper info from https://pokedexs.com/ [\#51](https://github.com/Necrobot-Private/NecroBot/issues/51)

**Fixed bugs:**

- Command-line shows wrong version [\#262](https://github.com/Necrobot-Private/NecroBot/issues/262)
- Error with leveling pokemon [\#243](https://github.com/Necrobot-Private/NecroBot/issues/243)

**Closed issues:**

- http://necrosocket.herokuapp.com/      \(Pokemon despawned or wrong link format!\) [\#264](https://github.com/Necrobot-Private/NecroBot/issues/264)
- Msniper won't create protocol [\#259](https://github.com/Necrobot-Private/NecroBot/issues/259)
- option of the time delay of potencial evolutions check [\#256](https://github.com/Necrobot-Private/NecroBot/issues/256)
- \[Minor Issue\] HumanWalkSnipe Spam on console despite being disabled [\#229](https://github.com/Necrobot-Private/NecroBot/issues/229)

**Merged pull requests:**

- Fixes bug \#262 - Show full version in command line during version check [\#270](https://github.com/Necrobot-Private/NecroBot/pull/270) ([jjskuld](https://github.com/jjskuld))
- Fixes bug \#243 - Fix error while upgrading pokemon [\#269](https://github.com/Necrobot-Private/NecroBot/pull/269) ([jjskuld](https://github.com/jjskuld))
- Catching/Sniping pokemon not respecting DelayBetweenPokemonCatch setting. [\#268](https://github.com/Necrobot-Private/NecroBot/pull/268) ([jjskuld](https://github.com/jjskuld))
- msniper conditions changed [\#265](https://github.com/Necrobot-Private/NecroBot/pull/265) ([msx752](https://github.com/msx752))

## [v1.0.0.5](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.5) (2016-09-22)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.4...v1.0.0.5)

**Implemented enhancements:**

- \[Feature request\] NO looting, JUST catching [\#215](https://github.com/Necrobot-Private/NecroBot/issues/215)

**Closed issues:**

- Bot traveling to gyms despite gyms disabled [\#231](https://github.com/Necrobot-Private/NecroBot/issues/231)

**Merged pull requests:**

- Add ability to prioritize or exclude gyms. Fixes issue \#231 [\#258](https://github.com/Necrobot-Private/NecroBot/pull/258) ([jjskuld](https://github.com/jjskuld))
- Fix MSnipe don't work when use subpath parameter. [\#257](https://github.com/Necrobot-Private/NecroBot/pull/257) ([Prawith](https://github.com/Prawith))

## [v1.0.0.4](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.4) (2016-09-21)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.3...v1.0.0.4)

**Fixed bugs:**

- Human Walk Snipe + GPX = Bot Crashes and gets Exception:       Error: System.NullReferenceException: Object reference not set to an instance of an object. [\#251](https://github.com/Necrobot-Private/NecroBot/issues/251)
- MSniped pokemons are not counted into Catch limit [\#202](https://github.com/Necrobot-Private/NecroBot/issues/202)
- Fix bug \#215 - Continue to walk pokestops when hit pokestop limit [\#255](https://github.com/Necrobot-Private/NecroBot/pull/255) ([jjskuld](https://github.com/jjskuld))

## [v1.0.0.3](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.3) (2016-09-21)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.2...v1.0.0.3)

**Implemented enhancements:**

- MSniper ideas [\#159](https://github.com/Necrobot-Private/NecroBot/issues/159)

**Fixed bugs:**

- Fix bug \#251 - Check for null before setting location name. [\#254](https://github.com/Necrobot-Private/NecroBot/pull/254) ([jjskuld](https://github.com/jjskuld))
- Fix bug \#202 - MSniper sniped pokemon should count against limits. [\#253](https://github.com/Necrobot-Private/NecroBot/pull/253) ([jjskuld](https://github.com/jjskuld))

## [v1.0.0.2](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.2) (2016-09-21)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.1...v1.0.0.2)

**Fixed bugs:**

- Bug fix \#244 - By default do not snipe regional pokemon [\#249](https://github.com/Necrobot-Private/NecroBot/pull/249) ([jjskuld](https://github.com/jjskuld))

**Closed issues:**

- Still Not Filtering Sniping [\#244](https://github.com/Necrobot-Private/NecroBot/issues/244)

## [v1.0.0.1](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.1) (2016-09-21)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v1.0.0.0...v1.0.0.1)

**Fixed bugs:**

- 1.0.0.0 Token expired ... [\#245](https://github.com/Necrobot-Private/NecroBot/issues/245)
- Starting Altitude Value Bug [\#232](https://github.com/Necrobot-Private/NecroBot/issues/232)
- Bug fix \#245 - Update RocketAPI to fix login token refresh. [\#248](https://github.com/Necrobot-Private/NecroBot/pull/248) ([jjskuld](https://github.com/jjskuld))
- Bug fix - Fix \#232 - Default altitude not set from elevation service. [\#247](https://github.com/Necrobot-Private/NecroBot/pull/247) ([jjskuld](https://github.com/jjskuld))

## [v1.0.0.0](https://github.com/Necrobot-Private/NecroBot/tree/v1.0.0.0) (2016-09-21)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.9.9.9...v1.0.0.0)

**Implemented enhancements:**

- Hide socket exception message / stop trying make new connect when there too many failure [\#234](https://github.com/Necrobot-Private/NecroBot/pull/234) ([samuraitruong](https://github.com/samuraitruong))
- msniper works with old and new method [\#223](https://github.com/Necrobot-Private/NecroBot/pull/223) ([msx752](https://github.com/msx752))

**Fixed bugs:**

- Bug checking if pokestop can be farmed [\#213](https://github.com/Necrobot-Private/NecroBot/issues/213)
- 0.9.8.8 hangs after logging in [\#104](https://github.com/Necrobot-Private/NecroBot/issues/104)
- display error message when encounter unsuccessful [\#235](https://github.com/Necrobot-Private/NecroBot/pull/235) ([msx752](https://github.com/msx752))
- Fix MSniper show "Object reference not set to an instance of an object." [\#233](https://github.com/Necrobot-Private/NecroBot/pull/233) ([Prawith](https://github.com/Prawith))
- Bug fix/better prompt error handling [\#227](https://github.com/Necrobot-Private/NecroBot/pull/227) ([jjskuld](https://github.com/jjskuld))
- Change encounter id form double to string to avoid problem with javascript number limit [\#225](https://github.com/Necrobot-Private/NecroBot/pull/225) ([samuraitruong](https://github.com/samuraitruong))

## [v0.9.9.9](https://github.com/Necrobot-Private/NecroBot/tree/v0.9.9.9) (2016-09-19)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.9.9.8...v0.9.9.9)

**Fixed bugs:**

- 0.9.9.8 Error: Microsoft.CSharp.RuntimeBinder.RuntimeBinderException [\#208](https://github.com/Necrobot-Private/NecroBot/issues/208)
- 0.9.8.1 SpeedUp messed up [\#14](https://github.com/Necrobot-Private/NecroBot/issues/14)
- Fix pokestop spin bug \(bug \#213\) [\#214](https://github.com/Necrobot-Private/NecroBot/pull/214) ([jjskuld](https://github.com/jjskuld))
- Bug fix - Console not showing poke stop name. [\#212](https://github.com/Necrobot-Private/NecroBot/pull/212) ([jjskuld](https://github.com/jjskuld))
- Fix \#208 [\#209](https://github.com/Necrobot-Private/NecroBot/pull/209) ([mo0ojava](https://github.com/mo0ojava))

## [v0.9.9.8](https://github.com/Necrobot-Private/NecroBot/tree/v0.9.9.8) (2016-09-18)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.9.9.7...v0.9.9.8)

**Fixed bugs:**

- Building solution got error "The name 'PokemonMoveMetaRegistry' does not exist in the current context". [\#189](https://github.com/Necrobot-Private/NecroBot/issues/189)
- 0.9.9.6 Keep on walking to a pokeshop but it never reach it [\#184](https://github.com/Necrobot-Private/NecroBot/issues/184)
- 0.9.9.6 'POGOProtos.Networking.Responses.DiskEncounterResponse' does not contain a definition for 'WildPokemon' [\#179](https://github.com/Necrobot-Private/NecroBot/issues/179)
- Fix \#179 WildPokemon-error with Incense/Lures [\#206](https://github.com/Necrobot-Private/NecroBot/pull/206) ([mo0ojava](https://github.com/mo0ojava))
- Bug fix for bot skipping pokestops [\#199](https://github.com/Necrobot-Private/NecroBot/pull/199) ([jjskuld](https://github.com/jjskuld))

## [v0.9.9.7](https://github.com/Necrobot-Private/NecroBot/tree/v0.9.9.7) (2016-09-17)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.9.9.6...v0.9.9.7)

**Implemented enhancements:**

- PokeRadar, PokeCrew, and PokeFastMap not available on non-humanwalk [\#132](https://github.com/Necrobot-Private/NecroBot/issues/132)
- Request: Add Buddy Pokemon Support [\#130](https://github.com/Necrobot-Private/NecroBot/issues/130)

**Fixed bugs:**

- no looting [\#101](https://github.com/Necrobot-Private/NecroBot/issues/101)
- updater bug fixed [\#186](https://github.com/Necrobot-Private/NecroBot/pull/186) ([msx752](https://github.com/msx752))
- necrobot2.exe.config: crash bug fixed [\#185](https://github.com/Necrobot-Private/NecroBot/pull/185) ([msx752](https://github.com/msx752))

**Closed issues:**

- \[Bug / Help Wanted\] Command arguments creates jsonvalid folder instead of disabling it [\#142](https://github.com/Necrobot-Private/NecroBot/issues/142)
- Pokesnipers cannot sniper 0.9.9.2 [\#131](https://github.com/Necrobot-Private/NecroBot/issues/131)
- Not generate the LastPos.ini when the program is closed [\#116](https://github.com/Necrobot-Private/NecroBot/issues/116)

## [v0.9.9.6](https://github.com/Necrobot-Private/NecroBot/tree/v0.9.9.6) (2016-09-17)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.9.9.5...v0.9.9.6)

**Implemented enhancements:**

- Priority for snipes from Msniper [\#136](https://github.com/Necrobot-Private/NecroBot/issues/136)
- Some of my recent work [\#174](https://github.com/Necrobot-Private/NecroBot/pull/174) ([samuraitruong](https://github.com/samuraitruong))
- msniper rare pokemon priority+ reconnect delay [\#171](https://github.com/Necrobot-Private/NecroBot/pull/171) ([msx752](https://github.com/msx752))
- msniper:  use berry while using msniper [\#170](https://github.com/Necrobot-Private/NecroBot/pull/170) ([msx752](https://github.com/msx752))
- msniper select bestball [\#169](https://github.com/Necrobot-Private/NecroBot/pull/169) ([msx752](https://github.com/msx752))
- msniper sniped pokemon's HP and LVL [\#168](https://github.com/Necrobot-Private/NecroBot/pull/168) ([msx752](https://github.com/msx752))
- msniper priority + rare pokemon filter [\#167](https://github.com/Necrobot-Private/NecroBot/pull/167) ([msx752](https://github.com/msx752))
- Enhancement/gym [\#156](https://github.com/Necrobot-Private/NecroBot/pull/156) ([HokoriXIII](https://github.com/HokoriXIII))

**Fixed bugs:**

- Duplication in the logs [\#114](https://github.com/Necrobot-Private/NecroBot/issues/114)

## [v0.9.9.5](https://github.com/Necrobot-Private/NecroBot/tree/v0.9.9.5) (2016-09-15)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.6.4...v0.9.9.5)

**Implemented enhancements:**

- Select team automatically, set in config. [\#53](https://github.com/Necrobot-Private/NecroBot/issues/53)
- \(Request\) Mass evolving settings \(Pidgey Hoarding \(\(EXP Trick\)\) [\#41](https://github.com/Necrobot-Private/NecroBot/issues/41)
- Better iOS emulation [\#158](https://github.com/Necrobot-Private/NecroBot/pull/158) ([jjskuld](https://github.com/jjskuld))
- JP - correct translation [\#151](https://github.com/Necrobot-Private/NecroBot/pull/151) ([msx752](https://github.com/msx752))
- GUI enhancements and bug fixes - 1\) Add option to choose a release ve… [\#145](https://github.com/Necrobot-Private/NecroBot/pull/145) ([jjskuld](https://github.com/jjskuld))
- Add CommandLine Args [\#134](https://github.com/Necrobot-Private/NecroBot/pull/134) ([NzV](https://github.com/NzV))
- Add killswitch override prompt.  Now you can override killswitch.  [\#128](https://github.com/Necrobot-Private/NecroBot/pull/128) ([jjskuld](https://github.com/jjskuld))
- Updated to latest POGOProtos \(a01a480\) [\#124](https://github.com/Necrobot-Private/NecroBot/pull/124) ([NzV](https://github.com/NzV))
- Msniper location service - ready [\#108](https://github.com/Necrobot-Private/NecroBot/pull/108) ([msx752](https://github.com/msx752))
- Elevation enhancements - Add mapzen elevation service [\#99](https://github.com/Necrobot-Private/NecroBot/pull/99) ([jjskuld](https://github.com/jjskuld))
- Navigation enhancements - Mapzen strategy added [\#98](https://github.com/Necrobot-Private/NecroBot/pull/98) ([jjskuld](https://github.com/jjskuld))
- Add settings to enable / disable each human walk snipe information source [\#81](https://github.com/Necrobot-Private/NecroBot/pull/81) ([holmeschou](https://github.com/holmeschou))
- MSniper location service - beta ready [\#73](https://github.com/Necrobot-Private/NecroBot/pull/73) ([msx752](https://github.com/msx752))
- Update rocket api for better 0.35 client login emulation [\#71](https://github.com/Necrobot-Private/NecroBot/pull/71) ([jjskuld](https://github.com/jjskuld))
- MSniper request limitation - [\#58](https://github.com/Necrobot-Private/NecroBot/pull/58) ([msx752](https://github.com/msx752))
- Add ability for websocket to manually level up pokemon [\#46](https://github.com/Necrobot-Private/NecroBot/pull/46) ([AndikaTanpaH](https://github.com/AndikaTanpaH))
- MSniper test server [\#45](https://github.com/Necrobot-Private/NecroBot/pull/45) ([msx752](https://github.com/msx752))
- MSniper double G17 [\#44](https://github.com/Necrobot-Private/NecroBot/pull/44) ([msx752](https://github.com/msx752))
- MSniper brodcaster activated [\#43](https://github.com/Necrobot-Private/NecroBot/pull/43) ([msx752](https://github.com/msx752))
- Location service for necrobot [\#39](https://github.com/Necrobot-Private/NecroBot/pull/39) ([msx752](https://github.com/msx752))
- Msx752 dev + NecroBot Special Location Service System [\#35](https://github.com/Necrobot-Private/NecroBot/pull/35) ([msx752](https://github.com/msx752))
- Update README.md [\#23](https://github.com/Necrobot-Private/NecroBot/pull/23) ([Salvationdk](https://github.com/Salvationdk))
- Lure catch and end of trip for human snipe [\#22](https://github.com/Necrobot-Private/NecroBot/pull/22) ([samuraitruong](https://github.com/samuraitruong))
- Update translation.zh-TW.json [\#18](https://github.com/Necrobot-Private/NecroBot/pull/18) ([erickerich](https://github.com/erickerich))
- Update translation.zh-TW.json [\#17](https://github.com/Necrobot-Private/NecroBot/pull/17) ([erickerich](https://github.com/erickerich))
- Update translation.zh-TW.json [\#13](https://github.com/Necrobot-Private/NecroBot/pull/13) ([erickerich](https://github.com/erickerich))
- remove encrypt.dll dependency + code sync [\#9](https://github.com/Necrobot-Private/NecroBot/pull/9) ([NzV](https://github.com/NzV))

**Fixed bugs:**

- \(SERVICE\) CatchError  [\#137](https://github.com/Necrobot-Private/NecroBot/issues/137)
- Json Schema limit error [\#133](https://github.com/Necrobot-Private/NecroBot/issues/133)
- Google Walk not working [\#111](https://github.com/Necrobot-Private/NecroBot/issues/111)
- Bot not collecting at pokestops. [\#90](https://github.com/Necrobot-Private/NecroBot/issues/90)
- FeroxRev login-procedure error [\#87](https://github.com/Necrobot-Private/NecroBot/issues/87)
- Bot does not snipe even with SnipeAtPokestops: true, unless UseSnipeLocationServer: true [\#61](https://github.com/Necrobot-Private/NecroBot/issues/61)
- Human Snipe Walk - TELEPORTS back to GPX track after catching target. Newest Source ATM. [\#55](https://github.com/Necrobot-Private/NecroBot/issues/55)
- Bot keeps catching after 998 pokemons [\#37](https://github.com/Necrobot-Private/NecroBot/issues/37)
- GUI Not Working  [\#31](https://github.com/Necrobot-Private/NecroBot/issues/31)
- build.bat [\#29](https://github.com/Necrobot-Private/NecroBot/issues/29)
- NecroBot2.GUI wont open [\#15](https://github.com/Necrobot-Private/NecroBot/issues/15)
- Not sniping at pokestops [\#7](https://github.com/Necrobot-Private/NecroBot/issues/7)
- 0.9.7.2 System.ArgumentNullException [\#3](https://github.com/Necrobot-Private/NecroBot/issues/3)
- New Bot is doesnt work [\#2](https://github.com/Necrobot-Private/NecroBot/issues/2)
- \(SERVICE\) CatchError  bug mostly fixed [\#153](https://github.com/Necrobot-Private/NecroBot/pull/153) ([msx752](https://github.com/msx752))
- Bug fix - Buffer console log until gui is loaded. [\#147](https://github.com/Necrobot-Private/NecroBot/pull/147) ([jjskuld](https://github.com/jjskuld))
- notify flood fixed [\#144](https://github.com/Necrobot-Private/NecroBot/pull/144) ([msx752](https://github.com/msx752))
- message flood fixed [\#135](https://github.com/Necrobot-Private/NecroBot/pull/135) ([msx752](https://github.com/msx752))
- Fix \#90 - Bot not spin pokestop. [\#100](https://github.com/Necrobot-Private/NecroBot/pull/100) ([samuraitruong](https://github.com/samuraitruong))
- MSniper fix [\#72](https://github.com/Necrobot-Private/NecroBot/pull/72) ([msx752](https://github.com/msx752))
- Fix \#37. [\#65](https://github.com/Necrobot-Private/NecroBot/pull/65) ([mo0ojava](https://github.com/mo0ojava))
- Fixes elevation-related issues [\#64](https://github.com/Necrobot-Private/NecroBot/pull/64) ([mo0ojava](https://github.com/mo0ojava))
- Fix : Teleport when GPX snipe walk back, update MoveTo location socke… [\#57](https://github.com/Necrobot-Private/NecroBot/pull/57) ([samuraitruong](https://github.com/samuraitruong))
- Fix GUI build - Awesomium browser only supports x86 builds [\#26](https://github.com/Necrobot-Private/NecroBot/pull/26) ([jjskuld](https://github.com/jjskuld))
- Fix Updater [\#11](https://github.com/Necrobot-Private/NecroBot/pull/11) ([NzV](https://github.com/NzV))
- Fix submodule [\#10](https://github.com/Necrobot-Private/NecroBot/pull/10) ([NzV](https://github.com/NzV))

**Merged pull requests:**

- merge+sync [\#60](https://github.com/Necrobot-Private/NecroBot/pull/60) ([msx752](https://github.com/msx752))

## [v0.6.4](https://github.com/Necrobot-Private/NecroBot/tree/v0.6.4) (2016-08-03)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.6.3...v0.6.4)

## [v0.6.3](https://github.com/Necrobot-Private/NecroBot/tree/v0.6.3) (2016-08-03)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/0.6.2...v0.6.3)

## [0.6.2](https://github.com/Necrobot-Private/NecroBot/tree/0.6.2) (2016-08-02)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.6.2...0.6.2)

## [v0.6.2](https://github.com/Necrobot-Private/NecroBot/tree/v0.6.2) (2016-08-02)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.6.1...v0.6.2)

## [v0.6.1](https://github.com/Necrobot-Private/NecroBot/tree/v0.6.1) (2016-08-02)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.6.0...v0.6.1)

## [v0.6.0](https://github.com/Necrobot-Private/NecroBot/tree/v0.6.0) (2016-08-02)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.5.0...v0.6.0)

## [v0.5.0](https://github.com/Necrobot-Private/NecroBot/tree/v0.5.0) (2016-07-31)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.4.0...v0.5.0)

## [v0.4.0](https://github.com/Necrobot-Private/NecroBot/tree/v0.4.0) (2016-07-30)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.3.3...v0.4.0)

## [v0.3.3](https://github.com/Necrobot-Private/NecroBot/tree/v0.3.3) (2016-07-29)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.3.2...v0.3.3)

## [v0.3.2](https://github.com/Necrobot-Private/NecroBot/tree/v0.3.2) (2016-07-29)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.3.1...v0.3.2)

## [v0.3.1](https://github.com/Necrobot-Private/NecroBot/tree/v0.3.1) (2016-07-28)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.3.0...v0.3.1)

## [v0.3.0](https://github.com/Necrobot-Private/NecroBot/tree/v0.3.0) (2016-07-28)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.2.1...v0.3.0)

## [v0.2.1](https://github.com/Necrobot-Private/NecroBot/tree/v0.2.1) (2016-07-28)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.2.0...v0.2.1)

## [v0.2.0](https://github.com/Necrobot-Private/NecroBot/tree/v0.2.0) (2016-07-28)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.9...v0.2.0)

## [v0.1.9](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.9) (2016-07-27)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.8...v0.1.9)

## [v0.1.8](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.8) (2016-07-27)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.7...v0.1.8)

## [v0.1.7](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.7) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.6...v0.1.7)

## [v0.1.6](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.6) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.5...v0.1.6)

## [v0.1.5](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.5) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.1.2...v0.1.5)

## [v0.1.1.2](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.1.2) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.4...v0.1.1.2)

## [v0.1.4](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.4) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.1.4...v0.1.4)

## [v0.1.1.4](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.1.4) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.1.0...v0.1.1.4)

## [v0.1.1.0](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.1.0) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.1.1...v0.1.1.0)

## [v0.1.1.1](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.1.1) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.3...v0.1.1.1)

## [v0.1.3](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.3) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.2...v0.1.3)

## [v0.1.2](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.2) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1.1...v0.1.2)

## [v0.1.1](https://github.com/Necrobot-Private/NecroBot/tree/v0.1.1) (2016-07-26)
[Full Changelog](https://github.com/Necrobot-Private/NecroBot/compare/v0.1-beta...v0.1.1)

## [v0.1-beta](https://github.com/Necrobot-Private/NecroBot/tree/v0.1-beta) (2016-07-25)


\* *This Change Log was automatically generated by [github_changelog_generator](https://github.com/skywinder/Github-Changelog-Generator)*

[![Build status](https://ci.appveyor.com/api/projects/status/g1dtrcdnjjbxugw6?svg=true)](https://ci.appveyor.com/project/jjskuld/necrobot/branch/master)
[![Github All Releases](https://img.shields.io/github/downloads/Necrobot-Private/NecroBot/total.svg)](https://github.com/Necrobot-Private/NecroBot/releases)
[![ license](https://img.shields.io/badge/license-AGPL-blue.svg)](https://raw.githubusercontent.com/Necrobot-Private/NecroBot/master/LICENSE.md)

<h1>Necrobot2 is now compatible with 0.73.1 API.</h1>

<p>
Necrobot2 itself is free but now you will need to purchase an API key from Bossland in order to run the bot.
<br/>
See https://talk.pogodev.org/d/51-api-hashing-service-by-pokefarmer for pricing for API keys.
</p>

<a href="">
    <img alt="Logo" src="http://image.prntscr.com/image/b238b63b4f044813a91f772241be8d45.jpg" width="450">
</a>

[![Stories in Ready](https://discordapp.com/api/guilds/220703917871333376/widget.png?style=banner3&time-)](https://discord.gg/7FWyWVp)

<strong><em> The contents of this repo are a proof of concept and are for educational use only </em></strong>

## `Screenshots`

- Electron Web UI App

<img src="http://i.imgur.com/Ph1sU94r.png" width="430">
<img src="http://i.imgur.com/4Dj2RjNr.png" width="430">

- All in One Windows GUI App

<img src="http://image.prntscr.com/image/fd77f0500e4f4a1cb4c8ff78e22b85c4.png" width="430">
<img src="http://image.prntscr.com/image/016259c838da4dfdb334195f0aa47f70.png" width="430">

- Console App

<img src="http://i.imgur.com/z6UfTm8.png" width="430">

## `Getting Started`

Please visit our website [http://necrobot2.com](http://necrobot2.com) to find some tips for setup and running.

<br/>

## Developers and Contributors

### Requirements

To contribute to development, you will need to download and install the required software first.

- [Git](https://git-scm.com/downloads)
- [Visual Studio 2017](https://www.visualstudio.com/vs/whatsnew/) - We are using C# 7.0 code so VS 2017 is required to compile.  VS 2015 or older will not be able to compile the code.
- [.NET 4.7 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=55170&desc=dotnet47)

### Cloning Source Code

Next, you need to get the source code.  This source code repository uses git submodules. So when you clone the source code, you will need to clone recursively:

```
git clone --recursive https://github.com/Necrobot-Private/NecroBot.git
```

Or if you already cloned without the recursive option, you can update the submodules by running:

```
git clone --recursive https://github.com/Necrobot-Private/NecroBot.git
cd NecroBot
git submodule update --init --recursive
```

## Third Party Dependencies

NecroBot uses DotNetBrowser http://www.teamdev.com/dotnetbrowser, which is a proprietary software. The use of DotNetBrowser is governed by DotNetBrowser Product Licence Agreement http://www.teamdev.com/jxbrowser-licence-agreement. If you would like to use DotNetBrowser in your development, please contact TeamDev.

## [Credits](http://pastebin.com/Yh4ynXbv)

## `Legal Disclaimer`

This Website and Project is in no way affiliated with, authorized, maintained, sponsored or endorsed by ANYONE. This is an independent and unofficial project for educational use ONLY. Do not use for any other purpose than education, testing and research.

<h2>Using this project for anything other than education, testing or research is not advised.</h2>

This is not a public release, just a project with releases for code testers for education and research on any privacy issues for the end user.

<hr/>
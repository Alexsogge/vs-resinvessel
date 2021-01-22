# vs-resinvessel
Mod for the game vintage story. It adds a small vessel, which can be attached to leaking logs to collect resin. Resin is collected automatically, so you don't have to collect it immediatly. 
Instead you can gather it all together after several ingame weeks, or whenever you want to. At least until one stack of resin is not reached :wink:

Build status: [![CircleCI](https://circleci.com/gh/Alexsogge/vs-resinvessel.svg?style=shield )](https://circleci.com/gh/circleci/circleci-docs)

![vessel on log](docs/images/emptyvessel_resinlog.png?raw=true | width=100) ![vessel filled](docs/images/filledvessel.png?raw=true | width=100)

## How to install

1. Grab the `ResinVessel.zip` from the latest release here https://github.com/Alexsogge/vs-resinvessel/releases
2. Put it in your local Mods folder
2a. If you are playing on a remote server you have to put it in the servers mods folder too

## How to develop

Just download the game or gameserver (which can be downloaded from here: https://cdn.vintagestory.at/stable/vs_server_1.14.5.tar.gz if needed replace the version)

Copy all dlls from the gameroot and the folders 'lib' and 'Mods' to the 'lib' folder of this repo.

After that, any dotnet IDE should find everything you need to develop this mod.

If you want to just build this mod, just run the Dockerfile which does this all automatically.

## Disclaimer

This mod is a work in progress. We do not want to harm you or your game, but cannot guarantee that it will not break anything. We want to stay compatible with earlier versions of this mod, but also cannot guarantee to don't break compatability.


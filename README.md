# Temnorid: Detective (Темнорід: Детектив)

**Four sacks of grain are gone and the night watchman is dead. Who do you believe, or do you say we do not know?** A short, fixed-camera detective adventure for **Windows, Android and iOS**, to play alone or as two investigators, in the world of [Temnorid](https://temnorid.com), built on the [IT Kamianets 3D Engine](https://3d.itkamianets.com) with Unity 6000.6. The game's text is in Ukrainian.

You walk through the village of Greyford, inspect the places where things happened, question the people who might know something, compare what they said with what you found, and then tell the elder whom you report, or that the evidence is not enough. **You can be wrong, and a wrong report is a normal ending**, not a game over. Nothing in the game ever tells you whether you were right. First playthrough about 15–25 minutes.

**Status:** version 0.2.0 (shared shell, light co-op, phone controls, self-updating Windows build). Website: https://detective.temnorid.com · Download: [Temnorid-Detective-Setup.exe](https://github.com/WebArtWork/detective.temnorid.com/releases/latest/download/Temnorid-Detective-Setup.exe).

> The case's hidden truth is developer-only (`design/CASE_01.md`). This README, the website and the game never reveal it.

## How it plays

| Area | What there is |
|---|---|
| Camera | Fixed cinematic shots in four zones (granary yard, square, watch post, ford), 7 shots in all, cut or glide at borders. Chosen by volumes with a priority and a 0.3 m hysteresis, so standing on a border never flickers. |
| Movement | WASD or arrows, Shift to move faster. Movement is relative to the camera as it was when the key went down, so a cut never turns held input around (tested across two shots that face different ways, in both directions). |
| Inspecting | One key (E), one prompt at a time, only for what is in plain view within about 2 m. Ten clues. A clue is recorded the first time and can be read again for free. |
| Conversations | Five people, hand-written and deterministic. Topics can be asked again (the answer is the same and nothing is recorded twice). Four follow-up questions appear only after you have found the clue they depend on, and they never name a clue you have not seen. |
| Case board | Four tabs: Evidence (with pins), People (profiles, statements, discrepancies), Timeline (each time marked as observed, reported or estimated, and uncertain where it is), Decision. A "discrepancy" is only two things that do not agree; the board never says who is lying and shows no guilt percentage. |
| The report | Accuse one of three suspects or report insufficient evidence, optionally naming up to three clues as reasons. A neutral warning says the decision can be wrong, you confirm once, and it cannot be undone in that run. You can report after looking at any one clue at the granary; you do not have to find everything. |
| The ending | The village's response to your *report* (what it does), never a verdict on the truth. Four authored endings. Then Replay (a fresh case) or the menu. |
| Saving | One local slot, written atomically after new clues, new statements, a change of place and the report. Continue puts you back where you were (never inside a wall). A damaged save is reported and you can start a new case. |
| Online | Two investigators in one village (see "Playing with others"). No Lords connection: the Lords incident is a local stand-in (the result screen says plainly that nothing was sent or changed). The mock report has no field for the real culprit or for "correct". |

### Controls

| Action | Keys |
|---|---|
| Move | WASD or arrow keys |
| Move faster | Left Shift |
| Inspect / talk | E |
| Journal | J |
| Case board | Tab |
| Back, close, pause | Esc |
| Menus, topics and the board | Mouse, or arrows and Enter |

On a phone: the left thumb stick walks, **Action** inspects or talks, **Run** goes faster, **II** pauses; the journal and the case board are places on the bar at the bottom.

## Playing with others

From the Menu scene choose **Play with others**: one player hosts (direct address, or the relay code), the other joins, and both load the village. Two investigators at most; nobody needs the other to go on.

- **The host owns the case**: the clues, the statements, the discrepancies, the pins and the report. Each investigator walks on their own machine, with their own fixed camera, and is shown to the other from the pose it publishes (about 15 times a second).
- **Inspecting, asking and pinning are requests to the host.** The host checks that the clue or the person exists, that the guest stood close enough when it asked (its last pose, with a little slack) and that the topic is open, then records it. The discovered state (what a save file holds: found clues, places seen, topics spoken, statements, discrepancies, pins, the clock) is sent to both, whenever it changes and every two seconds, so both notebooks agree. The hidden truth of the case is not part of it and never travels.
- **The report is the host's.** The guest sees the decision board and can lean toward a choice, which the host sees as a line; only the host confirms. The ending is then shown to both. Starting the case over is the host's too, and sends the guest back to the brief.
- **An online case is never saved**, so a guest's own offline case is left alone. If the guest leaves, the host goes on alone; if the host leaves, the guest returns to the menu.

## Updates

The Windows game updates itself: at start-up the Boot scene looks at the newest GitHub release of `WebArtWork/detective.temnorid.com` and, if it is newer, runs its installer silently and starts the game again. The Profile place can turn this off.

## Phones

Android and iOS builds come from Unity Build Profiles (the scene list is Boot, Menu, Village; landscape; Android minimum API 26, 64-bit IL2CPP; iOS 15). The stick, the Action, Run and Pause buttons appear only on phones and tablets, texts are enlarged there, and the shell's bars respect the safe area. Not yet built or tried on a device.

## Run it

1. Open the folder in Unity Hub with Unity **6000.6** (the project references the shared Temnorid packages by local path: `file:../../3d-unity-*`, so keep it next to `D:\work\temnorid\` and those folders).
2. Menu **Temnorid > Detective > Build Scene** rebuilds `Assets/Scenes/Village.unity` from primitives (it is generated, do not edit it by hand), then press Play.

From the command line:

```bash
Unity.exe -batchmode -nographics -projectPath . -executeMethod Temnorid.Detective.EditorTools.DetectiveSceneBuilder.Build -quit
Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform EditMode
Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform PlayMode
Unity.exe -batchmode -nographics -projectPath . -executeMethod Temnorid.Detective.EditorTools.DetectiveReleaseBuilder.BuildWindows -quit
"ISCC.exe" installer\Detective.iss
```

(Do not pass `-quit` together with `-runTests`.) The build is `Builds/Windows/TemnoridDetective/TemnoridDetective.exe` plus a zip; the installer is `Builds/Installer/Temnorid-Detective-Setup.exe`. See `TEST_REPORT.md` for what the tests expect and what was observed.

## Layout

```text
Assets/Scenes/                  Boot and Menu (the shared shell), Village (the one game scene: start screen, brief, village and screens, see "Differences")
Assets/Scripts/                 DetectiveShell (shell configuration, places), DetectiveNet and RemoteInvestigator (light co-op), TouchStick (phones), DetectiveRules (the case content and the pure rules: Investigation, conflicts, verdict, save data, mock report),
                                VillageLayout (every position), Loc (all text), DetectiveGame (the case as played),
                                DetectiveHud (screens), Player, Npc, Interactable, Figure, DetectiveAudio
Assets/Scripts/Editor/          DetectiveSceneBuilder, DetectiveReleaseBuilder
Assets/Shaders/WorldText.shader depth-tested text for the name tags
Assets/Tests/                   EditMode and PlayMode
design/                         the design documents this game was built from (GAME, MVP, LORDS, CASE_01, AGENT, overview)
docs/index.html, docs/CNAME     the website (detective.temnorid.com)
installer/Detective.iss         Inno Setup script
```

Packages (see `Packages/manifest.json`): the shared `3d-unity-shell`, `3d-unity-net` and `3d-unity-update` (local paths `file:../../`), and `3d-unity-core`, `3d-unity-input` (the only input entry point; the extra actions `Journal` and `Board` are added with `AddMap`), `3d-unity-save` (atomic save), `3d-unity-ui`, `3d-unity-synth` (the generated sounds), `3d-unity-graybox`, `3d-unity-feedback`, `3d-unity-combat` (only because feedback depends on it) and **`3d-unity-fixed-camera`** (shot volumes with priority and hysteresis, held-input basis, close-up override), which the Escape game already uses. No `3d-detective-*` package exists; all story logic is in this game.

## Gap report and differences from the guides

- The fixed-view constructor repository and the live package catalog could not be read (no network access in the build environment), so nothing was assumed from them. The camera package above is game-neutral and has no story code in it. `3d-unity-character`, `3d-unity-audio`, `3d-unity-assets` and `3d-unity-scene` are not used: the player is Unity's `CharacterController`, the sounds are generated in code, and the scene is built from primitives. Escape is not a dependency (only the camera package is shared).
- The guides name `Main.unity` as the entry and `Village.unity` as the gameplay scene. Here there is **one scene, `Village.unity`**, with the title screen as an overlay on it (the opening and closing shots use the camera's close-up override), as in the other Temnorid games.
- The village is one walkable area: the yard of the granary, the square, the watch post and the ford are joined by paths, with nothing locked.
- Texts are Ukrainian only (English is optional in the guide); every line is in `Loc.cs`. Clue and person ids are logical and never change with the language.
- Stefan has no dialogue; his body is the injury clue (e03). Everything is made of primitives with stable asset ids (`AssetIdTag`); the site's `/models` page lists them so they can be replaced with generated models later.

## Not verified / limitations

- Played with a bot through the real scene and looked at in the editor (title, brief, shots in all four zones, dialogue, clue and board screens, the result screen); not played by a human from start to finish with a real keyboard, and the Windows installer was built but not installed.
- No gamepad, no English text, no voice or music; the sounds are simple generated tones and a river-and-murmur ambience. Characters are blocks that turn to face you, with a swinging walk.
- Co-op was tested with a host on the real scene and a second session in the same process; not yet played by two people on two machines or over the relay. Phones are not tested on a device.
- Online: no Lords connection. A downloadable offline game can be modified, so nothing here may count as proof of anything in Lords; see `design/LORDS.md`.

## Deferred backlog

More cases (burglary, missing person, sabotage, embezzlement), accomplices, false confessions and planted clues, village variants and a case generator owned by Lords, voice and animated cutscenes, an English dictionary and other languages, gamepad, trials and appeals, magic that helps but never guarantees truth, real Lords synchronisation (a sanitised case projection in, a non-binding report out).

## Release

Windows installer or zip attached to a GitHub Release, and the "Download" button on `docs/index.html` pointing at that asset. The first release (v0.1.0) is published and the button points at its installer. Version 0.2.0 is the next release; a build made with this version only plays with builds of the same major.minor.

## Development notes

- Authority: rules that decide the outcome (the investigation, the report) run only where `NetSession.IsAuthority` is true (offline and on the host); `DetectiveGame.IsGuest` is true on the joiner, whose notebook is a mirror (`ApplyMirror`). Single player is unchanged when no session is online.
- The online code is `DetectiveNet.cs` (events `dt.state`, `dt.hi`, `dt.inspect`, `dt.ask`, `dt.pin`, `dt.lean`, `dt.no`) and the online test is `Assets/Tests/PlayMode/OnlineDetectiveTests.cs` (direct join on test port 7808).
- The shell is configured in `DetectiveShell.cs` (game id `detective`, places Case, Journal, Board and the shell's Profile).

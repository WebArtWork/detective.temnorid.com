# Temnorid: Detective — Instructions for Local AI Agent

## 1. Authority and setup

Use the real source repository and installed editor information, **not guessed APIs**.

1. Read the game repository's `AGENTS.md` if it exists.
2. Read the upstream [fixed-view constructor](https://github.com/IT-Kamianets/3d-constructor-fixed-view) and its current `AGENTS.md`, `README.md` and `Packages/manifest.json`.
3. Review https://3d.itkamianets.com/packages.md and the constructor's documentation where accessible.
4. Use the editor version required by the existing source/lockfiles (the previous Escape guide identified **Unity 6000.6 + URP**, but verify this again). Do not migrate engines just because a newer version exists.
5. Preserve existing project guidance; link or merge this document without silently discarding upstream requirements.
6. Prefer starting from the fixed-view constructor; **do not add `3d-constructor-character-view` or free camera for this MVP**.

Product name: `Temnorid: Detective`. Working identifier: `com.itkamianets.temnoriddetective` (validate against workspace naming standards). Target: Windows standalone, offline, version `0.1.0`.

## 2. Dependencies and ownership

Reuse only packages that **actually exist**, following their published contracts:

- `3d-unity-core`: initialization / common services.
- `3d-unity-scene`: scene representation if appropriate.
- `3d-unity-input`: **single entry point for keyboard/mouse/gamepad input**; no ad hoc device polling in game systems.
- `3d-unity-character`: playable character movement, if API supports it.
- `3d-unity-assets`: asset loading/references where present.
- `3d-unity-audio`: sound events where present.
- Constructor-specific fixed-view camera and controls packages: inspect and build minimal generic versions **only if the constructor requires them and they are not already implemented**.

Package names above describe intended responsibilities, **not verified C# APIs**. Inspect the sources before making adapters. Do not invent package namespaces, methods or manifest dependencies. If a package is missing, document the exact blocker, then build the minimum reusable contract only when required by the constructor's contribution policy. Keep all detective story logic inside this game, never inside shared 3D packages.

**Do not create a `3d-detective-*` library or schema package.** No separate backend, no custom engine fork, no Firebase, no paid assets, no mandatory generated 3D models. Existing Escape work may be referenced as a design comparison but is **not a dependency**; avoid tight coupling between games.

## 3. Proposed repository layout

```text
<game-root>/
├── AGENTS.md                       # Existing guidance + link to these docs
├── README.md                       # Open/run/build/test instructions
├── GAME.md                         # This project's gameplay guide
├── MVP.md                          # Acceptance criteria + implementation specification
├── CASE_01.md                      # Developer-only case truth and content
├── LORDS.md                        # Mock/future central-world integration
├── Assets/
│   ├── Scenes/Main.unity           # Bootstrap / title entry
│   └── TemnoridDetective/
│       ├── Scenes/Village.unity     # One contiguous playable village scene
│       ├── Scripts/                # Game-specific state, evidence, dialogue, verdict
│       ├── Content/                # Case data, texts, stable IDs, (ScriptableObjects/JSON optional)
│       ├── Prefabs/                # Investigator, NPCs, props, interactables
│       ├── Materials/              # Low-cost simple materials
│       ├── UI/                     # HUD, evidence journal, case board, accusation screen
│       ├── Audio/                  # Optional original/licensed effects
│       └── Tests/                  # EditMode + PlayMode
├── Packages/manifest.json
├── ProjectSettings/
└── docs/index.html                # Public explanation + GitHub Releases download
```

Adjust to existing project conventions, and explain differences in README. Let Unity author `.meta` files. Never commit `Library/`, `Temp/`, generated builds or package-clone directories. Never publish developer-only case truth as public marketing copy or in-game hints.

## 4. Code architecture (game-specific only)

Suggested logical modules, which may be implemented as plain C# classes and a small number of MonoBehaviours:

| Responsibility | Contract |
|---|---|
| `InvestigationSession` | Single source of truth for visited zones, collected clues, conversations, discovered contradictions and current verdict state |
| `FixedViewZoneAdapter` | Uses the constructor's actual fixed camera API; zero story logic |
| `PlayerInteraction` | Finds one actionable target in range, with occlusion check and stable ID |
| `EvidenceRegistry` | Case-specific clue definitions, inspection text, location, availability, linked topics |
| `DialogueController` | Deterministic branching dialogue; topics unlock through knowledge; no LLM/NPC calls |
| `CaseBoard` | Read-only presentation of learned facts + player-created hypotheses; never secretly adjudicates truth |
| `TimelineView` | Displays clues with **reported** versus independently verified times clearly distinguished |
| `VerdictService` | Validates choice, commits one immutable verdict per case run, produces correct/incorrect/undetermined results internally |
| `SaveService` | Serializes stable IDs and plain data, including conversation progress, without Unity references |
| `LordsInvestigationAdapter` | Mock launch context and mock outcome for MVP; future integration boundary only |

Use static authored data for 1 case. Avoid overengineering a generic detective editor, agentic dialogue, procedural testimony, relationship simulations, economy framework or reusable 3D plugin. ScriptableObjects or a small serialized data table are acceptable if they reduce content duplication; choose whichever integrates with the existing Unity project.

### State invariants

- All evidence has immutable unique `evidence_id`; inspection records discoveries only once.
- Dialogue topics and re-questioning are repeatable; they **must not change the case truth**.
- NPC statements do not automatically become facts. Store a clear `source_type = observation | testimony | record | inference` for case board display.
- Evidence may be combined conceptually in the case board, but no required item consumption/destruction.
- Player can finish the case with **partial evidence**; do not demand 100% completion.
- Once the verdict is submitted, mark `case_status = resolved` and lock in the outcome for that run. Replays start a new session.
- The case's hidden culprit and timelines are never overwritten by the player's selection.
- No on-screen "90% guilty", glowing true suspect, auto-solved deductions, or guaranteed verdict preview.

## 5. Implementation sequence (do not start with art)

**Phase 1 — Constructor health:** verify project imports, compiles and loads one simple zone. Confirm required engine packages and fixed-view input/controls.

**Phase 2 — Character & cameras:** build primitive village blockout, walking, collider walls, fixed camera zones, cut/blend, interaction prompt. Test movement when crossing opposing viewpoints.

**Phase 3 — Clue system:** implement evidence inspection; register the ten items from CASE_01; journal updates immediately; reopening items never duplicates evidence.

**Phase 4 — Conversations:** implement five NPCs, named speaker panels, topics/repeat, evidence-conditioned follow-up questions, contradictions without marking witness as definitely lying.

**Phase 5 — Case board:** suspect profiles, timeline, evidence list, contradictions/pinned notes; make it usable with mouse and keyboard.

**Phase 6 — Verdicts:** accuse one suspect or abstain, show confidence-neutral confirmation, resolve to one of four outcomes, ensure false conviction is a fully valid ending.

**Phase 7 — Persistence:** New Game, Continue, autosave, replay, corrupt-save recovery, settings.

**Phase 8 — Tests + build:** automated rule tests and PlayMode camera/screen smoke checks; Windows player; `docs/index.html`; GitHub Release download instructions; no broken links.

The playable story is more important than exhaustive polish. Use primitive shapes for houses, humans, storage sacks and wagon. No detailed face animations or voice acting required. For minimum effort, dialogue can be text boxes; NPCs may idle/turn toward the player.

## 6. Agent deliverables

- Working Unity project + reproducible Windows standalone build process.
- Case 01 implemented exactly enough to satisfy the player outcomes in `CASE_01.md`.
- README with prerequisites, editor version, run/build/test commands, and known limitations.
- `/docs/index.html` with story summary, controls, features, screenshots (optional), **a direct download button pointing to a real asset on the project's GitHub Releases**. Do not invent an asset URL; use a configurable placeholder until the release exists. The page should make the download visible without using source-code checkout instructions as a substitute.
- Tests and a compact test report documenting expected and observed behavior.
- A short backlog of deliberately deferred enhancements (more cases, dynamic village variants, voice, magic, real Lords sync).

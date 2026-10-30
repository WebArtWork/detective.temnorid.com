# Temnorid: Detective — MVP Engineering Specification

## 1. Goal and non-goals

Ship a **complete, offline, 15–25 minute investigation** with free exploration and **four valid report outcomes**. The goal is a playable detective game, not an extensive generic investigation framework.

**MVP includes:** one case, one contiguous village level, fixed cameras, movement, inspections, ten evidence items, five named NPCs, 3 suspects, interviews, evidence-conditioned dialogue, a case board, a decision screen, four authored epilogues, local saving and Windows release.

**MVP excludes:** real Lords API integration, multiplayer, player-to-player accusations, economic simulation, combat, voice acting, procedural case generator, AI conversations, server authentication, asset marketplace, custom backend, full court system, automatic suspect guilt scoring, complex RPG inventory, character leveling, magic for truth detection.

## 2. Required screens and UI

| Screen / state | Required behavior |
|---|---|
| Title | `New Case`, `Continue` (only if valid save), `Settings`, `Quit` |
| Intro / case brief | Who, where, what's missing, objective, `Begin Investigation` |
| Village HUD | Current high-level objective, contextual interaction prompt, `Journal [J]`, `Case Board [Tab]`, `Pause [Esc]`; never permanent suspect marker |
| Evidence inspect | Object label, readable description, optional simple 3D closeup, `Record / Close`; avoid trapping movement input |
| NPC dialogue | Speaker, dialog text, choices, available follow-up topics, leave/repeat; keyboard/mouse operable |
| Journal | Clues, locations, statements, `observed / reported` source labels; unvisited clues not spoiled |
| Case board | Evidence, People, Timeline, Decision tabs; contradictions and optional pinning; **no guilty percentages** |
| Report / confirmation | Choose `Orest`, `Mira`, `Taras`, or `Insufficient evidence`, optional reasons, explicit irreversible confirmation |
| Result / epilogue | The village's consequence for that **decision**, report summary, elapsed time, discovered evidence count, `Replay`, `Main Menu` |
| Pause | Resume, settings, restart investigation, main menu |
| Error fallback | If save corrupt/incompatible, explain and offer fresh run; avoid crashing |

Keep UI strings centralized. Ukrainian UI is preferred for this project; a small EN fallback dictionary is optional. Data source text must be localizable without changing logical IDs.

## 3. Player input and movement

Only use the verified `3d-unity-input` abstraction; inspect the real package before implementation. Do not bypass it with scattered `Input.GetKey*` / raw polling in game rules.

| Action | Suggested keyboard/mouse |
|---|---|
| Move | `WASD` / arrow keys |
| Move faster | Left Shift (optional, no stamina system) |
| Interact / inspect / talk | `E` |
| Journal | `J` |
| Case board | `Tab` |
| Pause / back / close modal | `Esc` |
| Dialogue choice / board navigation | Mouse click + keyboard tab/arrows + Enter |
| Camera control | **None** in normal world; fixed shots switch automatically |

Suggested tuning: walking speed ~2.5 m/s, optional faster walk ~4.0 m/s; interaction radius ~2 m; camera blend 0.2–0.4 s or hard cut, zone hysteresis ~0.3 m. These are defaults, not hard contractual engine requirements; measure for usability.

**Fixed-camera input continuity:** movement direction must be based on a stable world-relative input scheme or preserve an intended direction across a cut. If different shots face opposite directions, holding a movement key must not unexpectedly reverse the player. Test cut boundaries in both directions.

## 4. Single-scene level and NPC placement

Gameplay scene `Village.unity` contains:

1. **Granary / back gate**: evidence `e01`–`e04`, NPC Mira, victim investigation prompt (non-graphic), intro objective.
2. **Village square / market**: evidence `e05`–`e06`, NPCs Orest and Ada, Elder Halyna (report hub).
3. **Watch post**: evidence `e07`, `e10`, NPC Taras.
4. **Riverside ford**: evidence `e08`–`e09`; small cart/river props.

Use trigger zones for **at least six cinematic shot positions** across four areas. Minimum: 2 granary, 2 square, 1 watch post, 1 ford. Camera transition must not allow walking through walls, selecting off-screen NPCs through geometry, or incorrectly selecting targets in other rooms.

Keep all areas walkable before verdict. Simple box houses, cylinders, plank doors, cart wheels, river plane and flat NPC figures are enough for first playable build. Do not let expensive prop production block logic.

## 5. Game-state model (suggested internal contract)

Use typed classes/records and serialized versioned data. Names below are illustrative **game-side schema**, not a claim about any 3D package.

```text
InvestigationSave {
  save_version: 1
  case_id: "grain_01"
  case_status: "active" | "resolved"
  player_position: (x,y,z)
  player_heading: number
  active_camera_zone_id: string
  discovered_evidence_ids: set<string>
  visited_zone_ids: set<string>
  spoken_topic_ids: set<string>
  unlocked_statement_ids: set<string>
  discovered_conflict_ids: set<string>
  pinned_evidence_ids: list<string>
  case_notes: string?  // optional local only
  case_objective_id: string
  elapsed_seconds: number
  verdict: null | { kind: "accuse" | "insufficient", accused_suspect_id: string? }
  outcome_id: string? // only once resolved
  context_fixture_id: string // e.g. lords_mock_grain_theft
}
```

Truth (culprit and innocence) resides in protected **authored case definition** used by rules/tests, not player editing. **Do not make this mock schema a shared public network contract** or claim that keeping hidden Unity content is a security guarantee. The real Lords side must decide and validate authority independently.

### Required content IDs

Case: `grain_01`  
Zones: `granary`, `square`, `watch_post`, `ford`  
Suspects: `orest`, `mira`, `taras`  
Other NPCs: `ada`, `elder_halyna`  
Evidence: `e01_broken_strap`, `e02_torn_sacks`, `e03_watchman_injury`, `e04_stock_ledger`, `e05_cart_permit`, `e06_market_cargo_tally`, `e07_guard_log`, `e08_ford_tracks`, `e09_crossing_docket`, `e10_stefan_note`  
Conflicts: `c_orest_alibi`, `c_mira_ledger`, `c_taras_log`, `c_unregistered_cargo`, `c_orest_route`.

Other IDs needed are specified in `CASE_01.md`. Do not duplicate canonical case facts in multiple scripts: keep content centralized where practical.

## 6. Interaction contract

Each interactable must have:

- Stable `interactable_id`, display label, interaction kind (`inspect`, `talk`, `report`), an availability predicate, range, and a visual target/collider.
- One prioritized active HUD prompt: focused + unobstructed candidates before nearest.
- No interaction through blocking walls. Interaction surfaces should be forgiving; small clues can be hit via larger proxy colliders.
- On inspection, emit one evidence-discovered event if new; reinspection opens text but never increments a count twice.
- On NPC interaction, fetch topics available based on recorded evidence and conversation state; hide locked topic titles that would spoil facts.
- On report, check intro/scene inspection requirement and case not already resolved.

Evidence is **recorded**, not consumable. A clue remains re-readable after registration. Never require clicking the exact pixel of a tiny document.

## 7. Dialogue rules and contradictions

Dialogue is deterministic: `topic_id + known_clue_set → response + statement_id + maybe unlock new topic`. Each NPC has initial topics. At least these gated followups are required:

- Mira + `e04_stock_ledger` → admits spoiled-stock alteration (`c_mira_ledger`).
- Orest + `e09_crossing_docket` → admits going near ford (`c_orest_alibi` if prior parked-cart account is known).
- Taras + `e07_guard_log` → admits leaving watch (`c_taras_log`).
- Ada + `e06_market_cargo_tally` → clarifies cart was not logged.

Also support re-questioning NPCs after learning evidence. Conflicts are derived when **both** required source facts are known (regardless of discovery order). A `conflict` is merely an inconsistency—never a certified lie or automatic guilt deduction.

Statements should be preserved, time-ordered by discovery, and source-labelled. The board must never silently revise an older statement as if it never existed.

## 8. Case board / timeline usability

**Evidence tab:** Every discovered item shows ID-derived title, location, summary, category. Unfound evidence is either omitted or counted anonymously (`6 of 10`)—never named spoilers.

**People tab:** list of subjects, seen statements and contradictions; neutral descriptors like `Account changed` rather than `Suspect caught lying`.

**Timeline tab:** chronology approximations with a source tag. Player must distinguish witness-estimated times and record timestamps; an approximate memory should not become a precise observation. Sort by claimed time if possible, note `uncertain`.

**Decision tab:** suspect profile excerpts + insufficient-evidence option. Optional player choice of 1–3 supporting clue IDs; this is report flavor only, not a solution gate.

**Avoid automated certainty:** no percentage confidence, hidden guilty icon, glowing true suspect, case-board autosolver or definitive provenance from a single visual clue.

## 9. Verdict state machine and outcome rules

```text
MAIN_MENU
  -> CASE_BRIEF
  -> INVESTIGATING
       -> EXAMINING_EVIDENCE -> INVESTIGATING
       -> INTERVIEWING -> INVESTIGATING
       -> REVIEWING_CASE_BOARD -> INVESTIGATING
       -> REPORT_CONFIRMATION -> INVESTIGATING  (Cancel)
       -> REPORT_COMMITTED  (Confirm only after at least 1 granary clue)
  -> CASE_RESOLVED
       -> REPLAY_NEW_SESSION | MAIN_MENU
```

At `REPORT_COMMITTED`, persist the chosen option **atomically** and refuse double-submission. Mapping:

| Verdict | `outcome_id` | Correctness internally | Default village response |
|---|---|---|---|
| Accuse Orest | `reported_orest` | True | Orest detained for inquiry; follow-up needed |
| Accuse Mira | `reported_mira` | False | Mira detained; grain rationing disrupted |
| Accuse Taras | `reported_taras` | False | Taras removed from duty; patrol capacity reduced |
| Insufficient evidence | `unresolved` | Null / not applicable | No immediate arrest; village increases watch |

**No "You were wrong" popups.** A wrong report must end the case normally. The village's response is not proof of the allegation. Accusing the right person likewise does not magically settle a judicial process.

For testing, internal outcome correctness can be compared to the authored culprit, but **never transmit `internal_is_correct` as user-controlled truth to Lords**.

## 10. Persistence and failure modes

- One local save slot, autosave after new evidence, dialogue unlock, zone transition and verdict. Saving on simple state changes is cheap; debounced writes optional.
- Version the serialized save. Support missing/corrupt data with clear fallback, never load Unity object references as serialized state.
- Store enough to restore location, camera zone, evidence, dialogue, journal, outcome and elapsed time. Do not restore player inside walls or interaction props.
- `Continue` resumes the saved investigation if active; if resolved, it can reopen its result screen or offer Replay.
- New Case/Replay must reset **all** case-run data including dialogue, contradictions, notes and verdict.
- On interrupted write, use atomic temp-file replacement when supported or a safe backup strategy.
- Offline clock must not influence case truth or make testimony expire.
- No live accounts, API keys or external network writes.

## 11. Audio / presentation minimum

- Visually distinct four zones; atmospheric village/river sound; minimal UI and inspect effects.
- Non-graphic body outline/covered figure rather than detailed death imagery.
- NPCs face the investigator / simple idle; text dialog works without character voice or lip sync.
- Required text remains readable on 1080p Windows screen and is available with sound muted.
- Use clean captions for events; no forced timed dialogue, no reliance on stereo hearing for clues.

## 12. Automated tests (required)

### EditMode / pure-logic

1. Registering an already known evidence ID does not duplicate the clue or reward.
2. Evidence-conditioned dialogue topic remains locked until its evidence exists, then unlocks.
3. Dialogue topics are repeatable and replay does not duplicate statement records.
4. `c_orest_alibi` appears when both required Orest statements are known, regardless of question order.
5. `c_mira_ledger` and `c_taras_log` similarly require their two inputs.
6. Adding a conflict never assigns `culprit_id` automatically.
7. Choosing Orest commits `reported_orest`, without changing case truth.
8. Choosing Mira/Taras commits their wrong-report outcomes (successful run, not game over).
9. `Insufficient evidence` is allowed after minimum scene inspection and returns `unresolved`.
10. Verdict cannot be submitted twice; Cancel does not close investigation.
11. Save/load round-trip preserves visited zones, clues, statements, contradictions and outcome.
12. Restart/reset clears verdict and all discovered data; `truth_culprit_id` remains authored.
13. An unread/invalid save fails safely, enables New Case and shows the recovery message.
14. Player has no forced path to all ten clues; ending can be reached with partial investigation.
15. Mock Lords result contains **report** but does not claim authoritative judgment.

### PlayMode / integration

16. New Case opens intro, then village, then first evidence interaction.
17. Moving through at least two **opposing** fixed camera zones does not invert held movement input.
18. Inspector UI can open, close, and return control; walls block selecting out-of-range props.
19. Mira, Orest, Taras and Ada have readable conversation topics; gated lines unlock in correct order.
20. Board reflects source classification and approximate timeline, with no automatic guilt marking.
21. Return to elder and submit each of the four outcomes in test fixtures.
22. Save mid-case → quit → reload → continue works; result persists on relaunch.
23. Windows standalone starts without Editor-only dependencies and contains no obvious missing-script/pink-material errors.
24. `/docs/index.html` loads standalone in browser and release download points to a **real file**, once uploaded.

## 13. Manual acceptance walkthrough (one possible route)

1. Start `New Case`. Read the incident brief and move to granary.
2. Inspect all four granary clues. Talk to Mira, ask why she edited the ledger.
3. Move through square. Talk to Ada and Orest; inspect permit and cargo tally.
4. Move to watch post. Talk to Taras, inspect his log and Stefan's note; confront Taras.
5. Visit ford; inspect cart traces and crossing docket.
6. Return to Orest; ask about his earlier alibi in light of the docket.
7. Open case board; independently compare all accounts. Confirm there is **no guilt percentage**.
8. Submit accusation against Orest; see report outcome without truth banner.
9. Replay. Submit accusation against Mira from limited evidence; see a **valid but harmful** outcome, not failure.
10. Replay. Submit insufficient evidence, even without visiting every zone; see unresolved case ending.
11. Verify save/continue during interview stage and again after a result.

## 14. Release and `/docs/index.html`

Every Temnorid project gets `/docs/index.html` describing the game and containing a **download button** for a GitHub Release asset.

Minimum page sections:

- `Temnorid: Detective` + 1-sentence premise.
- `Investigate / Interview / Decide` feature blocks, emphasizing wrong accusations are possible.
- Case 01 spoiler-free introduction and recommended controls.
- Requirements (Windows), version number, known early-access limitations.
- **Download for Windows** linking to an actual GitHub release build archive or installer, not merely the GitHub repository root. Until published, display `Coming soon` rather than a fake download URL.
- Footer link to Temnorid game collection; only link to verified public pages.

Windows build/zip should have an obvious executable entry point, required Unity data folders, and a release filename. Verify install/run manually before publishing. Do not claim a tested release unless the build actually ran.

## 15. Deferred backlog

Multiple cases, repeatable community incidents, prosecution/evidence appeals, global crime events, voice acting, translations beyond lightweight dictionaries, procedurally generated suspects, social reputation, dynamic pricing / grain shortage calculations, multiplayer, real Lords case API, branching scenes, magical tools, cross-game Escape paths, advanced NPC schedules.

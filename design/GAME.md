# Temnorid: Detective — Game Design & Player Experience

## 1. Core promise

A **medieval 3D detective adventure with fixed cinematic cameras**. The central Temnorid game, **Lords**, can generate incidents such as village thefts, raids, attacks or killings. Here, the player becomes an investigator: walk from scene to scene, inspect objects, speak to residents, compare conflicting accounts, and **choose who—if anyone—to accuse**. The player can confidently choose the wrong person. There is no guaranteed 100%-correct answer shown to the player.

**MVP case:** `The Missing Grain` (ID `grain_01`) — four grain sacks were stolen and night watchman Stefan was killed. The culprit is authored in `CASE_01.md` as hidden developer information.

**Genre:** Narrative investigation / fixed-camera adventure / choice and consequence.  
**Camera:** Classic Resident Evil-style fixed shots, **not** point-and-click only and **not** over-the-shoulder combat.  
**World rules:** Grounded medieval society; magic may exist, but the first case involves **no magic, fantasy species, spell-based forensic certainty, magical lie detection, or modern technology**.  
**Tone:** Serious mystery, suspense, non-graphic injury, no gratuitous violence.  
**Session:** 15–25 minutes for first-time play.

## 2. Main gameplay loop

`Receive incident → Inspect scene → Discover clue → Interview residents → Revisit places as new topics unlock → Compare evidence and statements → Choose suspect OR insufficient evidence → Submit a fallible report → See village reaction → Replay / return to Lords (mock)`.

Crucial rules:

- No waypoint directly points to the culprit. Use goals such as `Find the missing transport record` instead of `Prove Orest guilty`.
- The case board labels **observations**, **records**, **testimony**, and **the player's hypotheses** as different things.
- A witness can be honestly mistaken, withholding details, or lying; the game does not label them in advance.
- Reasking questions may reveal different information after the player discovers evidence, but not every inconsistency has to mean guilt.
- The player may end an incomplete investigation; the judge/elder does not force a single answer.
- No mandatory fight, police chase, magic spell, stealth or puzzle-lock mechanic in Case 01.

## 3. Story premise

The village of **Greyford** (working placeholder name, not established permanent lore) discovers that four sacks of grain are missing from its granary. Night watchman **Stefan** has been found dead beside the back gate. Villagers argue over whether the theft was an outside raid, an inside job, or a quarrel turned deadly. A village elder asks the investigator to gather the facts and make a recommendation before panic spreads.

Intro (player-facing):

> "Four sacks are gone. Stefan, who kept watch by the back gate, was found dead at dawn. We have accusations, but no proof. Walk the village. Hear them out. Then tell me whom you believe—or tell me we do not know."

The scene includes three suspects and one independent witness. No suspect is cartoonishly evil; all have plausible reasons to avoid telling the whole truth.

## 4. One contiguous village, four investigation zones

Make **one Unity gameplay scene** (`Village.unity`), using cheap primitive blockout, with connected walkable areas. Camera zones are not level loads.

```text
               [WATCH POST]
                    |
[GRANARY / BACK GATE] -- [VILLAGE SQUARE / ELDER]
                                    |
                              [RIVERSIDE FORD]
```

**Zone A — Granary and back gate (4–6 min)**

- Opening shot: fixed high oblique camera shows the granary, broken gate and where Stefan was found (no gore).
- Player inspects disturbed sacks, damaged cart hardware, injury description and the granary ledger.
- Meets **Mira**, keeper of stock; her account explains the official count but raises an inconsistency.
- Objective: document the scene and hear the people connected with the granary.
- Camera grammar: wide establishing shot → close but still fixed shot at evidence shelf → fixed view of back gate. Use explicit interaction closeups for readable documents, not a free camera.

**Zone B — Village square / market (4–6 min)**

- Meets **Ada**, a baker who heard an argument and saw a covered cart at night; she did **not** see the driver's face.
- Meets **Orest**, wagon driver; his story conflicts with some physical/timeline evidence.
- Examines trade permit and a public cargo tally; both have ambiguous implications.
- Objective: establish who had access to a wagon and what time it moved.
- Camera grammar: village-wide shot from roof, a stall-side fixed shot, medium over-the-table framing for dialogue (still cinematic, not combat).

**Zone C — Watch post (3–5 min)**

- Meets **Taras**, village guard. His duty log looks suspicious.
- An old note by Stefan suggests somebody asked for late gate access.
- Player can confront Taras about the edited log; he gives a reason that may explain it but is not independently confirmed.
- Objective: reconstruct the period between last sighting of Stefan and the alarm.
- Camera grammar: overhead stair/window shot; low-angle post interior; brief fixed close shot of ledger.

**Zone D — Riverside ford (3–5 min)**

- Finds cart marks, evidence of a recent repair, and a toll record suggesting someone crossed late.
- Player may return to Orest with a new follow-up question.
- Objective: compare accounts of the night's wagon movement.
- Camera grammar: broad riverbank view, wheel-level fixed angle, close inspect view for docket.

**Return to Elder / Decision (2–3 min)**

- The player opens the case board, reviews suspect profiles, testimony and timeline.
- Present four buttons: `Accuse Orest`, `Accuse Mira`, `Accuse Taras`, `Insufficient evidence`.
- Before confirming, show an **accurate neutral warning**: `Your decision can be wrong. The village will act on your report.` No probability, green/red guilt indicator or hidden truth preview.
- Submit the report, play one authored ending, and offer replay.

The user can explore the four zones in flexible order after introduction. Dialogue and board tabs change based on discoveries, but physical access never dead-ends.

## 5. Conversations: mechanics, not cutscenes

- Approach an NPC → press Interact → choose a topic. Dialogue can be read and replayed, with `Back` to leave.
- Initial topics: `Where were you?`, `What did you see?`, `Who had access?`, plus NPC-specific topics.
- Evidence-conditioned topics appear **only if learned**, e.g. `Ask about the toll docket` after inspecting it.
- Do not show dialogue choices that leak an unseen clue by name.
- Each response records its source and unlocks relevant journal text.
- Player may talk to the same witness several times; `Exhausted` does not imply honesty.
- NPC stories are deterministic and hand-authored for MVP. There is no LLM chat, generated witness memory or fake “AI deductions.”
- At least **three meaningful evidence-conditioned follow-ups** must exist across the NPCs.

See `CASE_01.md` for concrete lines, flags and contradictions.

## 6. Evidence and case board

**Collecting evidence = learning / registering it**; do not fill a complex physical inventory with every footprint. Some objects are read-only documents, some are collectible small artifacts. Every clue is individually inspectable, has a stable ID, readable explanation and belongs to a source category.

Case board has four compact tabs:

1. **Evidence**: discovered entries, source, location, description; never hidden truth.
2. **People**: Mira, Orest, Taras, Ada and Stefan; known statements, unanswered questions, contradictions.
3. **Timeline**: approximate event times and their attribution; clear difference between `observed`, `reported`, and `estimated`.
4. **Decision**: concise accusation choices and optional note field or selected supporting reasons; the chosen reasons affect report text, not the fixed truth.

**Player-driven deduction:** The board may pin two clues beside one another (e.g. cart permit + ferry record) but must not automatically announce `Orest is the killer`. Automatically discovered **inconsistencies** can be highlighted as `Accounts differ` and link to the source text, not certified lies.

For MVP, evidence may be inspected on a simple text panel with object closeup, **no generated image or dynamic inventory-combination engine**.

## 7. Wrong decisions are real gameplay

- If player accuses the true culprit, they have submitted a well-aligned report; it is **not magically proven** or instant justice.
- If player accuses an innocent suspect, the case **still finishes**. The accused is detained/investigated and trust/social or resource consequences can follow later in Lords.
- If player selects insufficient evidence, the case finishes unresolved; the village may increase patrols and revisit the investigation later.
- Do not pop up `Correct! +100` or `Wrong!`; no guilt markers while playing or on the default result screen.
- The **development tests** know the canonical truth and assert correct branching. Player-facing epilogues show **what the village does in response to the report**, not omniscient knowledge of guilt.
- An optional spoiler/debug panel may reveal truth in Editor builds only; do not ship it in Windows player.

## 8. MVP visual/audio direction

- Visual: medium-fantasy medieval village built from boxes, cylinders and planes; muted daylight or overcast dawn; no costly character models. Use primitive human placeholders and color-neutral garments; do **not** use stereotypical color-coding of guilt.
- UI: clean parchment-style panels with readable type and accessible contrast; placeholder font acceptable.
- Camera: fixed positions and rotations selected by trigger zones; blend 0.2–0.4s when appropriate. Cutscene camera only for opening and outcome, using shared fixed-view system.
- Sound: footsteps, door creak, distant river, ambient village chatter and UI clicks; never make listening to an audio cue mandatory to solve a case (subtitle all spoken lines).
- Accessibility: all clues text-readable, no precise clicking on tiny 3D props, keyboard controls, text size comfortably legible at 1080p, no jump scares needed.

## 9. Deferred extensions

- Multiple cases per village; burglary-only, missing-person, sabotage, political assassination, embezzlement.
- Separate actors responsible for separate crimes; accomplices, false confessions, planted clues, magic that **helps** but never guarantees truth.
- Repeat offenders / dynamic Lords actors, reputation, trial system, village politics.
- Fully voiced dialogue, animated cutscenes, richer deduction mechanics, investigative travel between regions.
- Optional scene template variants, not procedurally generated guilt or arbitrary random suspicion for the first release.

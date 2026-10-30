# Temnorid: Detective — Agent Implementation Guides

**Version:** `0.1.0-mvp`  
**Project:** `Temnorid: Detective`  
**Target:** Unity Windows desktop demo, offline first  
**Constructor:** [IT-Kamianets/3d-constructor-fixed-view](https://github.com/IT-Kamianets/3d-constructor-fixed-view)  
**Suggested game repository:** `IT-Kamianets/temnorid-detective` (do not create/push without authorization)  
**Genre:** Fixed-camera detective adventure / investigation / narrative choices  
**Setting:** Medieval Temnorid; human characters, realistic animals, optional magic. **No fantasy creatures.**

## 1. Product vision

Lords is the shared strategic world. When someone steals village resources, attacks a villager, or kills a resident, the village can open an investigation. In Detective, the player walks between crime scenes, speaks with people who might have seen something, collects clues, reconstructs conflicting accounts, and submits a **fallible judgment**. The player may accuse the wrong person or decline to accuse anybody. A compelling result is not guaranteed to be the correct one.

**Player fantasy:** "I investigated a village crime, weighed imperfect testimony, and made a difficult decision with consequences."

**Crucial distinction:** this is **not** a scripted puzzle where every clue leads to a guaranteed correct answer. A solvable authored truth exists behind the scenes, but the interface must never reveal a suspect's guilt before the verdict. Wrong interpretations are valid completions, not game failures.

## 2. Read these files in order

1. [`AGENT.md`](./AGENT.md): Unity setup, architecture, existing 3D packages, implementation stages.
2. [`GAME.md`](./GAME.md): Player experience, scene flow, fixed cameras, conversation and investigation mechanics.
3. [`CASE_01.md`](./CASE_01.md): **Canonical case truth** (agent-only spoilers), clue graph, witness dialogues, choices, all endings.
4. [`MVP.md`](./MVP.md): Exact MVP scope, systems, state, data schema proposal, UI, controls, saves, tests, release requirements.
5. [`LORDS.md`](./LORDS.md): Future crime → investigation → non-certain accusation flow and integration contract; **not implemented in MVP**.

Case truth is for developers/content tooling; do not display developer-only truth inside the game or ship it in player-facing `/docs/index.html`.

## 3. MVP at a glance

| Feature | MVP decision |
|---|---|
| Core game | One authored investigation: **The Missing Grain** |
| Estimated playtime | 15–25 min first completion; 8–12 min replay |
| World | One small medieval village scene with four zones |
| Character | One controllable investigator, simple placeholder acceptable |
| Camera | Fixed cinematic views per zone; short blends/cuts |
| NPCs | Three suspects + one witness + one village elder (simple humanoids) |
| Interactions | Walk, examine, collect evidence, dialogue, case board, accusation |
| Evidence | Ten authored clues; no random clues or procedural dialogue |
| Outcomes | Accuse one of three people, or report insufficient evidence |
| Gameplay challenge | Spot contradictions and assemble a credible timeline |
| Combat | None |
| Graphics | Primitive/blockout shapes initially; do not require external asset generation |
| Online/Lords | Mock context and mock outcome only; no real cross-game mutation |
| Delivery | Tested Windows build attached to GitHub Release, `/docs/index.html` download page |

## 4. Must-have principles

- **Fixed-view constructor:** preserve the constructor and its engineering conventions; confirm actual API support before coding.
- **Low-cost MVP:** no expensive models, motion capture, procedural AI, combat engine, multiplayer or backend.
- **Meaningful doubt:** testimony can be mistaken, partial, self-serving, or dishonest. Do not tell the player which type it is until after case resolution (and only if revealing the truth is enabled).
- **Fairness:** every suspect must have an articulable hypothesis and the true culprit must be inferable from **multiple independent clue categories**, never a single glowing marker.
- **No forced conviction:** `Insufficient evidence` is a legitimate ending.
- **No softlocks:** a missed clue cannot permanently prevent finishing or revisiting a location; all critical zones remain accessible before the verdict.
- **No player punishment for examining evidence:** free reinspection and reversible notes; the verdict alone commits the case.
- **Lords consequences belong to Lords, not Unity:** this demo emits a **mock** case result and changes nothing in the central world.

## 5. Definition of done

A fresh player can start a Windows build, investigate locations in any practical order, interview all witnesses, compare clues on a case board, make either a correct or incorrect accusation (or abstain), view a coherent outcome, save/quit/continue, and replay. The camera never reverses input on a cut. Automated tests verify clues, contradictory testimony, verdict branching, save/load, and mock Lords output. `/docs/index.html` explains the game and links to its GitHub Release installer/build.

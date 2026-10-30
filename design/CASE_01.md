# Temnorid: Detective — CASE 01: The Missing Grain

> **SPOILERS — DEVELOPER-ONLY CASE TRUTH.** This file is a precise case-authoring and test reference. It must not be displayed as a spoiler in game UI or copied wholesale to public `/docs/index.html`.

## 1. Case identity and premise

- `case_id`: `grain_01`
- `title`: `The Missing Grain` / `Зникле зерно`
- `village_id`: `greyford_demo` (fictional placeholder, replaceable by Lords context later)
- `incident_type`: `RESOURCE_THEFT_AND_HOMICIDE`
- `resource`: `grain`; `quantity_missing`: `4 sacks` (demo narrative units, not actual Lords resource balances)
- `victim`: night watchman **Stefan**
- `suspects`: **Orest** (wagon driver), **Mira** (granary keeper), **Taras** (village guard)
- `witness`: **Ada** (baker)
- `case_giver`: **Elder Halyna** (civic authority)
- `investigator`: generic controllable human character, name optional
- **Canonical culprit (hidden): `orest`** — Orest stole the grain and struck Stefan during their confrontation. No accomplice in this MVP.
- **Canonical innocents (hidden): `mira`, `taras`.** They are not fully honest about unrelated misconduct.

The crime occurs off-screen. Do not stage gratuitous violence; aftermath inspection is sufficient.

## 2. Hidden chronological truth (not player-facing)

| Time | True event | What the player might initially think |
|---|---|---|
| 19:30 | Mira edits the spoiled-stock entry, trying to conceal her poor handling of several sacks **unrelated** to the four stolen ones. | Mira may be changing the count to hide theft. |
| 20:00 | Stefan starts his watch at the granary. | This is backed by routine records. |
| 20:30 | Orest claims he parked his cart at home and did not move it again. **This claim is false.** | It is just one witness account until checked. |
| 21:05 | Taras leaves his watch post briefly to meet a friend and play dice. He later hides this lapse. | He could have been involved in the crime. |
| ~21:15 | Orest brings his cart to the rear granary gate, removes four sacks and confronts Stefan. During the argument Orest strikes Stefan, who dies. | The night sounds and injury suggest confrontation, but no one sees the killer. |
| ~21:25 | Ada hears raised voices and later sees a tarp-covered cart pass the square; she **cannot identify** the driver. | A cart passed, but it could belong to several people. |
| ~21:40 | Orest takes his cart to the riverside crossing and makes a quick strap repair. The crossing docket includes a disputed `O` mark. | The mark is not absolute identification. |
| 22:05 | Taras discovers Stefan and raises the alarm. | His earlier absence makes him look suspicious. |

**Important:** The culprit's actual identity is hidden case data. The player can infer the most coherent hypothesis but must never receive omniscient proof or a 100%-certain verdict indicator.

## 3. Suspects: motive, opportunity, secret, behavior

### Orest (`orest`) — wagon driver, actual culprit

- Plausible motive: debt to merchants outside the village.
- Means: a cart capable of carrying four sacks; known access to deliveries.
- Public account: "I took my cart home before the evening bell. It never went out again."
- Secret: he transported the grain and killed Stefan during confrontation.
- Deflection: the red cart strap could be common; the docket initial might belong to someone else.
- Under questioning after riverside discovery: "I went near the ford, yes, but that does not mean I touched Stefan."
- Avoid implausibly easy confessions or villain dialogue.

### Mira (`mira`) — granary keeper, innocent of both central crimes

- Plausible motive: controls storehouse keys and stock counts; could benefit from falsifying shortages.
- Public account: "The stock ledger is correct."
- Secret: she hid mishandled / spoiled grain from earlier in the week and initially refuses to admit this.
- Evidence-conditioned follow-up: admits altering a spoilage entry, but can point out the four missing transport sacks are distinct from the spoiled lot.
- Consequence of false accusation: the village loses its most experienced storekeeper pending review and ration distribution slows.

### Taras (`taras`) — guard, innocent of theft and killing

- Plausible motive/opportunity: late patrol access, knew Stefan's route and the gate.
- Public account: "I was at my post until the alarm."
- Secret: went off duty briefly and altered the log to cover negligence.
- Evidence-conditioned follow-up: admits the lapse when confronted with conflicting times. This is misconduct, **not proof of homicide**.
- Consequence of false accusation: watch roster breaks down, trust in the guard weakens.

## 4. Other people

**Ada (`ada`) — baker and independent witness**
- Heard two men arguing in the distance around 21:15.
- Saw a covered cart pass later; the driver was obscured. She noticed a red strap on the rear, though rain and poor lighting limit confidence.
- Never states a certain identity. May misremember whether it was before or after the second bell.

**Elder Halyna (`elder_halyna`)**
- Gives case, describes impact, is available to receive verdict. Does not provide the true answer.
- Reminds investigator villagers may be hiding embarrassment rather than murder.
- The village acts on the report, not instant magical fact-checking.

**Stefan (`stefan`) — victim, no dialogue**
- His old work note is an ambiguous clue, not a signed final accusation.

## 5. All ten inspectable evidence items (MVP content list)

| Stable ID | Location | Source type | Player-visible clue | Developer interpretation |
|---|---|---|---|---|
| `e01_broken_strap` | Granary back gate | observation | A torn red cart strap lies in dirt near the gate. | Connected to Orest's cart; red straps are not unique. |
| `e02_torn_sacks` | Granary | observation | Four grain-sack spaces are empty; torn sack fibers remain. | Supports theft of exactly four sacks. |
| `e03_watchman_injury` | Granary | observation | Stefan has a blunt-force wound; no weapon can be identified from the injury. | Supports violent confrontation, not killer identity. |
| `e04_stock_ledger` | Granary office | record | A spoiled-stock entry was overwritten earlier; the later count lacks four transport sacks. | Mira's unrelated misconduct makes her suspicious. |
| `e05_cart_permit` | Village square stall | record | A delivery permit signed for Orest's cart is still valid this week. | Orest had plausible logistical access. |
| `e06_market_cargo_tally` | Village square | record | The evening public cargo tally records no authorized grain shipment. | Makes the late cart movement suspicious; not an ID. |
| `e07_guard_log` | Watch post | record | Taras's duty log has an overwritten time between 21:00 and 22:00. | Covers up temporary absence, not murder. |
| `e08_ford_tracks` | Riverside ford | observation | Fresh cart ruts and red fabric fibers beside a recently repaired strap fastening. | Strengthens Orest-cart hypothesis but could be borrowed/planted. |
| `e09_crossing_docket` | Riverside tollbox | record | A wet crossing docket notes one cart ~21:40 and a smeared `O` mark. | Corroborates transit; initial not conclusive. |
| `e10_stefan_note` | Watch post desk | record | Stefan wrote: "O. asked about passing the rear gate after the bell." | Suggests Orest, but `O.` might refer to another person. |

All 10 items must be available without lucky random spawns. At least 6 should be reasonably discoverable in a blind first playthrough. Interactables need large selection colliders / obvious contextual prompts. Use readable text, not unreadable textures. Physical document meshes can be rectangles.

## 6. Initial dialogue and evidence-conditioned follow-ups

Dialogue may be adapted for Ukrainian UI, but **keep meaning and facts consistent**.

### Mira: topics

- `mira_stock`: "Four transport sacks are gone. I checked the shelves twice."
- `mira_keys`: "Stefan had the back gate. I kept the ledger and store key."
- `mira_ledger_followup` **requires `e04_stock_ledger`**: Investigator: "Why did you overwrite the spoiled-stock entry?" Mira: "I was hiding spoiled grain. I feared punishment. It is not the four sacks taken tonight."
- Adds statement `s_mira_spoilage_admission` and contradiction `c_mira_ledger` (the ledger denial and alteration differ).

### Orest: topics

- `orest_where`: "I parked the wagon by my home before the evening bell. I slept after that."
- `orest_access`: "Of course I have a permit. I move supplies for the village."
- `orest_strap_followup` **requires `e01_broken_strap` or `e08_ford_tracks`**: "Red straps? Half the carts here use them."
- `orest_docket_followup` **requires `e09_crossing_docket`**: "I passed by the ford to check a wheel. I never went to the granary. That smudge could be anyone's initial."
- Adds contradiction `c_orest_alibi` (he first claimed no evening cart movement but later acknowledges going to ford). This is **strong but not definitive**.

### Taras: topics

- `taras_duty`: "My post was covered all evening. I found Stefan near the granary and raised the alarm."
- `taras_stefan`: "He was a decent man. We argued about watch schedules, nothing more."
- `taras_log_followup` **requires `e07_guard_log`**: "I left my station for a short while. I changed the log. I should not have."
- Adds `s_taras_absence_admission`, contradiction `c_taras_log`.

### Ada: topics

- `ada_noise`: "I heard two men argue. I could not make out the words."
- `ada_cart`: "A covered cart came past my stall. I saw a red strap, but I did not see who drove."
- `ada_time`: "It was after the first bell, I think. Do not swear by my times; it was dark and raining."
- `ada_recheck` **requires `e06_market_cargo_tally`**: "No, it wasn't a normal registered delivery. Nobody stopped at the tally board."
- Ada never changes her story to produce an eye-witness identification.

### Elder Halyna: topics

- `elder_start`: starts investigation and gives initial objective.
- `elder_status`: "What have you learned? You may question them more."
- `elder_decide`: opens decision UI **after the opening scene has been inspected**, even with incomplete evidence; confirm before locking the verdict.

### Statement metadata

Every journal quote should include `speaker`, `topic_id`, `known_at`, `source_type = testimony`, and the literal statement. The case board should be able to show that contradictory accounts both exist without silently deleting earlier testimony.

## 7. Derived conflicts / hypotheses (never certified guilt)

| Conflict ID | Discovery conditions | Case board label | Player interpretation options |
|---|---|---|---|
| `c_orest_alibi` | `orest_where` and `orest_docket_followup` | "Orest's cart story changed" | Guilt, embarrassment, misremembered trip, covering for someone |
| `c_mira_ledger` | `mira_stock` and `mira_ledger_followup` | "Mira concealed ledger changes" | Fraud unrelated to theft, or participation |
| `c_taras_log` | `taras_duty` and `taras_log_followup` | "Taras changed his duty account" | Negligence, covering an accomplice, or direct crime |
| `c_unregistered_cargo` | `e06_market_cargo_tally` and `ada_cart` | "A cart moved without a registered shipment" | Smuggling, emergency trip, unofficial business |
| `c_orest_route` | `e01_broken_strap` and `e08_ford_tracks` | "Similar red fabric appears at gate and ford" | Shared cart, common material, staged clue |

**No automatic deductive chain** from any conflict to a suspect's guilt. The case board uses neutral language and clearly distinguishes physical evidence from subjective inferences.

## 8. Outcomes (all valid completions)

At decision screen, `accused_suspect_id` is `orest | mira | taras | null`. If null, decision type is `INSUFFICIENT_EVIDENCE`. Require explicit confirmation, then commit exactly once.

**Result A — Accuse Orest (canonical culprit; internally correct)**
- Player report: "The wagon movements, late crossing and changing story suggest Orest."
- Elder response: "We'll hold Orest for questioning. The evidence will be heard."
- Default player result: `Case closed — accusation submitted. The village will hold Orest for inquiry.`
- Do not display omniscient green `CORRECT` to the player.

**Result B — Accuse Mira (false accusation)**
- Player report: "The altered ledger and store access suggest Mira."
- Elder: "The storekeeper will be detained while we investigate. Who will manage the rations?"
- Default result: `Case closed — accusation submitted. Grain distribution is now disrupted.`
- There is **no game-over** or retry demand. Truth remains unchanged.

**Result C — Accuse Taras (false accusation)**
- Player report: "The altered duty log and missing patrol time suggest Taras."
- Elder: "Taras will be relieved of duty pending inquiry. We are short of guards."
- Default result: `Case closed — accusation submitted. Patrol coverage is reduced.`
- There is **no game-over** or instant corrected confession.

**Result D — Insufficient evidence**
- Player report: "The evidence supports several theories. I cannot responsibly accuse a person."
- Elder: "Then we'll leave the case open and increase the watch."
- Default result: `Case unresolved — no arrest recommended. Investigation may continue later.`
- This is not a cowardice/failure badge; it is a meaningful choice with tradeoffs.

### Outcome semantics for tests

- `internal_truth_culprit_id = orest` must never change with accusations.
- `internal_is_correct = true` for `orest`, `false` for `mira` and `taras`, `null` for no accusation. This is **internal test state only**, not player-facing or part of client-authoritative Lords assertions.
- The player-facing outcome is based on chosen action and village **response**, not a guarantee the accusation is true.
- On replay, start fresh to prevent prior journal data from contaminating a new run. Optional developer-only spoilers can expose truth in test harness.

## 9. Case completion conditions and sequencing

- A run starts with intro and the objective `Inspect the granary`.
- After inspecting **any one** of `e01`, `e02`, `e03`, `e04`, the elder's decision option becomes available. Warn players when key witnesses/unvisited areas remain, but permit submission.
- Every zone is accessible from the start except the introductory dialogue modal; there is no item gate.
- All NPCs can be interviewed in any order; follow-up questions unlock as soon as relevant evidence is found.
- No time limit, no forced arrest, no locked-forever dialogue choices, and no evidence consumes other evidence.
- A decision cannot be undone within the same run after confirmation, but `Replay Case` resets it.
- A complete first-time playthrough can find every item, ask every topic, read the case board, and submit any outcome.

## 10. Example independent hypotheses (for UX copy, never hidden scoring)

- **Orest hypothesis:** licensed wagon, changing story, suspicious strap/crossing, victim note; perhaps he transported the grain and confronted Stefan.
- **Mira hypothesis:** stock-keeping access, changed ledger, possible hidden shortages; perhaps she engineered the theft or covered another offender.
- **Taras hypothesis:** altered watch log, unattended post and knowledge of routines; perhaps he let someone through or attacked Stefan.
- **Abstain hypothesis:** no eyewitness ID, several common materials, vague initials, unreliable times, separate hidden misconduct; further inquiry needed.

All four conclusions are understandable from incomplete information. The objectively strongest clue set favors Orest; avoid distributing evidence so sparsely that the intended solution is impossible to infer, and avoid one undeniable smoking gun that makes the dilemma trivial.

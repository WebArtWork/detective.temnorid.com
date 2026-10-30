# Test report: Temnorid: Detective 0.1.0

Run on Unity 6000.6.5f1, headless (`-batchmode -nographics`), against the generated `Village.unity`. Result: **29 edit-mode tests passed, 13 play-mode tests passed, 0 failed.** The numbers in brackets are the test numbers in `design/MVP.md` section 12.

## Edit-mode (pure rules, `Assets/Tests/EditMode/DetectiveRulesTests.cs`)

| Expected | Observed |
|---|---|
| Registering a known clue again does not duplicate it or its count (1) | Passed: a second and third inspect return false, one entry. An unknown id is refused. |
| A follow-up topic stays locked until its clue exists, then opens; either strap clue opens Orest's (2) | Passed for Mira's ledger topic and Orest's strap topic. |
| Topics can be repeated and replay does not duplicate the statement (3) | Passed: same statement object, one record. |
| Orest's story conflict appears when both statements are known, in either order (4) | Passed for both orders (a parametrised test). |
| Mira's and Taras's conflicts need both inputs (5) | Passed. The cargo and route conflicts, which mix clues and testimony, behave the same. |
| Adding a conflict never assigns a culprit or resolves anything (6) | Passed: all five conflicts found, status still Active, no verdict, no accused. |
| Accusing Orest commits `reported_orest` and does not change the truth (7) | Passed. |
| Accusing Mira or Taras commits their outcomes as a normal finished case (8) | Passed (two cases). |
| Insufficient evidence is allowed after the scene is inspected and gives `unresolved` (9) | Passed. |
| The verdict cannot be given twice; a refused report changes nothing; only the three suspects can be accused (10) | Passed. Cancelling is covered in play mode. |
| Save and load keep zones, clues, statements, conflicts, pins and the outcome (11) | Passed through the real save store and file. |
| A restart clears everything while the truth stays authored (12) | Passed. |
| An unreadable or contradicting save fails safely (13) | Passed: wrong version, other case, unknown ids, a topic whose clue was never found, a "resolved" save without an outcome, a pin on an unfound clue, and a corrupt file are all refused, and a refused load leaves the previous state alone. |
| No forced path to all ten clues; the case can end with partial evidence (14) | Passed: every clue can be inspected on its own; any one of the four granary clues is enough to report. |
| The mock Lords report holds a report and not a judgment (15) | Passed: no field named for the culprit, correctness or truth. |

Also covered: the ten clues, four zones and counts per zone; the elder records no statement; the timeline shows only what is known, in order, with the right basis (a witness's guess is "estimated", a record "observed", the smeared docket "uncertain"); pins; the objective never names a person; every text key the game needs exists; the result and decision texts never say "right", "wrong" or "guilty".

## Play-mode (the real scene, `Assets/Tests/PlayMode/DetectiveRunTests.cs`)

| Expected | Observed |
|---|---|
| New Case opens the brief, then the village, then the first clue (16) | Passed. |
| The scene has all ten clues and five people, at least six shots (two granary, two square, one watch post, one ford) and every clue stands outside every solid footprint | Passed: 15 interactables, 7 shots. |
| Every walkable spot has a shot and borders do not flicker | Passed over a 0.7 m grid of the whole village; pacing 20 cm across three borders changes the shot at most once. |
| Walking across two opposing shots does not invert held movement, in both directions (17) | Passed: south from the square into the ford (shots facing about 140 degrees apart) and back north; the player keeps heading the way the key was held. |
| The inspect screen opens, closes and returns control; walls block selecting a person (18) | Passed (a wall placed between the player and Mira removes the prompt). Reading a clue again records nothing. |
| The four people have readable topics and the gated lines unlock in order (19) | Passed. |
| The board reflects the source and timeline without guilt marking (20) | Passed: eleven timeline entries once everything is known, no resolution until a report. |
| All four outcomes can be submitted (21) | Passed from a thin investigation (one clue) for each outcome, and the full-case run submits the Orest report with three reasons. Cancel leaves the case open; a second confirm is refused. |
| Save mid-case, quit, reload and continue works; a result persists on relaunch (22) | Passed in the middle of an interview (clues, statements, conflict, pin, place) and after a report (Continue reopens the result). A save made inside a wall is moved to a safe place. |
| A full playthrough works | Passed: a bot walks the whole village, inspects all ten clues, asks every topic (including all four follow-ups), reads the journal and board, reports through the elder, quits and continues, then replays from nothing. |
| A damaged save is reported and a new case still works | Passed. |
| Pause stops the clock; restart from pause resets; settings are remembered | Passed. |
| The Windows player starts without the editor and has no missing-script errors (23) | Started and ran for 14 seconds with no exceptions in its log (see README for what was not done by hand). |
| `docs/index.html` loads standalone and the download button points to a real file (24) | The page is standalone with relative content only. The download button is deliberately disabled ("Coming soon"): no release exists yet, so there is no real file to point to. |

## Not covered by automated tests

A human playing from start to finish with a real keyboard and mouse; installing the installer; the look of the screens beyond what was inspected in the editor; behaviour on other machines and resolutions.

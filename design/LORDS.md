# Temnorid: Detective ↔ Lords — Future World Integration

> **Design contract only.** The MVP runs offline with mock case context. No Lords account connection, case creation, world mutation, or authoritative verdict enforcement ships in `0.1.0`.

## 1. Why Detective exists in the Temnorid ecosystem

`Lords` is the central shared strategic world with villages, resources, characters and events. `Detective` is a separate game played when an incident requires **human interpretation** instead of an automated guaranteed answer.

Potential Lords triggers:

- `RESOURCE_THEFT`: village stock stolen; investigate access, transport and witnesses.
- `VILLAGER_HOMICIDE`: a villager is killed; reconstruct movements and motives.
- `RAID_OR_SABOTAGE`: attackers damage property; identify potential organizer.
- `COMBINED_INCIDENT`: theft and murder occur together (demo case).

In the future, after a scouting or theft action in Lords, the village may launch a Detective case. Detective findings may influence **suspicion, patrol policy, arrest requests or reputation**—but game progress alone never creates certain guilt. Lords decides whether and how to act on a report, based on permissions and world rules.

## 2. Relationship to Temnorid: Scouting and Escape

```text
LORDS player sends scout or thief
             |
        Village incident
        /            \
   Caught in act   Escapes / unknown suspect
        |                    |
   ESCAPE GAME        DETECTIVE GAME
  prison attempt       investigate incident
        |                    |
   escaped/failed      accuse / no accusation
        \                    /
              LORDS
     (validates & applies outcomes)
```

These are *possible* paths, not guaranteed consequences.

- **Scouting** may reveal village information; a discovered scout can be captured and sent to Escape.
- **Theft** may lead to missing resources, prompting Detective even if no one was captured.
- **Detective** can recommend arresting a person; this may later create a prison/Escape scenario **only after** Lords' own authorization/adjudication process.
- A wrong accusation may inconvenience, punish or endanger an innocent character, but must have fair appeals/guardrails before cross-player assets/characters are confiscated.
- Detective is **not a substitute for authoritative gameplay state or proof of a crime**. It is an interactive investigation and narrative decision layer.

## 3. Who may investigate (future policy options)

For MVP: player is always a local investigator assigned by Elder Halyna.

In Lords, options to consider later:

1. Village head assigns a detective to a case.
2. A trusted village role may volunteer or be elected as investigator.
3. A village council reviews the completed report, or several players contribute evidence to one case.

**Recommended first integration:** one assigned investigator submits a **non-binding recommendation**; Lords only changes low-stakes policies pending a separate ruling. This avoids false reports becoming unrestricted griefing tools.

## 4. Proposed future launch context (illustrative, not a live API)

```json
{
  "schema_version": 1,
  "case_instance_id": "case-demo-001",
  "case_template_id": "grain_01",
  "village_id": "greyford_demo",
  "incident_type": "RESOURCE_THEFT_AND_HOMICIDE",
  "reported_resource": { "type": "grain", "quantity": 4, "unit": "sack" },
  "victim_character_ids": ["stefan_demo"],
  "subject_character_ids": ["orest_demo", "mira_demo", "taras_demo"],
  "assigned_investigator_id": "detective_demo",
  "locale": "uk-UA",
  "mode": "MOCK_OFFLINE"
}
```

Do not hardcode server-defined identities, real player names or actual account credentials into the MVP. A mock fixture uses the above placeholder text **for display only**. Production IDs and allowed suspects come from Lords, not client defaults.

### Dynamic Lords content (not MVP)

The MVP is one **authored fixed-truth case**, not procedurally generated. For a real shared world, don't repeatedly reuse `grain_01` unchanged while pretending it represents arbitrary live crimes. Choose one of:

- Launch curated authored cases for certain scripted Lords events; treat named characters as NPCs.
- Later build an authoritative case generator/registry that binds suspects, events, evidence and a hidden canonical incident truth at creation time.

The **client does not generate or choose a new culprit at run time** for real cases. Any randomized template selection or truth assignment must be owned and signed/validated by the world authority to prevent exploitation and contradictory case histories.

## 5. Proposed report payload (illustrative)

```json
{
  "schema_version": 1,
  "case_instance_id": "case-demo-001",
  "decision": "ACCUSE",
  "accused_subject_id": "orest_demo",
  "supporting_evidence_ids": [
    "e01_broken_strap",
    "e08_ford_tracks",
    "e09_crossing_docket"
  ],
  "reported_by_investigator_id": "detective_demo",
  "client_session_id": "local-demo-session-001",
  "result_status": "SUBMITTED_REPORT"
}
```

If the investigator abstains, use `decision = INSUFFICIENT_EVIDENCE`, `accused_subject_id = null`. These examples are **proposed fields, not an existing Lords endpoint**; coordinate exact names with the Lords service when integration begins.

Important: do **not** include `true_culprit_id`, `is_correct`, secret witness reliability, sealed evidence, or any hidden canonical truth in any user-submitted response. The client submits an **opinionated report**, not objective ground truth.

## 6. MVP mock adapter behavior

- Startup creates one in-memory/local fixture for `grain_01`, `greyford_demo` and investigator `detective_demo`.
- In-game UI may say `Incident from Lords — Demo` but must explicitly disclose **offline preview / no synchronization**.
- `SubmitReport` produces a local mock report event and result screen; does not call any network endpoint.
- Show spoiler-free preview: `Report submitted locally. A future Lords integration could allow the village to review and respond. No Lords state was changed.`
- No external tokens, keys, authentication, ledger, balances, wallet or personal data.

## 7. Security, fairness, and anti-griefing requirements for later

1. **Lords owns incident truth and authority**: an authorized service determines which events exist, who may investigate and which accusations are legally/actionably valid.
2. **Untrusted Unity clients**: a local save file, successful scene completion, evidence IDs, and verdict are not proof of an objective crime. In production, protect submitted evidence with server state and permitted actions.
3. **One active report per authorized role / case** unless explicit appeal or reopening. Record time, game version and actor ID; deduplicate repeated submissions.
4. **Evidence visibility**: limit investigator's knowledge to actually discovered/authorized sources; prevent spoilers or forging records via client IDs.
5. **Separate investigation from judgment**: a reported suspect should not immediately lose their city, resources, game access, or freedom solely because another player clicked Accuse.
6. **Appeals/reviews**: introduce defensible evidence review, deadlines, and limits before punitive multiplayer outcomes.
7. **Proportional consequences**: early integration should affect local suspicion/security/patrol adjustments, not permanent asset confiscation.
8. **Privacy & moderation**: present fictional game-character accusations with clearly bounded rules; do not encourage naming or doxxing real people.
9. **Failure-tolerant event processing**: reports should be idempotent and retried safely by server logic; delayed/out-of-order submissions must not create contradictory cases.
10. **No guarantee of truth**: the chance to accuse incorrectly is intentional. Servers validate report authenticity, **not magical accuracy**.

## 8. Consequence ideas (future, not v0.1.0)

| Player report | Possible world result after authorized review | Risks to design for |
|---|---|---|
| Accuse correct suspect | Arrest request, lower theft risk, recovered grain if independently located | Avoid automatic full recovery based only on accusation |
| Accuse wrong storekeeper | Reduced ration efficiency, village distrust, appeal path | Prevent indefinite griefing against another player |
| Accuse wrong guard | Temporary patrol shortage, affected security | Must not force permanent penalties without process |
| Insufficient evidence | Additional patrol spend, unresolved case, later reopening | Should not be an unlimited no-cost avoidance exploit |

Consequences are examples, not economy formulas. Do not invent actual Lords resource math or a current API.

## 9. Integration acceptance checklist (deferred)

- [ ] Authorized Lords event spawns an immutable case instance with stable IDs.
- [ ] Investigator role and allowed subject set are validated on server.
- [ ] Client receives a sanitized case projection, never hidden culprit or secret evidence.
- [ ] Case board and dialogue render server-approved information rather than invented guilt hints.
- [ ] Report submission includes idempotency key / case instance ID and supports abstention.
- [ ] Server rejects unauthorized suspects, stale case state and duplicate submissions.
- [ ] Reports create reviewable evidence + investigator recommendation, not immediate punishment.
- [ ] Wrong accusations remain possible and **have proportional, appealable consequences**.
- [ ] Lords/Scouting/Escape links are event-driven but can be played independently where appropriate.
- [ ] Complete integration is documented and tested before enabling cross-game effects.

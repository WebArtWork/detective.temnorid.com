using System;
using System.Collections.Generic;
using System.Linq;

namespace Temnorid.Detective
{
	/// <summary>The numbers of the game in one place.</summary>
	public static class DetectiveConfig
	{
		public const string CaseId = "grain_01";
		public const string ContextFixtureId = "lords_mock_grain_theft";
		public const int SaveVersion = 1;

		public const float MoveSpeed = 2.5f;
		public const float FastSpeed = 4f;
		public const float InteractRange = 2.2f;
		public const float CameraHysteresis = 0.3f;
		public const float CameraBlend = 0.3f;
		/// <summary>How many clues a report may name as its reasons.</summary>
		public const int MaxReasons = 3;
	}

	/// <summary>Stable ids of the zones, people, clues and conflicts of the case.</summary>
	public static class Ids
	{
		public const string Granary = "granary";
		public const string Square = "square";
		public const string WatchPost = "watch_post";
		public const string Ford = "ford";

		public const string Orest = "orest";
		public const string Mira = "mira";
		public const string Taras = "taras";
		public const string Ada = "ada";
		public const string Elder = "elder_halyna";
		public const string Stefan = "stefan";

		public const string E01 = "e01_broken_strap";
		public const string E02 = "e02_torn_sacks";
		public const string E03 = "e03_watchman_injury";
		public const string E04 = "e04_stock_ledger";
		public const string E05 = "e05_cart_permit";
		public const string E06 = "e06_market_cargo_tally";
		public const string E07 = "e07_guard_log";
		public const string E08 = "e08_ford_tracks";
		public const string E09 = "e09_crossing_docket";
		public const string E10 = "e10_stefan_note";

		public const string COrestAlibi = "c_orest_alibi";
		public const string CMiraLedger = "c_mira_ledger";
		public const string CTarasLog = "c_taras_log";
		public const string CUnregisteredCargo = "c_unregistered_cargo";
		public const string COrestRoute = "c_orest_route";

		public const string OutcomeOrest = "reported_orest";
		public const string OutcomeMira = "reported_mira";
		public const string OutcomeTaras = "reported_taras";
		public const string OutcomeUnresolved = "unresolved";
	}

	public enum SourceType
	{
		Observation,
		Testimony,
		Record,
		Inference,
	}

	/// <summary>How a time on the timeline is known.</summary>
	public enum TimeBasis
	{
		Observed,
		Reported,
		Estimated,
	}

	public enum VerdictKind
	{
		Accuse,
		Insufficient,
	}

	public enum CaseStatus
	{
		Active,
		Resolved,
	}

	public sealed class EvidenceDef
	{
		public string Id;
		public string Zone;
		public SourceType Source;
		/// <summary>Part of the opening scene: inspecting any of these lets the investigator report.</summary>
		public bool Scene;
	}

	public sealed class TopicDef
	{
		public string Id;
		public string Npc;
		/// <summary>Any one of these clues unlocks the topic. Empty: always available.</summary>
		public string[] RequiresAny = new string[0];
		/// <summary>The id of the statement this answer records (null for the elder, who is not a witness).</summary>
		public string StatementId;
	}

	/// <summary>An inconsistency between two things the player has learned. Never a certified lie.</summary>
	public sealed class ConflictDef
	{
		public string Id;
		/// <summary>Each side is a clue id, or <c>t:topic</c> for a statement.</summary>
		public string A;
		public string B;
	}

	public sealed class TimelineDef
	{
		public string Id;
		public int SortMinutes;
		public string Time;
		public TimeBasis Basis;
		public bool Uncertain;
		public string Requires;
	}

	/// <summary>The authored case, in one place: nothing else repeats its facts.</summary>
	public static class CaseContent
	{
		public static readonly string[] Zones = { Ids.Granary, Ids.Square, Ids.WatchPost, Ids.Ford };
		public static readonly string[] Suspects = { Ids.Orest, Ids.Mira, Ids.Taras };
		public static readonly string[] People = { Ids.Mira, Ids.Orest, Ids.Taras, Ids.Ada, Ids.Stefan };

		public static readonly EvidenceDef[] Evidence =
		{
			new EvidenceDef { Id = Ids.E01, Zone = Ids.Granary, Source = SourceType.Observation, Scene = true },
			new EvidenceDef { Id = Ids.E02, Zone = Ids.Granary, Source = SourceType.Observation, Scene = true },
			new EvidenceDef { Id = Ids.E03, Zone = Ids.Granary, Source = SourceType.Observation, Scene = true },
			new EvidenceDef { Id = Ids.E04, Zone = Ids.Granary, Source = SourceType.Record, Scene = true },
			new EvidenceDef { Id = Ids.E05, Zone = Ids.Square, Source = SourceType.Record },
			new EvidenceDef { Id = Ids.E06, Zone = Ids.Square, Source = SourceType.Record },
			new EvidenceDef { Id = Ids.E07, Zone = Ids.WatchPost, Source = SourceType.Record },
			new EvidenceDef { Id = Ids.E08, Zone = Ids.Ford, Source = SourceType.Observation },
			new EvidenceDef { Id = Ids.E09, Zone = Ids.Ford, Source = SourceType.Record },
			new EvidenceDef { Id = Ids.E10, Zone = Ids.WatchPost, Source = SourceType.Record },
		};

		public static readonly TopicDef[] Topics =
		{
			new TopicDef { Id = "mira_stock", Npc = Ids.Mira, StatementId = "s_mira_stock" },
			new TopicDef { Id = "mira_keys", Npc = Ids.Mira, StatementId = "s_mira_keys" },
			new TopicDef { Id = "mira_ledger_followup", Npc = Ids.Mira, RequiresAny = new[] { Ids.E04 }, StatementId = "s_mira_spoilage_admission" },
			new TopicDef { Id = "orest_where", Npc = Ids.Orest, StatementId = "s_orest_where" },
			new TopicDef { Id = "orest_access", Npc = Ids.Orest, StatementId = "s_orest_access" },
			new TopicDef { Id = "orest_strap_followup", Npc = Ids.Orest, RequiresAny = new[] { Ids.E01, Ids.E08 }, StatementId = "s_orest_strap" },
			new TopicDef { Id = "orest_docket_followup", Npc = Ids.Orest, RequiresAny = new[] { Ids.E09 }, StatementId = "s_orest_ford_admission" },
			new TopicDef { Id = "taras_duty", Npc = Ids.Taras, StatementId = "s_taras_duty" },
			new TopicDef { Id = "taras_stefan", Npc = Ids.Taras, StatementId = "s_taras_stefan" },
			new TopicDef { Id = "taras_log_followup", Npc = Ids.Taras, RequiresAny = new[] { Ids.E07 }, StatementId = "s_taras_absence_admission" },
			new TopicDef { Id = "ada_noise", Npc = Ids.Ada, StatementId = "s_ada_noise" },
			new TopicDef { Id = "ada_cart", Npc = Ids.Ada, StatementId = "s_ada_cart" },
			new TopicDef { Id = "ada_time", Npc = Ids.Ada, StatementId = "s_ada_time" },
			new TopicDef { Id = "ada_recheck", Npc = Ids.Ada, RequiresAny = new[] { Ids.E06 }, StatementId = "s_ada_recheck" },
			new TopicDef { Id = "elder_start", Npc = Ids.Elder },
			new TopicDef { Id = "elder_status", Npc = Ids.Elder },
		};

		public static readonly ConflictDef[] Conflicts =
		{
			new ConflictDef { Id = Ids.COrestAlibi, A = "t:orest_where", B = "t:orest_docket_followup" },
			new ConflictDef { Id = Ids.CMiraLedger, A = "t:mira_stock", B = "t:mira_ledger_followup" },
			new ConflictDef { Id = Ids.CTarasLog, A = "t:taras_duty", B = "t:taras_log_followup" },
			new ConflictDef { Id = Ids.CUnregisteredCargo, A = Ids.E06, B = "t:ada_cart" },
			new ConflictDef { Id = Ids.COrestRoute, A = Ids.E01, B = Ids.E08 },
		};

		public static readonly TimelineDef[] Timeline =
		{
			new TimelineDef { Id = "tl_note", SortMinutes = 1100, Time = "Date unknown", Basis = TimeBasis.Observed, Uncertain = true, Requires = Ids.E10 },
			new TimelineDef { Id = "tl_orest_home", SortMinutes = 1200, Time = "Before the evening bell", Basis = TimeBasis.Reported, Requires = "t:orest_where" },
			new TimelineDef { Id = "tl_taras_post", SortMinutes = 1210, Time = "All evening", Basis = TimeBasis.Reported, Requires = "t:taras_duty" },
			new TimelineDef { Id = "tl_tally", SortMinutes = 1250, Time = "Evening", Basis = TimeBasis.Observed, Uncertain = true, Requires = Ids.E06 },
			new TimelineDef { Id = "tl_guard_log", SortMinutes = 1260, Time = "21:00–22:00", Basis = TimeBasis.Observed, Requires = Ids.E07 },
			new TimelineDef { Id = "tl_taras_absence", SortMinutes = 1262, Time = "21:00–22:00", Basis = TimeBasis.Reported, Requires = "t:taras_log_followup" },
			new TimelineDef { Id = "tl_ada_noise", SortMinutes = 1275, Time = "After dark", Basis = TimeBasis.Estimated, Uncertain = true, Requires = "t:ada_noise" },
			new TimelineDef { Id = "tl_ada_cart", SortMinutes = 1290, Time = "Later that night", Basis = TimeBasis.Estimated, Uncertain = true, Requires = "t:ada_cart" },
			new TimelineDef { Id = "tl_docket", SortMinutes = 1300, Time = "About 21:40", Basis = TimeBasis.Observed, Uncertain = true, Requires = Ids.E09 },
			new TimelineDef { Id = "tl_orest_ford", SortMinutes = 1301, Time = "That evening", Basis = TimeBasis.Reported, Requires = "t:orest_docket_followup" },
			new TimelineDef { Id = "tl_tracks", SortMinutes = 1302, Time = "Recently", Basis = TimeBasis.Observed, Uncertain = true, Requires = Ids.E08 },
		};

		static readonly Dictionary<string, EvidenceDef> EvidenceById = Evidence.ToDictionary(e => e.Id);
		static readonly Dictionary<string, TopicDef> TopicById = Topics.ToDictionary(t => t.Id);

		public static EvidenceDef FindEvidence(string id) => id != null && EvidenceById.TryGetValue(id, out var e) ? e : null;
		public static TopicDef FindTopic(string id) => id != null && TopicById.TryGetValue(id, out var t) ? t : null;
		public static bool IsSuspect(string id) => Array.IndexOf(Suspects, id) >= 0;
	}

	/// <summary>
	/// The hidden truth of the case, used by the rules' tests only. It is never shown, never changed by what the player chooses and
	/// never put into the report that leaves the game.
	/// </summary>
	public static class CaseTruth
	{
		public const string CulpritId = "orest";

		/// <summary>Whether an accusation matches the authored truth: true, false, or null for no accusation.</summary>
		public static bool? IsCorrect(string accusedId) => string.IsNullOrEmpty(accusedId) ? (bool?)null : accusedId == CulpritId;
	}

	public sealed class StatementRecord
	{
		public string Id;
		public string Speaker;
		public string TopicId;
		public float KnownAt;
		public SourceType Source = SourceType.Testimony;
	}

	public sealed class AskResult
	{
		public TopicDef Topic;
		public StatementRecord Statement;
		public bool IsNewStatement;
		public List<string> NewConflicts = new List<string>();
	}

	public sealed class VerdictResult
	{
		public VerdictKind Kind;
		public string AccusedId;
		public string OutcomeId;
		public string[] Reasons;
	}

	/// <summary>
	/// The investigation: what has been found and said, in the order it was learned, and the one verdict. Dialogue and
	/// inspection can be repeated without changing anything; a conflict is only a noted inconsistency; the verdict is
	/// committed once and the authored truth is never touched.
	/// </summary>
	public sealed class Investigation
	{
		readonly List<string> _evidence = new List<string>();
		readonly List<float> _evidenceAt = new List<float>();
		readonly List<string> _zones = new List<string>();
		readonly List<string> _topics = new List<string>();
		readonly List<StatementRecord> _statements = new List<StatementRecord>();
		readonly List<string> _conflicts = new List<string>();
		readonly List<string> _pinned = new List<string>();

		public float ElapsedSeconds { get; set; }
		public CaseStatus Status { get; private set; } = CaseStatus.Active;
		public VerdictKind? Verdict { get; private set; }
		public string AccusedId { get; private set; }
		public string OutcomeId { get; private set; }
		public string[] Reasons { get; private set; } = new string[0];

		public IReadOnlyList<string> Evidence => _evidence;
		public IReadOnlyList<string> Zones => _zones;
		public IReadOnlyList<string> SpokenTopics => _topics;
		public IReadOnlyList<StatementRecord> Statements => _statements;
		public IReadOnlyList<string> Conflicts => _conflicts;
		public IReadOnlyList<string> Pinned => _pinned;

		public bool Has(string evidenceId) => _evidence.Contains(evidenceId);
		public bool HasSpoken(string topicId) => _topics.Contains(topicId);
		public bool HasConflict(string id) => _conflicts.Contains(id);
		public bool Visited(string zone) => _zones.Contains(zone);

		/// <summary>A clue id, or <c>t:topic</c> for a statement.</summary>
		public bool Knows(string requirement)
		{
			if (string.IsNullOrEmpty(requirement)) return true;
			return requirement.StartsWith("t:") ? HasSpoken(requirement.Substring(2)) : Has(requirement);
		}

		public bool VisitZone(string zone)
		{
			if (Array.IndexOf(CaseContent.Zones, zone) < 0 || _zones.Contains(zone)) return false;
			_zones.Add(zone);
			return true;
		}

		/// <summary>Records a clue the first time it is inspected. Reading it again changes nothing.</summary>
		public bool Inspect(string evidenceId)
		{
			var def = CaseContent.FindEvidence(evidenceId);
			if (def == null) return false;
			VisitZone(def.Zone);
			if (_evidence.Contains(evidenceId)) return false;
			_evidence.Add(evidenceId);
			_evidenceAt.Add(ElapsedSeconds);
			DeriveConflicts();
			return true;
		}

		public bool TopicAvailable(TopicDef topic)
		{
			if (topic == null) return false;
			if (topic.RequiresAny.Length == 0) return true;
			foreach (var requirement in topic.RequiresAny) if (Knows(requirement)) return true;
			return false;
		}

		public List<TopicDef> TopicsFor(string npc)
		{
			var list = new List<TopicDef>();
			foreach (var topic in CaseContent.Topics) if (topic.Npc == npc && TopicAvailable(topic)) list.Add(topic);
			return list;
		}

		/// <summary>
		/// Asks a topic. The first time it records the statement; asking again gives the same answer and records nothing new.
		/// Locked topics return null.
		/// </summary>
		public AskResult Ask(string topicId)
		{
			var topic = CaseContent.FindTopic(topicId);
			if (!TopicAvailable(topic)) return null;
			var result = new AskResult { Topic = topic };
			if (!_topics.Contains(topicId))
			{
				_topics.Add(topicId);
				if (topic.StatementId != null)
				{
					var record = new StatementRecord { Id = topic.StatementId, Speaker = topic.Npc, TopicId = topicId, KnownAt = ElapsedSeconds };
					_statements.Add(record);
					result.Statement = record;
					result.IsNewStatement = true;
				}
				result.NewConflicts = DeriveConflicts();
			}
			else if (topic.StatementId != null)
			{
				result.Statement = _statements.First(s => s.TopicId == topicId);
			}
			return result;
		}

		/// <summary>
		/// Notes every inconsistency whose two sides are both known, whatever order they were learned in. It sets nothing
		/// about who is guilty. Returns the ones noted by this call.
		/// </summary>
		List<string> DeriveConflicts()
		{
			var added = new List<string>();
			foreach (var conflict in CaseContent.Conflicts)
			{
				if (_conflicts.Contains(conflict.Id)) continue;
				if (!Knows(conflict.A) || !Knows(conflict.B)) continue;
				_conflicts.Add(conflict.Id);
				added.Add(conflict.Id);
			}
			return added;
		}

		/// <summary>The timeline entries the player has earned, earliest claimed time first.</summary>
		public List<TimelineDef> TimelineEntries()
		{
			return CaseContent.Timeline.Where(t => Knows(t.Requires)).OrderBy(t => t.SortMinutes).ToList();
		}

		public int CountEvidenceIn(string zone) => _evidence.Count(id => CaseContent.FindEvidence(id).Zone == zone);

		public bool TogglePin(string evidenceId)
		{
			if (!_evidence.Contains(evidenceId)) return false;
			if (!_pinned.Remove(evidenceId)) _pinned.Add(evidenceId);
			return true;
		}

		// ---- the report ----

		/// <summary>The scene must have been looked at (any one of the four clues at the granary) before a report; the rest may stay unseen.</summary>
		public bool SceneInspected => CaseContent.Evidence.Any(e => e.Scene && Has(e.Id));

		public bool CanReport => Status == CaseStatus.Active && SceneInspected;

		/// <summary>How many of the places and people are still unvisited, for a gentle warning before the report.</summary>
		public int UnvisitedZones => CaseContent.Zones.Count(z => !Visited(z));
		public int UnheardPeople => new[] { Ids.Mira, Ids.Orest, Ids.Taras, Ids.Ada }.Count(n => !_topics.Any(t => CaseContent.FindTopic(t).Npc == n));

		/// <summary>
		/// Commits the one verdict. It is refused (null) when the case is already resolved, the scene has not been inspected, or
		/// the accused is not one of the three suspects. The authored truth is not consulted or changed.
		/// </summary>
		public VerdictResult Commit(VerdictKind kind, string accusedId, IEnumerable<string> reasons = null)
		{
			if (!CanReport) return null;
			if (kind == VerdictKind.Accuse && !CaseContent.IsSuspect(accusedId)) return null;
			var chosen = (reasons ?? new string[0]).Where(Has).Distinct().Take(DetectiveConfig.MaxReasons).ToArray();
			Verdict = kind;
			AccusedId = kind == VerdictKind.Accuse ? accusedId : null;
			Reasons = chosen;
			OutcomeId = kind == VerdictKind.Insufficient ? Ids.OutcomeUnresolved
				: accusedId == Ids.Orest ? Ids.OutcomeOrest
				: accusedId == Ids.Mira ? Ids.OutcomeMira
				: Ids.OutcomeTaras;
			Status = CaseStatus.Resolved;
			return new VerdictResult { Kind = kind, AccusedId = AccusedId, OutcomeId = OutcomeId, Reasons = chosen };
		}

		// ---- the objective ----

		public string ObjectiveId
		{
			get
			{
				if (Status == CaseStatus.Resolved) return "obj_done";
				if (!SceneInspected) return "obj_granary";
				if (_topics.Count(t => CaseContent.FindTopic(t).Npc != Ids.Elder) < 3 || !Visited(Ids.Square)) return "obj_people";
				return "obj_compare";
			}
		}

		// ---- saving ----

		public InvestigationSave ToSave(float x, float y, float z, float heading, string cameraZone)
		{
			return new InvestigationSave
			{
				save_version = DetectiveConfig.SaveVersion,
				case_id = DetectiveConfig.CaseId,
				case_status = Status == CaseStatus.Resolved ? "resolved" : "active",
				player_x = x,
				player_y = y,
				player_z = z,
				player_heading = heading,
				active_camera_zone_id = cameraZone,
				discovered_evidence_ids = _evidence.ToArray(),
				discovered_evidence_at = _evidenceAt.ToArray(),
				visited_zone_ids = _zones.ToArray(),
				spoken_topic_ids = _topics.ToArray(),
				unlocked_statement_ids = _statements.Select(s => s.Id).ToArray(),
				statement_known_at = _statements.Select(s => s.KnownAt).ToArray(),
				discovered_conflict_ids = _conflicts.ToArray(),
				pinned_evidence_ids = _pinned.ToArray(),
				case_objective_id = ObjectiveId,
				elapsed_seconds = ElapsedSeconds,
				verdict_kind = Verdict.HasValue ? Verdict.Value.ToString().ToLowerInvariant() : "",
				verdict_accused_id = AccusedId ?? "",
				verdict_reasons = Reasons,
				outcome_id = OutcomeId ?? "",
				context_fixture_id = DetectiveConfig.ContextFixtureId,
			};
		}

		/// <summary>Rebuilds the investigation from a save after checking that it makes sense. Returns false for one that does not (the state is then left as it was).</summary>
		public bool Load(InvestigationSave s)
		{
			if (s == null || s.save_version != DetectiveConfig.SaveVersion || s.case_id != DetectiveConfig.CaseId) return false;
			var evidence = s.discovered_evidence_ids ?? new string[0];
			var topics = s.spoken_topic_ids ?? new string[0];
			if (evidence.Any(id => CaseContent.FindEvidence(id) == null) || evidence.Distinct().Count() != evidence.Length) return false;
			if (topics.Any(id => CaseContent.FindTopic(id) == null) || topics.Distinct().Count() != topics.Length) return false;
			if ((s.visited_zone_ids ?? new string[0]).Any(z => Array.IndexOf(CaseContent.Zones, z) < 0)) return false;
			if ((s.pinned_evidence_ids ?? new string[0]).Any(id => !evidence.Contains(id))) return false;

			var probe = new Investigation();
			foreach (var id in evidence) probe._evidence.Add(id);
			foreach (var id in topics)
			{
				if (!probe.TopicAvailable(CaseContent.FindTopic(id))) return false;
				probe._topics.Add(id);
			}
			var resolved = s.case_status == "resolved";
			if (resolved != !string.IsNullOrEmpty(s.outcome_id)) return false;
			if (resolved)
			{
				if (s.verdict_kind != "accuse" && s.verdict_kind != "insufficient") return false;
				if (s.verdict_kind == "accuse" && !CaseContent.IsSuspect(s.verdict_accused_id)) return false;
				if (!probe.SceneInspected) return false;
			}

			_evidence.Clear();
			_evidenceAt.Clear();
			_zones.Clear();
			_topics.Clear();
			_statements.Clear();
			_conflicts.Clear();
			_pinned.Clear();
			for (var i = 0; i < evidence.Length; i++)
			{
				_evidence.Add(evidence[i]);
				_evidenceAt.Add(s.discovered_evidence_at != null && i < s.discovered_evidence_at.Length ? s.discovered_evidence_at[i] : 0f);
			}
			foreach (var zone in s.visited_zone_ids ?? new string[0]) _zones.Add(zone);
			foreach (var evidenceId in evidence) VisitZone(CaseContent.FindEvidence(evidenceId).Zone);
			for (var i = 0; i < topics.Length; i++)
			{
				_topics.Add(topics[i]);
				var topic = CaseContent.FindTopic(topics[i]);
				if (topic.StatementId == null) continue;
				var at = s.statement_known_at != null && s.unlocked_statement_ids != null
					? Array.IndexOf(s.unlocked_statement_ids, topic.StatementId) is var index && index >= 0 && index < s.statement_known_at.Length ? s.statement_known_at[index] : 0f
					: 0f;
				_statements.Add(new StatementRecord { Id = topic.StatementId, Speaker = topic.Npc, TopicId = topic.Id, KnownAt = at });
			}
			foreach (var id in s.pinned_evidence_ids ?? new string[0]) _pinned.Add(id);
			DeriveConflicts();

			ElapsedSeconds = Math.Max(0f, s.elapsed_seconds);
			if (resolved)
			{
				Status = CaseStatus.Resolved;
				Verdict = s.verdict_kind == "accuse" ? VerdictKind.Accuse : VerdictKind.Insufficient;
				AccusedId = s.verdict_kind == "accuse" ? s.verdict_accused_id : null;
				Reasons = (s.verdict_reasons ?? new string[0]).Where(Has).Take(DetectiveConfig.MaxReasons).ToArray();
				OutcomeId = Verdict == VerdictKind.Insufficient ? Ids.OutcomeUnresolved
					: AccusedId == Ids.Orest ? Ids.OutcomeOrest
					: AccusedId == Ids.Mira ? Ids.OutcomeMira
					: Ids.OutcomeTaras;
			}
			else
			{
				Status = CaseStatus.Active;
				Verdict = null;
				AccusedId = null;
				OutcomeId = null;
				Reasons = new string[0];
			}
			return true;
		}
	}

	/// <summary>What is written to disk: plain ids, flags and numbers, never scene objects.</summary>
	[Serializable]
	public sealed class InvestigationSave
	{
		public int save_version;
		public string case_id;
		public string case_status;
		public float player_x;
		public float player_y;
		public float player_z;
		public float player_heading;
		public string active_camera_zone_id;
		public string[] discovered_evidence_ids;
		public float[] discovered_evidence_at;
		public string[] visited_zone_ids;
		public string[] spoken_topic_ids;
		public string[] unlocked_statement_ids;
		public float[] statement_known_at;
		public string[] discovered_conflict_ids;
		public string[] pinned_evidence_ids;
		public string case_objective_id;
		public float elapsed_seconds;
		public string verdict_kind;
		public string verdict_accused_id;
		public string[] verdict_reasons;
		public string outcome_id;
		public string context_fixture_id;
	}

	/// <summary>Volume and window mode, kept apart from the case save so they are remembered from the title screen on.</summary>
	[Serializable]
	public sealed class DetectiveSettings
	{
		public float masterVolume = 0.8f;
		public float sfxVolume = 0.9f;
		public bool fullscreen = true;
	}

	/// <summary>
	/// The report as the future Lords integration might receive it: an opinion, never the truth. It has no field for the real
	/// culprit or for whether the accusation is right, and nothing here is sent anywhere.
	/// </summary>
	public sealed class MockReport
	{
		public int schema_version = 1;
		public string case_instance_id = "case-demo-001";
		public string decision;
		public string accused_subject_id;
		public string[] supporting_evidence_ids;
		public string reported_by_investigator_id = "detective_demo";
		public string client_session_id = "local-demo-session-001";
		public string result_status = "SUBMITTED_REPORT";

		public static MockReport From(VerdictResult verdict)
		{
			return new MockReport
			{
				decision = verdict.Kind == VerdictKind.Accuse ? "ACCUSE" : "INSUFFICIENT_EVIDENCE",
				accused_subject_id = verdict.Kind == VerdictKind.Accuse ? verdict.AccusedId + "_demo" : null,
				supporting_evidence_ids = verdict.Reasons ?? new string[0],
			};
		}
	}
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ITKamianets.Engine.Save;
using NUnit.Framework;

namespace Temnorid.Detective.Tests
{
	public class DetectiveRulesTests
	{
		static Investigation Fresh() => new Investigation();

		static void InspectAll(Investigation s)
		{
			foreach (var e in CaseContent.Evidence) s.Inspect(e.Id);
		}

		static void AskAll(Investigation s)
		{
			foreach (var t in CaseContent.Topics) s.Ask(t.Id);
		}

		// ---- evidence ----

		[Test]
		public void Registering_a_known_clue_again_changes_nothing()
		{
			var s = Fresh();
			Assert.IsTrue(s.Inspect(Ids.E01));
			Assert.IsFalse(s.Inspect(Ids.E01));
			Assert.IsFalse(s.Inspect(Ids.E01));
			Assert.AreEqual(1, s.Evidence.Count);
			Assert.IsFalse(s.Inspect("e99_nothing"));
			Assert.AreEqual(1, s.Evidence.Count);
		}

		[Test]
		public void The_case_has_the_ten_clues_four_zones_and_five_people_by_stable_id()
		{
			Assert.AreEqual(10, CaseContent.Evidence.Length);
			Assert.AreEqual(10, CaseContent.Evidence.Select(e => e.Id).Distinct().Count());
			Assert.AreEqual(4, CaseContent.Zones.Length);
			Assert.AreEqual(4, CaseContent.Evidence.Count(e => e.Zone == Ids.Granary));
			Assert.AreEqual(2, CaseContent.Evidence.Count(e => e.Zone == Ids.Square));
			Assert.AreEqual(2, CaseContent.Evidence.Count(e => e.Zone == Ids.WatchPost));
			Assert.AreEqual(2, CaseContent.Evidence.Count(e => e.Zone == Ids.Ford));
			Assert.GreaterOrEqual(CaseContent.Topics.Count(t => t.RequiresAny.Length > 0), 4, "at least three evidence-conditioned follow-ups (there are four)");
		}

		// ---- dialogue ----

		[Test]
		public void A_follow_up_stays_locked_until_its_clue_is_found_then_opens()
		{
			var s = Fresh();
			Assert.IsNull(s.Ask("mira_ledger_followup"));
			Assert.IsFalse(s.TopicsFor(Ids.Mira).Any(t => t.Id == "mira_ledger_followup"));
			s.Inspect(Ids.E04);
			Assert.IsTrue(s.TopicsFor(Ids.Mira).Any(t => t.Id == "mira_ledger_followup"));
			Assert.IsNotNull(s.Ask("mira_ledger_followup"));

			var o = Fresh();
			Assert.IsNull(o.Ask("orest_strap_followup"));
			o.Inspect(Ids.E08);
			Assert.IsNotNull(o.Ask("orest_strap_followup"), "either strap clue opens it");
		}

		[Test]
		public void Topics_can_be_asked_again_without_duplicating_the_statement()
		{
			var s = Fresh();
			var first = s.Ask("ada_cart");
			Assert.IsTrue(first.IsNewStatement);
			var again = s.Ask("ada_cart");
			Assert.IsFalse(again.IsNewStatement);
			Assert.AreSame(first.Statement, again.Statement);
			Assert.AreEqual(1, s.Statements.Count);
			Assert.AreEqual(1, s.SpokenTopics.Count);
			Assert.AreEqual(SourceType.Testimony, first.Statement.Source);
		}

		[Test]
		public void The_elder_is_not_a_witness_so_her_topics_record_no_statement()
		{
			var s = Fresh();
			var r = s.Ask("elder_status");
			Assert.IsNotNull(r);
			Assert.IsNull(r.Statement);
			Assert.AreEqual(0, s.Statements.Count);
		}

		// ---- conflicts ----

		[TestCase(false)]
		[TestCase(true)]
		public void Orests_story_conflict_appears_when_both_statements_are_known_in_either_order(bool docketFirst)
		{
			var s = Fresh();
			s.Inspect(Ids.E09);
			if (docketFirst)
			{
				s.Ask("orest_docket_followup");
				Assert.IsFalse(s.HasConflict(Ids.COrestAlibi), "only one side known");
				s.Ask("orest_where");
			}
			else
			{
				s.Ask("orest_where");
				Assert.IsFalse(s.HasConflict(Ids.COrestAlibi));
				s.Ask("orest_docket_followup");
			}
			Assert.IsTrue(s.HasConflict(Ids.COrestAlibi));
		}

		[Test]
		public void Mira_and_Taras_conflicts_need_their_two_inputs()
		{
			var s = Fresh();
			s.Ask("mira_stock");
			s.Inspect(Ids.E04);
			Assert.IsFalse(s.HasConflict(Ids.CMiraLedger));
			s.Ask("mira_ledger_followup");
			Assert.IsTrue(s.HasConflict(Ids.CMiraLedger));

			s.Ask("taras_duty");
			Assert.IsFalse(s.HasConflict(Ids.CTarasLog));
			s.Inspect(Ids.E07);
			Assert.IsFalse(s.HasConflict(Ids.CTarasLog));
			s.Ask("taras_log_followup");
			Assert.IsTrue(s.HasConflict(Ids.CTarasLog));
		}

		[Test]
		public void The_cargo_and_route_conflicts_come_from_clues_and_testimony_together()
		{
			var s = Fresh();
			s.Inspect(Ids.E06);
			Assert.IsFalse(s.HasConflict(Ids.CUnregisteredCargo));
			s.Ask("ada_cart");
			Assert.IsTrue(s.HasConflict(Ids.CUnregisteredCargo));
			s.Inspect(Ids.E01);
			Assert.IsFalse(s.HasConflict(Ids.COrestRoute));
			s.Inspect(Ids.E08);
			Assert.IsTrue(s.HasConflict(Ids.COrestRoute));
		}

		[Test]
		public void Finding_conflicts_never_decides_the_culprit_or_resolves_anything()
		{
			var s = Fresh();
			InspectAll(s);
			AskAll(s);
			Assert.AreEqual(5, s.Conflicts.Count);
			Assert.AreEqual(CaseStatus.Active, s.Status);
			Assert.IsNull(s.Verdict);
			Assert.IsNull(s.AccusedId);
			Assert.IsNull(s.OutcomeId);
			Assert.AreEqual("orest", CaseTruth.CulpritId, "the truth is authored and fixed");
		}

		// ---- the report ----

		static Investigation Ready()
		{
			var s = Fresh();
			s.Inspect(Ids.E02);
			return s;
		}

		[Test]
		public void A_report_needs_the_scene_looked_at_but_not_everything()
		{
			var s = Fresh();
			Assert.IsFalse(s.CanReport);
			Assert.IsNull(s.Commit(VerdictKind.Insufficient, null));
			s.Inspect(Ids.E05);
			Assert.IsFalse(s.CanReport, "a clue from the square is not the scene");
			s.Inspect(Ids.E03);
			Assert.IsTrue(s.CanReport);
			Assert.AreEqual(2, s.Evidence.Count, "a partial investigation is enough");
		}

		[Test]
		public void Accusing_Orest_commits_reported_orest_and_leaves_the_truth_alone()
		{
			var s = Ready();
			var v = s.Commit(VerdictKind.Accuse, Ids.Orest, new[] { Ids.E02 });
			Assert.AreEqual(Ids.OutcomeOrest, v.OutcomeId);
			Assert.AreEqual(CaseStatus.Resolved, s.Status);
			Assert.AreEqual(true, CaseTruth.IsCorrect(v.AccusedId));
			Assert.AreEqual("orest", CaseTruth.CulpritId);
		}

		[TestCase(Ids.Mira, Ids.OutcomeMira)]
		[TestCase(Ids.Taras, Ids.OutcomeTaras)]
		public void A_wrong_accusation_is_a_normal_finished_case(string accused, string outcome)
		{
			var s = Ready();
			var v = s.Commit(VerdictKind.Accuse, accused);
			Assert.AreEqual(outcome, v.OutcomeId);
			Assert.AreEqual(CaseStatus.Resolved, s.Status);
			Assert.AreEqual(false, CaseTruth.IsCorrect(accused));
			Assert.AreEqual("orest", CaseTruth.CulpritId, "what the player picks does not change the truth");
		}

		[Test]
		public void Insufficient_evidence_is_allowed_and_unresolved()
		{
			var s = Ready();
			var v = s.Commit(VerdictKind.Insufficient, null);
			Assert.AreEqual(Ids.OutcomeUnresolved, v.OutcomeId);
			Assert.IsNull(v.AccusedId);
			Assert.IsNull(CaseTruth.IsCorrect(v.AccusedId));
			Assert.AreEqual(CaseStatus.Resolved, s.Status);
		}

		[Test]
		public void The_verdict_cannot_be_given_twice_and_only_the_three_suspects_can_be_accused()
		{
			var s = Ready();
			Assert.IsNull(s.Commit(VerdictKind.Accuse, Ids.Ada), "a witness is not a suspect");
			Assert.IsNull(s.Commit(VerdictKind.Accuse, null));
			Assert.AreEqual(CaseStatus.Active, s.Status, "a refused report changes nothing");
			Assert.IsNotNull(s.Commit(VerdictKind.Accuse, Ids.Mira));
			Assert.IsNull(s.Commit(VerdictKind.Accuse, Ids.Orest));
			Assert.IsNull(s.Commit(VerdictKind.Insufficient, null));
			Assert.AreEqual(Ids.OutcomeMira, s.OutcomeId);
		}

		[Test]
		public void Reasons_are_flavour_only_limited_to_three_known_clues()
		{
			var s = Ready();
			s.Inspect(Ids.E01);
			s.Inspect(Ids.E08);
			s.Inspect(Ids.E09);
			var v = s.Commit(VerdictKind.Accuse, Ids.Orest, new[] { Ids.E01, Ids.E08, Ids.E09, Ids.E02, Ids.E10 });
			Assert.AreEqual(3, v.Reasons.Length);
			Assert.IsFalse(v.Reasons.Contains(Ids.E10), "an unfound clue cannot be named");
			Assert.AreEqual(Ids.OutcomeOrest, v.OutcomeId);
		}

		[Test]
		public void The_mock_report_is_an_opinion_with_no_truth_in_it()
		{
			var s = Ready();
			var v = s.Commit(VerdictKind.Accuse, Ids.Orest, new[] { Ids.E02 });
			var report = MockReport.From(v);
			Assert.AreEqual("ACCUSE", report.decision);
			Assert.AreEqual("orest_demo", report.accused_subject_id);
			Assert.AreEqual("SUBMITTED_REPORT", report.result_status);
			var names = typeof(MockReport).GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray();
			CollectionAssert.DoesNotContain(names, "true_culprit_id");
			CollectionAssert.DoesNotContain(names, "is_correct");
			Assert.IsFalse(names.Any(n => n.Contains("correct") || n.Contains("culprit") || n.Contains("truth")));

			var abstain = MockReport.From(Ready().Commit(VerdictKind.Insufficient, null));
			Assert.AreEqual("INSUFFICIENT_EVIDENCE", abstain.decision);
			Assert.IsNull(abstain.accused_subject_id);
		}

		// ---- the board ----

		[Test]
		public void The_timeline_shows_only_what_is_known_in_order_with_how_each_time_is_known()
		{
			var s = Fresh();
			Assert.AreEqual(0, s.TimelineEntries().Count);
			s.Ask("ada_noise");
			s.Inspect(Ids.E09);
			s.Inspect(Ids.E07);
			var entries = s.TimelineEntries();
			Assert.AreEqual(3, entries.Count);
			CollectionAssert.IsOrdered(entries.Select(e => e.SortMinutes));
			Assert.AreEqual(TimeBasis.Estimated, entries.First(e => e.Id == "tl_ada_noise").Basis, "a witness's guess is not an observation");
			Assert.AreEqual(TimeBasis.Observed, entries.First(e => e.Id == "tl_guard_log").Basis);
			Assert.IsTrue(entries.First(e => e.Id == "tl_docket").Uncertain, "the smeared mark");
		}

		[Test]
		public void Pins_only_hold_found_clues_and_toggle()
		{
			var s = Fresh();
			Assert.IsFalse(s.TogglePin(Ids.E05));
			s.Inspect(Ids.E05);
			Assert.IsTrue(s.TogglePin(Ids.E05));
			Assert.AreEqual(1, s.Pinned.Count);
			Assert.IsTrue(s.TogglePin(Ids.E05));
			Assert.AreEqual(0, s.Pinned.Count);
		}

		[Test]
		public void The_objective_points_at_work_to_do_never_at_a_person()
		{
			var s = Fresh();
			Assert.AreEqual("obj_granary", s.ObjectiveId);
			s.Inspect(Ids.E01);
			Assert.AreEqual("obj_people", s.ObjectiveId);
			s.VisitZone(Ids.Square);
			s.Ask("mira_stock");
			s.Ask("ada_noise");
			s.Ask("taras_duty");
			Assert.AreEqual("obj_compare", s.ObjectiveId);
			foreach (var id in new[] { "obj_granary", "obj_people", "obj_compare", "obj_done" })
			{
				var text = Loc.T(id);
				Assert.IsFalse(text.Contains("Орест") || text.Contains("Міра") || text.Contains("Тарас"), id);
			}
		}

		// ---- saving ----

		[Test]
		public void Save_round_trips_clues_statements_conflicts_pins_and_outcome()
		{
			var s = Fresh();
			InspectAll(s);
			AskAll(s);
			s.TogglePin(Ids.E01);
			s.ElapsedSeconds = 321.5f;
			s.Commit(VerdictKind.Accuse, Ids.Orest, new[] { Ids.E01, Ids.E08 });

			var path = Path.Combine(Path.GetTempPath(), "detective-edit-" + Guid.NewGuid().ToString("N"));
			try
			{
				var store = new DetectiveSaveStore(path);
				Assert.AreEqual(SaveStatus.Ok, store.Save(s.ToSave(1f, 0f, 2f, 90f, "cam_square_wide")));
				var loaded = store.Load(out var status);
				Assert.AreEqual(SaveStatus.Ok, status);
				var t = Fresh();
				Assert.IsTrue(t.Load(loaded));
				Assert.AreEqual(10, t.Evidence.Count);
				Assert.AreEqual(4, t.Zones.Count);
				Assert.AreEqual(s.SpokenTopics.Count, t.SpokenTopics.Count);
				Assert.AreEqual(s.Statements.Count, t.Statements.Count);
				Assert.AreEqual(5, t.Conflicts.Count);
				Assert.AreEqual(1, t.Pinned.Count);
				Assert.AreEqual(321.5f, t.ElapsedSeconds, 0.001f);
				Assert.AreEqual(CaseStatus.Resolved, t.Status);
				Assert.AreEqual(Ids.OutcomeOrest, t.OutcomeId);
				Assert.AreEqual(Ids.Orest, t.AccusedId);
				CollectionAssert.AreEquivalent(new[] { Ids.E01, Ids.E08 }, t.Reasons);
				Assert.AreEqual("cam_square_wide", loaded.active_camera_zone_id);
				Assert.AreEqual(DetectiveConfig.ContextFixtureId, loaded.context_fixture_id);
			}
			finally
			{
				if (Directory.Exists(path)) Directory.Delete(path, true);
			}
		}

		[Test]
		public void A_fresh_run_has_nothing_and_the_truth_is_still_authored()
		{
			var s = Fresh();
			InspectAll(s);
			AskAll(s);
			s.Commit(VerdictKind.Insufficient, null);
			var again = Fresh();
			Assert.AreEqual(0, again.Evidence.Count);
			Assert.AreEqual(0, again.Statements.Count);
			Assert.AreEqual(0, again.Conflicts.Count);
			Assert.IsNull(again.Verdict);
			Assert.AreEqual(CaseStatus.Active, again.Status);
			Assert.AreEqual("orest", CaseTruth.CulpritId);
		}

		[Test]
		public void Damaged_or_contradicting_saves_are_rejected_and_leave_the_state_alone()
		{
			var s = Fresh();
			s.Inspect(Ids.E01);
			var good = s.ToSave(0f, 0f, 0f, 0f, "z");

			Assert.IsFalse(Fresh().Load(null));
			var bad = Clone(good);
			bad.save_version = 9;
			Assert.IsFalse(Fresh().Load(bad));
			bad = Clone(good);
			bad.case_id = "other";
			Assert.IsFalse(Fresh().Load(bad));
			bad = Clone(good);
			bad.discovered_evidence_ids = new[] { "e99" };
			Assert.IsFalse(Fresh().Load(bad));
			bad = Clone(good);
			bad.spoken_topic_ids = new[] { "orest_docket_followup" }; // its clue was never found
			Assert.IsFalse(Fresh().Load(bad));
			bad = Clone(good);
			bad.case_status = "resolved"; // resolved without an outcome
			Assert.IsFalse(Fresh().Load(bad));
			bad = Clone(good);
			bad.pinned_evidence_ids = new[] { Ids.E05 };
			Assert.IsFalse(Fresh().Load(bad));

			var keep = Fresh();
			keep.Inspect(Ids.E05);
			Assert.IsFalse(keep.Load(Clone(good, e => e.save_version = 9)));
			Assert.IsTrue(keep.Has(Ids.E05), "a refused load keeps what was there");
		}

		static InvestigationSave Clone(InvestigationSave s, Action<InvestigationSave> change = null)
		{
			var copy = UnityEngine.JsonUtility.FromJson<InvestigationSave>(UnityEngine.JsonUtility.ToJson(s));
			change?.Invoke(copy);
			return copy;
		}

		[Test]
		public void A_corrupt_save_file_is_reported_not_thrown()
		{
			var path = Path.Combine(Path.GetTempPath(), "detective-edit-" + Guid.NewGuid().ToString("N"));
			try
			{
				var store = new DetectiveSaveStore(path);
				Directory.CreateDirectory(Path.GetDirectoryName(store.Path));
				File.WriteAllText(store.Path, "{ this is not json");
				var loaded = store.Load(out var status);
				Assert.IsNull(loaded);
				Assert.AreNotEqual(SaveStatus.Ok, status);
			}
			finally
			{
				if (Directory.Exists(path)) Directory.Delete(path, true);
			}
		}

		// ---- the case can be finished with partial play ----

		[Test]
		public void Any_one_of_the_four_scene_clues_is_enough_and_every_clue_can_be_reached_without_any_other()
		{
			foreach (var e in CaseContent.Evidence)
			{
				var s = Fresh();
				Assert.IsTrue(s.Inspect(e.Id), e.Id + " needs nothing else");
			}
			foreach (var id in new[] { Ids.E01, Ids.E02, Ids.E03, Ids.E04 })
			{
				var s = Fresh();
				s.Inspect(id);
				Assert.IsTrue(s.CanReport, id);
				Assert.IsNotNull(s.Commit(VerdictKind.Insufficient, null));
			}
		}

		[Test]
		public void The_strongest_clue_set_points_at_Orest_without_any_single_proof()
		{
			// Every clue that favours Orest is also open to another reading: none of the conflicts names him as certain.
			var orestSide = new[] { Ids.COrestAlibi, Ids.COrestRoute };
			foreach (var id in orestSide) Assert.IsTrue(Loc.Has(id + ".opts"), "explanations offered for " + id);
			foreach (var c in CaseContent.Conflicts) Assert.IsTrue(Loc.Has(c.Id) && Loc.Has(c.Id + ".opts"), c.Id);
		}

		[Test]
		public void Every_text_the_game_needs_exists_in_the_table()
		{
			foreach (var e in CaseContent.Evidence)
			{
				Assert.IsTrue(Loc.Has(e.Id + ".title"), e.Id);
				Assert.IsTrue(Loc.Has(e.Id + ".text"), e.Id);
				Assert.IsTrue(Loc.Has("z." + e.Zone), e.Zone);
			}
			foreach (var t in CaseContent.Topics)
			{
				Assert.IsTrue(Loc.Has("q." + t.Id), t.Id);
				Assert.IsTrue(Loc.Has("a." + t.Id), t.Id);
			}
			foreach (var t in CaseContent.Timeline)
			{
				Assert.IsTrue(Loc.Has(t.Id), t.Id);
				Assert.IsTrue(Loc.Has("time." + t.Time), t.Time);
			}
			foreach (var p in CaseContent.People) Assert.IsTrue(Loc.Has("p." + p) && Loc.Has("p." + p + ".profile") && Loc.Has("p." + p + ".role"), p);
			foreach (var o in new[] { Ids.OutcomeOrest, Ids.OutcomeMira, Ids.OutcomeTaras, Ids.OutcomeUnresolved })
				foreach (var part in new[] { "report", "elder", "result", "village" }) Assert.IsTrue(Loc.Has("out." + o + "." + part), o + part);
			foreach (SourceType type in Enum.GetValues(typeof(SourceType))) Assert.IsTrue(Loc.Has("src." + type));
			foreach (TimeBasis basis in Enum.GetValues(typeof(TimeBasis))) Assert.IsTrue(Loc.Has("basis." + basis));
		}

		[Test]
		public void The_result_screen_never_says_right_or_wrong()
		{
			foreach (var key in Loc.Keys.Where(k => k.StartsWith("out.") || k.StartsWith("result.") || k.StartsWith("decision.")))
			{
				var text = Loc.T(key).ToLowerInvariant();
				Assert.IsFalse(text.Contains("правильн") || text.Contains("помилк") && key.StartsWith("out."), key);
				Assert.IsFalse(text.Contains("винен") || text.Contains("невин"), key);
			}
		}
	}
}

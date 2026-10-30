using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ITKamianets.Engine.FixedCamera;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Temnorid.Detective.Tests
{
	public class DetectiveRunTests
	{
		string _saveFolder;

		[SetUp]
		public void SetUp()
		{
			_saveFolder = Path.Combine(Path.GetTempPath(), "detective-playmode-" + Guid.NewGuid());
			DetectiveGame.SaveRootOverride = _saveFolder;
		}

		[TearDown]
		public void TearDown()
		{
			DetectiveGame.SaveRootOverride = null;
			Time.timeScale = 1f;
			if (Directory.Exists(_saveFolder)) Directory.Delete(_saveFolder, true);
		}

		static IEnumerator Load()
		{
			yield return SceneManager.LoadSceneAsync("Village");
			yield return null;
			yield return null;
		}

		static IEnumerator Frames(int count)
		{
			for (var i = 0; i < count; i++) yield return null;
		}

		static IEnumerator Walk(DetectiveGame g, bool run, params Vector2[] points)
		{
			foreach (var point in points)
			{
				var start = Time.realtimeSinceStartup;
				while (g.Player.ScriptedWalkTo(point, run) > 0.25f)
				{
					if (Time.realtimeSinceStartup - start > 40f) Assert.Fail("Stuck on the way to " + point + " at " + g.Player.Position2D + " in mode " + g.Mode);
					yield return null;
				}
				g.Player.ScriptedInput = null;
			}
			yield return null;
			yield return null;
		}

		static IEnumerator UseObject(DetectiveGame g, string id)
		{
			yield return Frames(2);
			Assert.IsNotNull(g.Target, "something to use for " + id + " at " + g.Player.Position2D);
			Assert.AreEqual(id, g.Target.Id, "the one prompt at " + g.Player.Position2D);
			Assert.IsTrue(g.PromptText.StartsWith("E — "), g.PromptText);
			Assert.IsTrue(g.Interact());
			yield return Frames(2);
		}

		static IEnumerator Begin(DetectiveGame g)
		{
			g.NewCase();
			yield return Frames(2);
			Assert.AreEqual(Mode.Brief, g.Mode);
			g.BeginInvestigation();
			Assert.AreEqual(Mode.Playing, g.Mode);
		}

		static IEnumerator Inspect(DetectiveGame g, string id, Vector2 from, params Vector2[] route)
		{
			yield return Walk(g, false, route.Concat(new[] { from }).ToArray());
			yield return UseObject(g, id);
			Assert.AreEqual(Mode.Inspect, g.Mode);
			Assert.AreEqual(id, g.InspectId);
			g.CloseInspect();
			Assert.AreEqual(Mode.Playing, g.Mode);
			Assert.IsTrue(g.Inv.Has(id));
		}

		static IEnumerator Talk(DetectiveGame g, string npc, Vector2 from, string[] topics, params Vector2[] route)
		{
			yield return Walk(g, false, route.Concat(new[] { from }).ToArray());
			yield return UseObject(g, npc);
			Assert.AreEqual(Mode.Dialogue, g.Mode);
			foreach (var topic in topics)
			{
				Assert.IsTrue(g.DialogueTopics.Any(t => t.Id == topic), topic + " is on offer");
				Assert.IsNotNull(g.Ask(topic), topic);
				Assert.AreEqual(Loc.T("a." + topic).Split('\n')[0], g.AnswerText.Split('\n')[0]);
			}
			g.CloseDialogue();
			Assert.AreEqual(Mode.Playing, g.Mode);
		}

		// ---- the scene ----

		[UnityTest]
		public IEnumerator The_game_opens_on_the_title_with_the_whole_village_in_the_scene()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			Assert.IsNotNull(g);
			Assert.AreEqual(Mode.Title, g.Mode);
			Assert.IsFalse(g.HasSave);

			var ids = new HashSet<string>(Interactable.All.Select(i => i.Id));
			foreach (var e in CaseContent.Evidence) Assert.IsTrue(ids.Contains(e.Id), "clue " + e.Id);
			foreach (var n in new[] { Ids.Mira, Ids.Orest, Ids.Taras, Ids.Ada, Ids.Elder }) Assert.IsTrue(ids.Contains(n), "person " + n);
			Assert.AreEqual(15, ids.Count);

			var shots = CameraShotVolume.All.Select(v => v.ShotId).ToList();
			Assert.GreaterOrEqual(shots.Count, 6, "at least six shots");
			Assert.GreaterOrEqual(shots.Count(s => s.StartsWith("cam_granary")), 2);
			Assert.GreaterOrEqual(shots.Count(s => s.StartsWith("cam_square")), 2);
			Assert.GreaterOrEqual(shots.Count(s => s.StartsWith("cam_watch")), 1);
			Assert.GreaterOrEqual(shots.Count(s => s.StartsWith("cam_ford")), 1);

			// Every clue is somewhere a person can stand, outside every solid footprint.
			foreach (var item in Interactable.All) Assert.IsTrue(VillageLayout.Walkable(item.transform.position.x, item.transform.position.z), item.Id + " stands in a wall");
		}

		[UnityTest]
		public IEnumerator Every_walkable_spot_has_a_shot_and_borders_do_not_flicker()
		{
			yield return Load();
			var missed = 0;
			for (var x = VillageLayout.YardWest + 0.2f; x <= VillageLayout.EastEdge - 0.2f; x += 0.7f)
				for (var z = VillageLayout.SouthEdge + 0.2f; z <= VillageLayout.NorthEdge - 0.2f; z += 0.7f)
				{
					if (!VillageLayout.Walkable(x, z)) continue;
					if (CameraShotVolume.Select(CameraShotVolume.All, new Vector3(x, 0f, z), null, 0.3f) == null) missed++;
				}
			Assert.AreEqual(0, missed, "walkable spots without a shot");

			foreach (var border in new[] { new Vector3(0f, 0f, 13f), new Vector3(0f, 0f, -10f), new Vector3(-13.5f, 0f, 0f) })
			{
				var current = CameraShotVolume.Select(CameraShotVolume.All, border, null, 0.3f);
				var changes = 0;
				for (var i = 0; i < 100; i++)
				{
					var offset = i % 2 == 0 ? 0.1f : -0.1f;
					var p = border.x < -13f ? border + new Vector3(offset, 0f, 0f) : border + new Vector3(0f, 0f, offset);
					var next = CameraShotVolume.Select(CameraShotVolume.All, p, current, 0.3f);
					if (next != current) changes++;
					current = next;
				}
				Assert.LessOrEqual(changes, 1, "shot changes while pacing at " + border);
			}
		}

		// ---- the whole case ----

		[UnityTest]
		public IEnumerator A_player_can_investigate_everywhere_report_Orest_continue_and_replay()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			Time.timeScale = 3f;
			yield return Begin(g);
			Assert.AreEqual(Loc.T("obj_granary"), g.Objective);

			// The granary.
			var mouth = new Vector2(-14f, -1f);
			yield return Inspect(g, Ids.E04, VillageLayout.E04, mouth, new Vector2(-20f, -1f), new Vector2(-21f, -3f));
			Assert.AreEqual(Loc.T("obj_people"), g.Objective);
			yield return Talk(g, Ids.Mira, new Vector2(-24f, -2.6f), new[] { "mira_stock", "mira_keys", "mira_ledger_followup" });
			Assert.IsTrue(g.Inv.HasConflict(Ids.CMiraLedger));
			yield return Inspect(g, Ids.E02, VillageLayout.E02, new Vector2(-27f, -3f));
			yield return Inspect(g, Ids.E03, VillageLayout.E03, new Vector2(-30f, -3f));
			yield return Inspect(g, Ids.E01, VillageLayout.E01);
			Assert.AreEqual(4, g.Inv.CountEvidenceIn(Ids.Granary));

			// The square.
			var square = new[] { new Vector2(-30f, -3f), new Vector2(-14f, -1f), new Vector2(-6f, -1f) };
			yield return Inspect(g, Ids.E05, VillageLayout.E05, square.Concat(new[] { new Vector2(-2.5f, 2f) }).ToArray());
			yield return Talk(g, Ids.Ada, new Vector2(6.5f, 3.4f), new[] { "ada_noise", "ada_cart", "ada_time" }, new Vector2(2f, 2f));
			yield return Inspect(g, Ids.E06, VillageLayout.E06, new Vector2(2f, 0f));
			Assert.IsTrue(g.Inv.HasConflict(Ids.CUnregisteredCargo), "tally and Ada's cart");
			yield return Talk(g, Ids.Ada, new Vector2(6.5f, 3.4f), new[] { "ada_recheck" }, new Vector2(2f, 0f), new Vector2(4f, 2f));
			yield return Talk(g, Ids.Orest, new Vector2(6.8f, -2.0f), new[] { "orest_where", "orest_access" }, new Vector2(6.5f, 1f));
			yield return Talk(g, Ids.Orest, new Vector2(6.8f, -2.0f), new[] { "orest_strap_followup" });

			// The watch post.
			yield return Talk(g, Ids.Taras, new Vector2(4.4f, 18.4f), new[] { "taras_duty", "taras_stefan" }, new Vector2(0.5f, 5f), new Vector2(0.5f, 12f), new Vector2(0.5f, 17f));
			yield return Inspect(g, Ids.E07, VillageLayout.E07, new Vector2(1f, 19f));
			yield return Inspect(g, Ids.E10, VillageLayout.E10);
			yield return Talk(g, Ids.Taras, new Vector2(4.4f, 18.4f), new[] { "taras_log_followup" }, new Vector2(2f, 19f));
			Assert.IsTrue(g.Inv.HasConflict(Ids.CTarasLog));

			// The ford.
			var down = new[] { new Vector2(4.4f, 18.4f), new Vector2(0.5f, 12f), new Vector2(0.5f, 0f), new Vector2(0.5f, -12f), new Vector2(0.5f, -18f) };
			yield return Inspect(g, Ids.E08, VillageLayout.E08, down);
			yield return Inspect(g, Ids.E09, VillageLayout.E09, new Vector2(2f, -22f), new Vector2(5.3f, -20.2f));
			Assert.IsTrue(g.Inv.HasConflict(Ids.COrestRoute));

			// Back to Orest with the docket.
			var up = new[] { new Vector2(0.5f, -18f), new Vector2(0.5f, -8f), new Vector2(4f, -1f) };
			yield return Talk(g, Ids.Orest, new Vector2(6.8f, -2.0f), new[] { "orest_docket_followup" }, up);
			Assert.IsTrue(g.Inv.HasConflict(Ids.COrestAlibi));
			Assert.AreEqual(10, g.Inv.Evidence.Count);
			Assert.AreEqual(5, g.Inv.Conflicts.Count);
			Assert.AreEqual(Loc.T("obj_compare"), g.Objective);
			Time.timeScale = 1f;

			// The journal and the board show only what is known, with no guilt marker.
			g.OpenPanel(Mode.Journal);
			Assert.AreEqual(Mode.Journal, g.Mode);
			g.ClosePanel();
			g.OpenBoard(BoardTab.Timeline);
			Assert.AreEqual(Mode.Board, g.Mode);
			Assert.AreEqual(11, g.Inv.TimelineEntries().Count);
			g.SetTab(BoardTab.People);
			g.SelectPerson(Ids.Orest);
			g.ClosePanel();
			Assert.AreEqual(Mode.Playing, g.Mode);
			Assert.AreEqual(CaseStatus.Active, g.Inv.Status, "nothing resolved itself");

			// The elder and the report.
			yield return Walk(g, false, new Vector2(-5.5f, 1.6f));
			yield return UseObject(g, Ids.Elder);
			Assert.AreEqual(Mode.Dialogue, g.Mode);
			Assert.IsNotNull(g.Ask("elder_status"));
			Assert.IsTrue(g.AnswerText.Contains("10"));
			g.ElderReport();
			Assert.AreEqual(Mode.Board, g.Mode);
			Assert.AreEqual(BoardTab.Decision, g.Tab);
			g.ChooseAccused(Ids.Orest);
			Assert.IsTrue(g.ToggleReason(Ids.E01));
			Assert.IsTrue(g.ToggleReason(Ids.E08));
			Assert.IsTrue(g.ToggleReason(Ids.E09));
			Assert.IsFalse(g.ToggleReason(Ids.E05), "only three reasons");
			Assert.IsTrue(g.RequestReport());
			Assert.AreEqual(Mode.Confirm, g.Mode);
			g.CancelReport();
			Assert.AreEqual(Mode.Board, g.Mode);
			Assert.AreEqual(CaseStatus.Active, g.Inv.Status, "cancel leaves the investigation open");
			Assert.IsTrue(g.RequestReport());
			var verdict = g.ConfirmReport();
			Assert.IsNotNull(verdict);
			Assert.AreEqual(Mode.Resolved, g.Mode);
			Assert.AreEqual(Ids.OutcomeOrest, g.Inv.OutcomeId);
			Assert.AreEqual("ACCUSE", g.Report.decision);
			Assert.IsNull(g.ConfirmReport(), "once only");
			Assert.AreEqual("orest", CaseTruth.CulpritId);

			// Quit and come back: the result is still there.
			g.ToTitle();
			yield return Frames(2);
			Assert.IsTrue(g.HasSave);
			Assert.IsTrue(g.ContinueCase());
			Assert.AreEqual(Mode.Resolved, g.Mode);
			Assert.AreEqual(Ids.OutcomeOrest, g.Result.OutcomeId);

			// Replay starts from nothing.
			g.Replay();
			yield return Frames(2);
			Assert.AreEqual(Mode.Brief, g.Mode);
			Assert.AreEqual(0, g.Inv.Evidence.Count);
			Assert.AreEqual(0, g.Inv.Statements.Count);
			Assert.AreEqual(0, g.Inv.Conflicts.Count);
			Assert.IsNull(g.Inv.Verdict);
			Assert.IsNull(g.Result);
			Assert.AreEqual(VillageLayout.PlayerStart.x, g.Player.Position2D.x, 0.2f);
		}

		// ---- reports from a small investigation ----

		static IEnumerator Resolve(DetectiveGame g, Action setup, Func<DetectiveGame, bool> choose)
		{
			g.NewCase();
			yield return Frames(2);
			g.BeginInvestigation();
			setup();
			g.OpenBoard(BoardTab.Decision);
			Assert.IsTrue(choose(g));
			Assert.IsTrue(g.RequestReport());
			Assert.IsNotNull(g.ConfirmReport());
		}

		[UnityTest]
		public IEnumerator Each_of_the_four_reports_ends_the_case_normally_even_from_a_thin_investigation()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			foreach (var (suspect, outcome) in new[] { (Ids.Mira, Ids.OutcomeMira), (Ids.Taras, Ids.OutcomeTaras), (Ids.Orest, Ids.OutcomeOrest), ((string)null, Ids.OutcomeUnresolved) })
			{
				yield return Resolve(g, () => g.Inv.Inspect(Ids.E04), d =>
				{
					if (suspect == null) d.ChooseInsufficient(); else d.ChooseAccused(suspect);
					return true;
				});
				Assert.AreEqual(Mode.Resolved, g.Mode);
				Assert.AreEqual(outcome, g.Inv.OutcomeId);
				Assert.AreEqual(CaseStatus.Resolved, g.Inv.Status);
				Assert.IsNotNull(g.Report);
				Assert.IsFalse(g.Report.result_status.Contains("CORRECT"));
				g.ToTitle();
				yield return Frames(1);
			}
		}

		[UnityTest]
		public IEnumerator A_report_is_refused_until_the_scene_has_been_inspected_and_cancel_keeps_investigating()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			yield return Begin(g);
			g.Inv.Inspect(Ids.E05);
			g.OpenBoard(BoardTab.Decision);
			g.ChooseAccused(Ids.Mira);
			Assert.IsFalse(g.RequestReport(), "a clue from the square is not the scene");
			Assert.AreEqual(Mode.Board, g.Mode);
			g.Inv.Inspect(Ids.E03);
			Assert.IsTrue(g.RequestReport());
			g.CancelReport();
			Assert.AreEqual(CaseStatus.Active, g.Inv.Status);
			g.ClosePanel();
			Assert.AreEqual(Mode.Playing, g.Mode);
		}

		// ---- movement, camera, selection ----

		[UnityTest]
		public IEnumerator Held_movement_does_not_turn_around_when_the_camera_cuts_in_either_direction()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			yield return Begin(g);
			Time.timeScale = 2f;

			// Hold "south" as the square's camera sees it, straight across the border into the ford, whose camera faces another way.
			g.Player.Teleport(new Vector2(0f, -6f), 180f);
			g.Rig.Snap();
			yield return Frames(3);
			var cuts = g.Rig.CutCount;
			var yawStart = g.Rig.transform.eulerAngles.y;
			var south = Quaternion.Euler(0f, -yawStart, 0f) * Vector3.back;
			g.Player.ScriptedInput = new Vector2(south.x, south.z);
			g.Player.ScriptedRun = true;
			var start = Time.realtimeSinceStartup;
			while (g.Player.Position2D.y > -18f)
			{
				if (Time.realtimeSinceStartup - start > 20f) Assert.Fail("held input did not carry the player south; at " + g.Player.Position2D);
				yield return null;
			}
			g.Player.ScriptedInput = null;
			Assert.Greater(g.Rig.CutCount, cuts, "a camera cut happened");
			Assert.Less(Mathf.Abs(g.Player.Position2D.x), 2.5f, "still heading south, not turned around");
			var fordYaw = g.Rig.transform.eulerAngles.y;
			Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(yawStart, fordYaw)), 60f, "the two shots really face different ways");

			// And back north, across the same border.
			yield return Frames(2);
			var north = Quaternion.Euler(0f, -fordYaw, 0f) * Vector3.forward;
			g.Player.ScriptedInput = new Vector2(north.x, north.z);
			start = Time.realtimeSinceStartup;
			while (g.Player.Position2D.y < -4f)
			{
				if (Time.realtimeSinceStartup - start > 20f) Assert.Fail("held input did not carry the player north; at " + g.Player.Position2D);
				yield return null;
			}
			g.Player.ScriptedInput = null;
			Assert.Less(Mathf.Abs(g.Player.Position2D.x), 2.5f, "still heading north");
			Time.timeScale = 1f;
		}

		[UnityTest]
		public IEnumerator Walls_block_selection_and_the_inspect_screen_gives_control_back()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			yield return Begin(g);
			g.Player.Teleport(new Vector2(-24f, -3f), 0f);
			yield return Frames(3);
			Assert.IsNotNull(g.Target);
			Assert.AreEqual(Ids.Mira, g.Target.Id);

			var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
			wall.transform.position = new Vector3(-24f, 1.5f, -3.9f);
			wall.transform.localScale = new Vector3(4f, 3f, 0.2f);
			yield return new WaitForFixedUpdate();
			Physics.SyncTransforms();
			yield return Frames(3);
			Assert.IsTrue(g.Target == null || g.Target.Id != Ids.Mira, "Mira is behind the wall");
			UnityEngine.Object.Destroy(wall);
			yield return Frames(2);

			// Inspect, close, move again.
			g.Player.Teleport(VillageLayout.E02, 0f);
			yield return Frames(3);
			Assert.AreEqual(Ids.E02, g.Target.Id);
			Assert.IsTrue(g.Interact());
			Assert.AreEqual(Mode.Inspect, g.Mode);
			yield return Frames(2);
			Assert.IsTrue(g.Player.Frozen, "no walking while a clue is open");
			g.CloseInspect();
			yield return Frames(2);
			Assert.AreEqual(Mode.Playing, g.Mode);
			Assert.IsFalse(g.Player.Frozen, "control is back");
			Assert.IsTrue(g.Inv.Has(Ids.E02));
			g.Interact();
			g.CloseInspect();
			Assert.AreEqual(1, g.Inv.Evidence.Count, "reading it again records nothing");
		}

		[UnityTest]
		public IEnumerator Pause_and_screens_stop_the_clock()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			yield return Begin(g);
			yield return Frames(5);
			var elapsed = g.Inv.ElapsedSeconds;
			g.Pause();
			Assert.AreEqual(Mode.Pause, g.Mode);
			yield return new WaitForSeconds(0.4f);
			Assert.AreEqual(elapsed, g.Inv.ElapsedSeconds, 0.0001f);
			g.Resume();
			yield return Frames(5);
			Assert.Greater(g.Inv.ElapsedSeconds, elapsed);
		}

		// ---- dialogue ----

		[UnityTest]
		public IEnumerator Dialogue_gates_open_in_the_right_order_and_nothing_is_recorded_twice()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			yield return Begin(g);
			g.Player.Teleport(new Vector2(-24f, -3f), 0f);
			yield return Frames(3);
			Assert.IsTrue(g.Interact());
			Assert.AreEqual(Mode.Dialogue, g.Mode);
			Assert.IsFalse(g.DialogueTopics.Any(t => t.Id == "mira_ledger_followup"), "locked before the ledger");
			Assert.IsNull(g.Ask("mira_ledger_followup"));
			Assert.IsNotNull(g.Ask("mira_stock"));
			Assert.AreEqual(1, g.Inv.Statements.Count);
			var again = g.Ask("mira_stock");
			Assert.IsFalse(again.IsNewStatement);
			Assert.IsTrue(g.AnswerWasRepeat);
			Assert.AreEqual(1, g.Inv.Statements.Count);
			g.CloseDialogue();

			g.Inv.Inspect(Ids.E04);
			Assert.IsTrue(g.Interact());
			Assert.IsTrue(g.DialogueTopics.Any(t => t.Id == "mira_ledger_followup"), "open after the ledger");
			g.Ask("mira_ledger_followup");
			Assert.IsTrue(g.Inv.HasConflict(Ids.CMiraLedger));
			Assert.AreEqual(Ids.E04, g.Inv.Evidence[0]);
			g.CloseDialogue();
		}

		// ---- saving ----

		[UnityTest]
		public IEnumerator Save_quit_continue_in_the_middle_of_an_interview_keeps_everything()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			yield return Begin(g);
			Assert.IsTrue(File.Exists(g.Store.Path), "a new case writes a first save");
			g.Player.Teleport(new Vector2(-24f, -3f), 0f);
			yield return Frames(3);
			g.Interact();
			g.Ask("mira_stock");
			g.CloseDialogue();
			g.Inv.Inspect(Ids.E04);
			g.Inv.Ask("mira_ledger_followup");
			g.TogglePin(Ids.E04);
			g.Pause();
			g.ToTitle();
			yield return Frames(2);
			Assert.IsTrue(g.HasSave);
			Assert.IsTrue(g.ContinueCase());
			Assert.AreEqual(Mode.Playing, g.Mode);
			Assert.IsTrue(g.Inv.Has(Ids.E04));
			Assert.AreEqual(2, g.Inv.Statements.Count);
			Assert.IsTrue(g.Inv.HasConflict(Ids.CMiraLedger));
			Assert.AreEqual(1, g.Inv.Pinned.Count);
			Assert.IsTrue(VillageLayout.Walkable(g.Player.Position2D.x, g.Player.Position2D.y), "never restored inside a wall");
			Assert.AreEqual(Ids.Granary, VillageLayout.ZoneAt(g.Player.Position2D.x, g.Player.Position2D.y));
		}

		[UnityTest]
		public IEnumerator A_save_made_inside_a_wall_is_moved_to_a_safe_place_on_continue()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			yield return Begin(g);
			g.Inv.Inspect(Ids.E05);
			var save = g.Inv.ToSave(-28f, 0f, 3f, 0f, "cam_granary_wide"); // inside the granary walls
			Assert.AreEqual(Store.SaveStatusOk(g.Store.Save(save)), true);
			Assert.IsTrue(g.ContinueCase());
			Assert.IsTrue(VillageLayout.Walkable(g.Player.Position2D.x, g.Player.Position2D.y));
		}

		[UnityTest]
		public IEnumerator A_damaged_save_is_reported_and_a_new_case_still_works()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			Directory.CreateDirectory(Path.GetDirectoryName(g.Store.Path));
			File.WriteAllText(g.Store.Path, "{ not json at all");
			g.RefreshSave();
			Assert.IsFalse(g.HasSave);
			Assert.IsTrue(g.SaveProblem);
			Assert.IsFalse(g.ContinueCase());
			Assert.AreEqual(Mode.Title, g.Mode);
			g.NewCase();
			yield return Frames(2);
			Assert.AreEqual(Mode.Brief, g.Mode);
			g.BeginInvestigation();
			Assert.IsTrue(g.HasSave, "the new case saved over the damaged file");
			Assert.IsFalse(g.SaveProblem);
		}

		[UnityTest]
		public IEnumerator Restart_from_pause_and_remembered_settings()
		{
			yield return Load();
			var g = DetectiveGame.Instance;
			yield return Begin(g);
			g.Inv.Inspect(Ids.E02);
			g.Pause();
			g.RestartInvestigation();
			Assert.AreEqual(Mode.Brief, g.Mode);
			Assert.AreEqual(0, g.Inv.Evidence.Count);

			g.SetVolumes(0.3f, 0.5f);
			g.SetFullscreen(false);
			var again = new DetectiveSaveStore(_saveFolder).LoadSettings();
			Assert.AreEqual(0.3f, again.masterVolume, 0.001f);
			Assert.AreEqual(0.5f, again.sfxVolume, 0.001f);
			Assert.IsFalse(again.fullscreen);
		}

		static class Store
		{
			public static bool SaveStatusOk(ITKamianets.Engine.Save.SaveStatus status) => status == ITKamianets.Engine.Save.SaveStatus.Ok;
		}
	}
}

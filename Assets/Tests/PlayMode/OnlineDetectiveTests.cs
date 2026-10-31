using System;
using System.Collections;
using System.IO;
using ITKamianets.Engine.Net;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Temnorid.Detective.Tests
{
	/// <summary>
	/// The real village with this machine as the host and a second investigator (a detached session in the same process) as the guest:
	/// the guest is shown on the host from its pose, an inspection and a question are events the host checks against the guest's
	/// pose, the discovered state is republished to the guest (and carries nothing but what was discovered), and the host's report
	/// is announced to the guest.
	/// </summary>
	public class OnlineDetectiveTests
	{
		NetSession _guest;
		string _saveFolder;

		[Serializable]
		class StateProbe
		{
			public int epoch;
			public InvestigationSave save;
		}

		static IEnumerator Until(Func<bool> condition, string what, float seconds = 10f)
		{
			var end = Time.realtimeSinceStartup + seconds;
			while (!condition())
			{
				if (Time.realtimeSinceStartup > end) Assert.Fail("Timed out waiting for " + what);
				yield return null;
			}
		}

		[SetUp]
		public void SetUp()
		{
			_saveFolder = Path.Combine(Path.GetTempPath(), "detective-online-" + Guid.NewGuid());
			DetectiveGame.SaveRootOverride = _saveFolder;
		}

		[TearDown]
		public void TearDown()
		{
			Time.timeScale = 1f;
			DetectiveGame.SaveRootOverride = null;
			if (_guest != null)
			{
				_guest.Leave();
				UnityEngine.Object.Destroy(_guest.gameObject);
			}
			NetSession.Instance.Leave();
			if (Directory.Exists(_saveFolder)) Directory.Delete(_saveFolder, true);
		}

		IEnumerator Stand(Vector2 spot)
		{
			for (var i = 0; i < 5; i++)
			{
				_guest.PublishPose(new NetPose { Position = new Vector3(spot.x, 0.05f, spot.y), Yaw = 90f, Speed = 0f });
				yield return new WaitForSeconds(0.08f);
			}
		}

		[UnityTest]
		[Timeout(120000)]
		public IEnumerator Two_investigators_share_one_notebook_and_the_hosts_report_reaches_the_guest()
		{
			var host = NetSession.Instance;
			host.Leave();
			host.GameId = "detective";
			host.GameVersion = Application.version;
			host.LocalName = "Hosty";
			host.MaxPlayers = 2;
			Assert.IsTrue(host.HostDirect(7808), host.LastError);

			_guest = NetSession.CreateDetached();
			_guest.GameId = "detective";
			_guest.GameVersion = Application.version;
			_guest.LocalName = "Guesty";
			_guest.LocalColour = 3;
			Assert.IsTrue(_guest.JoinDirect("127.0.0.1:7808"), _guest.LastError);
			yield return Until(() => host.Players.Count == 2 && _guest.Players.Count == 2 && !_guest.Connecting, "the guest to join");

			StateProbe lastState = null;
			string lastRaw = null;
			var denied = 0;
			_guest.On(DetectiveNet.StateEvent, (who, text) =>
			{
				lastRaw = text;
				lastState = JsonUtility.FromJson<StateProbe>(text);
			});
			_guest.On(DetectiveNet.DeniedEvent, (who, text) => denied++);

			yield return SceneManager.LoadSceneAsync("Village");
			yield return null;
			yield return null;

			var g = DetectiveGame.Instance;
			Assert.IsNotNull(g.Net, "an online case has the link to the other investigator");
			Assert.IsTrue(g.Online);
			Assert.IsFalse(g.IsGuest);
			Assert.AreEqual(Mode.Brief, g.Mode, "no title and no saved case when playing together");
			Assert.IsFalse(g.ContinueCase());
			g.BeginInvestigation();

			// The guest stands in the granary yard: the host shows them there from their pose.
			var guestId = _guest.LocalId;
			yield return Stand(new Vector2(-21f, -5.4f));
			yield return Until(() => g.Net.Remotes.ContainsKey(guestId) && (g.Net.Remotes[guestId].transform.position - new Vector3(-21f, 0.05f, -5.4f)).magnitude < 0.5f, "the guest to be shown");
			Assert.AreEqual("Guesty", g.Net.Remotes[guestId].Tag.GetComponent<TextMesh>().text);
			Assert.IsNull(g.Net.Remotes[guestId].GetComponent<Player>(), "the copy is only shown, it has no rules");

			// The guest loaded the village and hears the host's state; nothing is discovered yet.
			_guest.Send(DetectiveNet.HelloEvent, "", true);
			yield return Until(() => lastState != null, "the first state");
			Assert.AreEqual(0, lastState.save.discovered_evidence_ids.Length);

			// The guest inspects the ledger from where they stand: the host records it and both notebooks agree.
			_guest.Send(DetectiveNet.InspectEvent, Ids.E04, true);
			yield return Until(() => g.Inv.Has(Ids.E04), "the host to record the ledger");
			yield return Until(() => lastState.save.discovered_evidence_ids.Length == 1, "the guest to be told");
			Assert.AreEqual(Ids.E04, lastState.save.discovered_evidence_ids[0]);
			StringAssert.DoesNotContain("culprit", lastRaw.ToLowerInvariant());
			Assert.AreEqual(0, denied);

			// From too far away the same request is refused and nothing is recorded.
			yield return Stand(new Vector2(5f, 19f));
			_guest.Send(DetectiveNet.InspectEvent, Ids.E01, true);
			yield return Until(() => denied == 1, "the refusal");
			Assert.IsFalse(g.Inv.Has(Ids.E01));

			// A question needs the guest to stand by the person, and the follow-up needs its clue.
			yield return Stand(new Vector2(-24f, -3f));
			_guest.Send(DetectiveNet.AskEvent, "mira_stock", true);
			yield return Until(() => g.Inv.HasSpoken("mira_stock"), "the host to record the statement");
			_guest.Send(DetectiveNet.AskEvent, "mira_ledger_followup", true);
			yield return Until(() => g.Inv.HasConflict(Ids.CMiraLedger), "the conflict between the statement and the ledger");
			yield return Until(() => lastState.save.discovered_conflict_ids.Length == 1, "the guest to see the conflict");

			// Pinning and leaning are the guest's say; the host decides.
			_guest.Send(DetectiveNet.PinEvent, Ids.E04, true);
			yield return Until(() => g.Inv.Pinned.Count == 1, "the pin");
			_guest.Send(DetectiveNet.LeanEvent, Ids.Mira, true);
			yield return Until(() => g.PartnerLean == Ids.Mira, "the guest's lean");

			// Only the host reports; the result is announced to the guest.
			g.OpenBoard(BoardTab.Decision);
			g.ChooseAccused(Ids.Orest);
			Assert.IsTrue(g.RequestReport());
			Assert.IsNotNull(g.ConfirmReport());
			yield return Until(() => lastState.save.case_status == "resolved", "the result to reach the guest");
			Assert.AreEqual(Ids.OutcomeOrest, lastState.save.outcome_id);

			// Nothing was written to the offline save and a guest's request after the report changes nothing.
			Assert.AreEqual(0, g.Saves);
			Assert.IsFalse(File.Exists(g.Store.Path));
			_guest.Send(DetectiveNet.InspectEvent, Ids.E02, true);
			yield return new WaitForSeconds(0.5f);
			Assert.IsFalse(g.Inv.Has(Ids.E02));
		}
	}
}

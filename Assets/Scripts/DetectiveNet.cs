using System;
using System.Collections.Generic;
using ITKamianets.Engine.Net;
using ITKamianets.Engine.Shell;
using UnityEngine;

namespace Temnorid.Detective
{
	/// <summary>
	/// Two investigators in the same village. The host owns the <see cref="Investigation"/> (clues, statements, conflicts, the
	/// notebook's pins) and the report; the guest investigates with the same eyes and hands, and its notebook is a mirror.
	/// <list type="bullet">
	/// <item>Each investigator is simulated by its owner and published as a pose about 15 times a second; the other machine shows it
	/// with <see cref="RemoteInvestigator"/>. Cameras are each machine's own.</item>
	/// <item>Inspecting, asking and pinning are requests to the host: <c>dt.inspect</c> (evidence id), <c>dt.ask</c> (topic id) and
	/// <c>dt.pin</c> (evidence id). The host checks the id exists, that the guest stood close enough to it when it asked (its last
	/// pose, with a little slack), and that the topic is open, then changes its investigation. A refused request is answered with
	/// <c>dt.no</c>.</item>
	/// <item>The host republishes only what has been discovered: <c>dt.state</c> carries the same fields as a save file (evidence,
	/// visited places, spoken topics, statements, conflicts, pins, time, and the verdict once there is one) and the case epoch.
	/// It goes out whenever something changes and every two seconds, so a guest that loaded late catches up. The case truth
	/// (who did it) is never part of it: it is not in the investigation at all.</item>
	/// <item>The report is the host's: the guest sees the decision board and can lean toward a choice (<c>dt.lean</c>), which the host
	/// sees as a line, but only the host confirms. The result, once committed, reaches both through the next <c>dt.state</c>.
	/// Starting over is the host's too; a new epoch sends the guest back to the brief.</item>
	/// <item>An online case is never saved, so a joiner's offline save is never touched.</item>
	/// </list>
	/// If the guest leaves, the host simply goes on alone. If the host leaves, the guest goes back to the menu.
	/// </summary>
	public sealed class DetectiveNet : MonoBehaviour
	{
		public const string StateEvent = "dt.state";
		public const string HelloEvent = "dt.hi";
		public const string InspectEvent = "dt.inspect";
		public const string AskEvent = "dt.ask";
		public const string PinEvent = "dt.pin";
		public const string LeanEvent = "dt.lean";
		public const string DeniedEvent = "dt.no";

		const float PoseEvery = 1f / 15f;
		const float StateResend = 2f;
		/// <summary>How far past an object's own reach a guest's last pose may be when it asks (the pose is a little old).</summary>
		public const float ReachSlack = 2.5f;

		[Serializable]
		sealed class StateMessage
		{
			public int epoch;
			public InvestigationSave save;
		}

		DetectiveGame _game;
		NetSession _net;
		int _epoch;
		long _signature = -1;
		float _nextPose;
		float _nextState;
		bool _leaving;
		TextMesh _tagTemplate;
		readonly Dictionary<ulong, RemoteInvestigator> _remotes = new Dictionary<ulong, RemoteInvestigator>();

		public int Epoch => _epoch;
		public IReadOnlyDictionary<ulong, RemoteInvestigator> Remotes => _remotes;

		public void Init(DetectiveGame game)
		{
			_game = game;
			_net = NetSession.Instance;
			_epoch = _net.IsHost ? 1 : 0;
			foreach (var npc in game.Npcs)
				if (npc != null && npc.Tag != null)
				{
					_tagTemplate = npc.Tag.GetComponent<TextMesh>();
					break;
				}
			_net.On(StateEvent, OnState);
			_net.On(HelloEvent, OnHello);
			_net.On(InspectEvent, OnInspect);
			_net.On(AskEvent, OnAsk);
			_net.On(PinEvent, OnPin);
			_net.On(LeanEvent, OnLean);
			_net.On(DeniedEvent, OnDenied);
			_net.Disconnected += OnDisconnected;
			_net.PlayerLeft += OnPlayerLeft;
			if (_net.IsClient) _net.Send(HelloEvent, "", true);
		}

		void OnDestroy()
		{
			if (_net == null) return;
			_net.Off(StateEvent, OnState);
			_net.Off(HelloEvent, OnHello);
			_net.Off(InspectEvent, OnInspect);
			_net.Off(AskEvent, OnAsk);
			_net.Off(PinEvent, OnPin);
			_net.Off(LeanEvent, OnLean);
			_net.Off(DeniedEvent, OnDenied);
			_net.Disconnected -= OnDisconnected;
			_net.PlayerLeft -= OnPlayerLeft;
		}

		/// <summary>Host: a new case begins; guests are sent back to the brief by the next state.</summary>
		public void NewEpoch()
		{
			_epoch++;
			_signature = -1;
		}

		// ---- every machine ----

		void Update()
		{
			if (_net == null || !_net.IsOnline) return;
			ShowRemotes();

			if (Time.unscaledTime >= _nextPose)
			{
				_nextPose = Time.unscaledTime + PoseEvery;
				if (_game.Player != null)
					_net.PublishPose(new NetPose
					{
						Position = _game.Player.transform.position,
						Yaw = _game.Player.Yaw,
						Speed = _game.Player.Speed,
						State = (int)_game.Mode,
					});
			}

			if (_net.IsHost) HostFrame();
		}

		void ShowRemotes()
		{
			foreach (var player in _net.Players)
			{
				if (player.Id == _net.LocalId) continue;
				if (!_remotes.TryGetValue(player.Id, out var remote) || remote == null)
				{
					remote = RemoteInvestigator.Create(_game.Player, _tagTemplate, player.Id, player.Name, ShellConfig.Current.ColourOf(player.Colour));
					_remotes[player.Id] = remote;
					_game.Toast(Loc.T("net.joined", player.Name));
				}
				if (_net.TryGetPose(player.Id, out var pose)) remote.Body.Apply(pose);
			}
		}

		void OnDisconnected(string reason)
		{
			// The host left or the connection dropped: nobody keeps the case.
			if (_leaving) return;
			_leaving = true;
			ShellFlow.QuitToMenu();
		}

		void OnPlayerLeft(NetPlayer player)
		{
			_game.Toast(Loc.T("net.left", player.Name));
			if (_remotes.TryGetValue(player.Id, out var remote) && remote != null) Destroy(remote.gameObject);
			_remotes.Remove(player.Id);
			if (_net.IsHost) _game.PartnerLean = null;
		}

		// ---- host: publishes the discovered state ----

		void HostFrame()
		{
			var inv = _game.Inv;

			// The guest's footsteps count as visits to places, so both notebooks agree about where the village has been seen.
			if (inv.Status == CaseStatus.Active && _game.Mode != Mode.Title && _game.Mode != Mode.Brief)
				foreach (var player in _net.Players)
				{
					if (player.Id == _net.LocalId || !_net.TryGetPose(player.Id, out var pose)) continue;
					if (pose.State == (int)Mode.Brief) continue;
					inv.VisitZone(VillageLayout.ZoneAt(pose.Position.x, pose.Position.z));
				}

			var signature = Signature(inv);
			if (signature != _signature || Time.unscaledTime >= _nextState)
			{
				_signature = signature;
				_nextState = Time.unscaledTime + StateResend;
				PublishState();
			}
		}

		static long Signature(Investigation inv)
		{
			long h = inv.Status == CaseStatus.Resolved ? 7 : 3;
			h = h * 131 + inv.Evidence.Count;
			h = h * 131 + inv.Zones.Count;
			h = h * 131 + inv.SpokenTopics.Count;
			h = h * 131 + inv.Conflicts.Count;
			h = h * 131 + inv.Pinned.Count;
			foreach (var id in inv.Pinned) h = h * 31 + id.GetHashCode();
			h = h * 131 + (inv.OutcomeId == null ? 0 : inv.OutcomeId.GetHashCode());
			return h;
		}

		void PublishState()
		{
			var message = new StateMessage { epoch = _epoch, save = _game.Inv.ToSave(0f, 0f, 0f, 0f, "") };
			_net.Send(StateEvent, JsonUtility.ToJson(message));
		}

		void OnHello(ulong sender, string payload)
		{
			if (_net.IsHost) _signature = -1;
		}

		// ---- the guest asks, the host checks ----

		public void RequestInspect(string id)
		{
			if (_net.IsClient) _net.Send(InspectEvent, id ?? "", true);
		}

		public void RequestAsk(string topicId)
		{
			if (_net.IsClient) _net.Send(AskEvent, topicId ?? "", true);
		}

		public void RequestPin(string evidenceId)
		{
			if (_net.IsClient) _net.Send(PinEvent, evidenceId ?? "", true);
		}

		public void SendLean(string choice)
		{
			if (_net.IsClient) _net.Send(LeanEvent, choice ?? "", true);
		}

		/// <summary>Host: is this guest, by its last pose, close enough to the object with this id to use it?</summary>
		bool Near(ulong player, string objectId)
		{
			if (!_net.TryGetPose(player, out var pose)) return false;
			foreach (var item in Interactable.All)
			{
				if (item.Id != objectId) continue;
				var to = item.transform.position - pose.Position;
				to.y = 0f;
				if (to.magnitude <= Mathf.Max(DetectiveConfig.InteractRange, item.Range) + ReachSlack) return true;
			}
			return false;
		}

		void Deny(ulong player) => _net.SendTo(player, DeniedEvent, "");

		bool Valid(ulong sender) => _net.IsHost && _net.Find(sender) != null && _game.Inv.Status == CaseStatus.Active;

		void OnInspect(ulong sender, string id)
		{
			if (!Valid(sender) || CaseContent.FindEvidence(id) == null) return;
			if (!Near(sender, id))
			{
				Deny(sender);
				return;
			}
			var inv = _game.Inv;
			var before = inv.Conflicts.Count;
			if (!inv.Inspect(id)) return;
			_game.Audio.Play(Sfx.Page);
			_game.Toast(Loc.T("new.evidence", Loc.T(id + ".title")));
			_game.ToastNewConflicts(before);
		}

		void OnAsk(ulong sender, string topicId)
		{
			var topic = CaseContent.FindTopic(topicId);
			if (!Valid(sender) || topic == null) return;
			if (!Near(sender, topic.Npc))
			{
				Deny(sender);
				return;
			}
			var inv = _game.Inv;
			var before = inv.Conflicts.Count;
			var was = inv.HasSpoken(topicId);
			if (inv.Ask(topicId) == null)
			{
				Deny(sender);
				return;
			}
			if (!was) _game.ToastNewConflicts(before);
		}

		void OnPin(ulong sender, string evidenceId)
		{
			if (!Valid(sender)) return;
			_game.Inv.TogglePin(evidenceId);
		}

		void OnLean(ulong sender, string choice)
		{
			if (!_net.IsHost || _net.Find(sender) == null) return;
			if (choice == "-" || CaseContent.IsSuspect(choice)) _game.PartnerLean = choice;
		}

		void OnDenied(ulong sender, string payload)
		{
			if (_net.IsClient) _game.Toast(Loc.T("net.denied"));
		}

		// ---- the guest shows what the host says ----

		void OnState(ulong sender, string payload)
		{
			if (!_net.IsClient || string.IsNullOrEmpty(payload)) return;
			StateMessage message;
			try
			{
				message = JsonUtility.FromJson<StateMessage>(payload);
			}
			catch (ArgumentException)
			{
				return;
			}
			if (message == null || message.save == null || message.epoch < _epoch) return;

			var newCase = _epoch != 0 && message.epoch != _epoch;
			_epoch = message.epoch;
			var inv = _game.Inv;
			var evidenceBefore = newCase ? 0 : inv.Evidence.Count;
			var conflictsBefore = newCase ? 0 : inv.Conflicts.Count;
			_game.ApplyMirror(message.save, newCase);
			if (newCase) return;

			inv = _game.Inv;
			for (var i = evidenceBefore; i < inv.Evidence.Count; i++)
			{
				_game.Audio.Play(Sfx.Page);
				_game.Toast(Loc.T("new.evidence", Loc.T(inv.Evidence[i] + ".title")));
			}
			_game.ToastNewConflicts(conflictsBefore);
		}
	}
}

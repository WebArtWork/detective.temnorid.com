using System.Collections.Generic;
using System.Linq;
using ITKamianets.Engine.FixedCamera;
using ITKamianets.Engine.Input;
using ITKamianets.Engine.Net;
using ITKamianets.Engine.Save;
using ITKamianets.Engine.Shell;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Temnorid.Detective
{
	public enum Mode
	{
		Title,
		Brief,
		Playing,
		Inspect,
		Dialogue,
		Journal,
		Board,
		Confirm,
		Resolved,
		Pause,
		Settings,
	}

	public enum BoardTab
	{
		Evidence,
		People,
		Timeline,
		Decision,
	}

	/// <summary>
	/// The investigation as played: it owns the <see cref="Investigation"/>, decides what every object and person does, runs the
	/// screens' state, the report and the saving. Scene objects only show what it says, and <see cref="DetectiveHud"/> only shows
	/// the screens, so a test (or a bot) can play the whole case through this class.
	/// </summary>
	[DefaultExecutionOrder(-100)]
	public sealed class DetectiveGame : MonoBehaviour
	{
		public static DetectiveGame Instance { get; private set; }
		/// <summary>Tests point the save somewhere else.</summary>
		public static string SaveRootOverride;

		[Header("Scene")]
		public Player Player;
		public FixedViewCamera Rig;
		public DetectiveAudio Audio;
		public Npc[] Npcs;
		public Transform OverviewCamera, OverviewLook, EndCamera, EndLook;

		public Investigation Inv { get; private set; } = new Investigation();
		public DetectiveSettings Settings { get; private set; } = new DetectiveSettings();
		public DetectiveSaveStore Store { get; private set; }

		public Mode Mode { get; private set; } = Mode.Title;
		public Mode PanelReturn { get; private set; } = Mode.Playing;
		public BoardTab Tab { get; private set; } = BoardTab.Evidence;
		public bool HasSave { get; private set; }
		public bool SaveProblem { get; private set; }
		public Interactable Target { get; private set; }
		public string PromptText { get; private set; } = "";
		public string ToastText { get; private set; } = "";
		public float ToastUntil { get; private set; }
		public string InspectId { get; private set; }
		public bool InspectWasNew { get; private set; }
		public string NpcId { get; private set; }
		public string AnswerText { get; private set; } = "";
		public string QuestionText { get; private set; } = "";
		public bool AnswerWasRepeat { get; private set; }
		public string SelectedPerson { get; private set; } = Ids.Orest;
		public string DecisionAccused { get; private set; }
		public bool DecisionInsufficient { get; private set; }
		public List<string> DecisionReasons { get; } = new List<string>();
		public VerdictResult Result { get; private set; }
		public MockReport Report { get; private set; }
		public int Saves { get; private set; }

		/// <summary>The link to the other investigator. Null unless the case was opened from an online lobby.</summary>
		public DetectiveNet Net { get; private set; }
		/// <summary>True while a session is shared: nothing is saved, and the host owns the case.</summary>
		public bool Online => Net != null && NetSession.Instance.IsOnline;
		/// <summary>True on the investigator who joined: the notebook is a mirror of the host's and the report is the host's.</summary>
		public bool IsGuest => Online && NetSession.Instance.IsClient;
		/// <summary>Starting over is the host's call when two investigate together.</summary>
		public bool CanRestart => !IsGuest;
		/// <summary>What the other investigator leans toward in the decision: an id, "-" for not enough evidence, or null.</summary>
		public string PartnerLean { get; internal set; }
		public string Zone { get; private set; } = Ids.Square;

		float _nextStep;
		bool _cinematic;
		InputAction _journalKey, _boardKey;

		public string Objective => Loc.T(Inv.ObjectiveId);
		public bool ToastActive => Time.unscaledTime < ToastUntil;
		public string ZoneName => Loc.T("z." + Zone);

		void Awake()
		{
			Instance = this;
			Time.timeScale = 1f;
			EngineInput.Default.PlayerEnabled = true;

			var map = EngineInput.Default.Asset.FindActionMap("Detective")
				?? EngineInput.Default.AddMap("Detective", m =>
				{
					m.AddAction("Journal", InputActionType.Button, "<Keyboard>/j", groups: EngineInputActions.Groups.KeyboardMouse);
					m.AddAction("Board", InputActionType.Button, "<Keyboard>/tab", groups: EngineInputActions.Groups.KeyboardMouse);
				});
			_journalKey = map.FindAction("Journal");
			_boardKey = map.FindAction("Board");

			ShellFlow.BeforeQuit += OnBeforeQuit;
			Store = new DetectiveSaveStore(SaveRootOverride);
			Settings = Store.LoadSettings();
			ApplySettings();
			RefreshSave();
		}

		void Start()
		{
			Rig.Subject = Player.transform;
			Player.Rig = Rig;
			foreach (var npc in Npcs) npc.Target = Player.transform;
			Player.Teleport(VillageLayout.PlayerStart, 0f);
			Rig.Snap();
			SetCinematic(true, OverviewCamera, OverviewLook);
			if (NetSession.Instance.IsOnline)
			{
				Net = gameObject.AddComponent<DetectiveNet>();
				Net.Init(this);
				StartOnline();
			}
		}

		void OnDestroy()
		{
			ShellFlow.BeforeQuit -= OnBeforeQuit;
			if (Instance == this) Instance = null;
		}

		/// <summary>Leaving through the shell saves an offline case where it stands. An online case is never saved.</summary>
		void OnBeforeQuit()
		{
			if (!Online && Mode != Mode.Title) SaveNow();
		}

		/// <summary>Where this investigator stands at the start: side by side with the other one, in the order of the roster.</summary>
		public Vector2 StartSpot()
		{
			var players = NetSession.Instance.Players;
			var slot = 0;
			for (var i = 0; i < players.Count; i++) if (players[i].Id == NetSession.Instance.LocalId) slot = i;
			var count = Mathf.Max(1, players.Count);
			return VillageLayout.PlayerStart + new Vector2((slot - (count - 1) * 0.5f) * 1.4f, 0f);
		}

		/// <summary>Shared case: both investigators begin on the brief, each at their own spot. There is no title and no saved case.</summary>
		void StartOnline()
		{
			ResetScreens();
			Player.Teleport(StartSpot(), 0f);
			Zone = Ids.Square;
			Rig.Snap();
			Mode = Mode.Brief;
			SetCinematic(true, OverviewCamera, OverviewLook);
		}

		/// <summary>Guest: shows what the host's notebook says. A new case from the host starts the brief again.</summary>
		internal void ApplyMirror(InvestigationSave save, bool newCase)
		{
			if (newCase)
			{
				Inv = new Investigation();
				ResetScreens();
				PartnerLean = null;
				Player.SetVisible(true);
				Player.Teleport(StartSpot(), 0f);
				Zone = Ids.Square;
				Rig.Snap();
				Mode = Mode.Brief;
				SetCinematic(true, OverviewCamera, OverviewLook);
			}
			if (!Inv.Load(save)) return;
			if (Inv.Status == CaseStatus.Resolved && Mode != Mode.Resolved)
			{
				Result = new VerdictResult { Kind = Inv.Verdict.Value, AccusedId = Inv.AccusedId, OutcomeId = Inv.OutcomeId, Reasons = Inv.Reasons };
				Report = MockReport.From(Result);
				Mode = Mode.Resolved;
				Audio.Play(Sfx.Bell);
				SetCinematic(true, EndCamera, EndLook);
			}
		}

		// ---- starting, saving ----

		/// <summary>Reads the save slot to decide whether Continue is offered.</summary>
		public void RefreshSave()
		{
			var saved = Store.Load(out var status);
			SaveProblem = status == SaveStatus.Corrupt || status == SaveStatus.UnsupportedVersion || status == SaveStatus.IoError;
			HasSave = false;
			if (saved == null) return;
			if (new Investigation().Load(saved)) HasSave = true;
			else SaveProblem = true;
		}

		void SetCinematic(bool on, Transform camera = null, Transform look = null)
		{
			if (on == _cinematic)
			{
				if (on)
				{
					Rig.PopOverride();
					Rig.PushOverride(camera, look, 45f);
				}
				return;
			}
			_cinematic = on;
			if (on) Rig.PushOverride(camera, look, 45f);
			else Rig.PopOverride();
		}

		public void NewCase()
		{
			if (IsGuest)
			{
				Toast(Loc.T("net.replay.host"));
				return;
			}
			Inv = new Investigation();
			PartnerLean = null;
			if (Online) Net.NewEpoch();
			ResetScreens();
			Player.SetVisible(true);
			Player.Teleport(Online ? StartSpot() : VillageLayout.PlayerStart, 0f);
			Zone = Ids.Square;
			Rig.Snap();
			Mode = Mode.Brief;
			SetCinematic(true, OverviewCamera, OverviewLook);
			SaveNow();
		}

		void ResetScreens()
		{
			Tab = BoardTab.Evidence;
			DecisionAccused = null;
			DecisionInsufficient = false;
			DecisionReasons.Clear();
			Result = null;
			Report = null;
			NpcId = null;
			InspectId = null;
			AnswerText = "";
			QuestionText = "";
			ToastUntil = 0f;
		}

		/// <summary>Leaves the case brief and starts investigating.</summary>
		public void BeginInvestigation()
		{
			if (Mode != Mode.Brief) return;
			Mode = Mode.Playing;
			SetCinematic(false);
			Rig.Snap();
		}

		public bool ContinueCase()
		{
			if (Online) return false;
			var saved = Store.Load(out _);
			var inv = new Investigation();
			if (saved == null || !inv.Load(saved))
			{
				SaveProblem = true;
				HasSave = false;
				return false;
			}
			Inv = inv;
			ResetScreens();
			Player.SetVisible(true);
			var spot = new Vector2(saved.player_x, saved.player_z);
			if (!VillageLayout.Walkable(spot.x, spot.y)) spot = VillageLayout.SpawnFor(Inv.Zones.Count > 0 ? Inv.Zones[Inv.Zones.Count - 1] : Ids.Square);
			Player.Teleport(spot, saved.player_heading);
			Zone = VillageLayout.ZoneAt(spot.x, spot.y);
			Rig.Snap();
			if (Inv.Status == CaseStatus.Resolved)
			{
				Result = new VerdictResult { Kind = Inv.Verdict.Value, AccusedId = Inv.AccusedId, OutcomeId = Inv.OutcomeId, Reasons = Inv.Reasons };
				Report = MockReport.From(Result);
				Mode = Mode.Resolved;
				SetCinematic(true, EndCamera, EndLook);
			}
			else
			{
				Mode = Mode.Playing;
				SetCinematic(false);
			}
			return true;
		}

		/// <summary>Writes the save. Called after new clues, new statements, a change of place and the report.</summary>
		public void SaveNow()
		{
			if (Online) return; // a shared case never touches the offline save
			var p = Player != null ? Player.transform.position : Vector3.zero;
			var save = Inv.ToSave(p.x, p.y, p.z, Player != null ? Player.Yaw : 0f, Rig != null ? Rig.ShotId : "");
			if (Store.Save(save) == SaveStatus.Ok) Saves++;
			RefreshSave();
		}

		void ApplySettings()
		{
			if (Audio != null) Audio.SetVolumes(Settings.masterVolume, Settings.sfxVolume);
			if (!Application.isEditor && Screen.fullScreen != Settings.fullscreen) Screen.fullScreen = Settings.fullscreen;
		}

		public void SetVolumes(float master, float sfx)
		{
			Settings.masterVolume = Mathf.Clamp01(master);
			Settings.sfxVolume = Mathf.Clamp01(sfx);
			ApplySettings();
			Store.SaveSettings(Settings);
		}

		public void SetFullscreen(bool on)
		{
			Settings.fullscreen = on;
			ApplySettings();
			Store.SaveSettings(Settings);
		}

		// ---- the frame ----

		void Update()
		{
			var dt = Time.deltaTime;
			Player.Frozen = Mode != Mode.Playing;
			var input = EngineInput.Default;
			var back = input.UI.Cancel.WasPressedThisFrame();

			switch (Mode)
			{
				case Mode.Playing:
					PlayingFrame(dt, input, back);
					break;
				case Mode.Brief:
					if (input.Player.Interact.WasPressedThisFrame() || input.UI.Submit.WasPressedThisFrame()) BeginInvestigation();
					break;
				case Mode.Inspect:
					if (back || input.Player.Interact.WasPressedThisFrame()) CloseInspect();
					break;
				case Mode.Dialogue:
					if (back) CloseDialogue();
					break;
				case Mode.Journal:
					if (back || _journalKey.WasPressedThisFrame()) ClosePanel();
					else if (_boardKey.WasPressedThisFrame()) OpenBoard(Tab);
					break;
				case Mode.Board:
					if (back || _boardKey.WasPressedThisFrame()) ClosePanel();
					else if (_journalKey.WasPressedThisFrame()) OpenPanel(Mode.Journal);
					break;
				case Mode.Confirm:
					if (back) CancelReport();
					break;
				case Mode.Pause:
					if (back) Resume();
					break;
				case Mode.Settings:
					if (back) CloseSettings();
					break;
			}
		}

		void PlayingFrame(float dt, EngineInput input, bool back)
		{
			if (!IsGuest) Inv.ElapsedSeconds += dt; // the guest's clock is the host's, mirrored
			WatchZone();

			Target = Interactable.Pick(Player.transform.position, Player.transform.forward, DetectiveConfig.InteractRange, _ => true);
			PromptText = Target != null ? Prompt(Target) : "";

			if (Player.Speed > 0.5f)
			{
				_nextStep -= dt;
				if (_nextStep <= 0f)
				{
					_nextStep = Player.Speed > 3f ? 0.34f : 0.5f;
					Audio.Play(Sfx.Footstep, 0.3f);
				}
			}

			if (back)
			{
				Pause();
				return;
			}
			if (_journalKey.WasPressedThisFrame())
			{
				OpenPanel(Mode.Journal);
				return;
			}
			if (_boardKey.WasPressedThisFrame())
			{
				OpenBoard(Tab);
				return;
			}
			if (input.Player.Interact.WasPressedThisFrame()) Interact();
		}

		void WatchZone()
		{
			var p = Player.Position2D;
			var zone = VillageLayout.ZoneAt(p.x, p.y);
			if (zone == Zone && Inv.Visited(zone)) return;
			Zone = zone;
			if (!IsGuest && Inv.VisitZone(zone)) SaveNow();
		}

		// ---- interaction ----

		static string PromptKey => TouchButton.IsNeeded ? "prompt.touch" : "prompt";

		string Prompt(Interactable item)
		{
			if (item.Kind == InteractKind.Talk) return Loc.T(PromptKey, Loc.T("verb.talk"), Loc.T("p." + item.Id + ".with"));
			return Loc.T(PromptKey, Loc.T("verb.inspect"), Loc.T(item.Id + ".title").ToLowerInvariant());
		}

		/// <summary>Uses what is in front of the player. Returns whether anything was done.</summary>
		public bool Interact()
		{
			if (Mode != Mode.Playing || Target == null) return false;
			Use(Target.Id);
			return true;
		}

		/// <summary>The effect of using something, by its stable id: a clue is inspected, a person is spoken to.</summary>
		public void Use(string id)
		{
			if (CaseContent.FindEvidence(id) != null) OpenInspect(id);
			else if (Npcs.Any(n => n.Id == id)) OpenDialogue(id);
		}

		public void Toast(string text, float seconds = 3.4f)
		{
			ToastText = text;
			ToastUntil = Time.unscaledTime + seconds;
		}

		internal void ToastNewConflicts(int before)
		{
			for (var i = before; i < Inv.Conflicts.Count; i++)
			{
				Audio.Play(Sfx.Note);
				Toast(Loc.T("new.conflict", Loc.T(Inv.Conflicts[i])), 4.5f);
			}
		}

		// ---- clues ----

		public void OpenInspect(string id)
		{
			if (Mode != Mode.Playing && Mode != Mode.Board && Mode != Mode.Journal) return;
			var before = Inv.Conflicts.Count;
			if (IsGuest)
			{
				// The host decides what is recorded; the notebook follows when it says so.
				InspectWasNew = CaseContent.FindEvidence(id) != null && !Inv.Has(id);
				Net.RequestInspect(id);
			}
			else InspectWasNew = Inv.Inspect(id);
			InspectId = id;
			Mode = Mode.Inspect;
			if (InspectWasNew)
			{
				Audio.Play(Sfx.Page);
				if (!IsGuest)
				{
					Toast(Loc.T("new.evidence", Loc.T(id + ".title")));
					ToastNewConflicts(before);
					SaveNow();
				}
			}
			else Audio.Play(Sfx.Click, 0.5f);
		}

		public void CloseInspect()
		{
			if (Mode == Mode.Inspect) Mode = Mode.Playing;
		}

		// ---- conversations ----

		public void OpenDialogue(string npc)
		{
			if (Mode != Mode.Playing) return;
			NpcId = npc;
			AnswerText = "";
			QuestionText = "";
			AnswerWasRepeat = false;
			Mode = Mode.Dialogue;
			Audio.Play(Sfx.Click, 0.5f);
		}

		public IEnumerable<TopicDef> DialogueTopics => NpcId == null ? Enumerable.Empty<TopicDef>() : Inv.TopicsFor(NpcId);

		/// <summary>Asks a topic. The first answer records a statement; asking again repeats the answer and records nothing.</summary>
		public AskResult Ask(string topicId)
		{
			if (Mode != Mode.Dialogue) return null;
			var topic = CaseContent.FindTopic(topicId);
			if (topic == null || topic.Npc != NpcId) return null;
			var was = Inv.HasSpoken(topicId);
			var before = Inv.Conflicts.Count;
			AskResult result;
			if (IsGuest)
			{
				if (!Inv.TopicAvailable(topic)) return null;
				result = new AskResult { Topic = topic, IsNewStatement = !was };
				Net.RequestAsk(topicId);
			}
			else result = Inv.Ask(topicId);
			if (result == null) return null;
			QuestionText = Loc.T("q." + topicId);
			AnswerText = Loc.T("a." + topicId);
			if (topicId == "elder_status") AnswerText += "\n\n" + Loc.T("evidence.count", Inv.Evidence.Count);
			AnswerWasRepeat = was;
			Audio.Play(Sfx.Click, 0.5f);
			if (!was)
			{
				ToastNewConflicts(before);
				SaveNow();
			}
			return result;
		}

		public void CloseDialogue()
		{
			if (Mode != Mode.Dialogue) return;
			Mode = Mode.Playing;
			NpcId = null;
		}

		/// <summary>The elder's hub: opens the decision, once the scene has been looked at.</summary>
		public void ElderReport()
		{
			if (Mode != Mode.Dialogue || NpcId != Ids.Elder) return;
			if (!Inv.CanReport)
			{
				QuestionText = Loc.T("elder.report");
				AnswerText = Loc.T("elder.locked");
				return;
			}
			Mode = Mode.Playing;
			NpcId = null;
			OpenBoard(BoardTab.Decision);
		}

		// ---- journal and board ----

		public void OpenPanel(Mode panel)
		{
			if (Mode != Mode.Playing && Mode != Mode.Journal && Mode != Mode.Board && Mode != Mode.Pause) return;
			if (Mode == Mode.Playing || Mode == Mode.Pause) PanelReturn = Mode;
			Mode = panel;
		}

		public void OpenBoard(BoardTab tab)
		{
			OpenPanel(Mode.Board);
			if (Mode == Mode.Board) Tab = tab;
		}

		public void SetTab(BoardTab tab)
		{
			if (Mode == Mode.Board) Tab = tab;
		}

		public void SelectPerson(string person)
		{
			if (System.Array.IndexOf(CaseContent.People, person) >= 0) SelectedPerson = person;
		}

		public void ClosePanel()
		{
			if (Mode != Mode.Journal && Mode != Mode.Board) return;
			Mode = PanelReturn;
		}

		public void TogglePin(string evidenceId)
		{
			if (IsGuest)
			{
				if (!Inv.Has(evidenceId)) return;
				Net.RequestPin(evidenceId);
				Audio.Play(Sfx.Click, 0.5f);
				return;
			}
			if (Inv.TogglePin(evidenceId)) Audio.Play(Sfx.Click, 0.5f);
		}

		// ---- the report ----

		public void ChooseAccused(string suspectId)
		{
			if (Mode != Mode.Board || !CaseContent.IsSuspect(suspectId)) return;
			DecisionAccused = suspectId;
			DecisionInsufficient = false;
			if (Online) Net.SendLean(suspectId);
		}

		public void ChooseInsufficient()
		{
			if (Mode != Mode.Board) return;
			DecisionAccused = null;
			DecisionInsufficient = true;
			if (Online) Net.SendLean("-");
		}

		public bool ToggleReason(string evidenceId)
		{
			if (!Inv.Has(evidenceId)) return false;
			if (DecisionReasons.Remove(evidenceId)) return true;
			if (DecisionReasons.Count >= DetectiveConfig.MaxReasons) return false;
			DecisionReasons.Add(evidenceId);
			return true;
		}

		public bool HasChoice => DecisionInsufficient || DecisionAccused != null;

		/// <summary>Shows the neutral confirmation for the chosen report. Nothing is committed yet.</summary>
		public bool RequestReport()
		{
			if (Mode != Mode.Board) return false;
			if (IsGuest)
			{
				Toast(Loc.T("net.host.only"));
				return false;
			}
			if (!Inv.CanReport)
			{
				Toast(Loc.T("decision.locked"));
				return false;
			}
			if (!HasChoice) return false;
			Mode = Mode.Confirm;
			return true;
		}

		/// <summary>Cancel: back to the board, the investigation goes on.</summary>
		public void CancelReport()
		{
			if (Mode == Mode.Confirm) Mode = Mode.Board;
		}

		/// <summary>Commits the one verdict. A second try is refused.</summary>
		public VerdictResult ConfirmReport()
		{
			if (Mode != Mode.Confirm) return null;
			var verdict = Inv.Commit(DecisionInsufficient ? VerdictKind.Insufficient : VerdictKind.Accuse, DecisionAccused, DecisionReasons);
			if (verdict == null) return null;
			Result = verdict;
			Report = MockReport.From(verdict);
			Mode = Mode.Resolved;
			Audio.Play(Sfx.Bell);
			SetCinematic(true, EndCamera, EndLook);
			SaveNow();
			return verdict;
		}

		public void Replay() => NewCase();

		// ---- pause and settings ----

		public void Pause()
		{
			if (Mode != Mode.Playing) return;
			SaveNow();
			PanelReturn = Mode.Playing;
			Mode = Mode.Pause;
		}

		public void Resume()
		{
			if (Mode == Mode.Pause) Mode = Mode.Playing;
		}

		public void OpenSettings()
		{
			PanelReturn = Mode;
			Mode = Mode.Settings;
		}

		public void CloseSettings()
		{
			if (Mode == Mode.Settings) Mode = PanelReturn == Mode.Pause ? Mode.Pause : Mode.Title;
		}

		/// <summary>From the pause menu: start the case over.</summary>
		public void RestartInvestigation()
		{
			if (Mode == Mode.Pause) NewCase();
		}

		public void ToTitle()
		{
			if (Mode == Mode.Pause || Mode == Mode.Playing || Mode == Mode.Resolved) SaveNow();
			RefreshSave();
			Mode = Mode.Title;
			SetCinematic(true, OverviewCamera, OverviewLook);
		}

		public string ElapsedText
		{
			get
			{
				var total = Mathf.FloorToInt(Inv.ElapsedSeconds);
				return (total / 60) + ":" + (total % 60).ToString("00");
			}
		}
	}
}

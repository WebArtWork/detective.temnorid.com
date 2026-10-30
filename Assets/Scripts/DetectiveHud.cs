using System;
using System.Linq;
using System.Text;
using ITKamianets.Engine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Temnorid.Detective
{
	/// <summary>All the screens, drawn with plain uGUI built in code, in Ukrainian. It only shows what <see cref="DetectiveGame"/> says and calls it when a button is pressed.</summary>
	public sealed class DetectiveHud : MonoBehaviour
	{
		public DetectiveGame Game;

		static readonly Color Panel = new Color(0.16f, 0.12f, 0.08f, 0.94f);
		static readonly Color Ink = new Color(0.95f, 0.9f, 0.78f);
		static readonly Color Gold = new Color(0.92f, 0.75f, 0.35f);
		static readonly Color Muted = new Color(0.75f, 0.7f, 0.6f);
		static readonly Color Chosen = new Color(0.55f, 0.4f, 0.15f, 1f);
		static readonly Color Plain = new Color(0.3f, 0.22f, 0.14f, 1f);

		UiBuilder _ui;
		GameObject _title, _brief, _hud, _inspect, _dialogue, _journal, _board, _confirm, _resolved, _pause, _settings;
		GameObject _promptBox, _toastBox, _titleContinue;
		GameObject[] _tabs;
		Button[] _tabButtons;
		Image _fade;
		Text _objective, _zone, _count, _prompt, _toast, _titleNote;
		Text _inspectTitle, _inspectMeta, _inspectBody, _inspectNew;
		Text _speaker, _question, _answer;
		Button[] _topicButtons;
		Text[] _topicTexts;
		GameObject _reportButton;
		Text _journalText;
		Button[] _evidenceButtons;
		Text[] _evidenceTexts;
		Text _evidenceDetail, _evidenceHeader, _pinText;
		Button[] _personButtons;
		Text _personDetail, _timelineText;
		Button[] _acceptButtons;
		Text[] _acceptTexts;
		Button[] _reasonButtons;
		Text[] _reasonTexts;
		Text _decisionProfiles, _decisionNote, _decisionLocked;
		GameObject _submitButton;
		Text _confirmText, _confirmWarn;
		Text _resultReport, _resultElder, _resultResult, _resultVillage, _resultStats, _resultReasons, _resultMock;
		Text _masterText, _sfxText, _fullscreenText;
		Mode _shown = (Mode)(-1);
		string _selectedEvidence;

		void Start()
		{
			_ui = new UiBuilder
			{
				ButtonColour = Plain,
				ButtonHighlight = new Color(1f, 0.92f, 0.65f),
				ButtonSelected = new Color(1f, 0.85f, 0.45f),
				ButtonPressed = new Color(0.8f, 0.65f, 0.3f),
			};
			UiBuilder.EnsureEventSystem(transform);
			var root = UiBuilder.CreateCanvas(transform).transform;
			BuildHud(root);
			BuildInspect(root);
			BuildDialogue(root);
			BuildJournal(root);
			BuildBoard(root);
			BuildConfirm(root);
			BuildResolved(root);
			BuildPause(root);
			BuildSettings(root);
			BuildBrief(root);
			BuildTitle(root);
			var fade = UiBuilder.Box("Fade", root, Vector2.zero, Vector2.one, Color.black);
			_fade = fade.GetComponent<Image>();
			_fade.raycastTarget = false;
			_fade.color = Color.clear;
		}

		// ---- building blocks ----

		Button Btn(Transform parent, string text, float x0, float y0, float x1, float y1, Action onClick, int size = 32)
		{
			var button = _ui.Button(text, parent, new Vector2(x0, y0), new Vector2(x1, y1), () =>
			{
				if (Game != null) Game.Audio.Play(Sfx.Click, 0.5f);
				onClick();
			});
			var label = button.GetComponentInChildren<Text>();
			label.fontSize = size;
			return button;
		}

		Text Lbl(Transform parent, string text, int size, TextAnchor anchor, float x0, float y0, float x1, float y1, Color colour, bool wrap = false)
			=> _ui.Label("Label", parent, text, size, anchor, new Vector2(x0, y0), new Vector2(x1, y1), colour, wrap);

		static GameObject Screen(string name, Transform root, Color colour, float x0 = 0f, float y0 = 0f, float x1 = 1f, float y1 = 1f)
			=> UiBuilder.Box(name, root, new Vector2(x0, y0), new Vector2(x1, y1), colour);

		GameObject Card(Transform parent, float x0, float y0, float x1, float y1) => UiBuilder.Box("Card", parent, new Vector2(x0, y0), new Vector2(x1, y1), Panel);

		// ---- screens ----

		void BuildTitle(Transform root)
		{
			_title = Screen("Title", root, new Color(0.05f, 0.04f, 0.03f, 0.55f));
			var t = _title.transform;
			Lbl(t, Loc.T("title"), 92, TextAnchor.MiddleCenter, 0.1f, 0.68f, 0.9f, 0.88f, Gold);
			Lbl(t, Loc.T("subtitle"), 46, TextAnchor.MiddleCenter, 0.1f, 0.6f, 0.9f, 0.7f, Ink);
			Btn(t, Loc.T("menu.new"), 0.35f, 0.46f, 0.65f, 0.55f, () => Game.NewCase());
			_titleContinue = Btn(t, Loc.T("menu.continue"), 0.35f, 0.35f, 0.65f, 0.44f, () => Game.ContinueCase()).gameObject;
			Btn(t, Loc.T("menu.settings"), 0.35f, 0.24f, 0.65f, 0.33f, () => Game.OpenSettings());
			Btn(t, Loc.T("menu.quit"), 0.35f, 0.13f, 0.65f, 0.22f, Quit);
			_titleNote = Lbl(t, "", 30, TextAnchor.MiddleCenter, 0.1f, 0.06f, 0.9f, 0.12f, new Color(1f, 0.6f, 0.5f));
			Lbl(t, Loc.T("menu.demo"), 26, TextAnchor.LowerCenter, 0.1f, 0.0f, 0.9f, 0.05f, Muted);
		}

		static void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		void BuildBrief(Transform root)
		{
			_brief = Screen("Brief", root, new Color(0f, 0f, 0f, 0.15f));
			var panel = Card(_brief.transform, 0.2f, 0.08f, 0.8f, 0.58f).transform;
			Lbl(panel, Loc.T("brief.head"), 38, TextAnchor.MiddleCenter, 0.04f, 0.86f, 0.96f, 0.98f, Gold);
			Lbl(panel, Loc.T("a.elder_start"), 32, TextAnchor.MiddleLeft, 0.05f, 0.38f, 0.95f, 0.86f, Ink, true);
			Lbl(panel, Loc.T("brief.who") + "\n" + Loc.T("brief.mock"), 24, TextAnchor.MiddleLeft, 0.05f, 0.2f, 0.95f, 0.38f, Muted, true);
			Btn(panel, Loc.T("brief.go"), 0.25f, 0.04f, 0.75f, 0.18f, () => Game.BeginInvestigation(), 34);
		}

		void BuildHud(Transform root)
		{
			_hud = Screen("Hud", root, Color.clear);
			var t = _hud.transform;
			var box = UiBuilder.Box("ObjectiveBox", t, new Vector2(0.015f, 0.89f), new Vector2(0.45f, 0.985f), new Color(0f, 0f, 0f, 0.55f));
			Lbl(box.transform, Loc.T("obj.title"), 22, TextAnchor.UpperLeft, 0.03f, 0.55f, 0.97f, 0.97f, Muted);
			_objective = Lbl(box.transform, "", 28, TextAnchor.LowerLeft, 0.03f, 0.04f, 0.97f, 0.62f, Ink, true);
			var info = UiBuilder.Box("InfoBox", t, new Vector2(0.6f, 0.89f), new Vector2(0.985f, 0.985f), new Color(0f, 0f, 0f, 0.55f));
			_zone = Lbl(info.transform, "", 28, TextAnchor.UpperRight, 0.03f, 0.5f, 0.97f, 0.97f, Ink);
			_count = Lbl(info.transform, "", 24, TextAnchor.LowerRight, 0.03f, 0.04f, 0.97f, 0.5f, Muted);
			var keys = UiBuilder.Box("KeysBox", t, new Vector2(0.52f, 0.008f), new Vector2(0.985f, 0.05f), new Color(0f, 0f, 0f, 0.5f));
			Lbl(keys.transform, Loc.T("keys"), 22, TextAnchor.MiddleRight, 0.02f, 0f, 0.98f, 1f, new Color(0.9f, 0.85f, 0.75f));
			_promptBox = UiBuilder.Box("PromptBox", t, new Vector2(0.25f, 0.07f), new Vector2(0.75f, 0.14f), new Color(0f, 0f, 0f, 0.7f));
			_prompt = Lbl(_promptBox.transform, "", 34, TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f, Gold);
			_toastBox = UiBuilder.Box("ToastBox", t, new Vector2(0.2f, 0.17f), new Vector2(0.8f, 0.26f), new Color(0f, 0f, 0f, 0.7f));
			_toast = Lbl(_toastBox.transform, "", 30, TextAnchor.MiddleCenter, 0.02f, 0f, 0.98f, 1f, Ink, true);
		}

		void BuildInspect(Transform root)
		{
			_inspect = Screen("Inspect", root, new Color(0f, 0f, 0f, 0.35f));
			var panel = Card(_inspect.transform, 0.2f, 0.18f, 0.8f, 0.82f).transform;
			_inspectTitle = Lbl(panel, "", 46, TextAnchor.MiddleCenter, 0.04f, 0.84f, 0.96f, 0.98f, Gold);
			_inspectMeta = Lbl(panel, "", 26, TextAnchor.MiddleCenter, 0.04f, 0.76f, 0.96f, 0.84f, Muted);
			_inspectBody = Lbl(panel, "", 36, TextAnchor.MiddleCenter, 0.07f, 0.3f, 0.93f, 0.76f, Ink, true);
			_inspectNew = Lbl(panel, "", 26, TextAnchor.MiddleCenter, 0.04f, 0.2f, 0.96f, 0.3f, new Color(0.7f, 0.9f, 0.7f));
			Btn(panel, Loc.T("record"), 0.3f, 0.04f, 0.7f, 0.18f, () => Game.CloseInspect(), 32);
		}

		void BuildDialogue(Transform root)
		{
			_dialogue = Screen("Dialogue", root, new Color(0f, 0f, 0f, 0.1f));
			var panel = Card(_dialogue.transform, 0.08f, 0.03f, 0.92f, 0.5f).transform;
			_speaker = Lbl(panel, "", 36, TextAnchor.MiddleLeft, 0.03f, 0.86f, 0.5f, 0.99f, Gold);
			_question = Lbl(panel, "", 26, TextAnchor.MiddleLeft, 0.03f, 0.74f, 0.97f, 0.86f, Muted);
			_answer = Lbl(panel, "", 32, TextAnchor.UpperLeft, 0.03f, 0.4f, 0.97f, 0.74f, Ink, true);
			_topicButtons = new Button[5];
			_topicTexts = new Text[5];
			for (var i = 0; i < 5; i++)
			{
				var index = i;
				var x0 = i < 3 ? 0.02f + i * 0.325f : 0.02f + (i - 3) * 0.325f;
				var y0 = i < 3 ? 0.22f : 0.04f;
				_topicButtons[i] = Btn(panel, "", x0, y0, x0 + 0.31f, y0 + 0.16f, () => OnTopic(index), 24);
				_topicTexts[i] = _topicButtons[i].GetComponentInChildren<Text>();
				_topicTexts[i].horizontalOverflow = HorizontalWrapMode.Wrap;
			}
			// The elder has two topics, so the third top slot is free for the report button.
			_reportButton = Btn(panel, Loc.T("elder.report"), 0.02f + 2 * 0.325f, 0.22f, 0.02f + 2 * 0.325f + 0.31f, 0.38f, () => Game.ElderReport(), 24).gameObject;
			Btn(panel, Loc.T("dialogue.leave"), 0.02f + 2 * 0.325f, 0.04f, 0.02f + 2 * 0.325f + 0.31f, 0.2f, () => Game.CloseDialogue(), 26);
		}

		string[] _shownTopics = new string[5];

		void OnTopic(int index)
		{
			if (_shownTopics[index] != null) Game.Ask(_shownTopics[index]);
		}

		void BuildJournal(Transform root)
		{
			_journal = Screen("Journal", root, new Color(0f, 0f, 0f, 0.5f));
			var panel = Card(_journal.transform, 0.12f, 0.08f, 0.88f, 0.92f).transform;
			Lbl(panel, Loc.T("journal"), 48, TextAnchor.MiddleCenter, 0.05f, 0.9f, 0.95f, 0.99f, Gold);
			_journalText = Lbl(panel, "", 26, TextAnchor.UpperLeft, 0.04f, 0.14f, 0.96f, 0.9f, Ink, true);
			Btn(panel, Loc.T("close"), 0.3f, 0.02f, 0.7f, 0.12f, () => Game.ClosePanel(), 30);
		}

		void BuildBoard(Transform root)
		{
			_board = Screen("Board", root, new Color(0f, 0f, 0f, 0.5f));
			var panel = Card(_board.transform, 0.05f, 0.05f, 0.95f, 0.95f).transform;
			var names = new[] { "tab.evidence", "tab.people", "tab.timeline", "tab.decision" };
			_tabButtons = new Button[4];
			for (var i = 0; i < 4; i++)
			{
				var tab = (BoardTab)i;
				_tabButtons[i] = Btn(panel, Loc.T(names[i]), 0.02f + i * 0.2f, 0.91f, 0.2f + i * 0.2f, 0.99f, () => Game.SetTab(tab), 28);
			}
			Btn(panel, Loc.T("close"), 0.84f, 0.91f, 0.98f, 0.99f, () => Game.ClosePanel(), 26);
			_tabs = new GameObject[4];
			for (var i = 0; i < 4; i++) _tabs[i] = UiBuilder.Box("Tab" + i, panel, new Vector2(0.01f, 0.01f), new Vector2(0.99f, 0.9f), Color.clear);
			BuildEvidenceTab(_tabs[0].transform);
			BuildPeopleTab(_tabs[1].transform);
			_timelineText = Lbl(_tabs[2].transform, "", 28, TextAnchor.UpperLeft, 0.02f, 0.02f, 0.98f, 0.98f, Ink, true);
			BuildDecisionTab(_tabs[3].transform);
		}

		void BuildEvidenceTab(Transform t)
		{
			_evidenceHeader = Lbl(t, "", 26, TextAnchor.MiddleLeft, 0.01f, 0.92f, 0.45f, 0.99f, Muted);
			_evidenceButtons = new Button[10];
			_evidenceTexts = new Text[10];
			for (var i = 0; i < 10; i++)
			{
				var id = CaseContent.Evidence[i].Id;
				var y1 = 0.91f - i * 0.088f;
				_evidenceButtons[i] = Btn(t, "", 0.01f, y1 - 0.08f, 0.45f, y1, () => _selectedEvidence = id, 24);
				_evidenceTexts[i] = _evidenceButtons[i].GetComponentInChildren<Text>();
				_evidenceTexts[i].alignment = TextAnchor.MiddleLeft;
				_evidenceTexts[i].horizontalOverflow = HorizontalWrapMode.Wrap;
			}
			_evidenceDetail = Lbl(t, "", 28, TextAnchor.UpperLeft, 0.48f, 0.2f, 0.99f, 0.98f, Ink, true);
			var pin = Btn(t, "", 0.6f, 0.03f, 0.9f, 0.15f, () => { if (_selectedEvidence != null) Game.TogglePin(_selectedEvidence); }, 28);
			_pinText = pin.GetComponentInChildren<Text>();
		}

		void BuildPeopleTab(Transform t)
		{
			_personButtons = new Button[5];
			for (var i = 0; i < 5; i++)
			{
				var id = CaseContent.People[i];
				var y1 = 0.98f - i * 0.15f;
				_personButtons[i] = Btn(t, Loc.T("p." + id) + " — " + Loc.T("p." + id + ".role"), 0.01f, y1 - 0.13f, 0.38f, y1, () => Game.SelectPerson(id), 24);
				_personButtons[i].GetComponentInChildren<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
			}
			_personDetail = Lbl(t, "", 26, TextAnchor.UpperLeft, 0.4f, 0.02f, 0.99f, 0.98f, Ink, true);
		}

		void BuildDecisionTab(Transform t)
		{
			Lbl(t, Loc.T("decision.head"), 34, TextAnchor.MiddleLeft, 0.01f, 0.9f, 0.6f, 0.99f, Gold);
			_acceptButtons = new Button[4];
			_acceptTexts = new Text[4];
			for (var i = 0; i < 3; i++)
			{
				var id = CaseContent.Suspects[i];
				var x0 = 0.01f + i * 0.245f;
				_acceptButtons[i] = Btn(t, Loc.T("decision.accuse", Loc.T("p." + id)), x0, 0.78f, x0 + 0.235f, 0.89f, () => Game.ChooseAccused(id), 24);
			}
			_acceptButtons[3] = Btn(t, Loc.T("decision.insufficient"), 0.745f, 0.78f, 0.99f, 0.89f, () => Game.ChooseInsufficient(), 24);
			for (var i = 0; i < 4; i++) _acceptTexts[i] = _acceptButtons[i].GetComponentInChildren<Text>();
			_decisionProfiles = Lbl(t, "", 22, TextAnchor.UpperLeft, 0.01f, 0.36f, 0.5f, 0.77f, Muted, true);
			Lbl(t, Loc.T("decision.reasons"), 24, TextAnchor.MiddleLeft, 0.52f, 0.7f, 0.99f, 0.77f, Muted);
			_reasonButtons = new Button[10];
			_reasonTexts = new Text[10];
			for (var i = 0; i < 10; i++)
			{
				var id = CaseContent.Evidence[i].Id;
				var col = i % 2;
				var row = i / 2;
				var x0 = 0.52f + col * 0.245f;
				var y1 = 0.69f - row * 0.075f;
				_reasonButtons[i] = Btn(t, "", x0, y1 - 0.068f, x0 + 0.24f, y1, () => Game.ToggleReason(id), 18);
				_reasonTexts[i] = _reasonButtons[i].GetComponentInChildren<Text>();
				_reasonTexts[i].horizontalOverflow = HorizontalWrapMode.Wrap;
			}
			_decisionNote = Lbl(t, Loc.T("decision.warn"), 26, TextAnchor.MiddleLeft, 0.01f, 0.2f, 0.5f, 0.35f, Ink, true);
			_decisionLocked = Lbl(t, Loc.T("decision.locked"), 24, TextAnchor.MiddleLeft, 0.01f, 0.1f, 0.6f, 0.2f, new Color(1f, 0.7f, 0.5f), true);
			_submitButton = Btn(t, Loc.T("confirm"), 0.6f, 0.02f, 0.99f, 0.14f, () => Game.RequestReport(), 28).gameObject;
		}

		void BuildConfirm(Transform root)
		{
			_confirm = Screen("Confirm", root, new Color(0f, 0f, 0f, 0.6f));
			var panel = Card(_confirm.transform, 0.22f, 0.22f, 0.78f, 0.78f).transform;
			_confirmText = Lbl(panel, "", 40, TextAnchor.MiddleCenter, 0.05f, 0.5f, 0.95f, 0.95f, Gold, true);
			_confirmWarn = Lbl(panel, "", 28, TextAnchor.MiddleCenter, 0.06f, 0.25f, 0.94f, 0.5f, Ink, true);
			Btn(panel, Loc.T("cancel"), 0.06f, 0.05f, 0.46f, 0.22f, () => Game.CancelReport(), 30);
			Btn(panel, Loc.T("confirm"), 0.54f, 0.05f, 0.94f, 0.22f, () => Game.ConfirmReport(), 30);
		}

		void BuildResolved(Transform root)
		{
			_resolved = Screen("Resolved", root, new Color(0.04f, 0.03f, 0.03f, 0.55f));
			var panel = Card(_resolved.transform, 0.12f, 0.05f, 0.88f, 0.95f).transform;
			Lbl(panel, Loc.T("result.head"), 54, TextAnchor.MiddleCenter, 0.05f, 0.89f, 0.95f, 0.99f, Gold);
			_resultReport = Lbl(panel, "", 28, TextAnchor.UpperLeft, 0.05f, 0.7f, 0.95f, 0.89f, Ink, true);
			_resultElder = Lbl(panel, "", 28, TextAnchor.UpperLeft, 0.05f, 0.55f, 0.95f, 0.7f, Ink, true);
			_resultResult = Lbl(panel, "", 32, TextAnchor.UpperLeft, 0.05f, 0.4f, 0.95f, 0.55f, Gold, true);
			_resultVillage = Lbl(panel, "", 26, TextAnchor.UpperLeft, 0.05f, 0.3f, 0.95f, 0.4f, Muted, true);
			_resultReasons = Lbl(panel, "", 24, TextAnchor.UpperLeft, 0.05f, 0.22f, 0.95f, 0.3f, Muted, true);
			_resultStats = Lbl(panel, "", 26, TextAnchor.MiddleLeft, 0.05f, 0.15f, 0.95f, 0.22f, Ink);
			_resultMock = Lbl(panel, Loc.T("result.mock"), 22, TextAnchor.MiddleLeft, 0.05f, 0.1f, 0.95f, 0.16f, Muted, true);
			Btn(panel, Loc.T("replay"), 0.1f, 0.01f, 0.48f, 0.09f, () => Game.Replay(), 28);
			Btn(panel, Loc.T("menu"), 0.52f, 0.01f, 0.9f, 0.09f, () => Game.ToTitle(), 28);
		}

		void BuildPause(Transform root)
		{
			_pause = Screen("Pause", root, new Color(0f, 0f, 0f, 0.6f));
			var panel = Card(_pause.transform, 0.33f, 0.14f, 0.67f, 0.86f).transform;
			Lbl(panel, Loc.T("pause"), 52, TextAnchor.MiddleCenter, 0.05f, 0.86f, 0.95f, 0.98f, Gold);
			Btn(panel, Loc.T("resume"), 0.1f, 0.7f, 0.9f, 0.83f, () => Game.Resume());
			Btn(panel, Loc.T("journal"), 0.1f, 0.55f, 0.9f, 0.68f, () => Game.OpenPanel(Mode.Journal));
			Btn(panel, Loc.T("board"), 0.1f, 0.4f, 0.9f, 0.53f, () => Game.OpenBoard(BoardTab.Evidence));
			Btn(panel, Loc.T("menu.settings"), 0.1f, 0.25f, 0.9f, 0.38f, () => Game.OpenSettings());
			Btn(panel, Loc.T("restart"), 0.1f, 0.12f, 0.9f, 0.23f, () => Game.RestartInvestigation(), 26);
			Btn(panel, Loc.T("menu"), 0.1f, 0.01f, 0.9f, 0.11f, () => Game.ToTitle(), 26);
		}

		void BuildSettings(Transform root)
		{
			_settings = Screen("Settings", root, new Color(0.05f, 0.04f, 0.03f, 0.97f));
			var t = _settings.transform;
			Lbl(t, Loc.T("menu.settings"), 64, TextAnchor.MiddleCenter, 0.1f, 0.84f, 0.9f, 0.96f, Gold);
			_masterText = Lbl(t, "", 38, TextAnchor.MiddleRight, 0.1f, 0.7f, 0.5f, 0.8f, Ink);
			Btn(t, "−", 0.52f, 0.7f, 0.58f, 0.8f, () => Game.SetVolumes(Game.Settings.masterVolume - 0.1f, Game.Settings.sfxVolume));
			Btn(t, "+", 0.6f, 0.7f, 0.66f, 0.8f, () => Game.SetVolumes(Game.Settings.masterVolume + 0.1f, Game.Settings.sfxVolume));
			_sfxText = Lbl(t, "", 38, TextAnchor.MiddleRight, 0.1f, 0.57f, 0.5f, 0.67f, Ink);
			Btn(t, "−", 0.52f, 0.57f, 0.58f, 0.67f, () => Game.SetVolumes(Game.Settings.masterVolume, Game.Settings.sfxVolume - 0.1f));
			Btn(t, "+", 0.6f, 0.57f, 0.66f, 0.67f, () => Game.SetVolumes(Game.Settings.masterVolume, Game.Settings.sfxVolume + 0.1f));
			_fullscreenText = Btn(t, "", 0.3f, 0.44f, 0.7f, 0.54f, () => Game.SetFullscreen(!Game.Settings.fullscreen), 34).GetComponentInChildren<Text>();
			Lbl(t, Loc.T("controls"), 30, TextAnchor.MiddleCenter, 0.12f, 0.22f, 0.88f, 0.4f, Muted, true);
			Btn(t, Loc.T("back"), 0.38f, 0.06f, 0.62f, 0.16f, () => Game.CloseSettings());
		}

		// ---- showing ----

		void Update()
		{
			if (Game == null || _fade == null) return;
			var mode = Game.Mode;
			if (mode != _shown)
			{
				Show(mode);
				_shown = mode;
			}
			Refresh(mode);
		}

		void Show(Mode mode)
		{
			_title.SetActive(mode == Mode.Title);
			_brief.SetActive(mode == Mode.Brief);
			_hud.SetActive(mode == Mode.Playing);
			_inspect.SetActive(mode == Mode.Inspect);
			_dialogue.SetActive(mode == Mode.Dialogue);
			_journal.SetActive(mode == Mode.Journal);
			_board.SetActive(mode == Mode.Board);
			_confirm.SetActive(mode == Mode.Confirm);
			_resolved.SetActive(mode == Mode.Resolved);
			_pause.SetActive(mode == Mode.Pause);
			_settings.SetActive(mode == Mode.Settings);

			GameObject screen = null;
			switch (mode)
			{
				case Mode.Title: screen = _title; break;
				case Mode.Brief: screen = _brief; break;
				case Mode.Inspect: screen = _inspect; break;
				case Mode.Dialogue: screen = _dialogue; break;
				case Mode.Journal: screen = _journal; break;
				case Mode.Board: screen = _board; break;
				case Mode.Confirm: screen = _confirm; break;
				case Mode.Resolved: screen = _resolved; break;
				case Mode.Pause: screen = _pause; break;
				case Mode.Settings: screen = _settings; break;
			}
			if (screen != null) UiBuilder.Select(screen);
		}

		void Refresh(Mode mode)
		{
			var inv = Game.Inv;
			switch (mode)
			{
				case Mode.Title:
					_titleContinue.SetActive(Game.HasSave);
					UiBuilder.SetText(_titleNote, Game.SaveProblem ? Loc.T("menu.corrupt") : "");
					break;
				case Mode.Playing:
					UiBuilder.SetText(_objective, Game.Objective);
					UiBuilder.SetText(_zone, Game.ZoneName);
					UiBuilder.SetText(_count, Loc.T("evidence.count", inv.Evidence.Count));
					var prompt = Game.PromptText;
					_promptBox.SetActive(prompt != "");
					UiBuilder.SetText(_prompt, prompt);
					var toast = Game.ToastActive;
					_toastBox.SetActive(toast);
					if (toast) UiBuilder.SetText(_toast, Game.ToastText);
					break;
				case Mode.Inspect:
					var id = Game.InspectId;
					var def = CaseContent.FindEvidence(id);
					UiBuilder.SetText(_inspectTitle, Loc.T(id + ".title"));
					UiBuilder.SetText(_inspectMeta, Loc.T("location") + ": " + Loc.T("z." + def.Zone) + "    ·    " + Loc.T("source") + ": " + Loc.T("src." + def.Source));
					UiBuilder.SetText(_inspectBody, Loc.T(id + ".text"));
					UiBuilder.SetText(_inspectNew, Game.InspectWasNew ? Loc.T("new.evidence", Loc.T(id + ".title")) : Loc.T("recorded.already"));
					break;
				case Mode.Dialogue:
					RefreshDialogue();
					break;
				case Mode.Journal:
					RefreshJournal();
					break;
				case Mode.Board:
					RefreshBoard();
					break;
				case Mode.Confirm:
					var accused = Game.DecisionInsufficient ? Loc.T("decision.insufficient") : Loc.T("decision.accuse", Loc.T("p." + Game.DecisionAccused));
					UiBuilder.SetText(_confirmText, accused);
					UiBuilder.SetText(_confirmWarn, Loc.T("decision.warn") + "\n" + Loc.T("decision.warn2") + "\n" + Loc.T("decision.unseen", inv.UnvisitedZones, inv.UnheardPeople));
					break;
				case Mode.Resolved:
					RefreshResolved();
					break;
				case Mode.Settings:
					UiBuilder.SetText(_masterText, Loc.T("volume.master") + ": " + Mathf.RoundToInt(Game.Settings.masterVolume * 100f) + "%");
					UiBuilder.SetText(_sfxText, Loc.T("volume.sfx") + ": " + Mathf.RoundToInt(Game.Settings.sfxVolume * 100f) + "%");
					UiBuilder.SetText(_fullscreenText, Loc.T("fullscreen") + ": " + (Game.Settings.fullscreen ? "✓" : "—"));
					break;
			}
		}

		void RefreshDialogue()
		{
			var npc = Game.NpcId;
			if (npc == null) return;
			UiBuilder.SetText(_speaker, Loc.T("p." + npc));
			UiBuilder.SetText(_question, Game.QuestionText == "" ? Loc.T("dialogue.pick") : Game.QuestionText);
			UiBuilder.SetText(_answer, Game.AnswerText + (Game.AnswerWasRepeat ? "\n" + Loc.T("dialogue.repeat") : ""));
			var topics = Game.DialogueTopics.ToList();
			for (var i = 0; i < _topicButtons.Length; i++)
			{
				var present = i < topics.Count;
				if (_topicButtons[i].gameObject.activeSelf != present) _topicButtons[i].gameObject.SetActive(present);
				_shownTopics[i] = present ? topics[i].Id : null;
				if (present) UiBuilder.SetText(_topicTexts[i], Loc.T("q." + topics[i].Id));
			}
			_reportButton.SetActive(npc == Ids.Elder);
		}

		void RefreshJournal()
		{
			var inv = Game.Inv;
			var sb = new StringBuilder();
			sb.Append(Loc.T("obj.title")).Append(": ").Append(Game.Objective).Append("\n\n");
			sb.Append(Loc.T("journal.evidence")).Append(" (").Append(Loc.T("evidence.count", inv.Evidence.Count)).Append("):\n");
			if (inv.Evidence.Count == 0) sb.Append("  ").Append(Loc.T("journal.none")).Append('\n');
			foreach (var e in inv.Evidence)
				sb.Append("• ").Append(Loc.T(e + ".title")).Append(" — ").Append(Loc.T("z." + CaseContent.FindEvidence(e).Zone)).Append(" [").Append(Loc.T("src." + CaseContent.FindEvidence(e).Source)).Append("]\n");
			sb.Append("\n").Append(Loc.T("journal.statements")).Append(":\n");
			if (inv.Statements.Count == 0) sb.Append("  ").Append(Loc.T("no.statements")).Append('\n');
			foreach (var s in inv.Statements)
				sb.Append("• ").Append(Loc.T("p." + s.Speaker)).Append(" [").Append(Loc.T("src." + s.Source)).Append("]: «").Append(Loc.T("a." + s.TopicId)).Append("»\n");
			sb.Append("\n").Append(Loc.T("journal.conflicts")).Append(":\n");
			if (inv.Conflicts.Count == 0) sb.Append("  ").Append(Loc.T("journal.none")).Append('\n');
			foreach (var c in inv.Conflicts) sb.Append("• ").Append(Loc.T(c)).Append('\n');
			UiBuilder.SetText(_journalText, sb.ToString());
		}

		void RefreshBoard()
		{
			var inv = Game.Inv;
			for (var i = 0; i < 4; i++)
			{
				if (_tabs[i].activeSelf != (i == (int)Game.Tab)) _tabs[i].SetActive(i == (int)Game.Tab);
				_tabButtons[i].GetComponent<Image>().color = i == (int)Game.Tab ? Chosen : Plain;
			}

			switch (Game.Tab)
			{
				case BoardTab.Evidence:
					UiBuilder.SetText(_evidenceHeader, Loc.T("evidence.count", inv.Evidence.Count));
					for (var i = 0; i < 10; i++)
					{
						var e = CaseContent.Evidence[i].Id;
						var known = inv.Has(e);
						if (_evidenceButtons[i].gameObject.activeSelf != known) _evidenceButtons[i].gameObject.SetActive(known);
						if (known) UiBuilder.SetText(_evidenceTexts[i], (inv.Pinned.Contains(e) ? "★ " : "") + Loc.T(e + ".title"));
					}
					if (_selectedEvidence == null || !inv.Has(_selectedEvidence)) _selectedEvidence = inv.Evidence.Count > 0 ? inv.Evidence[0] : null;
					if (_selectedEvidence == null)
					{
						UiBuilder.SetText(_evidenceDetail, Loc.T("journal.none"));
						UiBuilder.SetText(_pinText, "");
					}
					else
					{
						var d = CaseContent.FindEvidence(_selectedEvidence);
						UiBuilder.SetText(_evidenceDetail, Loc.T(_selectedEvidence + ".title") + "\n" + Loc.T("location") + ": " + Loc.T("z." + d.Zone) + "\n" + Loc.T("source") + ": " + Loc.T("src." + d.Source) + "\n\n" + Loc.T(_selectedEvidence + ".text"));
						UiBuilder.SetText(_pinText, inv.Pinned.Contains(_selectedEvidence) ? Loc.T("unpin") : Loc.T("pin"));
					}
					break;
				case BoardTab.People:
					RefreshPerson();
					break;
				case BoardTab.Timeline:
					var sb = new StringBuilder();
					var entries = inv.TimelineEntries();
					if (entries.Count == 0) sb.Append(Loc.T("no.timeline"));
					foreach (var t in entries)
						sb.Append("• ").Append(Loc.T("time." + t.Time)).Append("  [").Append(Loc.T("basis." + t.Basis)).Append(t.Uncertain ? " " + Loc.T("uncertain") : "").Append("]  ").Append(Loc.T(t.Id)).Append('\n');
					sb.Append("\n").Append(Loc.T("timeline.note"));
					UiBuilder.SetText(_timelineText, sb.ToString());
					break;
				case BoardTab.Decision:
					RefreshDecision();
					break;
			}
		}

		void RefreshPerson()
		{
			var inv = Game.Inv;
			var person = Game.SelectedPerson;
			var sb = new StringBuilder();
			sb.Append(Loc.T("p." + person)).Append(" — ").Append(Loc.T("p." + person + ".role")).Append("\n").Append(Loc.T("p." + person + ".profile")).Append("\n\n");
			sb.Append(Loc.T("journal.statements")).Append(":\n");
			var statements = inv.Statements.Where(s => s.Speaker == person).ToList();
			if (statements.Count == 0) sb.Append("  ").Append(Loc.T("no.statements")).Append('\n');
			foreach (var s in statements) sb.Append("• [").Append(Loc.T("src." + s.Source)).Append("] «").Append(Loc.T("a." + s.TopicId)).Append("»\n");
			var related = inv.Conflicts.Where(c => CaseContent.Conflicts.First(x => x.Id == c) is var def && (Speaks(def.A, person) || Speaks(def.B, person))).ToList();
			if (related.Count > 0)
			{
				sb.Append("\n").Append(Loc.T("journal.conflicts")).Append(":\n");
				foreach (var c in related) sb.Append("• ").Append(Loc.T(c)).Append("\n   ").Append(Loc.T("conflict.options")).Append(": ").Append(Loc.T(c + ".opts")).Append('\n');
			}
			UiBuilder.SetText(_personDetail, sb.ToString());
		}

		static bool Speaks(string requirement, string person) => requirement.StartsWith("t:") && CaseContent.FindTopic(requirement.Substring(2)).Npc == person;

		void RefreshDecision()
		{
			var inv = Game.Inv;
			for (var i = 0; i < 3; i++) _acceptButtons[i].GetComponent<Image>().color = Game.DecisionAccused == CaseContent.Suspects[i] ? Chosen : Plain;
			_acceptButtons[3].GetComponent<Image>().color = Game.DecisionInsufficient ? Chosen : Plain;
			var sb = new StringBuilder();
			foreach (var s in CaseContent.Suspects) sb.Append(Loc.T("p." + s)).Append(": ").Append(Loc.T("p." + s + ".profile")).Append("\n\n");
			UiBuilder.SetText(_decisionProfiles, sb.ToString());
			for (var i = 0; i < 10; i++)
			{
				var e = CaseContent.Evidence[i].Id;
				var known = inv.Has(e);
				if (_reasonButtons[i].gameObject.activeSelf != known) _reasonButtons[i].gameObject.SetActive(known);
				if (!known) continue;
				UiBuilder.SetText(_reasonTexts[i], (Game.DecisionReasons.Contains(e) ? "✓ " : "") + Loc.T(e + ".title"));
				_reasonButtons[i].GetComponent<Image>().color = Game.DecisionReasons.Contains(e) ? Chosen : Plain;
			}
			_decisionLocked.gameObject.SetActive(!inv.CanReport);
			_submitButton.SetActive(inv.CanReport && Game.HasChoice);
		}

		void RefreshResolved()
		{
			var r = Game.Result;
			if (r == null) return;
			UiBuilder.SetText(_resultReport, Loc.T("result.report") + ": " + Loc.T("out." + r.OutcomeId + ".report"));
			UiBuilder.SetText(_resultElder, Loc.T("result.response") + ": " + Loc.T("out." + r.OutcomeId + ".elder"));
			UiBuilder.SetText(_resultResult, Loc.T("out." + r.OutcomeId + ".result"));
			UiBuilder.SetText(_resultVillage, Loc.T("result.village") + ": " + Loc.T("out." + r.OutcomeId + ".village"));
			UiBuilder.SetText(_resultReasons, r.Reasons.Length == 0 ? "" : Loc.T("result.reasons") + ": " + string.Join("; ", r.Reasons.Select(e => Loc.T(e + ".title"))));
			UiBuilder.SetText(_resultStats, Loc.T("result.time") + ": " + Game.ElapsedText + "     " + Loc.T("result.evidence") + ": " + Game.Inv.Evidence.Count + " / 10");
		}
	}
}

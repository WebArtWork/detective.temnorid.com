using ITKamianets.Engine.Shell;
using UnityEngine;

namespace Temnorid.Detective
{
	/// <summary>
	/// What the shared shell (splash, landing, lobby, top and bottom bars) needs to know about this game. The shell's own
	/// screens are in English; the places of the in-game bars are in Ukrainian like the rest of the game.
	/// </summary>
	public static class DetectiveShell
	{
		public const string Repo = "WebArtWork/detective.temnorid.com";

		/// <summary>The place ids of the in-game bar. The first is the village itself (the case, the start screen, the clues in the world).</summary>
		public const string PlaceCase = "case";
		public const string PlaceJournal = "journal";
		public const string PlaceBoard = "board";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Configure()
		{
			ShellConfig.Current = new ShellConfig
			{
				Title = "Temnorid Detective",
				Tagline = "Two investigators, one village, one case",
				GameId = "detective",
				Repo = Repo,
				InstallerSuffix = "-Setup.exe",
				MaxPlayers = 2,
				Multiplayer = true,
				GameScene = "Village",
				Accent = new Color(0.92f, 0.75f, 0.35f),
				Places =
				{
					new ShellPlace(PlaceCase, Loc.T("place.case"), "С"),
					new ShellPlace(PlaceJournal, Loc.T("journal"), "Щ"),
					new ShellPlace(PlaceBoard, Loc.T("board"), "Д"),
				},
			};
		}
	}
}

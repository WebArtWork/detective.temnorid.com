using System.Collections.Generic;
using ITKamianets.Engine.FixedCamera;
using ITKamianets.Engine.Graybox;
using ITKamianets.Engine.Graybox.EditorTools;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Temnorid.Detective.EditorTools
{
	/// <summary>
	/// Builds Greyford from Unity primitives: the granary yard with its back gate, the square with the elder's hall, the bakery,
	/// the stall and Orest's house, the watch post and the ford; plus the investigator, the five villagers, the ten clues, the camera
	/// shots, the light and the screens. Run it from the menu (Temnorid > Detective > Build Scene) or the command line
	/// (<c>-executeMethod Temnorid.Detective.EditorTools.DetectiveSceneBuilder.Build</c>); it rebuilds
	/// <c>Assets/Scenes/Village.unity</c> from scratch each time, so do not edit that scene by hand. Positions come from <see cref="VillageLayout"/>.
	/// </summary>
	public static class DetectiveSceneBuilder
	{
		public const string ScenePath = "Assets/Scenes/Village.unity";
		const string MaterialDir = "Assets/Materials";

		static Material _grass, _dirt, _paving, _stone, _wood, _woodDark, _plaster, _plasterWarm, _thatch, _paper, _sack, _strap, _canvas, _leaf, _skin, _iron, _water, _cloth, _ledger, _dark;

		[MenuItem("Temnorid/Detective/Build Scene")]
		public static void Build()
		{
			PlayerSettings.productName = "Temnorid Detective";
			PlayerSettings.companyName = "Temnorid";
			PlayerSettings.bundleVersion = "0.1.0";
			PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.itkamianets.temnoriddetective");
			PlayerSettings.runInBackground = true;
			PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;

			var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.52f);
			RenderSettings.fog = true;
			RenderSettings.fogMode = FogMode.Exponential;
			RenderSettings.fogDensity = 0.004f;
			RenderSettings.fogColor = new Color(0.66f, 0.68f, 0.7f);
			MakeMaterials();

			var world = new GameObject("World").transform;
			var props = new GameObject("Props").transform;
			var shots = new GameObject("CameraShots").transform;
			var talk = new GameObject("Interactables").transform;
			var people = new GameObject("People").transform;

			BuildGround(world, props);
			BuildBoundaries(world, props);
			BuildGranary(world, props, talk);
			BuildSquare(world, props, talk);
			BuildWatchPost(world, props, talk);
			BuildFord(world, props, talk);
			BuildTrees(props);
			BuildShots(shots);

			var npcs = new List<Npc>
			{
				BuildNpc(people, talk, Ids.Elder, VillageLayout.Elder, 180f, new Color(0.38f, 0.3f, 0.45f), new Color(0.25f, 0.2f, 0.28f), new Color(0.8f, 0.8f, 0.82f), false),
				BuildNpc(people, talk, Ids.Mira, VillageLayout.Mira, 0f, new Color(0.45f, 0.5f, 0.35f), new Color(0.3f, 0.25f, 0.2f), new Color(0.3f, 0.2f, 0.12f), false),
				BuildNpc(people, talk, Ids.Orest, VillageLayout.Orest, 270f, new Color(0.5f, 0.38f, 0.25f), new Color(0.25f, 0.22f, 0.2f), new Color(0.25f, 0.18f, 0.1f), false),
				BuildNpc(people, talk, Ids.Ada, VillageLayout.Ada, 180f, new Color(0.85f, 0.8f, 0.68f), new Color(0.35f, 0.3f, 0.28f), new Color(0.5f, 0.35f, 0.2f), false),
				BuildNpc(people, talk, Ids.Taras, VillageLayout.Taras, 180f, new Color(0.35f, 0.42f, 0.5f), new Color(0.2f, 0.2f, 0.25f), new Color(0.2f, 0.15f, 0.1f), true),
			};

			var player = BuildPlayer();
			var rig = BuildCamera(player);
			BuildSun();

			var overviewCam = Empty(shots, "OverviewCamera", new Vector3(-6f, 13f, -17f));
			var overviewLook = Empty(shots, "OverviewLook", new Vector3(-29f, 0.5f, -1f));
			var endCam = Empty(shots, "EndCamera", new Vector3(2f, 11f, -15f));
			var endLook = Empty(shots, "EndLook", new Vector3(0f, 0.5f, 1f));

			var gameObject = new GameObject("Game");
			var audio = gameObject.AddComponent<DetectiveAudio>();
			var game = gameObject.AddComponent<DetectiveGame>();
			game.Player = player;
			game.Rig = rig;
			game.Audio = audio;
			game.Npcs = npcs.ToArray();
			game.OverviewCamera = overviewCam;
			game.OverviewLook = overviewLook;
			game.EndCamera = endCam;
			game.EndLook = endLook;
			var hud = gameObject.AddComponent<DetectiveHud>();
			hud.Game = game;

			EditorSceneManager.SaveScene(scene, ScenePath);
			EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
			AssetDatabase.SaveAssets();
			Debug.Log("[Detective] Scene built: " + ScenePath);
		}

		static void MakeMaterials()
		{
			_grass = Mat("Grass", new Color(0.3f, 0.4f, 0.24f));
			_dirt = Mat("Dirt", new Color(0.42f, 0.34f, 0.24f));
			_paving = Mat("Paving", new Color(0.52f, 0.5f, 0.46f));
			_stone = Mat("Stone", new Color(0.5f, 0.5f, 0.52f));
			_wood = Mat("Wood", new Color(0.5f, 0.35f, 0.2f));
			_woodDark = Mat("WoodDark", new Color(0.3f, 0.2f, 0.12f));
			_plaster = Mat("Plaster", new Color(0.72f, 0.68f, 0.58f));
			_plasterWarm = Mat("PlasterWarm", new Color(0.7f, 0.58f, 0.45f));
			_thatch = Mat("Thatch", new Color(0.55f, 0.46f, 0.25f));
			_paper = Mat("Paper", new Color(0.9f, 0.86f, 0.72f));
			_sack = Mat("Sack", new Color(0.7f, 0.62f, 0.45f));
			_strap = Mat("RedStrap", new Color(0.7f, 0.15f, 0.12f));
			_canvas = Mat("Canvas", new Color(0.75f, 0.72f, 0.62f));
			_leaf = Mat("Leaf", new Color(0.2f, 0.38f, 0.18f));
			_skin = Mat("Skin", new Color(0.8f, 0.66f, 0.55f));
			_iron = Mat("Iron", new Color(0.25f, 0.26f, 0.3f));
			_water = Mat("Water", new Color(0.3f, 0.45f, 0.58f));
			_cloth = Mat("Cloth", new Color(0.4f, 0.4f, 0.42f));
			_ledger = Mat("LedgerBook", new Color(0.45f, 0.18f, 0.14f));
			_dark = Mat("Dark", new Color(0.08f, 0.07f, 0.07f));
		}

		// ---- ground and the edge of the village ----

		static void BuildGround(Transform world, Transform props)
		{
			Box(world, "Ground", new Vector3(-11f, -0.25f, -4f), new Vector3(60f, 0.5f, 74f), _grass);
			Prim(PrimitiveType.Cube, "SquarePaving", props, new Vector3(0.5f, 0.02f, 0.5f), new Vector3(26f, 0.04f, 21f), _paving);
			Prim(PrimitiveType.Cube, "YardPath", props, new Vector3(-24f, 0.02f, 0f), new Vector3(22f, 0.04f, 4.5f), _dirt);
			Prim(PrimitiveType.Cube, "NorthPath", props, new Vector3(0f, 0.02f, 20f), new Vector3(3.2f, 0.04f, 20f), _dirt);
			Prim(PrimitiveType.Cube, "SouthPath", props, new Vector3(0f, 0.02f, -22f), new Vector3(3.2f, 0.04f, 24f), _dirt);
			Prim(PrimitiveType.Cube, "WatchYard", props, new Vector3(1.5f, 0.02f, 19f), new Vector3(16f, 0.04f, 10f), _dirt);
		}

		static void Barrier(Transform parent, string name, float x0, float z0, float x1, float z1)
		{
			var go = new GameObject(name);
			go.transform.SetParent(parent, false);
			go.transform.position = new Vector3((x0 + x1) * 0.5f, 1.5f, (z0 + z1) * 0.5f);
			var box = go.AddComponent<BoxCollider>();
			box.size = new Vector3(Mathf.Max(0.4f, Mathf.Abs(x1 - x0)), 3f, Mathf.Max(0.4f, Mathf.Abs(z1 - z0)));
		}

		static void FenceRun(Transform parent, string name, float x0, float z0, float x1, float z1)
		{
			var centre = new Vector3((x0 + x1) * 0.5f, 0.55f, (z0 + z1) * 0.5f);
			var size = new Vector3(Mathf.Max(0.18f, Mathf.Abs(x1 - x0)), 1.1f, Mathf.Max(0.18f, Mathf.Abs(z1 - z0)));
			Box(parent, name, centre, size, _wood);
		}

		static void BuildBoundaries(Transform world, Transform props)
		{
			// The edge of the walkable village: nothing to see, nothing to cross.
			Barrier(world, "East", 15.5f, -36.5f, 15.5f, 29.5f);
			Barrier(world, "North", -13.5f, 29f, 15.5f, 29f);
			Barrier(world, "South", -13.5f, -36.5f, 15.5f, -36.5f);
			Barrier(world, "WestUpper", -13.5f, 9.4f, -13.5f, 29f);
			Barrier(world, "WestLower", -13.5f, -36.5f, -13.5f, -9.4f);

			// The granary yard: a fence all round, open to the square on the east side.
			var fence = new GameObject("YardFence").transform;
			fence.SetParent(props, false);
			FenceRun(fence, "North", VillageLayout.YardWest, 9.5f, VillageLayout.WestEdge, 9.5f);
			FenceRun(fence, "South", VillageLayout.YardWest, -9.5f, VillageLayout.WestEdge, -9.5f);
			FenceRun(fence, "WestN", VillageLayout.YardWest, 1.6f, VillageLayout.YardWest, 9.5f);
			FenceRun(fence, "WestS", VillageLayout.YardWest, -9.5f, VillageLayout.YardWest, -1.6f);
			FenceRun(fence, "EastN", VillageLayout.WestEdge, 2.5f, VillageLayout.WestEdge, 9.5f);
			FenceRun(fence, "EastS", VillageLayout.WestEdge, -9.5f, VillageLayout.WestEdge, -2.5f);
			Barrier(world, "BehindGate", -36.4f, -1.6f, -36.4f, 1.6f);
		}

		// ---- zone A: the granary and the back gate ----

		static void BuildGranary(Transform world, Transform props, Transform talk)
		{
			var granary = new GameObject("Granary").transform;
			granary.SetParent(props, false);
			Box(granary, "Walls", new Vector3(-28.5f, 3f, 3.6f), new Vector3(11f, 6f, 6.2f), _plasterWarm);
			Prim(PrimitiveType.Cube, "Roof", granary, new Vector3(-28.5f, 6.25f, 3.6f), new Vector3(12.2f, 0.5f, 7.4f), _thatch);
			Prim(PrimitiveType.Cube, "Ridge", granary, new Vector3(-28.5f, 6.8f, 3.6f), new Vector3(12.2f, 0.6f, 1.2f), _thatch);
			Prim(PrimitiveType.Cube, "Door", granary, new Vector3(-28.5f, 1.4f, 0.47f), new Vector3(2.6f, 2.8f, 0.06f), _dark);
			granary.gameObject.AddComponent<AssetIdTag>().Id = "buildings/granary";

			// The shelf of sack places, four of them empty with torn fibres left on the boards (e02).
			var rack = new GameObject("SackRack").transform;
			rack.SetParent(props, false);
			Box(rack, "Shelf", new Vector3(-26.5f, 0.7f, -0.1f), new Vector3(3f, 0.12f, 0.9f), _woodDark);
			Box(rack, "PostL", new Vector3(-27.9f, 0.35f, -0.1f), new Vector3(0.12f, 0.7f, 0.8f), _woodDark);
			Box(rack, "PostR", new Vector3(-25.1f, 0.35f, -0.1f), new Vector3(0.12f, 0.7f, 0.8f), _woodDark);
			for (var i = 0; i < 8; i++)
			{
				var x = -27.6f + i * 0.4f;
				if (i >= 2 && i < 6) Prim(PrimitiveType.Cube, "Fibres", rack, new Vector3(x, 0.78f, -0.1f), new Vector3(0.22f, 0.02f, 0.3f), _sack);
				else Prim(PrimitiveType.Sphere, "Sack", rack, new Vector3(x, 0.98f, -0.1f), new Vector3(0.36f, 0.4f, 0.4f), _sack);
			}
			rack.gameObject.AddComponent<AssetIdTag>().Id = "props/sack-rack";
			Inspect(talk, Ids.E02, VillageLayout.E02);

			// Stefan, covered, near the back gate (e03).
			var body = new GameObject("StefanCovered").transform;
			body.SetParent(props, false);
			Prim(PrimitiveType.Cube, "Cloth", body, new Vector3(-31f, 0.16f, -1.9f), new Vector3(1.9f, 0.3f, 0.65f), _cloth);
			Prim(PrimitiveType.Sphere, "Head", body, new Vector3(-32.1f, 0.2f, -1.9f), new Vector3(0.38f, 0.3f, 0.38f), _cloth);
			body.gameObject.AddComponent<AssetIdTag>().Id = "props/covered-body";
			Inspect(talk, Ids.E03, VillageLayout.E03);

			// The back gate and the torn strap (e01).
			var gate = new GameObject("BackGate").transform;
			gate.SetParent(props, false);
			Box(gate, "PostN", new Vector3(-35f, 1.3f, 1.7f), new Vector3(0.3f, 2.6f, 0.3f), _woodDark);
			Box(gate, "PostS", new Vector3(-35f, 1.3f, -1.7f), new Vector3(0.3f, 2.6f, 0.3f), _woodDark);
			Prim(PrimitiveType.Cube, "Beam", gate, new Vector3(-35f, 2.6f, 0f), new Vector3(0.3f, 0.3f, 3.7f), _woodDark);
			var leaf = Prim(PrimitiveType.Cube, "OpenLeaf", gate, new Vector3(-34.4f, 0.9f, 2.3f), new Vector3(1.2f, 1.8f, 0.08f), _wood);
			leaf.transform.rotation = Quaternion.Euler(0f, 40f, 0f);
			gate.gameObject.AddComponent<AssetIdTag>().Id = "props/back-gate";
			var strap = new GameObject("BrokenStrap").transform;
			strap.SetParent(props, false);
			Prim(PrimitiveType.Cube, "Strap", strap, new Vector3(-33.1f, 0.03f, 0.7f), new Vector3(0.9f, 0.02f, 0.09f), _strap).transform.rotation = Quaternion.Euler(0f, 25f, 0f);
			Prim(PrimitiveType.Cube, "Torn", strap, new Vector3(-32.7f, 0.03f, 0.55f), new Vector3(0.18f, 0.02f, 0.05f), _strap).transform.rotation = Quaternion.Euler(0f, -20f, 0f);
			strap.gameObject.AddComponent<AssetIdTag>().Id = "items/red-strap";
			Inspect(talk, Ids.E01, VillageLayout.E01);

			// The granary office with the stock ledger (e04).
			var office = new GameObject("Office").transform;
			office.SetParent(props, false);
			Box(office, "Walls", new Vector3(-21f, 1.5f, -8.4f), new Vector3(5f, 3f, 2f), _plaster);
			Prim(PrimitiveType.Cube, "Roof", office, new Vector3(-21f, 3.2f, -8.4f), new Vector3(5.6f, 0.4f, 2.8f), _thatch);
			Box(office, "Table", new Vector3(-21f, 0.45f, -6.5f), new Vector3(2.4f, 0.9f, 0.8f), _wood);
			Prim(PrimitiveType.Cube, "Ledger", office, new Vector3(-21f, 0.95f, -6.5f), new Vector3(0.55f, 0.08f, 0.4f), _ledger);
			Prim(PrimitiveType.Cube, "Pages", office, new Vector3(-21f, 0.99f, -6.5f), new Vector3(0.5f, 0.04f, 0.36f), _paper);
			office.gameObject.AddComponent<AssetIdTag>().Id = "buildings/granary-office";
			Inspect(talk, Ids.E04, VillageLayout.E04);

			// A few sacks and a cart wheel in the yard for company.
			for (var i = 0; i < 4; i++) Prim(PrimitiveType.Sphere, "YardSack", props, new Vector3(-17f + i * 0.45f, 0.25f, 6.8f), new Vector3(0.5f, 0.5f, 0.5f), _sack);
		}

		// ---- zone B: the square ----

		static void BuildSquare(Transform world, Transform props, Transform talk)
		{
			// The elder's hall.
			var hall = new GameObject("ElderHall").transform;
			hall.SetParent(props, false);
			Box(hall, "Walls", new Vector3(-8.9f, 2.5f, 8.1f), new Vector3(7.4f, 5f, 3.8f), _plaster);
			Prim(PrimitiveType.Cube, "Roof", hall, new Vector3(-8.9f, 5.25f, 8.1f), new Vector3(8.4f, 0.5f, 4.8f), _thatch);
			Prim(PrimitiveType.Cube, "Door", hall, new Vector3(-8.9f, 1.3f, 6.17f), new Vector3(1.6f, 2.6f, 0.06f), _woodDark);
			hall.gameObject.AddComponent<AssetIdTag>().Id = "buildings/elder-hall";
			Prim(PrimitiveType.Cube, "Bench", props, new Vector3(-7.5f, 0.25f, 4.4f), new Vector3(2.2f, 0.5f, 0.6f), _wood);

			// The bakery and Ada's counter.
			var bakery = new GameObject("Bakery").transform;
			bakery.SetParent(props, false);
			Box(bakery, "Walls", new Vector3(10.3f, 2.5f, 8.5f), new Vector3(6.6f, 5f, 3.4f), _plasterWarm);
			Prim(PrimitiveType.Cube, "Roof", bakery, new Vector3(10.3f, 5.25f, 8.5f), new Vector3(7.6f, 0.5f, 4.4f), _thatch);
			Box(bakery, "Counter", new Vector3(6.7f, 0.45f, 4.3f), new Vector3(3.4f, 0.9f, 1f), _wood);
			Prim(PrimitiveType.Cube, "Awning", bakery, new Vector3(6.7f, 2.4f, 4.9f), new Vector3(3.8f, 0.1f, 2.4f), _canvas);
			Prim(PrimitiveType.Cylinder, "PostL", bakery, new Vector3(4.9f, 1.2f, 3.8f), new Vector3(0.12f, 1.2f, 0.12f), _woodDark);
			Prim(PrimitiveType.Cylinder, "PostR", bakery, new Vector3(8.5f, 1.2f, 3.8f), new Vector3(0.12f, 1.2f, 0.12f), _woodDark);
			for (var i = 0; i < 5; i++) Prim(PrimitiveType.Sphere, "Loaf", bakery, new Vector3(5.7f + i * 0.45f, 1.0f, 4.3f), new Vector3(0.32f, 0.2f, 0.2f), _plasterWarm);
			bakery.gameObject.AddComponent<AssetIdTag>().Id = "buildings/bakery";

			// The permit stall (e05).
			var stall = new GameObject("PermitStall").transform;
			stall.SetParent(props, false);
			Box(stall, "Table", new Vector3(-2.5f, 0.45f, 6.65f), new Vector3(2.8f, 0.9f, 1.1f), _wood);
			Prim(PrimitiveType.Cube, "Permit", stall, new Vector3(-2.5f, 0.92f, 6.65f), new Vector3(0.6f, 0.02f, 0.42f), _paper);
			Prim(PrimitiveType.Cube, "Canopy", stall, new Vector3(-2.5f, 2.3f, 6.65f), new Vector3(3.4f, 0.1f, 2f), _canvas);
			foreach (var x in new[] { -3.9f, -1.1f }) Prim(PrimitiveType.Cylinder, "Post", stall, new Vector3(x, 1.15f, 6.2f), new Vector3(0.1f, 1.15f, 0.1f), _woodDark);
			stall.gameObject.AddComponent<AssetIdTag>().Id = "props/market-stall";
			Inspect(talk, Ids.E05, VillageLayout.E05);

			// The public cargo tally board (e06).
			var board = new GameObject("TallyBoard").transform;
			board.SetParent(props, false);
			foreach (var x in new[] { -1.4f, 1.4f }) Prim(PrimitiveType.Cylinder, "Post", board, new Vector3(x, 1.0f, -7.5f), new Vector3(0.12f, 1.0f, 0.12f), _woodDark);
			Prim(PrimitiveType.Cube, "Board", board, new Vector3(0f, 1.4f, -7.5f), new Vector3(3.2f, 1.4f, 0.12f), _paper);
			for (var i = 0; i < 4; i++) Prim(PrimitiveType.Cube, "Line", board, new Vector3(0f, 1.8f - i * 0.3f, -7.42f), new Vector3(2.6f, 0.03f, 0.02f), _dark);
			board.gameObject.AddComponent<AssetIdTag>().Id = "props/tally-board";
			Inspect(talk, Ids.E06, VillageLayout.E06);

			// Orest's house and cart.
			var house = new GameObject("OrestHouse").transform;
			house.SetParent(props, false);
			Box(house, "Walls", new Vector3(11.2f, 2.2f, -6f), new Vector3(5.6f, 4.4f, 5.6f), _plaster);
			Prim(PrimitiveType.Cube, "Roof", house, new Vector3(11.2f, 4.6f, -6f), new Vector3(6.6f, 0.5f, 6.6f), _thatch);
			Prim(PrimitiveType.Cube, "Door", house, new Vector3(8.37f, 1.2f, -4.8f), new Vector3(0.06f, 2.4f, 1.3f), _woodDark);
			house.gameObject.AddComponent<AssetIdTag>().Id = "buildings/house";
			var cart = new GameObject("OrestCart").transform;
			cart.SetParent(props, false);
			Box(cart, "Bed", new Vector3(6.3f, 0.75f, -6.7f), new Vector3(2.2f, 0.5f, 1.5f), _wood);
			foreach (var z in new[] { -7.55f, -5.85f })
				foreach (var x in new[] { 5.6f, 7.0f })
					Prim(PrimitiveType.Cylinder, "Wheel", cart, new Vector3(x, 0.45f, z), new Vector3(0.9f, 0.05f, 0.9f), _woodDark).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			Prim(PrimitiveType.Cube, "Tarp", cart, new Vector3(6.3f, 1.15f, -6.7f), new Vector3(2f, 0.4f, 1.3f), _canvas);
			Prim(PrimitiveType.Cube, "Strap", cart, new Vector3(6.3f, 1.37f, -6.7f), new Vector3(0.1f, 0.03f, 1.4f), _strap);
			cart.gameObject.AddComponent<AssetIdTag>().Id = "props/cart";

			// A well and a few crates make the square feel lived in.
			Prim(PrimitiveType.Cylinder, "Well", props, new Vector3(-1f, 0.5f, -1.5f), new Vector3(1.4f, 0.5f, 1.4f), _stone, true);
			Prim(PrimitiveType.Cube, "Crate", props, new Vector3(4.2f, 0.4f, -9f), new Vector3(0.8f, 0.8f, 0.8f), _wood);
		}

		// ---- zone C: the watch post ----

		static void BuildWatchPost(Transform world, Transform props, Transform talk)
		{
			var post = new GameObject("WatchPost").transform;
			post.SetParent(props, false);
			Box(post, "Back", new Vector3(1.1f, 2f, 25.8f), new Vector3(8.6f, 4f, 0.4f), _plaster);
			Box(post, "Left", new Vector3(-3f, 2f, 24.3f), new Vector3(0.4f, 4f, 3.4f), _plaster);
			Box(post, "Right", new Vector3(5.2f, 2f, 24.3f), new Vector3(0.4f, 4f, 3.4f), _plaster);
			Prim(PrimitiveType.Cube, "Floor", post, new Vector3(1.1f, 0.05f, 24.3f), new Vector3(8.2f, 0.1f, 3.4f), _woodDark);
			Prim(PrimitiveType.Cube, "Roof", post, new Vector3(1.1f, 4.2f, 24.5f), new Vector3(9.4f, 0.4f, 4.4f), _thatch);
			Box(post, "Desk", new Vector3(1.1f, 0.45f, 23.4f), new Vector3(3f, 0.9f, 0.9f), _wood);
			Prim(PrimitiveType.Cube, "Log", post, new Vector3(1.7f, 0.93f, 23.4f), new Vector3(0.55f, 0.04f, 0.4f), _paper);
			Prim(PrimitiveType.Cube, "Note", post, new Vector3(0.5f, 0.93f, 23.4f), new Vector3(0.32f, 0.02f, 0.4f), _paper);
			Prim(PrimitiveType.Cube, "Bench", post, new Vector3(3.9f, 0.3f, 25.2f), new Vector3(1.5f, 0.6f, 0.5f), _woodDark);
			Prim(PrimitiveType.Cylinder, "Lamp", post, new Vector3(2.2f, 1.0f, 23.4f), new Vector3(0.2f, 0.1f, 0.2f), _iron);
			post.gameObject.AddComponent<AssetIdTag>().Id = "buildings/watch-post";
			Inspect(talk, Ids.E07, VillageLayout.E07);
			Inspect(talk, Ids.E10, VillageLayout.E10);
			Prim(PrimitiveType.Cylinder, "Beacon", props, new Vector3(-5f, 1.5f, 20f), new Vector3(0.3f, 1.5f, 0.3f), _woodDark);
		}

		// ---- zone D: the ford ----

		static void BuildFord(Transform world, Transform props, Transform talk)
		{
			Prim(PrimitiveType.Cube, "River", props, new Vector3(1f, 0.005f, -31f), new Vector3(34f, 0.04f, 5.5f), _water);
			Prim(PrimitiveType.Cube, "FordStones", props, new Vector3(0.3f, 0.03f, -31f), new Vector3(3.2f, 0.05f, 5.5f), _stone);

			var toll = new GameObject("TollBox").transform;
			toll.SetParent(props, false);
			Box(toll, "Walls", new Vector3(7.2f, 1.2f, -22.3f), new Vector3(2.8f, 2.4f, 2.6f), _woodDark);
			Prim(PrimitiveType.Cube, "Roof", toll, new Vector3(7.2f, 2.55f, -22.3f), new Vector3(3.4f, 0.3f, 3.2f), _thatch);
			Prim(PrimitiveType.Cube, "Docket", toll, new Vector3(5.7f, 1.2f, -22.1f), new Vector3(0.04f, 0.34f, 0.44f), _paper);
			Prim(PrimitiveType.Cube, "Mark", toll, new Vector3(5.68f, 1.2f, -22.1f), new Vector3(0.02f, 0.14f, 0.14f), _dark);
			toll.gameObject.AddComponent<AssetIdTag>().Id = "props/toll-box";
			Inspect(talk, Ids.E09, VillageLayout.E09);

			var tracks = new GameObject("FordTracks").transform;
			tracks.SetParent(props, false);
			foreach (var x in new[] { -2.2f, -0.8f })
				Prim(PrimitiveType.Cube, "Rut", tracks, new Vector3(x, 0.05f, -22.8f), new Vector3(0.32f, 0.05f, 4f), Mat("Rut", new Color(0.18f, 0.13f, 0.09f)));
			Prim(PrimitiveType.Cube, "Fibres", tracks, new Vector3(-1.5f, 0.09f, -23.6f), new Vector3(0.8f, 0.03f, 0.2f), _strap);
			Prim(PrimitiveType.Cube, "Fastening", tracks, new Vector3(-1.5f, 0.12f, -22.5f), new Vector3(0.3f, 0.12f, 0.2f), _iron);
			Prim(PrimitiveType.Cube, "NewStrap", tracks, new Vector3(-1.5f, 0.2f, -22.5f), new Vector3(0.5f, 0.03f, 0.1f), _strap);
			tracks.gameObject.AddComponent<AssetIdTag>().Id = "props/cart-tracks";
			Inspect(talk, Ids.E08, VillageLayout.E08);
		}

		static void BuildTrees(Transform props)
		{
			var random = new System.Random(8);
			var spots = new List<Vector3>();
			for (var z = -34f; z <= 27f; z += 6f) { spots.Add(new Vector3(14.2f, 0f, z)); spots.Add(new Vector3(-12.6f, 0f, z + 2f)); }
			for (var x = -34f; x <= -15f; x += 6f) { spots.Add(new Vector3(x, 0f, 12f)); spots.Add(new Vector3(x, 0f, -12f)); }
			foreach (var spot in spots)
			{
				if (!VillageLayout.Walkable(spot.x, spot.z) && spot.x > -13f) continue;
				Tree(props, spot + new Vector3((float)random.NextDouble() - 0.5f, 0f, (float)random.NextDouble() - 0.5f), 3.8f + (float)random.NextDouble() * 2f);
			}
		}

		static void Tree(Transform parent, Vector3 position, float height)
		{
			var tree = new GameObject("Tree").transform;
			tree.SetParent(parent, false);
			Prim(PrimitiveType.Cylinder, "Trunk", tree, position + Vector3.up * (height * 0.2f), new Vector3(0.4f, height * 0.2f, 0.4f), _woodDark);
			Prim(PrimitiveType.Sphere, "Crown", tree, position + Vector3.up * (height * 0.65f), new Vector3(height * 0.6f, height * 0.6f, height * 0.6f), _leaf);
			tree.gameObject.AddComponent<AssetIdTag>().Id = "props/tree";
		}

		// ---- camera shots ----

		static void BuildShots(Transform shots)
		{
			Shot(shots, "cam_granary_wide", new Vector3(-24.2f, 3f, 0f), new Vector3(23.6f, 12f, 22f), new Vector3(-14.5f, 7f, -9f), 52f, 0, true);
			Shot(shots, "cam_granary_shelf", new Vector3(-26.5f, 3f, -2f), new Vector3(10f, 12f, 5.4f), new Vector3(-20f, 3.2f, -5.6f), 48f, 5, false);
			Shot(shots, "cam_granary_gate", new Vector3(-31f, 3f, 0f), new Vector3(8f, 12f, 8f), new Vector3(-30f, 3.8f, -8.4f), 50f, 6, false);
			Shot(shots, "cam_square_wide", new Vector3(1.2f, 3f, 1.5f), new Vector3(29.6f, 12f, 23.4f), new Vector3(2f, 9.5f, -15f), 55f, 0, true);
			Shot(shots, "cam_square_stall", new Vector3(4.5f, 3f, 5.8f), new Vector3(19f, 12f, 9.5f), new Vector3(3f, 3.2f, 0.4f), 50f, 5, false);
			Shot(shots, "cam_watch_post", new Vector3(1.2f, 3f, 21.4f), new Vector3(29.6f, 12f, 17.2f), new Vector3(-9f, 4.4f, 12.4f), 52f, 0, true);
			Shot(shots, "cam_ford", new Vector3(1.2f, 3f, -23.6f), new Vector3(29.6f, 12f, 27f), new Vector3(-10f, 5.2f, -13f), 52f, 0, true);
		}

		// ---- people ----

		static Npc BuildNpc(Transform parent, Transform talk, string id, Vector2 at, float yaw, Color tunic, Color trousers, Color hair, bool helmet)
		{
			var root = new GameObject(id);
			root.transform.SetParent(parent, false);
			root.transform.SetPositionAndRotation(new Vector3(at.x, 0.05f, at.y), Quaternion.Euler(0f, yaw, 0f));
			var capsule = root.AddComponent<CapsuleCollider>();
			capsule.height = 1.8f;
			capsule.radius = 0.35f;
			capsule.center = new Vector3(0f, 0.9f, 0f);
			BuildFigure(root.transform, id, tunic, trousers, hair, helmet);
			var npc = root.AddComponent<Npc>();
			npc.Id = id;
			var item = root.AddComponent<Interactable>();
			item.Id = id;
			item.Kind = InteractKind.Talk;
			item.Range = 2.4f;
			item.FocusOffset = new Vector3(0f, 1.2f, 0f);
			root.AddComponent<AssetIdTag>().Id = "characters/" + id.Replace('_', '-');
			var tag = Text(root.transform, "NameTag", Loc.T("p." + id), root.transform.position + new Vector3(0f, 2.25f, 0f), Quaternion.identity, 0.018f, new Color(1f, 0.95f, 0.8f));
			npc.Tag = tag.transform;
			return npc;
		}

		static Player BuildPlayer()
		{
			var root = new GameObject("Player");
			root.transform.position = new Vector3(VillageLayout.PlayerStart.x, 0.05f, VillageLayout.PlayerStart.y);
			var body = root.AddComponent<CharacterController>();
			body.height = 1.8f;
			body.radius = 0.3f;
			body.center = new Vector3(0f, 0.9f, 0f);
			body.stepOffset = 0.25f;
			var player = root.AddComponent<Player>();
			BuildFigure(root.transform, "Investigator", new Color(0.3f, 0.36f, 0.45f), new Color(0.22f, 0.2f, 0.2f), new Color(0.2f, 0.14f, 0.1f), false);
			root.AddComponent<AssetIdTag>().Id = "characters/investigator";
			return player;
		}

		static Figure BuildFigure(Transform parent, string name, Color tunic, Color trousers, Color hair, bool helmet)
		{
			var rig = new GameObject(name + "Visual").transform;
			rig.SetParent(parent, false);
			var figure = rig.gameObject.AddComponent<Figure>();
			var tunicMat = Mat(name + "Tunic", tunic);
			var trouserMat = Mat(name + "Trousers", trousers);
			var body = new GameObject("Body").transform;
			body.SetParent(rig, false);
			figure.Body = body;
			LocalPrim(PrimitiveType.Cube, "Torso", body, new Vector3(0f, 1.15f, 0f), new Vector3(0.55f, 0.7f, 0.3f), tunicMat);
			LocalPrim(PrimitiveType.Sphere, "Head", body, new Vector3(0f, 1.7f, 0f), new Vector3(0.3f, 0.32f, 0.3f), _skin);
			if (helmet)
			{
				LocalPrim(PrimitiveType.Cylinder, "Helmet", body, new Vector3(0f, 1.82f, 0f), new Vector3(0.34f, 0.1f, 0.34f), _iron);
				LocalPrim(PrimitiveType.Cylinder, "Spear", body, new Vector3(0.45f, 1.2f, 0.1f), new Vector3(0.04f, 1.0f, 0.04f), _woodDark);
				LocalPrim(PrimitiveType.Cube, "SpearHead", body, new Vector3(0.45f, 2.25f, 0.1f), new Vector3(0.08f, 0.22f, 0.03f), _iron);
			}
			else
			{
				LocalPrim(PrimitiveType.Cube, "Hair", body, new Vector3(0f, 1.86f, -0.02f), new Vector3(0.3f, 0.1f, 0.3f), Mat(name + "Hair", hair));
			}
			figure.LegL = Limb(rig, "LegL", new Vector3(-0.14f, 0.8f, 0f), new Vector3(0.18f, 0.8f, 0.2f), trouserMat);
			figure.LegR = Limb(rig, "LegR", new Vector3(0.14f, 0.8f, 0f), new Vector3(0.18f, 0.8f, 0.2f), trouserMat);
			figure.ArmL = Limb(body, "ArmL", new Vector3(-0.38f, 1.45f, 0f), new Vector3(0.14f, 0.65f, 0.16f), tunicMat);
			figure.ArmR = Limb(body, "ArmR", new Vector3(0.38f, 1.45f, 0f), new Vector3(0.14f, 0.65f, 0.16f), tunicMat);
			return figure;
		}

		static Transform Limb(Transform parent, string name, Vector3 pivot, Vector3 size, Material material)
		{
			var joint = new GameObject(name).transform;
			joint.SetParent(parent, false);
			joint.localPosition = pivot;
			var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
			mesh.name = "Mesh";
			Object.DestroyImmediate(mesh.GetComponent<Collider>());
			mesh.transform.SetParent(joint, false);
			mesh.transform.localPosition = new Vector3(0f, -size.y * 0.5f, 0f);
			mesh.transform.localScale = size;
			mesh.GetComponent<Renderer>().sharedMaterial = material;
			return joint;
		}

		static void LocalPrim(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
		{
			var go = GameObject.CreatePrimitive(type);
			go.name = name;
			Object.DestroyImmediate(go.GetComponent<Collider>());
			go.transform.SetParent(parent, false);
			go.transform.localPosition = localPosition;
			go.transform.localScale = scale;
			go.GetComponent<Renderer>().sharedMaterial = material;
		}

		static FixedViewCamera BuildCamera(Player player)
		{
			var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
			cameraObject.transform.position = new Vector3(2f, 9.5f, -15f);
			var camera = cameraObject.AddComponent<Camera>();
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.backgroundColor = new Color(0.66f, 0.68f, 0.7f);
			camera.fieldOfView = 55f;
			camera.nearClipPlane = 0.1f;
			camera.farClipPlane = 140f;
			cameraObject.AddComponent<AudioListener>();
			var rig = cameraObject.AddComponent<FixedViewCamera>();
			rig.Subject = player.transform;
			rig.Hysteresis = DetectiveConfig.CameraHysteresis;
			rig.BlendSeconds = DetectiveConfig.CameraBlend;
			player.Rig = rig;
			return rig;
		}

		static void BuildSun()
		{
			var sun = new GameObject("Overcast");
			sun.transform.rotation = Quaternion.Euler(52f, -30f, 0f);
			var light = sun.AddComponent<Light>();
			light.type = LightType.Directional;
			light.intensity = 1.05f;
			light.color = new Color(0.9f, 0.92f, 1f);
			light.shadows = LightShadows.Soft;
			light.shadowStrength = 0.6f;
		}

		// ---- small helpers ----

		static CameraShotVolume Shot(Transform parent, string id, Vector3 centre, Vector3 size, Vector3 anchor, float fov, int priority, bool cut)
		{
			var volume = new GameObject(id).transform;
			volume.SetParent(parent, false);
			volume.position = centre;
			var shot = volume.gameObject.AddComponent<CameraShotVolume>();
			shot.ShotId = id;
			shot.Size = size;
			shot.Fov = fov;
			shot.Priority = priority;
			shot.Cut = cut;
			shot.Anchor = Empty(volume, id + "_anchor", anchor);
			return shot;
		}

		static Transform Empty(Transform parent, string name, Vector3 position)
		{
			var go = new GameObject(name).transform;
			go.SetParent(parent, false);
			go.position = position;
			return go;
		}

		/// <summary>A clue: an empty at the place the player stands to inspect it, with a forgiving range.</summary>
		static void Inspect(Transform parent, string id, Vector2 at)
		{
			var go = new GameObject(id);
			go.transform.SetParent(parent, false);
			go.transform.position = new Vector3(at.x, 0f, at.y);
			var item = go.AddComponent<Interactable>();
			item.Id = id;
			item.Kind = InteractKind.Inspect;
			item.Range = 2.4f;
			item.FocusOffset = new Vector3(0f, 1.1f, 0f);
		}

		/// <summary>A line of text at a world position (see <see cref="GrayboxBuilder.Text"/>).</summary>
		static TextMesh Text(Transform parent, string name, string text, Vector3 position, Quaternion rotation, float size, Color colour)
		{
			var mesh = GrayboxBuilder.Text(parent, name, text, Vector3.zero, Quaternion.identity, size, colour, MaterialDir);
			mesh.transform.SetPositionAndRotation(position, rotation);
			return mesh;
		}

		static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material) => GrayboxBuilder.Box(parent, name, position, scale, material);

		static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool keepCollider = false) => GrayboxBuilder.Prim(type, name, parent, position, scale, material, keepCollider);

		static Material Mat(string name, Color colour) => GrayboxBuilder.Mat(name, colour, MaterialDir);
	}
}

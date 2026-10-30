using UnityEngine;

namespace Temnorid.Detective
{
	/// <summary>
	/// Where things are in Greyford, as plain numbers (x east, z north, metres). The scene builder, the game and the tests all read
	/// this, so a clue and its footprint cannot drift apart.
	/// <code>
	///   z 28   [WATCH POST x-13.5..15, z 13..28]
	///   z 12        |
	///   z  0  [GRANARY YARD x-35..-13.5] -- [SQUARE x-13.5..15, z-10..12]
	///   z-10                                   |
	///   z-36                              [FORD x-13.5..15, z-36..-10]
	/// </code>
	/// </summary>
	public static class VillageLayout
	{
		public const float WestEdge = -13.5f;
		public const float EastEdge = 15f;
		public const float NorthEdge = 28.5f;
		public const float SouthEdge = -35.5f;
		public const float YardWest = -35f;
		public const float YardHalfDepth = 9.5f;

		public static readonly Vector2 PlayerStart = new Vector2(-8f, -2f);

		// People.
		public static readonly Vector2 Elder = new Vector2(-5.5f, 3f);
		public static readonly Vector2 Mira = new Vector2(-24f, -4.5f);
		public static readonly Vector2 Orest = new Vector2(7.8f, -3.3f);
		public static readonly Vector2 Ada = new Vector2(6.5f, 5.3f);
		public static readonly Vector2 Taras = new Vector2(5f, 19.8f);

		// Where the player stands to use each clue (the objects themselves are just behind these points).
		public static readonly Vector2 E01 = new Vector2(-32.4f, -0.4f);
		public static readonly Vector2 E02 = new Vector2(-27f, -1.4f);
		public static readonly Vector2 E03 = new Vector2(-30.1f, -0.7f);
		public static readonly Vector2 E04 = new Vector2(-21f, -5.4f);
		public static readonly Vector2 E05 = new Vector2(-2.5f, 5.3f);
		public static readonly Vector2 E06 = new Vector2(0f, -6.2f);
		public static readonly Vector2 E07 = new Vector2(1f, 21.8f);
		public static readonly Vector2 E10 = new Vector2(-0.8f, 21.8f);
		public static readonly Vector2 E08 = new Vector2(-1.5f, -21f);
		public static readonly Vector2 E09 = new Vector2(5.4f, -22f);

		/// <summary>Footprints of everything solid, with a little margin: nothing is ever placed or restored inside one.</summary>
		public static readonly Rect[] Obstacles =
		{
			new Rect(-34f, 0.5f, 11f, 6.2f),     // the granary
			new Rect(-23.6f, -9.6f, 5f, 2.4f),   // the granary office
			new Rect(-22.4f, -7f, 2.8f, 1f),     // the ledger table
			new Rect(-28f, -0.6f, 3f, 1f),       // the sack rack
			new Rect(-12.6f, 6.2f, 7.4f, 3.8f),  // the elder's hall
			new Rect(7f, 6.8f, 6.6f, 3.4f),      // the bakery
			new Rect(5f, 3.8f, 3.4f, 1f),        // the bakery counter
			new Rect(-3.9f, 6.1f, 2.8f, 1.1f),   // the permit stall table
			new Rect(8.4f, -8.8f, 5.6f, 5.6f),   // Orest's house
			new Rect(5f, -7.9f, 2.6f, 2.4f),     // Orest's cart
			new Rect(-3.2f, 22.6f, 8.6f, 3.4f),  // the watch post hut
			new Rect(5.8f, -23.6f, 2.8f, 2.6f),  // the toll box
			new Rect(-1.9f, -2.4f, 1.8f, 1.8f),  // the well
		};

		public static string ZoneAt(float x, float z)
		{
			if (x < WestEdge && Mathf.Abs(z) <= YardHalfDepth + 0.5f) return Ids.Granary;
			if (z > 13f) return Ids.WatchPost;
			if (z < -10.2f) return Ids.Ford;
			return Ids.Square;
		}

		public static bool Walkable(float x, float z)
		{
			var inYard = x >= YardWest && x <= WestEdge && Mathf.Abs(z) <= YardHalfDepth;
			var inCorridor = x >= WestEdge && x <= EastEdge && z >= SouthEdge && z <= NorthEdge;
			if (!inYard && !inCorridor) return false;
			foreach (var obstacle in Obstacles) if (obstacle.Contains(new Vector2(x, z))) return false;
			return true;
		}

		public static Vector2 SpawnFor(string zone)
		{
			switch (zone)
			{
				case Ids.Granary: return new Vector2(-20f, 0f);
				case Ids.WatchPost: return new Vector2(0f, 16f);
				case Ids.Ford: return new Vector2(0f, -17f);
				default: return PlayerStart;
			}
		}
	}
}

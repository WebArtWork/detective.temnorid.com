using ITKamianets.Engine.Synth;
using UnityEngine;

namespace Temnorid.Detective
{
	public enum Sfx
	{
		Click,
		/// <summary>A clue recorded: a short page turn.</summary>
		Page,
		Footstep,
		/// <summary>The report is committed.</summary>
		Bell,
		/// <summary>A new discrepancy noted.</summary>
		Note,
	}

	/// <summary>
	/// The sounds, made from simple waves and noise when the game starts, so no audio files are needed: clicks, a door,
	/// footsteps, an alert and a the river and a quiet village murmur that loops under everything.
	/// </summary>
	public sealed class DetectiveAudio : MonoBehaviour
	{
		SoundBoard _boardCache;

		/// <summary>The game's Awake may set the volumes before this component has woken up, so the board is made on first use.</summary>
		SoundBoard Board => _boardCache != null ? _boardCache : (_boardCache = gameObject.AddComponent<SoundBoard>());

		public float MasterVolume => Board.MasterVolume;
		public float SfxVolume => Board.SfxVolume;

		void Awake()
		{
			var kinds = System.Enum.GetValues(typeof(Sfx));
			var clips = new AudioClip[kinds.Length];
			foreach (Sfx kind in kinds) clips[(int)kind] = Make(kind);
			Board.Init(Ambience(), clips, 0.3f);
		}

		public void SetVolumes(float master, float sfx) => Board.SetVolumes(master, sfx);

		public void Play(Sfx kind, float volume = 1f) => Board.Play((int)kind, volume);

		static AudioClip Make(Sfx kind)
		{
			var random = new System.Random((int)kind * 131 + 7);
			float Noise() => Tone.Noise(random);
			switch (kind)
			{
				case Sfx.Click:
					return Tone.Build("click", 0.05f, t => Mathf.Sin(t * 2f * Mathf.PI * 1000f) * Tone.Envelope(t, 0.05f) * 0.45f);
				case Sfx.Page:
					return Tone.Build("page", 0.22f, t => Noise() * Tone.Envelope(t, 0.22f, 0.02f) * (0.5f - 0.3f * t / 0.22f));
				case Sfx.Footstep:
					return Tone.Build("step", 0.12f, t => (Noise() * 0.35f + Mathf.Sin(t * 2f * Mathf.PI * 90f) * 0.6f) * Tone.Envelope(t, 0.12f, 0.002f) * 0.55f);
				case Sfx.Bell:
					return Tone.Build("bell", 1.6f, t => (Mathf.Sin(t * 2f * Mathf.PI * 330f) + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 660f)) * Mathf.Exp(-3f * t) * 0.4f);
				default:
					return Tone.Build("note", 0.3f, t => Mathf.Sin(t * 2f * Mathf.PI * (t < 0.12f ? 600f : 800f)) * Tone.Envelope(t, 0.3f, 0.01f) * 0.4f);
			}
		}

		static AudioClip Ambience()
		{
			var random = new System.Random(5);
			var smooth = 0f;
			var low = 0f;
			const float length = 6f;
			return Tone.Build("ambience", length, t =>
			{
				var white = (float)(random.NextDouble() * 2.0 - 1.0);
				smooth += (white - smooth) * 0.06f; // the river
				low += (white - low) * 0.004f; // distant murmur
				var knock = t % 2.9f < 0.03f ? Mathf.Sin(t * 2f * Mathf.PI * 220f) * (1f - (t % 2.9f) / 0.03f) * 0.25f : 0f;
				var edge = Mathf.Clamp01(Mathf.Min(t, length - t) / 0.2f);
				return (smooth * 0.9f + low * 4f + knock) * edge;
			});
		}
	}
}

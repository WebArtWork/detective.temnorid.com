using ITKamianets.Engine.Save;

namespace Temnorid.Detective
{
	/// <summary>Reads and writes the <see cref="InvestigationSave"/> in the single save slot. The write is atomic (the save package swaps a temporary file in).</summary>
	public sealed class DetectiveSaveStore
	{
		const string Slot = "default";
		const int PayloadVersion = 1;

		readonly SaveStore _store;

		public DetectiveSaveStore(string rootDirectory = null)
		{
			_store = new SaveStore("detective", rootDirectory);
		}

		public string Path => _store.PathFor(Slot);

		/// <summary>The saved game, or null. <paramref name="status"/> says why it is null: no save yet, a damaged file (kept aside, never deleted) or a newer version.</summary>
		public InvestigationSave Load(out SaveStatus status)
		{
			var result = _store.Load<InvestigationSave>(Slot, PayloadVersion);
			status = result.Status;
			return result.Ok ? result.Value : null;
		}

		public SaveStatus Save(InvestigationSave save) => _store.Save(Slot, save, PayloadVersion);

		public void Delete() => _store.Delete(Slot);

		public DetectiveSettings LoadSettings()
		{
			var result = _store.Load<DetectiveSettings>("settings", PayloadVersion);
			return result.Ok && result.Value != null ? result.Value : new DetectiveSettings();
		}

		public void SaveSettings(DetectiveSettings settings) => _store.Save("settings", settings, PayloadVersion);
	}
}

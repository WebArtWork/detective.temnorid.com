using UnityEngine;

namespace Temnorid.Detective
{
	/// <summary>A villager: a stable id, a name tag, and a slow turn toward the investigator when close. All it says is in <see cref="Investigation"/>.</summary>
	public sealed class Npc : MonoBehaviour
	{
		public string Id;
		public Transform Target;
		/// <summary>The name tag over the head: it always faces the camera.</summary>
		public Transform Tag;
		public float TurnRange = 6f;
		float _home;

		void Start() => _home = transform.eulerAngles.y;

		void Update()
		{
			if (Target == null) return;
			var to = Target.position - transform.position;
			to.y = 0f;
			var want = _home;
			if (to.sqrMagnitude < TurnRange * TurnRange && to.sqrMagnitude > 0.01f) want = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
			if (Tag != null && Camera.main != null) Tag.rotation = Camera.main.transform.rotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, want, 0f), 1f - Mathf.Exp(-4f * Time.deltaTime));
		}
	}
}

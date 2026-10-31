using ITKamianets.Engine.Net;
using UnityEngine;

namespace Temnorid.Detective
{
	/// <summary>The other investigator as seen on this machine: a copy of the investigator's figure that follows the poses it is sent, swinging its limbs while it moves, with a name tag that faces the camera.</summary>
	public sealed class RemoteInvestigator : MonoBehaviour
	{
		public ulong PlayerId;
		public RemoteBody Body;
		public Figure Figure;
		public Transform Tag;

		/// <summary>Builds a remote investigator from the local one's figure. No rules, no collider: it is only shown.</summary>
		public static RemoteInvestigator Create(Player template, TextMesh tagTemplate, ulong id, string name, Color tint)
		{
			// Built under an inactive holder so the copied Player and CharacterController never wake up.
			var holder = new GameObject("RemoteHolder");
			holder.SetActive(false);
			var copy = Instantiate(template.gameObject, holder.transform);
			copy.name = "Investigator " + name;
			var player = copy.GetComponent<Player>();
			if (player != null) DestroyImmediate(player);
			var controller = copy.GetComponent<CharacterController>();
			if (controller != null) DestroyImmediate(controller);
			copy.transform.SetParent(null, false);
			Destroy(holder);

			var remote = copy.AddComponent<RemoteInvestigator>();
			remote.PlayerId = id;
			remote.Body = copy.AddComponent<RemoteBody>();
			remote.Figure = copy.GetComponentInChildren<Figure>();
			foreach (var renderer in copy.GetComponentsInChildren<Renderer>(true))
			{
				if (renderer.name == "Torso") renderer.material.color = tint;
			}
			if (tagTemplate != null)
			{
				var tag = Instantiate(tagTemplate.gameObject, copy.transform);
				tag.transform.localPosition = new Vector3(0f, 2.25f, 0f);
				tag.GetComponent<TextMesh>().text = name;
				remote.Tag = tag.transform;
			}
			copy.SetActive(true);
			return remote;
		}

		public void SetName(string name)
		{
			if (Tag == null) return;
			var mesh = Tag.GetComponent<TextMesh>();
			if (mesh != null && mesh.text != name) mesh.text = name;
		}

		void Update()
		{
			if (Figure != null && Body != null) Figure.Speed = Body.Speed;
			var camera = Camera.main;
			if (Tag != null && camera != null) Tag.rotation = camera.transform.rotation;
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
	public class DiffItem {

		public static readonly HashSet<GameObject> items = new HashSet<GameObject> ();
		public static readonly HashSet<GameObject> itemCopies = new HashSet<GameObject> ();

		public static void RegisterItems (GameObject container) {

			if (!container) return;

			foreach (var mr in container.GetComponentsInChildren<MeshRenderer> ()) {

				if (!DiffHit.IsInDiffLayer (mr.gameObject)) continue;
				items.Add (mr.gameObject);

				// disable shadow and hide
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				mr.enabled = false;
			}
		}

		public static void UnregisterItems () {

			items.Clear ();
			itemCopies.Clear ();
		}
	}
}
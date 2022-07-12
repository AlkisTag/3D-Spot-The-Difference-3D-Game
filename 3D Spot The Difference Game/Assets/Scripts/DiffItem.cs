using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
	public class DiffItem : MonoBehaviour {

		public static readonly HashSet<DiffItem> items = new HashSet<DiffItem> ();

		private void Awake () {
			items.Add (this);
		}

		private void OnDestroy () {
			items.Remove (this);
		}
	}
}
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
	public class Billboard : MonoBehaviour {

		private static readonly HashSet<Billboard> billboards = new HashSet<Billboard> ();

		private void OnEnable () {
			billboards.Add (this);
		}

		private void OnDisable () {
			billboards.Remove (this);
		}

		public void UpdateDirection (Vector3 camFwd) {

			transform.LookAt (transform.position + camFwd);
		}

		public static void UpdateBillboards (Vector3 camFwd) {

			foreach (var b in billboards) {
				b.UpdateDirection (camFwd);
			}
		}
	}
}
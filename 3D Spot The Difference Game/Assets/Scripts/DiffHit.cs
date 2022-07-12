using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts {

	public class DiffHit : MonoBehaviour {

		private static DiffHit me;

		private readonly HashSet<GameObject> foundDiffs = new HashSet<GameObject> ();
		public GameObject markPrefab;

		private Camera rayCam;
		private const float maxDist = 1000f;
		private const int layerMask = 1 << 3;

		private Vector3 otherCameraOffset;

		private void Awake () {

			me = this;
			rayCam = GetComponent<Camera> ();
		}

		private void Start () {

			var otherCam = CamControl.GetOtherCamera (rayCam);
			if (!otherCam) return;

			otherCameraOffset = otherCam.transform.root.position - rayCam.transform.root.position;
		}

		public static void RegisterTap (Vector2 screenPos) {

			if (!me) return;
			screenPos.y %= Screen.height * .5f;
			screenPos.y += Screen.height * me.rayCam.rect.y;
			var ray = me.rayCam.ScreenPointToRay (screenPos);

			if (!Physics.Raycast (ray, out var hit, maxDist, layerMask)) {
				return;
			}

			var go = hit.collider.gameObject;
			if (me.foundDiffs.Contains (go)) {
				return;
			}
			me.foundDiffs.Add (go);

			me.CreateMark (go.transform.position, go.transform.localScale);
			if (me.otherCameraOffset != Vector3.zero) {
				me.CreateMark (go.transform.position + me.otherCameraOffset, go.transform.localScale);
			}
		}

		private void CreateMark (Vector3 pos, Vector3 scale) {

			var markGo = Instantiate (markPrefab, pos, Quaternion.identity);
			markGo.transform.localScale = scale;
			var mark = markGo.GetComponent<Billboard> ();
			if (mark) {
				mark.UpdateDirection (rayCam.transform.forward);
			}
		}
	}

}
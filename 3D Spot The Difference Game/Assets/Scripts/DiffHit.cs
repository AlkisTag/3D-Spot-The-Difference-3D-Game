using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {

	public class DiffHit : MonoBehaviour {

		private static DiffHit me;

		private readonly HashSet<GameObject> foundDiffs = new HashSet<GameObject> ();
		public GameObject markPrefab;
		public WrongMark wrongMarkPrefab;
		private const float maxMarkScale = 3f;

		private Camera[] rayCams;
		private const float maxDist = 1000f;
		public LayerMask layerMask;
		public const int diffLayer = 3;

		private Vector3 camerasOffset;

		public Text foundText;
		public Text totalText;

		public GameObject levelCompletedMenu;

		private void Awake () {

			me = this;
		}

		private void Start () {

			totalText.text = DiffItem.items.Count.ToString ();

			rayCams = CamControl.GetCameras ();
			if (rayCams == null) return;

			camerasOffset = rayCams[1].transform.root.position - rayCams[0].transform.root.position;
		}

		public static void RegisterTap (Vector2 screenPos) {

			if (!me) return;

			var initPos = screenPos;
			foreach (var cam in me.rayCams) {

				// get screen pos, adjusted into this camera's viewport
				screenPos = initPos;
				screenPos.y %= Screen.height * .5f;
				screenPos.y += Screen.height * cam.rect.y;
				var ray = cam.ScreenPointToRay (screenPos);

				// if nothing hit, skip to next camera (if all checked, will lead to life loss below)
				if (!Physics.Raycast (ray, out var hit, maxDist, me.layerMask) || hit.collider.gameObject.layer != diffLayer) {
					continue;
				}

				// if tapped an already found difference, exit (no life loss)
				var go = hit.collider.gameObject;
				if (me.foundDiffs.Contains (go)) {
					return;
				}

				// mark found difference
				me.foundDiffs.Add (go);
				me.foundText.text = me.foundDiffs.Count.ToString ();

				// show mark on both objects
				var uniformScale = GetUniformScaleForMark (go.transform.localScale);
				var markGo = me.CreateMark (go.transform.position, uniformScale);

				if (me.camerasOffset != Vector3.zero) {

					MoveToCameraLayer (markGo, cam == me.rayCams[0] ? 0 : 1);
					var posDelta = cam == me.rayCams[0] ? me.camerasOffset : -me.camerasOffset;
					markGo = me.CreateMark (go.transform.position + posDelta, uniformScale);
					MoveToCameraLayer (markGo, cam == me.rayCams[0] ? 1 : 0);
				}

				// check if all differences found
				if (IsLevelCompleted () && me.levelCompletedMenu) {
					me.levelCompletedMenu.SetActive (true);
				}

				// exit (don't lose life)
				return;
			}

			// if we reached this point, tap was a miss, so lose life
			me.CreateWrongMark (initPos);
			Hearts.SetHearts (-1, true);
		}

		private GameObject CreateMark (Vector3 pos, Vector3 scale) {

			var markGo = Instantiate (markPrefab, pos, Quaternion.identity);
			markGo.transform.localScale = scale;
			var mark = markGo.GetComponent<Billboard> ();
			if (mark) {
				mark.UpdateDirection (me.rayCams[0].transform.forward);
			}

			return markGo;
		}

		private void CreateWrongMark (Vector2 pos) {

			var markGo = Instantiate (wrongMarkPrefab.gameObject, wrongMarkPrefab.transform.parent);
			var rectTr = markGo.GetComponent<RectTransform> ();
			rectTr.anchoredPosition = pos;
			markGo.SetActive (true);
		}

		public static bool IsLevelCompleted () => me && me.foundDiffs.Count >= DiffItem.items.Count && DiffItem.items.Count > 0;

		private static Vector3 GetUniformScaleForMark (Vector3 scale) {

			float max = Mathf.Max (Mathf.Abs (scale.x), Mathf.Abs (scale.y), Mathf.Abs (scale.z));
			max = Mathf.Min (max, maxMarkScale);
			return Vector3.one * max;
		}

		public static void MoveToCameraLayer (GameObject go, int cameraInd) {

			if (!go || cameraInd < 0 || cameraInd >= CamControl.camLayers.Length) return;

			go.layer = CamControl.camLayers[cameraInd];
			foreach (Transform child in go.transform) {
				// do not change layer if this is in Difference layer (used to mark diff colliders)
				if (child.gameObject.layer == diffLayer) continue;
				child.gameObject.layer = CamControl.camLayers[cameraInd];
			}
		}
	}
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {

	public class DiffHit : MonoBehaviour {

		private static DiffHit me;

		private readonly HashSet<GameObject> foundDiffs = new HashSet<GameObject> ();
		public WrongMark wrongMarkPrefab;

		private Camera[] rayCams;
		private const float maxDist = 1000f;
		public LayerMask[] layerMasks = new LayerMask[2];
		public static readonly int[] diffLayers = new int[] { 8, 9 };

		private Vector3 camerasOffset;

		public Text foundText;
		public Text totalText;

		public GameObject levelCompletedMenu;

		public Canvas referenceCanvas;

		public ParticleSystem sparklesPrefabSphere;
		public ParticleSystem sparklesPrefabBox;

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
			for (int i = 0; i < 2; i++) {
				var cam = me.rayCams[i];

				// get screen pos, adjusted into this camera's viewport
				screenPos = initPos;
				screenPos.y %= Screen.height * .5f;
				screenPos.y += Screen.height * cam.rect.y;
				var ray = cam.ScreenPointToRay (screenPos);

				// if nothing hit, skip to next camera (if all checked, will lead to life loss below)
				if (!Physics.Raycast (ray, out var hit, maxDist, me.layerMasks[i]) || hit.collider.gameObject.layer != diffLayers[i]) {
					continue;
				}

				// if tapped an already found difference, exit (no life loss)
				var go = hit.collider.gameObject;
				if (me.foundDiffs.Contains (go) || DiffItem.itemCopies.Contains (go)) {
					return;
				}

				// mark found difference
				me.foundDiffs.Add (go);
				me.foundText.text = me.foundDiffs.Count.ToString ();

				// show mark on both objects
				me.MarkFoundDiff (go);
				MarkFader.ShowMarks ();
				ScreenFlash.FlashGood ();

				// check if all differences found
				if (IsLevelCompleted () && me.levelCompletedMenu) {
					me.levelCompletedMenu.SetActive (true);
				}

				// exit (don't lose life)
				return;
			}

			// if we reached this point, tap was a miss, so lose life
			me.CreateWrongMark (initPos);
			Hearts.LoseHeartWithAnim (initPos);
			ScreenFlash.FlashBad ();
		}

		private void MarkFoundDiff (GameObject go) {

			if (!go) return;

			var mr = go.GetComponent<MeshRenderer> ();
			if (!mr || mr.enabled) return;

			mr.enabled = true;
			CreateSparklesOn (go);

			if (camerasOffset != Vector3.zero) {

				var isFirstLayer = go.layer == diffLayers[0];
				var diffLayerInd = isFirstLayer ? 1 : 0;
				var posDelta = isFirstLayer ? camerasOffset : -camerasOffset;

				var copy = Instantiate (go, go.transform.position + posDelta, go.transform.rotation);
				DiffItem.itemCopies.Add (copy);

				copy.layer = diffLayers[diffLayerInd];
				CreateSparklesOn (copy);
			}
		}

		private void CreateWrongMark (Vector2 pos) {

			var markGo = Instantiate (wrongMarkPrefab.gameObject, wrongMarkPrefab.transform.parent);
			var rectTr = markGo.GetComponent<RectTransform> ();
			rectTr.anchoredPosition = pos;

			if (referenceCanvas) {
				rectTr.localScale *= referenceCanvas.transform.localScale.x;
			}

			markGo.SetActive (true);
		}

		private void CreateSparklesOn (GameObject item) {

			if (!item) return;

			GameObject go = null;
			ParticleSystem prt;

			// emit with box shape if item has BoxCollider
			var bc = item.GetComponentInChildren<BoxCollider> ();
			if (bc) {
				go = Instantiate (sparklesPrefabBox.gameObject, item.transform.position, item.transform.rotation);

				prt = go.GetComponent<ParticleSystem> ();
				var shape = prt.shape;
				shape.position = bc.center;
				shape.scale = bc.size - Vector3.one * DiffItem.extendBox;
			}
			else {
				// emit with sphere shape if item has SphereCollider
				var sc = item.GetComponentInChildren<SphereCollider> ();
				if (sc) {
					go = Instantiate (sparklesPrefabSphere.gameObject, item.transform.position, item.transform.rotation);

					prt = go.GetComponent<ParticleSystem> ();
					var shape = prt.shape;
					shape.position = sc.center;
					shape.radius = sc.radius - DiffItem.extendSphere;
				}
			}

			if (!go) return;

			go.transform.localScale = item.transform.localScale;
			go.layer = CamControl.camLayers[item.layer == diffLayers[0] ? 0 : 1];
		}

		public static bool IsLevelCompleted () => me && me.foundDiffs.Count >= DiffItem.items.Count && DiffItem.items.Count > 0;

		public static void MoveToCameraLayer (GameObject go, int cameraInd) {

			if (!go || cameraInd < 0 || cameraInd >= CamControl.camLayers.Length) return;

			go.layer = CamControl.camLayers[cameraInd];
			foreach (Transform child in go.transform) {
				// do not change layer if this is in Difference layers (used to mark diff colliders)
				if (IsInDiffLayer (child.gameObject)) continue;

				child.gameObject.layer = CamControl.camLayers[cameraInd];
			}
		}

		public static bool IsInDiffLayer (GameObject go) {

			var lr = go.layer;
			return lr == diffLayers[0] || lr == diffLayers[1];
		}
	}
}
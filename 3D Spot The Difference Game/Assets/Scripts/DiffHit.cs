using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {

	public class DiffHit : MonoBehaviour {

		private static DiffHit me;

		private readonly HashSet<GameObject> foundDiffs = new HashSet<GameObject> ();
		public GameObject markPrefab;
		public WrongMark wrongMarkPrefab;

		private Camera rayCam;
		private const float maxDist = 1000f;
		public LayerMask layerMask;
		private const int diffLayer = 3;

		private Vector3 otherCameraOffset;

		public Text foundText;
		public Text totalText;
		
		public GameObject levelCompletedMenu;

		private void Awake () {

			me = this;
			rayCam = GetComponent<Camera> ();
		}

		private void Start () {

			totalText.text = DiffItem.items.Count.ToString ();

			var otherCam = CamControl.GetOtherCamera (rayCam);
			if (!otherCam) return;

			otherCameraOffset = otherCam.transform.root.position - rayCam.transform.root.position;
		}

		public static void RegisterTap (Vector2 screenPos) {

			if (!me) return;
			var initPos = screenPos;
			screenPos.y %= Screen.height * .5f;
			screenPos.y += Screen.height * me.rayCam.rect.y;
			var ray = me.rayCam.ScreenPointToRay (screenPos);

			if (!Physics.Raycast (ray, out var hit, maxDist, me.layerMask) || hit.collider.gameObject.layer != diffLayer) {
				me.CreateWrongMark (initPos);
				Hearts.SetHearts (-1, true);
				return;
			}

			var go = hit.collider.gameObject;
			if (me.foundDiffs.Contains (go)) {
				return;
			}
			me.foundDiffs.Add (go);

			me.foundText.text = me.foundDiffs.Count.ToString ();

			me.CreateMark (go.transform.position, go.transform.localScale);
			if (me.otherCameraOffset != Vector3.zero) {
				me.CreateMark (go.transform.position + me.otherCameraOffset, go.transform.localScale);
			}

			if (IsLevelCompleted () && me.levelCompletedMenu) {
				me.levelCompletedMenu.SetActive (true);
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

		private void CreateWrongMark (Vector2 pos) {

			var markGo = Instantiate (wrongMarkPrefab.gameObject, wrongMarkPrefab.transform.parent);
			var rectTr = markGo.GetComponent<RectTransform> ();
			rectTr.anchoredPosition = pos;
			markGo.SetActive (true);
		}

		public static bool IsLevelCompleted () => me && me.foundDiffs.Count >= DiffItem.items.Count;
	}
}
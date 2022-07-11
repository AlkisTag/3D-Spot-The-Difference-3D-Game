using UnityEngine;

namespace Assets.Scripts {
	public class CamControl : MonoBehaviour {

		private PanAndZoom panAndZoom;

		public Camera[] cams = new Camera[2];
		private readonly Transform[] pivots = new Transform[2];
		private readonly Transform[] tilts = new Transform[2];
		private float tiltRot = 0;
		private float pivotRot = 150;
		public float tiltRotMin = -30;
		public float tiltRotMax = 15;

		private float fov;
		public float fovMin = 24f;
		public float fovMax = 45f;

		public Vector2 turnSens = new Vector2 (.5f, .5f);
		private Vector2 turnMomentum;
		private Vector2 turnMomentumDelta;
		[SerializeField]
		private float turnMomentumDecay = .4f;

		public Vector2 maxPan = new Vector2 (5f, 5f);
		private Vector2 curPan;
		public float panSens = .1f;
		private Vector3 camInitPos;

		void Awake () {

			panAndZoom = GetComponent<PanAndZoom> ();
			panAndZoom.onStartTouch += PanAndZoom_onStartTouch;
			panAndZoom.onEndTouch += PanAndZoom_onEndTouch;
			panAndZoom.onTap += PanAndZoom_onTap;
			panAndZoom.onSwipe += PanAndZoom_onSwipe;
			panAndZoom.onPinch += PanAndZoom_onPinch;

			for (int i = 0; i < 2; i++) {
				tilts[i] = cams[i].transform.parent;
				pivots[i] = tilts[i].parent;
			}
			fov = cams[0].fieldOfView;
			camInitPos = cams[0].transform.localPosition;
		}

		private void PanAndZoom_onPinch (float distOld, float distNew, Vector2 pos, Vector2 delta) {

			float mul = distOld / distNew;
			fov = Mathf.Clamp (fov * mul, fovMin, fovMax);

			float fovNormalized = 1f - (fov - fovMin) / (fovMax - fovMin);
			Vector2 maxPanNorm = maxPan * fovNormalized;
			Vector2 screenCenter = new Vector2 (Screen.width * .5f,
				pos.y > Screen.height * .5f ? Screen.height * .75f : Screen.height * .25f);
			Vector2 centerDelta = pos - screenCenter;
			curPan = curPan * mul - delta * panSens * fovNormalized + centerDelta * panSens * Mathf.Max(0f, 1f / mul - 1f);
			curPan = new Vector2 (
				Mathf.Clamp (curPan.x, -maxPanNorm.x, maxPanNorm.x),
				Mathf.Clamp (curPan.y, -maxPanNorm.y, maxPanNorm.y));
			Vector3 camPos = camInitPos + (Vector3)curPan;
			foreach (var c in cams) {
				c.fieldOfView = fov;
				c.transform.localPosition = camPos;
			}
		}

		private void PanAndZoom_onSwipe (Vector2 delta) {
			RotateFromScreenDelta (delta);
		}

		private void PanAndZoom_onTap (Vector2 pos) {
		}

		private void PanAndZoom_onEndTouch (Vector2 pos, Vector2 vel) {
			turnMomentum = vel;
			turnMomentumDelta = Vector2.zero;
		}

		private void PanAndZoom_onStartTouch (Vector2 pos) {
			turnMomentum = Vector2.zero;
			turnMomentumDelta = Vector2.zero;
		}

		private void RotateFromScreenDelta (Vector2 delta) {

			pivotRot = (pivotRot + delta.x * turnSens.x) % 360f;
			tiltRot = Mathf.Clamp (tiltRot + delta.y * turnSens.y, tiltRotMin, tiltRotMax);

			var rot = Quaternion.Euler (0, pivotRot, 0);
			foreach (var p in pivots) {
				p.localRotation = rot;
			}

			rot = Quaternion.Euler (tiltRot, 0, 0);
			foreach (var t in tilts) {
				t.localRotation = rot;
			}
		}

		void Update () {

			if (turnMomentum != Vector2.zero) {

				RotateFromScreenDelta (turnMomentum);
				turnMomentum = Vector2.SmoothDamp (turnMomentum, Vector2.zero, ref turnMomentumDelta, turnMomentumDecay);
			}
		}
	}
}
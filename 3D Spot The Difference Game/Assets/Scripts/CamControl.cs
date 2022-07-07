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
		public float fovSensLerp = 1f;

		public Vector2 turnSens = new Vector2 (.5f, .5f);
		private Vector2 turnMomentum;
		private Vector2 turnMomentumDelta;
		[SerializeField]
		private float turnMomentumDecay = .4f;

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
		}

		private void PanAndZoom_onPinch (float distOld, float distNew, Vector2 pos, Vector2 delta) {

			fov = Mathf.Clamp (fov * Mathf.Lerp (1f, distOld / distNew, fovSensLerp), fovMin, fovMax);
			foreach (var c in cams) {
				c.fieldOfView = fov;
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
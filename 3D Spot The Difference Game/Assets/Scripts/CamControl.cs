using UnityEngine;

namespace Assets.Scripts {
	public class CamControl : MonoBehaviour {

		private static CamControl me;
		private PanAndZoom panAndZoom;

		public static readonly int[] camLayers = new int[] { 6, 7 };

		public Camera[] cams = new Camera[2];
		private readonly Transform[] pivots = new Transform[2];
		private readonly Transform[] tilts = new Transform[2];
		private float tiltRot = 0;
		private float pivotRot = 150;
		public float tiltRotMin = -40;
		public float tiltRotMax = 40;

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

		[SerializeField]
		private float levelClearUnzoomRate = .9f;

		void Awake () {

			me = this;
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

		private void PanAndZoom_onPinch (float distOld, float distNew, Vector2 pos, Vector2 delta, float rot) {

			PanAndZoom_onPinchChecked (distOld, distNew, pos, delta, rot);
		}

		private void PanAndZoom_onPinchChecked (float distOld, float distNew, Vector2 pos, Vector2 delta, float rot, bool forced = false) {

			if (!forced && Hearts.IsGameOver (true)) return;

			float mul = distOld / distNew;
			fov = Mathf.Clamp (fov * mul, fovMin, fovMax);

			float fovNormalized = 1f - (fov - fovMin) / (fovMax - fovMin);
			Vector2 maxPanNorm = maxPan * fovNormalized;
			Vector2 screenCenter = new Vector2 (Screen.width * .5f,
				pos.y > Screen.height * .5f ? Screen.height * .75f : Screen.height * .25f);
			Vector2 centerDelta = pos - screenCenter;
			curPan = curPan * mul - fovNormalized * panSens * delta + Mathf.Max (0f, 1f / mul - 1f) * panSens * centerDelta;
			curPan = new Vector2 (
				Mathf.Clamp (curPan.x, -maxPanNorm.x, maxPanNorm.x),
				Mathf.Clamp (curPan.y, -maxPanNorm.y, maxPanNorm.y));
			Vector3 camPos = camInitPos + (Vector3)curPan;
			foreach (var c in cams) {
				c.fieldOfView = fov;
				c.transform.localPosition = camPos;
			}
			RotateY (rot);

			MarkFader.ShowMarks ();
		}

		private void PanAndZoom_onSwipe (Vector2 delta) {

			if (Hearts.IsGameOver (true)) return;

			RotateFromScreenDelta (delta);
			if (panAndZoom.IsHeldDown ()) MarkFader.ShowMarks ();
		}

		private void PanAndZoom_onTap (Vector2 pos) {

			if (Hearts.IsGameOver (true)) return;

			DiffHit.RegisterTap (pos);
		}

		private void PanAndZoom_onEndTouch (Vector2 pos, Vector2 vel) {

			if (Hearts.IsGameOver (true)) return;

			turnMomentum = vel;
			turnMomentumDelta = Vector2.zero;
		}

		private void PanAndZoom_onStartTouch (Vector2 pos) {

			if (Hearts.IsGameOver (true)) return;

			turnMomentum = Vector2.zero;
			turnMomentumDelta = Vector2.zero;
		}

		private void RotateFromScreenDelta (Vector2 delta) {

			if (delta == Vector2.zero || Hearts.IsGameOver (true)) return;

			var sens = turnSens * (fov / fovMax);
			pivotRot = (pivotRot + delta.x * sens.x) % 360f;
			tiltRot = Mathf.Clamp (tiltRot + delta.y * sens.y, tiltRotMin, tiltRotMax);

			var rot = Quaternion.Euler (0, pivotRot, 0);
			foreach (var p in pivots) {
				p.localRotation = rot;
			}

			rot = Quaternion.Euler (tiltRot, 0, 0);
			foreach (var t in tilts) {
				t.localRotation = rot;
			}

			var camFwd = cams[0].transform.forward;
			Billboard.UpdateBillboards (camFwd);
		}

		private void RotateY (float degrees) {

			if (degrees == 0f || Hearts.IsGameOver (true)) return;

			pivotRot = (pivotRot - degrees) % 360f;

			var rot = Quaternion.Euler (0, pivotRot, 0);
			foreach (var p in pivots) {
				p.localRotation = rot;
			}

			var camFwd = cams[0].transform.forward;
			Billboard.UpdateBillboards (camFwd);
		}

		void Update () {

			if (DiffHit.IsLevelCompleted ()) {

				PanAndZoom_onPinchChecked (levelClearUnzoomRate, 1f,
					new Vector2 (Screen.width * .5f, Screen.height * .25f), Vector2.zero, 0f, true);

				MarkFader.ShowMarks ();
				return;
			}

			if (turnMomentum != Vector2.zero) {

				RotateFromScreenDelta (turnMomentum);
				turnMomentum = Vector2.SmoothDamp (turnMomentum, Vector2.zero, ref turnMomentumDelta, turnMomentumDecay);
			}
		}

		public static Camera[] GetCameras () => me ? me.cams : null;

		public static Camera GetCamera (int index) {

			if (!me || me.cams == null || index < 0 || index >= me.cams.Length) return null;
			return me.cams[index];
		}

		public static void ModifyMinMaxZoom (float minZoomFactor, float maxZoomFactor) {

			if (!me) return;

			if (minZoomFactor > 0) {
				me.fovMax *= minZoomFactor;
				ThumbnailGen.ModifyCameraFov (minZoomFactor);
			}
			if (maxZoomFactor > 0) {
				me.fovMin /= maxZoomFactor;
				me.maxPan /= maxZoomFactor;
			}

			// simulate zoom to clamp current zoom if needed
			me.PanAndZoom_onPinchChecked (1f, 1f, Vector2.zero, Vector2.zero, 0f);
		}

		public static void AdjustControlsToLevel (LevelType levelType) {

			if (!me) return;

			if (levelType == LevelType.Floating) {
				me.tiltRotMin = -3f * me.tiltRotMax;
			}
		}
	}
}
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
		public bool hasGround;
		public float groundClearance = .5f;

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
		public float panZoomSens = .1f;
		private Vector3 camInitPos;

		[SerializeField]
		private float levelClearUnzoomRate = .9f;

		private const float minCamYForGroundPan = .2f;
		private int curFingers = 0;
		private Vector3 motionOrigin;
		private int panCurCameraIndex = 0;

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

			SetCurFingers (2, pos);

			float mul = distOld / distNew;
			fov = Mathf.Clamp (fov * mul, fovMin, fovMax);
			foreach (var c in cams) {
				c.fieldOfView = fov;
			}

			RotateY (rot);
			var correctedPos = pos;
			if (panCurCameraIndex == 1) {
				correctedPos.y -= Screen.height * .5f;
			}
			PanCameraToPoint (motionOrigin, correctedPos, false);

			MarkFader.ShowMarks ();
		}

		private void PanAndZoom_onSwipe (Vector2 delta, Vector2 pos) {

			if (Hearts.IsGameOver (true)) return;

			SetCurFingers (1, GetLowerScreenCenter ());

			RotateFromScreenDelta (delta);
			if (panAndZoom.IsHeldDown ()) MarkFader.ShowMarks ();
		}

		private void PanAndZoom_onTap (Vector2 pos) {

			if (Hearts.IsGameOver (true)) return;

			if (pos.x > 0 && pos.y > 0 && pos.x < Screen.width && pos.y < Screen.height) {
				DiffHit.RegisterTap (pos);
			}
		}

		private void PanAndZoom_onEndTouch (Vector2 pos, Vector2 vel) {

			if (Hearts.IsGameOver (true)) return;

			SetCurFingers (0, pos);

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

			PanCameraToPoint (motionOrigin, GetLowerScreenCenter ());

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

		private void SetCurFingers (int fingers, Vector2 screenPos) {

			if (curFingers == fingers) return;
			curFingers = fingers;

			if (curFingers > 0) {
				motionOrigin = GetCameraAimedPos (screenPos);
				if (curFingers == 2) {
					panCurCameraIndex = screenPos.y > Screen.height * .5f ? 1 : 0;
				}
			}
		}

		private void PanCameraToPoint (Vector3 worldPoint, Vector2 atScreenPoint, bool clampToCamera = true) {

			var cam = cams[0];

			var newLocalPoint = cam.transform.InverseTransformPoint (worldPoint);

			if (clampToCamera) {
				atScreenPoint.y %= Screen.height * .5f;
			}

			var screenPoint3D = new Vector3 (atScreenPoint.x, atScreenPoint.y, newLocalPoint.z);
			var oldWorldPoint = cam.ScreenToWorldPoint (screenPoint3D);
			var oldLocalPoint = cam.transform.InverseTransformPoint (oldWorldPoint);

			curPan += (Vector2)(newLocalPoint - oldLocalPoint);
			ClampPan ();
			UpdateCameraPosFromPan ();
		}

		private Vector3 GetCameraAimedPos (Vector2 screenPos) {

			var cam = cams[0];
			var camFwd = cam.transform.forward;
			screenPos.y %= Screen.height * .5f;

			var pNormal = Mathf.Abs (camFwd.y) > minCamYForGroundPan ?
				Vector3.down * Mathf.Sign (camFwd.y) :
				new Vector3 (-camFwd.x, 0f, -camFwd.z).normalized;

			var plane = new Plane (pNormal, Vector3.zero);

			var ray = cam.ScreenPointToRay (screenPos);
			if (!plane.Raycast (ray, out var dist)) {
				Debug.Log ("raycast fail");
				return Vector3.zero;
			}

			return ray.GetPoint (dist);
		}

		private void ClampPan () {

			float fovNormalized = 1f - (fov - fovMin) / (fovMax - fovMin);
			Vector2 maxPanNorm = maxPan * fovNormalized;

			curPan = new Vector2 (
				Mathf.Clamp (curPan.x, -maxPanNorm.x, maxPanNorm.x),
				Mathf.Clamp (curPan.y, -maxPanNorm.y, maxPanNorm.y));
		}

		private void UpdatePanFromCameraPos (Camera cam) {

			Vector3 camLocalPos = cam.transform.localPosition - camInitPos;
			curPan = new Vector2 (camLocalPos.x, camLocalPos.y);
			ClampPan ();
			UpdateCameraPosFromPan ();
		}

		private void UpdateCameraPosFromPan () {

			var c = cams[0];
			c.transform.localPosition = camInitPos + (Vector3)curPan;

			if (hasGround && c.transform.position.y < groundClearance) {
				var p = c.transform.position;
				p.y = groundClearance;
				c.transform.position = p;
			}

			cams[1].transform.localPosition = c.transform.localPosition;
		}

		private Vector2 GetLowerScreenCenter () => new Vector2 (Screen.width * .5f, Screen.height * .25f);

		void Update () {

			if (DiffHit.IsLevelCompleted ()) {

				PanAndZoom_onPinchChecked (levelClearUnzoomRate, 1f,
					GetLowerScreenCenter (), Vector2.zero, 0f, true);

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
				me.turnSens *= minZoomFactor;
			}
			if (maxZoomFactor > 0) {
				me.fovMin /= maxZoomFactor;
				me.maxPan /= maxZoomFactor;
				me.turnSens /= maxZoomFactor;
			}

			// simulate zoom to clamp current zoom if needed
			me.PanAndZoom_onPinchChecked (1f, 1f, Vector2.zero, Vector2.zero, 0f);
		}

		public static void AdjustControlsToLevel (LevelType levelType) {

			if (!me) return;

			switch (levelType) {
				case LevelType.Grounded:
					me.hasGround = true;
					break;
				case LevelType.Floating:
					me.tiltRotMin = -3f * me.tiltRotMax;
					break;
				default:
					break;
			}
		}
	}
}
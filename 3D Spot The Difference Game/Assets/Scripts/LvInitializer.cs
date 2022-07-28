using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
	public class LvInitializer : MonoBehaviour {

		private const int levelSceneIndex = 2;

		private static LevelInfo levelToLoad;

		[SerializeField]
		private GameObject[] levelRoots = new GameObject[2];
		private readonly GameObject[] levelVariants = new GameObject[2];

		[SerializeField]
		private GameObject shadowRendererPrefab;

		void Awake () {

			DiffItem.UnregisterItems ();

			if (levelRoots == null || levelRoots.Length != 2 || !levelRoots[0] || !levelRoots[1]) {
				Debug.LogError ("Invalid levelRoots array");
				return;
			}

			if (levelToLoad == null) {

				Debug.Log ("No level to load");
				return;
			}

			// create variant objects
			for (int i = 0; i < levelRoots.Length; i++) {
				var go = Instantiate (i == 0 ? levelToLoad.variant1 : levelToLoad.variant2,
					levelRoots[i].transform.position, levelRoots[i].transform.rotation);

				levelVariants[i] = go;

				// register diff items of variant
				DiffItem.RegisterItems (go);

				// apply camera-specific layer to objects
				// (to only render them in their respective camera)
				DiffHit.MoveToCameraLayer (go, i);
			}

			// create shadow renderers from 1st variant, on both variants
			var children = levelToLoad.variant1.GetComponentsInChildren<MeshRenderer> ();
			foreach (var child in children) {
				CreateShadowRendererFor (child);
			}

			// hide realtime shadows on both variants
			foreach (var lv in levelVariants) {
				DisableShadows (lv.transform);
			}

			// apply bgColor to fog (disable fog if alpha = 0)
			if (levelToLoad.bgColor.a == 0f) {
				RenderSettings.fog = false;
			}
			else {
				RenderSettings.fog = true;
				RenderSettings.fogColor = levelToLoad.bgColor;
			}
		}

		private void Start () {

			// apply bgColor to camera background colors
			foreach (var cam in CamControl.GetCameras ()) {
				cam.backgroundColor = levelToLoad.bgColor;
			}
		}

		private void OnDestroy () {
			DiffItem.UnregisterItems ();
		}

		public static void TransitionToLevel (LevelInfo levelInfo) {

			levelToLoad = levelInfo;
			SceneManager.LoadScene (levelSceneIndex);
		}

		private void DisableShadows (Transform tr) {

			foreach (var mr in tr.GetComponentsInChildren<MeshRenderer> ()) {

				if (mr.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On ||
					mr.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.TwoSided) {

					mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				}

			}
		}

		private void CreateShadowRendererFor (MeshRenderer mr) {

			if (!mr || mr.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.On) return;

			if (DiffHit.IsInDiffLayer (mr.gameObject)) return;

			var mf = mr.GetComponent<MeshFilter> ();
			if (!mf) return;

			for (int i = 0; i < levelRoots.Length; i++) {
				var go = Instantiate (shadowRendererPrefab, levelRoots[i].transform);
				go.layer = CamControl.camLayers[i];
				var mfNew = go.GetComponent<MeshFilter> ();
				mfNew.sharedMesh = mf.sharedMesh;

				var mrNew = go.GetComponent<MeshRenderer> ();
				mrNew.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
				mrNew.sharedMaterial = mr.sharedMaterial;
			}
		}
	}
}
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
	public class LvInitializer : MonoBehaviour {

		private const int levelSceneIndex = 2;

		private static LevelInfo levelToLoad;

		[SerializeField]
		private GameObject[] levelRoots = new GameObject[2];

		void Awake () {

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

				// apply camera-specific layer to objects
				// (to only render them in their respective camera)
				go.layer = levelRoots[i].layer;
				foreach (Transform child in go.transform) {
					// do not change layer if this is in Difference layer (used to mark diff colliders)
					if (child.gameObject.layer == DiffHit.diffLayer) continue;
					child.gameObject.layer = levelRoots[i].layer;
				}
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

		public static void TransitionToLevel (LevelInfo levelInfo) {

			levelToLoad = levelInfo;
			SceneManager.LoadScene (levelSceneIndex);
		}
	}
}
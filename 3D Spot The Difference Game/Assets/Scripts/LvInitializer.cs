using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
	public class LvInitializer : MonoBehaviour {

		private const int levelSceneIndex = 2;

		private static readonly GameObject[] levelPrefabs = new GameObject[2];

		[SerializeField]
		private GameObject[] levelRoots = new GameObject[2];

		void Awake () {

			if (levelRoots == null || levelRoots.Length != 2 || !levelRoots[0] || !levelRoots[1]) {
				Debug.LogError ("Invalid levelRoots array");
				return;
			}

			if (!levelPrefabs[0] || !levelPrefabs[1]) {

				if (levelPrefabs[0]) Debug.Log ("Invalid 2nd level variant");
				else if (levelPrefabs[1]) Debug.Log ("Invalid 1st level variant");
				else Debug.Log ("No level variants provided");
				return;
			}

			for (int i = 0; i < levelRoots.Length; i++) {
				Instantiate (levelPrefabs[i], levelRoots[i].transform.position, levelRoots[i].transform.rotation);
			}
		}

		public static bool PrepareLevelToLoad (GameObject firstVariantPrefab, GameObject secondVariantPrefab) {

			if (!firstVariantPrefab) {
				Debug.LogError ("Invalid firstVariantPrefab");
				return false;
			}

			if (!secondVariantPrefab) {
				Debug.LogError ("Invalid variantPrefab2");
				return false;
			}

			levelPrefabs[0] = firstVariantPrefab;
			levelPrefabs[1] = secondVariantPrefab;
			return true;
		}

		public static void TransitionToLevel (GameObject firstVariantPrefab, GameObject secondVariantPrefab) {

			if (!PrepareLevelToLoad (firstVariantPrefab, secondVariantPrefab)) {
				return;
			}

			SceneManager.LoadScene (levelSceneIndex);
		}
	}
}
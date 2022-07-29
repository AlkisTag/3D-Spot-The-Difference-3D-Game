using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
	public class MainMenu : MonoBehaviour {

		private static MainMenu me;

		public const int sceneIndex = 1;

		public LevelInfo[] levelInfos;

		[SerializeField]
		private Sprite[] levelThumbnails;

		[SerializeField]
		private LevelButton levelBtnTemplate;

		private void Awake () {

			me = this;
		}

		private void Start () {

			foreach (var lv in levelInfos) {

				var go = Instantiate (levelBtnTemplate.gameObject, levelBtnTemplate.transform.parent);
				go.SetActive (true);

				var btn = go.GetComponent<LevelButton> ();
				btn.SetLevelAndThumbnail (lv, GetLevelThumbnail (lv));
			}
		}

		private Sprite GetLevelThumbnail (LevelInfo levelInfo) {

			if (levelInfo == null) return null;

			var n = ThumbnailGen.SimplifyName (levelInfo.name);
			foreach (var s in levelThumbnails) {
				if (s.name.Equals (n)) {
					return s;
				}
			}

			return null;
		}

		public static void RequestTransitionToLevel (LevelInfo levelInfo) {

			if (me) me.TransitionToLevel (levelInfo);
		}

		public void TransitionToLevel (LevelInfo levelInfo) {

			if (levelInfo == null) {
				Debug.LogError ("invalid levelInfo passed");
				return;
			}

			LvInitializer.TransitionToLevel (levelInfo);
		}

		public static bool IsInMainMenu () => SceneManager.GetActiveScene ().buildIndex == sceneIndex;
	}
}
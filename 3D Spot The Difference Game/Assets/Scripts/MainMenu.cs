using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class MainMenu : MonoBehaviour {

		public const int sceneIndex = 1;

		public LevelInfo[] levelInfos;

		[SerializeField]
		private Sprite[] levelThumbnails;

		[SerializeField]
		private Button levelBtnTemplate;

		[SerializeField]
		private bool btnHasText;

		[SerializeField]
		private int btnImageChildIndex = -1;

		private void Start () {

			foreach (var lv in levelInfos) {

				var go = Instantiate (levelBtnTemplate.gameObject, levelBtnTemplate.transform.parent);

				if (btnHasText) {
					var txt = go.GetComponentInChildren<Text> ();
					txt.text = lv.name;
				}

				if (btnImageChildIndex >= 0) {
					var imgChild = go.transform.GetChild (btnImageChildIndex);
					var img = imgChild.GetComponent<Image> ();
					img.sprite = GetLevelThumbnail (lv);
				}

				var btn = go.GetComponent<Button> ();
				btn.onClick.AddListener (() => TransitionToLevel (lv));

				go.SetActive (true);
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
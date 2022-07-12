using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class Hearts : MonoBehaviour {

		private static Hearts me;

		public Sprite heartFull;
		public Sprite heartEmpty;

		private Image[] heartImgs;
		private const int maxHearts = 3;
		private int curHearts = maxHearts;

		public GameObject gameOverMenu;

		void Awake () {

			me = this;

			heartImgs = new Image[maxHearts];
			heartImgs[0] = GetComponentInChildren<Image> ();

			for (int i = 1; i < maxHearts; i++) {
				var go = Instantiate (heartImgs[0].gameObject, heartImgs[0].transform.parent);
				heartImgs[i] = go.GetComponent<Image> ();
			}
			UpdateHearts ();
		}

		public static void SetHearts (int hearts, bool relative = false) {

			if (relative) hearts += me.curHearts;
			hearts = Mathf.Clamp (hearts, 0, maxHearts);
			me.curHearts = hearts;
			me.UpdateHearts ();

			if (GameOver () && me.gameOverMenu) {
				me.gameOverMenu.SetActive (true);
			}
		}

		private void UpdateHearts () {

			for (int i = 0; i < maxHearts; i++) {
				heartImgs[i].sprite = i < curHearts ? heartFull : heartEmpty;
			}
		}

		public static bool GameOver () => me && me.curHearts == 0;

		public void RestartGame () {
			SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		}
	}
}
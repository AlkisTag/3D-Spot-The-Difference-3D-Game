using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class Hearts : MonoBehaviour {

		private static Hearts me;

		[SerializeField]
		private Sprite heartFull;
		[SerializeField]
		private Sprite heartEmpty;

		private Image[] heartImgs;
		private const int maxHearts = 6;
		private int curHearts = maxHearts;

		public FadeableDialog gameOverMenu;

		private const string key = "curHearts";

		[SerializeField]
		private GameObject heartPulsePrefab;
		[SerializeField]
		private float heartPulseInterval = .25f;

		void Awake () {

			me = this;

			heartImgs = new Image[maxHearts];
			heartImgs[0] = GetComponentInChildren<Image> ();

			for (int i = 1; i < maxHearts; i++) {
				var go = Instantiate (heartImgs[0].gameObject, heartImgs[0].transform.parent);
				heartImgs[i] = go.GetComponent<Image> ();
			}

			LoadPersistent ();
			UpdateHearts ();
		}

		public static void SetHearts (int hearts, bool relative = false) {

			if (relative) hearts += me.curHearts;
			hearts = Mathf.Clamp (hearts, 0, maxHearts);
			me.curHearts = hearts;
			me.UpdateHearts ();
			me.SavePersistent ();

			if (IsGameOver () && me.gameOverMenu) {
				me.gameOverMenu.FadeIn ();
			}
		}

		private void UpdateHearts () {

			for (int i = 0; i < maxHearts; i++) {
				heartImgs[i].sprite = i < curHearts ? heartFull : heartEmpty;
			}
		}

		public void RefillHearts () {

			SetHearts (maxHearts);

			if (gameOverMenu) {
				me.gameOverMenu.FadeOut ();
			}

			StartCoroutine (HeartsRefilledAnim ());
		}

		private IEnumerator HeartsRefilledAnim () {

			for (int i = 0; i < maxHearts; i++) {
				Instantiate (heartPulsePrefab, heartImgs[i].transform);
				yield return new WaitForSecondsRealtime (heartPulseInterval);
			}
		}

		private void LoadPersistent () {

			try {
				curHearts = PlayerPrefs.GetInt (key, 0);
			}
			catch (System.Exception) { }

			if (curHearts <= 0 || curHearts > maxHearts) {
				curHearts = maxHearts;
			}
		}

		private void SavePersistent () {

			try {
				PlayerPrefs.SetInt (key, curHearts);
				PlayerPrefs.Save ();
			}
			catch (System.Exception) { }
		}

		public static bool IsGameOver (bool orLevelCompleted = false) =>
			(me && me.curHearts == 0) || (orLevelCompleted && DiffHit.IsLevelCompleted ());
	}
}
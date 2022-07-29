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

		[SerializeField]
		private HeartLost heartLostPrefab;
		[SerializeField]
		private RectTransform lostHeartsContainer;

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

		public static void LoseHeartWithAnim (Vector2 tapPos) {

			SetHearts (-1, true);
			if (!me || !me.heartLostPrefab || !me.lostHeartsContainer) return;

			var lastHeart = me.GetLastFullHeart ();
			if (!lastHeart) return;

			var go = Instantiate (me.heartLostPrefab.gameObject, lastHeart.transform);
			var rtr = go.GetComponent<RectTransform> ();
			rtr.SetParent (rtr.root, true);
			rtr.SetParent (me.lostHeartsContainer, true);
			rtr.anchorMin = Vector2.zero;
			rtr.anchorMax = Vector2.zero;
			rtr.anchoredPosition += new Vector2 (Screen.width * .5f, Screen.height * .5f);

			var lost = go.GetComponent<HeartLost> ();
			lost.SetRouteTo (tapPos);
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

		private Image GetLastFullHeart () {

			var i = Mathf.Clamp (curHearts, 0, maxHearts - 1);
			return heartImgs[i];
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
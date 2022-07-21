using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class DialogHandler : MonoBehaviour {

		private static DialogHandler me;

		public FadeableDialog pauseMenu;

		public FadeableDialog promptMenu;
		public Text promptText;
		public Button[] promptMenuButtons;

		private void Awake () {
			me = this;
		}

		void Update () {

			if (Input.GetKeyDown (KeyCode.Escape) && !Hearts.IsGameOver (true)) {
				ShowPauseMenu ();
			}
		}

		public static void ShowPauseMenu () {

			if (!me) return;
			me.pauseMenu.FadeIn ();
		}

		public static void HidePauseMenu () {

			if (!me) return;
			me.pauseMenu.FadeOut ();
		}

		public static void Prompt (string text, System.Action<bool> onAnswered,
			string yesText = "Yes", string noText = "No") {

			if (!me) return;

			me.promptText.text = text;
			for (int i = 0; i < me.promptMenuButtons.Length; i++) {
				var btn = me.promptMenuButtons[i];

				var t = btn.GetComponentInChildren<Text> ();
				t.text = i == 1 ? yesText : noText;

				btn.onClick.RemoveAllListeners ();
				int ii = i;
				btn.onClick.AddListener (() => {
					me.promptMenu.FadeOut ();
					onAnswered (ii == 1);
				});
			}

			me.promptMenu.FadeIn ();
		}

		public void RestartLevel (bool askFirst) {

			if (!askFirst) {
				SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
				return;
			}

			bool wasPaused = pauseMenu.IsOpened ();
			HidePauseMenu ();
			Prompt ("Restart Level?", (yes) => {
				if (yes) RestartLevel (false);
				else if (wasPaused) ShowPauseMenu ();
			});
		}

		public void ReturnToMenu (bool askFirst) {

			if (!askFirst) {
				SceneManager.LoadScene (MainMenu.sceneIndex);
				return;
			}

			bool wasPaused = pauseMenu.IsOpened ();
			HidePauseMenu ();
			Prompt ("Quit to Menu?", (yes) => {
				if (yes) ReturnToMenu (false);
				else if (wasPaused) ShowPauseMenu ();
			});
		}
	}
}

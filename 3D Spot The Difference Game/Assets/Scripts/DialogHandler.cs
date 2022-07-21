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

			if (Input.GetKeyDown (KeyCode.Escape)) {

				if (MainMenu.IsInMainMenu ()) {
					QuitGame (true);
				}
				else if (!Hearts.IsGameOver (true)) {
					ShowPauseMenu ();
				}
			}
		}

		public static void ShowPauseMenu () {

			if (!me || !me.pauseMenu) return;
			me.pauseMenu.FadeIn ();
		}

		public static void HidePauseMenu () {

			if (!me || !me.pauseMenu) return;
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

		public void PromptedAction (string text, System.Action action, bool askFirst) {

			if (!askFirst) {
				action?.Invoke ();
				return;
			}

			bool wasPaused = pauseMenu && pauseMenu.IsOpened ();
			HidePauseMenu ();
			Prompt (text, (yes) => {
				if (yes) action?.Invoke ();
				else if (wasPaused) ShowPauseMenu ();
			});
		}

		public void RestartLevel (bool askFirst) {

			PromptedAction ("Restart Level?", () => SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex), askFirst);
		}

		public void ReturnToMenu (bool askFirst) {

			PromptedAction ("Quit to Menu?", () => SceneManager.LoadScene (MainMenu.sceneIndex), askFirst);
		}


		public void QuitGame (bool askFirst) {

			PromptedAction ("Exit game?", Application.Quit, askFirst);
		}
	}
}

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class LevelButton : MonoBehaviour {

		private LevelInfo levelInfo;

		[SerializeField]
		private Image thumbnailImage;

		public void SetLevelAndThumbnail (LevelInfo levelInfo, Sprite thumbnail) {

			if (this.levelInfo == levelInfo) return;
			this.levelInfo = levelInfo;

			thumbnailImage.sprite = thumbnail;

			var btn = GetComponent<Button> ();
			btn.onClick.RemoveAllListeners ();
			btn.onClick.AddListener (() => MainMenu.RequestTransitionToLevel (this.levelInfo));
		}
	}
}
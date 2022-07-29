using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class ScreenFlash : MonoBehaviour {

		private static ScreenFlash me;

		private Image img;

		[SerializeField]
		private Color[] colors = new Color[] { Color.green, Color.red };
		private Color curColor;

		[SerializeField]
		private float flashDur = .5f;
		private float timer;

		[SerializeField]
		private AnimationCurve flashAlphaCurve;

		void Awake () {

			me = this;

			img = GetComponent<Image> ();
			curColor = img.color;

			gameObject.SetActive (false);
		}

		void Update () {

			timer += Time.unscaledDeltaTime;
			if (timer > flashDur) {
				gameObject.SetActive (false);
				return;
			}

			float a = Mathf.Clamp01 (timer / flashDur);

			var c = curColor;
			c.a *= flashAlphaCurve.Evaluate (a);
			img.color = c;
		}

		public static void Flash (int colorIndex) {

			if (!me) return;

			me.timer = 0f;
			me.img.color = Color.clear;
			me.curColor = me.colors[colorIndex];
			me.gameObject.SetActive (true);
		}

		public static void FlashGood () {
			Flash (0);
		}

		public static void FlashBad () {
			Flash (1);
		}
	}
}
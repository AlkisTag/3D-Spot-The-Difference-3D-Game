using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class HeartPulse : MonoBehaviour {

		private Image img;
		private Color initCol;
		private Vector3 initScale;

		[SerializeField]
		private float maxScale = 3f;
		[SerializeField]
		private float animDur = .4f;
		private float timer;

		void Awake () {

			img = GetComponent<Image> ();
			initCol = img.color;
			initScale = transform.localScale;
		}

		void Update () {

			timer += Time.unscaledDeltaTime;
			if (timer > maxScale) {
				Destroy (gameObject);
				return;
			}

			float a = timer / animDur;

			var c = initCol;
			c.a *= 1f - a;
			img.color = c;

			transform.localScale = initScale * Mathf.Lerp (1f, maxScale, a);
		}
	}
}
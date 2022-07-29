using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class HeartLost : MonoBehaviour {

		private Image img;
		private Color initCol;
		private Vector3 initScale;
		private Vector2 initPos;
		private RectTransform rtr;

		public Vector2 flyTargetPos;
		[SerializeField]
		private AnimationCurve flyPosLerp = AnimationCurve.Linear (0, 0, 1, 1);
		[SerializeField]
		private AnimationCurve posToCenterLerp = AnimationCurve.Linear (0, 0, 1, 0);
		[SerializeField]
		private float flyDur = 1.1f;

		[SerializeField]
		private float maxScale = 3f;
		[SerializeField]
		private float fadeDur = .5f;
		[SerializeField]
		private float spinSpeed = 90f;
		private float spinDir = 1f;

		private float timer;

		void Awake () {

			img = GetComponent<Image> ();
			initCol = img.color;

			rtr = GetComponent<RectTransform> ();

			spinDir = Random.value > .5f ? 1f : -1f;
		}

		private void Start () {

			initScale = transform.localScale;
			initPos = rtr.anchoredPosition;
		}

		void Update () {

			timer += Time.unscaledDeltaTime;
			if (timer > flyDur + fadeDur) {
				Destroy (gameObject);
				return;
			}

			float a;
			if (timer < flyDur) {

				a = Mathf.Clamp01 (timer / flyDur);
				a = flyPosLerp.Evaluate (a);

				var pos = Vector2.Lerp (initPos, flyTargetPos, a);
				pos = Vector2.Lerp (pos, new Vector2 (Screen.width * .5f, Screen.height * .5f), posToCenterLerp.Evaluate (a));

				transform.localPosition = pos;
				return;
			}

			float t = timer - flyDur;
			a = t / fadeDur;

			var c = initCol;
			c.a *= 1f - a;
			img.color = c;

			transform.localScale = initScale * Mathf.Lerp (1f, maxScale, a);
			transform.localEulerAngles = new Vector3 (0, 0, spinDir * spinSpeed * t);
		}

		public void SetRouteTo (Vector2 endPos) {

			initPos = rtr.anchoredPosition;
			flyTargetPos = endPos;
		}
	}
}
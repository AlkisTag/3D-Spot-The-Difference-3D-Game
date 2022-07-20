using UnityEngine;

namespace Assets.Scripts {
	public class AnimScaleIn : MonoBehaviour {

		[SerializeField]
		private AnimationCurve scaleCurve;
		[SerializeField]
		private float animDur = .6f;
		private float timer;

		private Vector3 initScale;

		void Awake () {

			initScale = transform.localScale;
			UpdateAnim ();
		}

		void Update () {

			timer += Time.deltaTime;
			UpdateAnim ();
			if (timer >= animDur) enabled = false;
		}

		private void UpdateAnim () {

			var s = scaleCurve.Evaluate (Mathf.Clamp01 (timer / animDur));
			transform.localScale = initScale * s;
		}
	}
}
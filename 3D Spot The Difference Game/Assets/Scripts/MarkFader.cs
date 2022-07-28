using UnityEngine;

namespace Assets.Scripts {
	public class MarkFader : MonoBehaviour {

		//private static MarkFader me;

		//public Material markMat;
		//private Color initCol;

		//public float maxAlpha = 2f;
		//public float fadeRate = 1f;
		//private float timer;

		//void Awake () {

		//	me = this;
		//	initCol = markMat.color;
		//}

		//private void OnDestroy () {

		//	if (me == this) markMat.color = initCol;
		//}

		//void Update () {

		//	if (timer <= 0f) return;

		//	timer -= Time.deltaTime * fadeRate;
		//	var c = initCol;
		//	c.a *= Mathf.Clamp01 (timer);
		//	markMat.color = c;
		//}

		public static void ShowMarks () {

			//if (me) me.timer = me.maxAlpha;
		}
	}
}
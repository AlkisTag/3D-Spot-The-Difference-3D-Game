using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class WrongMark : MonoBehaviour {

		private float timer;
		private const float lifetime = 1f;

		private Image img;
		private Color col;

		private void Awake () {

			img = GetComponent<Image> ();
			col = img.color;
		}

		void Update () {

			timer += Time.deltaTime;
			if (timer >= lifetime) {
				Destroy (gameObject);
				return;
			}

			var c = col;
			c.a *= 1f - timer / lifetime;
			img.color = c;
		}
	}
}
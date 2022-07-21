using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
	public class Preloading : MonoBehaviour {

		void Start () {

			StartCoroutine (GotoNextScene ());
		}

		IEnumerator GotoNextScene () {

			yield return null;

			SceneManager.LoadScene (MainMenu.sceneIndex);
		}
	}
}
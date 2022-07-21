using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
	public class Preloading : MonoBehaviour {

		private const int mainMenuSceneIndex = 1;

		void Start () {

			StartCoroutine (GotoNextScene ());
		}

		IEnumerator GotoNextScene () {

			yield return null;

			SceneManager.LoadScene (mainMenuSceneIndex);
		}
	}
}
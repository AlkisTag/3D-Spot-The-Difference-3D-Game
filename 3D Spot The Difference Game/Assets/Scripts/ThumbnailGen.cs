using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
	public class ThumbnailGen : MonoBehaviour {

		private static ThumbnailGen me;

		private Camera cam;
		public string outDir;
		private const string ext = ".png";

		void Start () {

			if (me) return;
			me = this;

			cam = GetComponentInChildren<Camera> ();
			if (!cam) {
				Debug.LogWarning ("ThumbnailGen has no camera", gameObject);
				return;
			}

			if (!System.IO.Directory.Exists (outDir)) {
				Debug.LogWarning ("Directory does not exist: " + outDir, gameObject);
				return;
			}
			var sep = System.IO.Path.DirectorySeparatorChar.ToString ();
			if (!outDir.EndsWith (sep)) outDir += sep;

			DontDestroyOnLoad (gameObject);

			Debug.LogWarning ("Starting thumbnail generation. If this was not intended, disable this script!", gameObject);
			StartCoroutine (GenCoroutine ());
		}

		IEnumerator GenCoroutine () {

			var mainMenu = FindObjectOfType<MainMenu> ();
			if (!mainMenu) {
				Debug.LogWarning ("MainMenu not found", gameObject);
				yield break;
			}

			var rt = cam.targetTexture;
			var mRt = new RenderTexture (rt.width, rt.height, rt.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			mRt.antiAliasing = rt.antiAliasing;

			cam.targetTexture = mRt;

			var levelInfos = new List<LevelInfo> (mainMenu.levelInfos);
			foreach (var lv in levelInfos) {

				if (!mainMenu) {
					mainMenu = FindObjectOfType<MainMenu> ();
				}
				Debug.Log ("Rendering " + lv.name + "...", gameObject);
				mainMenu.TransitionToLevel (lv);

				yield return new WaitWhile (MainMenu.IsInMainMenu);
				yield return null;

				cam.Render ();
				yield return null;

				var tex = new Texture2D (mRt.width, mRt.height, TextureFormat.ARGB32, false);
				RenderTexture.active = mRt;
				tex.ReadPixels (new Rect (0, 0, mRt.width, mRt.height), 0, 0);
				tex.Apply ();
				RenderTexture.active = null;

				var bytes = tex.EncodeToPNG ();
				System.IO.File.WriteAllBytes (outDir + SimplifyName (lv.name) + ext, bytes);

				DestroyImmediate (tex);

				SceneManager.LoadScene (MainMenu.sceneIndex);
				yield return new WaitUntil (MainMenu.IsInMainMenu);
			}
			Debug.Log ("Done!", gameObject);
		}

		private static string SimplifyName (string n) {

			var rgx = new System.Text.RegularExpressions.Regex ("[^a-zA-Z0-9-]");
			n = rgx.Replace (n, "");
			n = n.ToLowerInvariant ().Replace (' ', '-');

			return n;
		}
	}
}
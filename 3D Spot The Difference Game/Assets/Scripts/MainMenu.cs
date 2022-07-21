﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
	public class MainMenu : MonoBehaviour {

		public LevelInfo[] levelInfos;

		[SerializeField]
		private Button levelBtnTemplate;

		private void Start () {

			foreach (var lv in levelInfos) {

				var go = Instantiate (levelBtnTemplate.gameObject, levelBtnTemplate.transform.parent);

				var txt = go.GetComponentInChildren<Text> ();
				txt.text = lv.name;

				var btn = go.GetComponent<Button> ();
				btn.onClick.AddListener (() => TransitionToLevel (lv));

				go.SetActive (true);
			}
		}

		public void TransitionToLevel (LevelInfo levelInfo) {

			if (levelInfo == null) {
				Debug.LogError ("invalid levelInfo passed");
				return;
			}

			LvInitializer.TransitionToLevel (levelInfo.variant1, levelInfo.variant2);
		}
	}

	[System.Serializable]
	public class LevelInfo {

		public string name;
		public GameObject variant1;
		public GameObject variant2;
	}
}
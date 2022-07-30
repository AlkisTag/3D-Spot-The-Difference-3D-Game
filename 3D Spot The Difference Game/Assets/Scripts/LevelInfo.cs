using UnityEngine;

namespace Assets.Scripts {

	[System.Serializable]
	public class LevelInfo {

		public string name;
		public GameObject variant1;
		public GameObject variant2;
		public LevelType type;
		public Color bgColor;
		public Color diffOutlineColor = Color.yellow;
		public float relativeExposure;
		public float minZoomFactor = 1f;
		public float maxZoomFactor = 1f;
	}

	public enum LevelType { Grounded, Floating }
}

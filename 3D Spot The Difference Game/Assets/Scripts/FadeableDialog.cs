using UnityEngine;

[RequireComponent (typeof (CanvasGroup))]
public class FadeableDialog : MonoBehaviour {

	public float fadeDur = .25f;
	private float fading = 0f;
	public bool fadeOnStart;

	private Vector2 initPos;
	public Vector2 displace;
	private Vector3 initScale;
	public bool useScaleY;

	public CanvasGroup canvasGroup;
	public RectTransform targetRectTransform;

	private bool hasInit;
	private float tillHide;

	private void Awake () {
		Init ();
	}

	void Init () {

		if (hasInit) return;
		hasInit = true;

		if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup> ();
		if (!targetRectTransform) targetRectTransform = GetComponent<RectTransform> ();

		if (displace != Vector2.zero) initPos = targetRectTransform.anchoredPosition;
		if (useScaleY) initScale = targetRectTransform.localScale;
	}

	private void Start () {

		if (fadeOnStart) {
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			if (useScaleY) UpdateTargetTransformScale ();
			fading = 1f;
		}
	}

	private void OnEnable () {

		if (canvasGroup.alpha == 0f) {
			canvasGroup.interactable = false;
			if (useScaleY) UpdateTargetTransformScale ();
			fading = 1f;
		}
	}

	void Update () {

		if (fading == 0f) {
			if (tillHide > 0f) {
				tillHide -= Time.unscaledDeltaTime;
				if (tillHide <= 0f) FadeOut ();
			}
			return;
		}

		// move to target alpha
		var targetAlpha = fading > 0f ? 1f : 0f;
		canvasGroup.alpha = Mathf.MoveTowards (canvasGroup.alpha, targetAlpha,
			Time.unscaledDeltaTime / fadeDur);

		// update offset
		if (displace != Vector2.zero) UpdateTargetTransformPos ();

		// update scale
		if (useScaleY) UpdateTargetTransformScale ();

		// end fade if reached
		if (canvasGroup.alpha == targetAlpha) {
			fading = 0f;
			canvasGroup.interactable = true;
			if (targetAlpha == 0f) gameObject.SetActive (false);
		}
	}

	public void SetInitPos (Vector2 anchoredPosition) {

		initPos = anchoredPosition;
		UpdateTargetTransformPos ();
	}

	private void UpdateTargetTransformPos () {

		targetRectTransform.anchoredPosition = initPos
			+ displace * (1f - canvasGroup.alpha);
	}

	private void UpdateTargetTransformScale () {

		var s = initScale;
		s.y *= canvasGroup.alpha;
		targetRectTransform.localScale = s;
	}

	public void FadeIn () {

		Init ();
		if (!gameObject.activeSelf) {
			gameObject.SetActive (true);
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			if (useScaleY) UpdateTargetTransformScale ();
		}
		fading = 1f;
		tillHide = 0f;
	}

	public void FadeInInstant () {

		Init ();
		if (!gameObject.activeSelf) gameObject.SetActive (true);
		canvasGroup.alpha = 1f;
		fading = 1f;
		tillHide = 0f;
	}

	public void FadeOut () {

		if (!gameObject.activeSelf) return;
		Init ();
		canvasGroup.interactable = false;
		fading = -1f;
	}

	public void FadeOutInstant () {

		if (!gameObject.activeSelf) return;
		Init ();
		canvasGroup.alpha = 0f;
		fading = -1f;
	}

	public bool IsOpened () {

		return gameObject.activeSelf && fading != -1f;
	}

	public bool IsFading () {

		return gameObject.activeSelf && fading != 0f;
	}

	public void ScheduleFadeOut (float delay) {

		tillHide = delay;
	}
}

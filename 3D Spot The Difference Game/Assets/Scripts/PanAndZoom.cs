using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary> A modular and easily customisable Unity MonoBehaviour for handling swipe and pinch motions on mobile. </summary>
public class PanAndZoom : MonoBehaviour {

	/// <summary> Called as soon as the player touches the screen. The argument is the screen position. </summary>
	public event Action<Vector2> onStartTouch;
	/// <summary> Called as soon as the player stops touching the screen. The argument is the screen position, and screen velocity on release. </summary>
	public event Action<Vector2, Vector2> onEndTouch;
	/// <summary> Called if the player completed a quick tap motion. The argument is the screen position. </summary>
	public event Action<Vector2> onTap;
	/// <summary> Called if the player swiped the screen. The argument is the screen movement delta, and screen position. </summary>
	public event Action<Vector2, Vector2> onSwipe;
	/// <summary> Called if the player pinched the screen. The arguments are the distance between the fingers before and after, the end screen position, the screen movement delta, and rotation in degrees. </summary>
	public event Action<float, float, Vector2, Vector2, float> onPinch;

	[Header ("Tap")]
	[Tooltip ("The maximum movement for a touch motion to be treated as a tap")]
	public float maxDistanceForTap = 40;
	[Tooltip ("The maximum duration for a touch motion to be treated as a tap")]
	public float maxDurationForTap = 0.4f;

	[Header ("Desktop debug")]
	[Tooltip ("Use the mouse on desktop?")]
	public bool useMouse = true;
	[Tooltip ("The simulated pinch speed using the scroll wheel")]
	public float mouseScrollSpeed = 2;

	[Header ("UI")]
	[Tooltip ("Are touch motions listened to if they are over UI elements?")]
	public bool ignoreUI = false;

	Vector2 touch0StartPosition;
	Vector2 touch0LastPosition;
	float touch0StartTime;

	Vector2 momentum;
	Vector2 momentumLerpSpeed;
	public float momentumLerpTime = 0.025f;

	bool canUseMouse;

	/// <summary> Has the player at least one finger on the screen? </summary>
	public bool isTouching { get; private set; }

	/// <summary> The point of contact if it exists in Screen space. </summary>
	public Vector2 touchPosition { get { return touch0LastPosition; } }

	void Start () {
		canUseMouse = Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer && Input.mousePresent;
	}

	void Update () {
		if (useMouse && canUseMouse) {
			UpdateWithMouse ();
		}
		else {
			UpdateWithTouch ();
		}
	}

	void UpdateWithMouse () {
		if (Input.GetMouseButtonDown (0)) {
			if (ignoreUI || !IsPointerOverUIObject ()) {
				touch0StartPosition = Input.mousePosition;
				touch0StartTime = Time.time;
				touch0LastPosition = touch0StartPosition;

				isTouching = true;

				onStartTouch?.Invoke (Input.mousePosition);
			}
		}

		if (Input.GetMouseButton (0) && isTouching) {

			bool shift = Input.GetKey (KeyCode.LeftShift);
			Vector2 move = (Vector2)Input.mousePosition - touch0LastPosition;
			touch0LastPosition = Input.mousePosition;
			if (shift) {
				momentum = Vector2.zero;
			}
			else {
				momentum = Vector2.SmoothDamp (momentum, move, ref momentumLerpSpeed, momentumLerpTime);
			}

			if (move != Vector2.zero) {
				if (shift) OnPinch (Input.mousePosition, 1f, 1f, move, 0f);
				else OnSwipe (move, Input.mousePosition);
			}
		}

		if (Input.GetMouseButtonUp (0) && isTouching) {

			if (Time.time - touch0StartTime <= maxDurationForTap
			   && Vector2.Distance (Input.mousePosition, touch0StartPosition) <= maxDistanceForTap) {
				OnClick (Input.mousePosition);
			}

			onEndTouch?.Invoke (Input.mousePosition, momentum);
			isTouching = false;
			momentum = Vector2.zero;
			momentumLerpSpeed = Vector2.zero;
		}

		if (Input.mouseScrollDelta.y != 0) {

			if (!isTouching) {
				onStartTouch?.Invoke (Input.mousePosition);
			}
			OnPinch (Input.mousePosition, 1, Input.mouseScrollDelta.y < 0 ? (1 / mouseScrollSpeed) : mouseScrollSpeed, Vector2.zero, 0f);
			if (!isTouching) {
				onEndTouch?.Invoke (Input.mousePosition, Vector2.zero);
			}
		}
	}

	void UpdateWithTouch () {
		int touchCount = Input.touches.Length;

		if (touchCount == 1) {
			Touch touch = Input.touches[0];

			switch (touch.phase) {
				case TouchPhase.Began: {
						if (ignoreUI || !IsPointerOverUIObject ()) {
							touch0StartPosition = touch.position;
							touch0StartTime = Time.time;
							touch0LastPosition = touch0StartPosition;

							isTouching = true;

							onStartTouch?.Invoke (touch0StartPosition);
						}

						break;
					}
				case TouchPhase.Moved: {
						touch0LastPosition = touch.position;

						if (isTouching) {
							if (touch.deltaPosition != Vector2.zero) {
								OnSwipe (touch.deltaPosition, touch0LastPosition);
							}
							momentum = Vector2.SmoothDamp (momentum, touch.deltaPosition, ref momentumLerpSpeed, momentumLerpTime);
						}
						break;
					}
				case TouchPhase.Ended: {
						if (Time.time - touch0StartTime <= maxDurationForTap
							&& Vector2.Distance (touch.position, touch0StartPosition) <= maxDistanceForTap
							&& isTouching) {
							OnClick (touch.position);
						}

						onEndTouch?.Invoke (touch.position, momentum);
						momentum = Vector2.zero;
						momentumLerpSpeed = Vector2.zero;
						isTouching = false;
						break;
					}
				case TouchPhase.Stationary:
				case TouchPhase.Canceled:
					break;
			}
		}
		else if (touchCount == 2) {
			Touch touch0 = Input.touches[0];
			Touch touch1 = Input.touches[1];

			if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended) return;

			isTouching = true;

			Vector2 prev0 = touch0.position - touch0.deltaPosition;
			Vector2 prev1 = touch1.position - touch1.deltaPosition;

			float previousDistance = Vector2.Distance (prev0, prev1);
			float currentDistance = Vector2.Distance (touch0.position, touch1.position);

			float rot = 0f;
			if (previousDistance > 1f && currentDistance > 1f) {
				rot = Vector2.SignedAngle (touch0.position - touch1.position, prev0 - prev1);
			}

			Vector2 center = (touch0.position + touch1.position) * .5f;
			Vector2 centerPrev = (prev0 + prev1) * .5f;
			OnPinch (center, previousDistance, currentDistance, center - centerPrev, rot);

			momentum = Vector2.zero;
		}
		else {
			if (isTouching) {
				onEndTouch?.Invoke (touch0LastPosition, momentum);
				momentum = Vector2.zero;
				momentumLerpSpeed = Vector2.zero;
				isTouching = false;
			}
		}
	}

	void OnClick (Vector2 position) {
		if (onTap != null && (ignoreUI || !IsPointerOverUIObject ())) {
			onTap (position);
		}
	}

	void OnSwipe (Vector2 deltaPosition, Vector2 position) {
		onSwipe?.Invoke (deltaPosition, position);
	}

	void OnPinch (Vector2 center, float oldDistance, float newDistance, Vector2 touchDelta, float rotation) {
		onPinch?.Invoke (oldDistance, newDistance, center, touchDelta, rotation);
	}

	/// <summary> Checks if the the current input is over canvas UI </summary>
	public bool IsPointerOverUIObject () {

		if (EventSystem.current == null) return false;
		PointerEventData eventDataCurrentPosition = new PointerEventData (EventSystem.current);
		eventDataCurrentPosition.position = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult> ();
		EventSystem.current.RaycastAll (eventDataCurrentPosition, results);
		return results.Count > 0;
	}

	public bool IsHeldDown () => isTouching && Time.time - touch0StartTime > maxDurationForTap;
}
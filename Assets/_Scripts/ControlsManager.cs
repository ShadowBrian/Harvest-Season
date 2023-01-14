using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsManager : MonoBehaviour
{

	Controls controls;

	public CameraInteractions cameraInteractions;

	public LockedNavigation lockedNav;

	public InputDevice currentInputDevice { get; private set; }
	public string currentScheme { get; private set; }

	public bool IsGamepad => currentScheme.Equals("Gamepad");

	public Vector2 controllerSensitivityScale;

	public Shotgun gun;
	public void Awake()
	{
		controls = new Controls();

		/*controls.Main.Interact.performed += InteractPressed;
		controls.Main.Running.performed += HandleRunning;
		controls.Main.Pause.performed += Pause;
		controls.Main.UIControllerSelect.performed += UISelect;
		controls.Main.Fire.performed += Fire;*/

		InputSystem.onActionChange += OnActionChange;

		controls.Enable();
	}

	private void OnActionChange(object obj, InputActionChange change)
	{
		if (change == InputActionChange.ActionPerformed)
		{
			InputDevice lastDevice = ((InputAction)obj).activeControl.device;

			if (lastDevice != currentInputDevice || currentInputDevice == null)
			{
				if (lastDevice.ToString().Contains("Mouse") || lastDevice.ToString().Contains("Keyboard"))
				{
					if (currentScheme != "Keyboard")
						lockedNav.OnControllerInput(false);

					currentScheme = "Keyboard";
				}
				else
				{
					if (currentScheme != "Gamepad")
						lockedNav.OnControllerInput(true);

					currentScheme = "Gamepad";
				}
			}
		}
	}

	public void OnDestroy()
	{
		/*controls.Main.Interact.performed -= InteractPressed;
		controls.Main.Running.performed -= HandleRunning;
		controls.Main.Pause.performed -= Pause;
		controls.Main.UIControllerSelect.performed -= UISelect;
		controls.Main.Fire.performed -= Fire;*/


		InputSystem.onActionChange -= OnActionChange;
	}

	bool Fired;
	bool Running, PressedRunning;

	public void HandleVRInput()
	{
		if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.triggerButton, XRHandSide.LeftHand))
		{
			InteractPressed();
		}

		if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primaryButton, XRHandSide.LeftHand))
		{
			UISelect();
		}

		HandleRunning();

		if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primary2DAxisClick, XRHandSide.LeftHand) && !PressedRunning)
		{
			Running = !Running;
			PressedRunning = true;
		}

		if (PlayerController.Instance.moveInput.magnitude < 0.2f)
		{
			Running = false;
		}

		if (UnityXRInputBridge.instance.GetButtonUp(XRButtonMasks.primary2DAxisClick, XRHandSide.LeftHand) && PressedRunning)
		{
			PressedRunning = false;
		}

		if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.triggerButton, XRHandSide.RightHand) && !Fired)
		{
			Fire();
			Fired = true;
		}

		if (UnityXRInputBridge.instance.GetButtonUp(XRButtonMasks.triggerButton, XRHandSide.RightHand) && Fired)
		{
			Fired = false;
		}
	}

	bool JustRotated;

	public void Update()
	{
		if (GameManager.Instance)
		{
			if (GameManager.Instance.AllowInput)
			{
				PlayerController.Instance.moveInput = UnityXRInputBridge.instance.GetVec2(XR2DAxisMasks.primary2DAxis, XRHandSide.LeftHand);// controls.Main.Move.ReadValue<Vector2>();
				if (!JustRotated)
				{
					if (UnityXRInputBridge.instance.GetVec2(XR2DAxisMasks.primary2DAxis, XRHandSide.RightHand).x > 0.5f)
					{
						PlayerController.Instance.lookInput = new Vector2(45, 0);
						JustRotated = true;
					}
					if (UnityXRInputBridge.instance.GetVec2(XR2DAxisMasks.primary2DAxis, XRHandSide.RightHand).x < -0.5f)
					{
						PlayerController.Instance.lookInput = new Vector2(-45, 0);
						JustRotated = true;
					}
				}
				else
				{
					PlayerController.Instance.lookInput = Vector2.zero;
					if (UnityXRInputBridge.instance.GetVec2(XR2DAxisMasks.primary2DAxis, XRHandSide.RightHand).x < 0.5f && UnityXRInputBridge.instance.GetVec2(XR2DAxisMasks.primary2DAxis, XRHandSide.RightHand).x > -0.5f)
					{
						JustRotated = false;
					}
				}
				HandleVRInput();


				/*if (!IsGamepad)
                {
                    PlayerController.Instance.lookInput = controls.Main.Look.ReadValue<Vector2>() * 0.5f * 0.1f;
                }
                else
                {
                    PlayerController.Instance.lookInput = controls.Main.Look.ReadValue<Vector2>() * Time.deltaTime * controllerSensitivityScale; 
                }*/
				//PlayerController.Instance.lookInput = controls.Main.Look.ReadValue<Vector2>() * 0.5f * 0.1f;
			}
			else
			{
				PlayerController.Instance.moveInput = Vector2.zero;
				PlayerController.Instance.lookInput = Vector2.zero;
			}
		}

		lockedNav.Input(controls.Main.UIController.ReadValue<float>());
	}

	public void Fire()
	{
		if (!GameManager.Instance.GameOver)
		{
			gun.TryFire();
		}
	}

	public void UISelect()
	{
		if (lockedNav.gameObject.activeInHierarchy)
		{
			lockedNav.SelectButton();
		}
	}

	public void InteractPressed()
	{
		if (GameManager.Instance && GameManager.Instance.AllowInput)
		{
			cameraInteractions.TryInteract();
		}
	}

	public void Pause()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.Pause();
		}
	}

	public void HandleRunning()
	{
		if (GameManager.Instance && GameManager.Instance.AllowInput)
		{
			PlayerController.Instance.isRunning = Running;//ctx.ReadValueAsButton();
		}
	}

}

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : HealthEntity
{
	PlayerCombatController _combatCtrl;
        JoystickController _joystick;
        Rigidbody _rigid;
	Animator _anim;

	InputAction _touchMove;
	InputAction _touch;

	[SerializeField] float _moveSpeed = 5.0f;

        Vector2 _touchStartPos;


	void Start()
        {
		_rigid = GetComponent<Rigidbody>();
		_anim = GetComponent<Animator>();
		_combatCtrl = GetComponent<PlayerCombatController>();

		_touch = InputManager.instance.Touch;
		_touchMove = InputManager.instance.TouchMove;
		_joystick = UIHandler.instance._joystick;

		_touch.performed += StartTouch;
		_touch.canceled += EndtTouch;
	} 
        
	private void FixedUpdate()
	{                 
		if (_touch.ReadValue<float>() != 0) 
                        OnMove();

	}

	void OnMove()
        {
		Vector2 touchPos = _touchMove.ReadValue<Vector2>();
		Vector2 dir = (touchPos - _touchStartPos).normalized;
		Vector3 movePos = _rigid.position + new Vector3(dir.x, 0.0f, dir.y) * Time.fixedDeltaTime * _moveSpeed;

		_rigid.MovePosition(movePos);
		_joystick.UpdateJoystick(_touchStartPos, touchPos); 
		
		if (!_combatCtrl._isAttacking) 
			transform.LookAt(movePos);
	}

	void StartTouch(InputAction.CallbackContext context)
	{
		_anim.SetBool("moving", true);
		_joystick.EnableJoystickUI();
		_touchStartPos = _touchMove.ReadValue<Vector2>();
	}
	void EndtTouch(InputAction.CallbackContext context)
	{
		_anim.SetBool("moving", false);
		_joystick.DisableJoystickUI();
	}

	public void LookAt(Vector3 target, float rotSpeed = 0.1f)
	{
		Quaternion targetRot = Quaternion.LookRotation((target - transform.position).normalized);

		Quaternion rot = Quaternion.Lerp(
			_rigid.rotation,
			targetRot,
			rotSpeed * Time.fixedDeltaTime
		);

		_rigid.MoveRotation(rot);
	}

	public override void OnDead()
	{
		throw new NotImplementedException();
	}

	//public void UpdateRate()
	//{
	//	if (_hpBar == null)
	//		return;
	//	_hpBar.UpdateRate((float)_hp / _initHp);
	//}
}
 
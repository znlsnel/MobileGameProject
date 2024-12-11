using System.Collections;
using UnityEngine;

public class TaskNavigatorUI : MonoBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	[SerializeField] GameObject _player;
	[SerializeField] Vector3 _offset;
	[SerializeField] float _zOffset;

	[Space(10)]
	[SerializeField] GameObject _ingredient;
	[SerializeField] GameObject _potionTable;
	[SerializeField] Counter _counter;
	[SerializeField] CoinSpawner _coinSpawner;
	[SerializeField] GameObject _dungeon;

	[Space(10)]
	[SerializeField] PickupManager _porterPickup;
	[SerializeField] PickupManager _potionTablePickup;

	[Space(10)]
	[SerializeField] RectTransform _rect;
	[SerializeField] RectTransform _characterUIRect;
	[SerializeField] GameObject _characterUI;
	[SerializeField] GameObject _screenUI;


	GameObject _target;
	// ��ᰡ ���ٸ� -> ����
	// ��Ḧ �� ��Ҵٸ� -> ���
	// ������ ���Դٸ� -> �������̺�
	// ī���Ϳ� ������ ���� ������ ���� ��� �ִٸ� -> ī����
	// ���� ���Դٸ� -> ���λ�����


	void Start()
	{
		StartCoroutine(UpdateCurTarget());
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Vector3 pos = _player.transform.position + _offset;
		transform.position = pos;

		pos = _characterUIRect.localPosition;
		pos.z = _zOffset;
		_characterUIRect.localPosition = pos;

		LookAt();
		UpdateRectUI();
	}

	// �켱����
	// 5. ����
	// 4. ���
	// 2. �������̺�
	// 1. ī����
	// 3. ����
	IEnumerator UpdateCurTarget()
	{
		while (true)
		{
			bool ingredient = _porterPickup.GetItemType() == EItemType.INGREDIENT_POTION &&
			_porterPickup.GetItemCount() > 0;

			// ��Ḧ ��� ���� �ʰ� ���̺� ������ ���ٸ�
			bool dungeon = !DungeonDoorway.instance.isPlayerInDungeon() && _porterPickup.GetItemCount() == 0 && _potionTablePickup.GetItemCount() == 0;

			// ���� ���̺� �������� ���Դٸ� 
			bool potionTable = _potionTablePickup.GetItemCount() > 0;

			// ������ ��� �ִٸ�?
			bool counter = _porterPickup.GetItemType() == EItemType.POTION;

			// ������ ���Դٸ� ?
			bool coin = _coinSpawner.GetCoinCnt() > 0;

			if (counter)
				_target = _counter.gameObject;
			else if (potionTable)
				_target = _potionTable;
			else if (coin)
				_target = _coinSpawner.gameObject;
			else if (ingredient)
				_target = _ingredient;
			else if (dungeon)
				_target = _dungeon;
			else
				_target = null;

			_characterUI.SetActive(_target != null && IsRectTransformInView(_rect) == false);
			SetUIActive(_target != null);

			yield return new WaitForSeconds(0.3f);
		}
	}
	public void LookAt(float speed = 720)
	{
		if (_target == null)
			return;

		Vector3 dir = _target.transform.position - transform.position;
		dir.y = 0.0f;
		Quaternion targetRotation = Quaternion.LookRotation(dir);

		// ���� ȸ������ ��ǥ ȸ������ �ε巴�� ȸ��
		transform.rotation = Quaternion.RotateTowards(
		    transform.rotation,
		    targetRotation,
		    speed * Time.deltaTime
		);
	}
	void SetUIActive(bool active)
	{
		if (_target != null && _screenUI.activeSelf == false && active) 
		{
			UpdateRectUI();
		}
		_screenUI.SetActive(active);

	}
	void UpdateRectUI()
	{
		if (_target == null)
			return;

		Vector2 targetPos = Camera.main.WorldToScreenPoint(_target.transform.position);
		_rect.localPosition = _rect.parent.InverseTransformPoint(targetPos);

	}

	bool IsRectTransformInView(RectTransform rect)
	{

		// ȭ�� ��踦 ����
		Vector2 screenMax = new Vector2(Screen.width, Screen.height)/2;
		Vector2 screenMin =  -screenMax; // (0, 0) 

		// ��� �ڳʰ� ȭ�� ��� ���� �ִ��� Ȯ��
		Debug.Log(_rect.localPosition);

		if (_rect.localPosition.x > screenMin.x && _rect.localPosition.x < screenMax.x &&
			_rect.localPosition.y > screenMin.y && _rect.localPosition.y <  screenMax.y)
		{
			return true; 
		} 
		

		return false; 
	}
}
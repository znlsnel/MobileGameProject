using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IItemReceiver
{
	public void ReceiveItem(GameObject item);
	public bool CheckItemType(EItemType type);
}

public class PickupManager : MonoBehaviour, IItemReceiver
{
	[SerializeField] Transform _handPos;
	[SerializeField] Transform _handEndPos;
	[SerializeField] EItemType _itemType;

	[SerializeField] int _maxCarrySizeX = 1;
	[SerializeField] int _maxCarrySizeZ = 1;

	[Space(10)]
	[SerializeField] float _xOffset = 0;
	[SerializeField] float _zOffset = 0;
	[SerializeField] float _yOffset = 0;

	[Space(10)]
	Stack<GameObject> _items = new Stack<GameObject>();

	[NonSerialized] public bool isReceivingItem = false;
	public int _maxCarrySize = 8;

	public bool destoryItem = false;
	public UnityAction _onGetItem;

	bool isEnable = true;

	public void SetActive(bool act)
	{
		isEnable = act;
	}

	public int _carryCap { get { return _maxCarrySize - _items.Count; } }
	public Stack<GameObject> GetItemStack() { return _items; }
	public int GetItemCount() { return _items.Count; }

	int _dirX = 1;
	int _dirZ = 1;

	private void Start()
	{
		if (_handEndPos != null)
		{
			Vector3 d = _handEndPos.position - _handPos.position;
			_dirX = d.x > 0 ? 1 : -1;
			_dirZ = d.z > 0 ? 1 : -1;
		} 
	}
	public void InitPickupManager(Stack<GameObject> stack)
	{
		_items = stack;
	}

	public bool CheckItemType(EItemType type)
	{
		if (_itemType != EItemType.ALL)
			return type == _itemType;

		else if (_items.Count == 0)
			return true;
		 
		return _items.Peek().GetComponent<Item>()._itemType == type;
	}

	public void ReceiveItem(GameObject go)
	{
		Item item = go.GetComponent<Item>();
		if (isEnable == false || _carryCap == 0 || !CheckItemType(item._itemType))
			return;

		isReceivingItem = true;

		go.GetComponent<IngredientItem>()?.OnHand();
		 
		go.transform.SetParent(null);
		go.transform.rotation = _handPos.rotation;
		Vector3 pos = Vector3.zero;

		Renderer renderer = _items.Count == 0 ? null : _items.Peek().GetComponent<Renderer>();
		if (renderer != null)
		{
			int yIdx = _items.Count / (_maxCarrySizeX * _maxCarrySizeZ);
			int zIdx = _items.Count % (_maxCarrySizeX * _maxCarrySizeZ) / _maxCarrySizeX;
			int xIdx = _items.Count % (_maxCarrySizeX * _maxCarrySizeZ) % _maxCarrySizeX;

			pos.y += yIdx * (renderer.bounds.size.y + _yOffset);
			pos.z +=_dirZ *  zIdx * (renderer.bounds.size.z + _zOffset);
			pos.x += _dirX * xIdx * (renderer.bounds.size.x + _xOffset);
		}

		go.transform.SetParent(_handPos);
		StartCoroutine(MoveInParabola(go, go.transform.position, pos));

		if (destoryItem == false)
			_items.Push(go);

	}

	float moveEndTime = 0.0f;

	IEnumerator MoveInParabola(GameObject go, Vector3 start, Vector3 offset, float height = 2.0f, float duration = 0.2f)
	{
		moveEndTime = Time.deltaTime + duration;
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			float t = elapsedTime / duration;
			Vector3 pos = _handPos.position;
			Vector3 currentPos = Vector3.Lerp(start, pos + offset, t);
			// 포물선 높이 추가
			currentPos.y += height * Mathf.Sin(Mathf.PI * t);

			go.transform.position = currentPos;

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		Vector3 end =  _handPos.position;
		go.transform.position = end + offset;
		_onGetItem?.Invoke();

		if (destoryItem)
			go.GetComponent<Item>()?.Relase(); 
		

		yield return new WaitForSeconds(duration);
		if (Time.time > moveEndTime)
			isReceivingItem = false;
	}

	public void ClearItem()
	{
		foreach (var item in _items)
			item.GetComponent<Item>()?.Relase();
		_items.Clear();
		
	}
}

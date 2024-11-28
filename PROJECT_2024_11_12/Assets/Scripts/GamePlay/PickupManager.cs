using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
	[SerializeField] Transform _handPos;

	[SerializeField] int _maxCarrySizeX = 1;
	[SerializeField] int _maxCarrySizeZ = 1;

	[Space(10)]
	[SerializeField] float _xOffset = 0;
	[SerializeField] float _zOffset = 0;
	[SerializeField] float _yOffset = 0;

	[Space(10)]
	Stack<GameObject> _items = new Stack<GameObject>();

	public int _maxCarrySize = 8;
	bool isReceivingItem = false;

	public int _carryCap { get { return _maxCarrySize - _items.Count; } }
	public Stack<GameObject> GetItemStack() { return _items; }

	public void InitPickupManager(Stack<GameObject> stack)
	{
		_items = stack;
	}

	public void PickUpItem(GameObject go)
	{
		if (_carryCap == 0)
			return; 

		isReceivingItem = true;
		 
		go.GetComponent<Item>()?.OnHand();

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
			pos.z -= zIdx * (renderer.bounds.size.z + _zOffset);
			pos.x += xIdx * (renderer.bounds.size.x + _xOffset);
		}

		go.transform.SetParent(_handPos);
		StartCoroutine(MoveInParabola(go, go.transform.position, pos));
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
			// ������ ���� �߰�
			currentPos.y += height * Mathf.Sin(Mathf.PI * t);

			go.transform.position = currentPos;

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		Vector3 end =  _handPos.position;
		go.transform.position = end + offset;

		yield return new WaitForSeconds(1.0f);
		if (Time.time > moveEndTime)
			isReceivingItem = false;
	}
}
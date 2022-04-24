using System;
using UnityEngine;

public class SummonBlueprintController : MonoBehaviour
{
	public bool ok;

	private new Transform transform;

	private void Awake()
	{
		transform = base.transform;
	}

	public void PushState(Vector3 position, Quaternion rotation, bool ok)
	{
		transform.position = position;
		transform.rotation = rotation;
		this.ok = ok;
	}
}

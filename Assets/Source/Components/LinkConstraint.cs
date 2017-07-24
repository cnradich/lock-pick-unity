using UnityEngine;


[DisallowMultipleComponent]
[ExecuteInEditMode]
public class LinkConstraint : MonoBehaviour
{

	[SerializeField]
	private Transform linkedTransform;

	[SerializeField]
	private bool linkPosition = true;

	[SerializeField]
	private bool linkRotation = true;

	//[SerializeField]
	//private bool linkScale = true;

	private Matrix4x4 linkedParentTRS;

	private Matrix4x4 worldTRS;

	private Matrix4x4 localTRS;

	private Matrix4x4 linkedTRS;

	private void Start()
	{
		UpdateLastTransform();
		linkedTRS = linkedParentTRS.inverse * worldTRS;
	}

	private void Update()
	{
		if(linkedTransform != null)
		{
			ApplyLinkedTransform();
		}
	}

	private void ApplyLinkedTransform()
	{
		//Matrix4x4 currentLocalTRS = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
		Matrix4x4 oldLocalTRS = localTRS;

		localTRS.SetTRS(transform.localPosition, transform.localRotation, transform.localScale);
		worldTRS = transform.localToWorldMatrix;
		linkedParentTRS = linkedTransform.localToWorldMatrix;

		if(oldLocalTRS != localTRS)
		{
			linkedTRS = linkedParentTRS.inverse * worldTRS;
		}


		Matrix4x4 newTRS = linkedParentTRS * linkedTRS;


		if(linkPosition)
		{
			transform.position = newTRS.GetColumn(3);
		}

		if(linkRotation)
		{
			transform.rotation = Quaternion.LookRotation(newTRS.GetColumn(2), newTRS.GetColumn(1));
		}

		// TODO figure out how unity handles scaling
		//if(linkScale)
		//{
		//	transform.localScale = new Vector3(newTRS.GetColumn(0).magnitude, newTRS.GetColumn(1).magnitude, newTRS.GetColumn(2).magnitude);
		//}

		localTRS.SetTRS(transform.localPosition, transform.localRotation, transform.localScale);

	}

	private void UpdateLastTransform()
	{
		worldTRS = transform.localToWorldMatrix;
		localTRS.SetTRS(transform.localPosition, transform.localRotation, transform.localScale);

		if(linkedTransform != null)
		{
			linkedParentTRS = linkedTransform.localToWorldMatrix;
		}
	}
}

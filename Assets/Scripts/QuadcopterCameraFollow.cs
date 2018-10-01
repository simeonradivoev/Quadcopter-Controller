using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterCameraFollow : MonoBehaviour {

	[SerializeField] private bool followQuadcopter;
	[SerializeField] private Vector3 cameraOffset;
	[SerializeField] private float distnace = 0.2f;
	[SerializeField] private float zoomSpeed = 1f;
	[SerializeField] private Vector2 zoomRange = new Vector2(0.2f,1f);
	[SerializeField] private Transform overviewTransform;
	private float yawDestination;
	private QuadcopterController controller;
	private QuadcopterPathController pathController;

	// Use this for initialization
	void Start ()
	{
		controller = FindObjectOfType<QuadcopterController>();
		pathController = FindObjectOfType<QuadcopterPathController>();
		if (controller == null)
		{
			Debug.LogError("Could not find quadcopter controller");
		}
		if (pathController == null)
		{
			Debug.LogError("Could not find quadcopter path controller");
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			if (followQuadcopter)
			{
				transform.position = overviewTransform.position;
				transform.rotation = overviewTransform.rotation;
				followQuadcopter = false;
			}
			else
			{
				followQuadcopter = true;
			}
		}

		if (followQuadcopter)
		{
			distnace += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
			distnace = Mathf.Clamp(distnace, zoomRange.x, zoomRange.y);

			if (controller != null && pathController != null)
			{
				if (pathController.FollowPath)
				{
					transform.position = controller.transform.position + (Quaternion.AngleAxis(yawDestination, Vector3.up) * cameraOffset.normalized * distnace);
				}
				else
				{
					transform.position = controller.transform.position + Quaternion.LookRotation(controller.DesiredDirection, Vector3.up) * cameraOffset.normalized * distnace;
				}

				transform.LookAt(controller.transform);

				if (Input.GetKey(KeyCode.A))
				{
					this.yawDestination += Time.deltaTime * controller.TurnSpeed;
				}

				if (Input.GetKey(KeyCode.D))
				{
					this.yawDestination -= Time.deltaTime * controller.TurnSpeed;
				}
			}
		}
	}
}

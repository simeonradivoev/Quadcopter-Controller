using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QuadcopterPathManager : MonoBehaviour {

	[SerializeField] private Camera camera;
	[SerializeField] private Transform targetGizmo;
	[SerializeField] private float maxNavMeshDitance = 2;
	[SerializeField] private float heightAdd = 0.4f;
	private QuadcopterPathController controller;

	private void OnGUI()
	{
		GUILayout.Space(6);
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.Label(new GUIContent("<b>Left Click</b> to place a waypoint."));
		GUILayout.Label(new GUIContent("Press '<b>P</b>' to cancel path."));
		GUILayout.Label(new GUIContent("Press '<b>T</b>' to change cameras."));
		GUILayout.Label(new GUIContent("<b>Scroll Wheel</b> to zoom in/out."));
		GUILayout.EndVertical();

		if (GUILayout.Button("Quit"))
		{
			Application.Quit();
		}

		if (controller != null && controller.FollowPath)
		{
			if (GUILayout.Button("Cancel Path"))
			{
				controller.SetFollowPath(false);
				targetGizmo.gameObject.SetActive(false);
			}
		}
	}

	// Use this for initialization
	void Start ()
	{
		controller = FindObjectOfType<QuadcopterPathController>();
		if (controller == null)
		{
			Debug.LogError("Could not find Quadcopter path controller");
		}
		else
		{
			controller.OnStopFollowingPath += OnStopFollwingPath;
		}
		targetGizmo.gameObject.SetActive(false);
	}

	void Update()
	{
		if (controller != null)
		{
			if (Input.GetMouseButtonDown(0))
			{
				Ray ray = camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit groundHitDistance;
				if (Physics.Raycast(ray, out groundHitDistance))
				{
					Vector3 groundHit = groundHitDistance.point;
					NavMeshHit navMeshHit;
					if (NavMesh.SamplePosition(groundHit, out navMeshHit, maxNavMeshDitance, -1))
					{
						Vector3 point = navMeshHit.position + Vector3.up * heightAdd;
						controller.SetWorldDestination(point);
						controller.SetFollowPath(true);
						targetGizmo.transform.position = point;
						targetGizmo.gameObject.SetActive(true);
					}
				}
			}else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.P))
			{
				controller.SetFollowPath(false);
			}
		}
	}

	private void OnStopFollwingPath()
	{
		targetGizmo.gameObject.SetActive(false);
	}
}

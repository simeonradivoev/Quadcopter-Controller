using System;
using UnityEngine;
using UnityEngine.AI;

public class QuadcopterPathController : MonoBehaviour
{
	[SerializeField] private bool followPath;
	[SerializeField] private Vector3 worldDestination;
	[SerializeField] private bool faceDestination = true;
	[SerializeField] private float maxSpeed = 1;
	[SerializeField] private int navMeshMask = -1;
	[SerializeField] private float pathCornerDistanceTreshold = 1;
	[SerializeField] private float pathLastCornerDistanceTreshold = 0.1f;
	[SerializeField] private float minPathCornerHeight = 0.5f;

	public event Action OnStopFollowingPath;
	private bool freeFlightDestination;
	private QuadcopterController controller;
	private NavMeshPath path;
	private int currentCornerIndex;

	// Use this for initialization
	void Start ()
	{
		controller = GetComponent<QuadcopterController>();
		controller.AddCalculateDesiredVelocityListener(CalculateDesiredWorldVelocity,0);
		path = new NavMeshPath();
	}

	private void CalculateDesiredWorldVelocity(QuadcopterController.VelocityContext context)
	{
		if (followPath)
		{
			if (freeFlightDestination)
			{
				Vector3 worldDifference = worldDestination - controller.Rigidbody.position;
				float distance = worldDifference.magnitude;
				if (distance < pathLastCornerDistanceTreshold)
				{
					StopPathFollowing();
				}
				context.DesiredWorldVelocity = Vector3.ClampMagnitude(worldDifference, maxSpeed);

				if (faceDestination)
				{
					context.DesiredDirection = new Vector3(worldDifference.x, 0, worldDifference.z).normalized;
				}
			}
			else if(path != null && path.status != NavMeshPathStatus.PathInvalid && currentCornerIndex < path.corners.Length)
			{
				var currentCorner = path.corners[currentCornerIndex] + Vector3.up * minPathCornerHeight;
				float treshold = currentCornerIndex < path.corners.Length - 1 ? pathCornerDistanceTreshold : pathLastCornerDistanceTreshold;
				float cornerDistance = Vector3.Distance(controller.Rigidbody.position, currentCorner);
				if (cornerDistance <= treshold)
				{
					currentCornerIndex++;
				}

				Vector3 worldDifference = currentCorner - controller.Rigidbody.position;
				context.DesiredWorldVelocity = Vector3.ClampMagnitude(worldDifference, maxSpeed);

				if (faceDestination)
				{
					context.DesiredDirection = new Vector3(worldDifference.x, 0, worldDifference.z).normalized;
				}
			}
			else
			{
				StopPathFollowing();
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (path != null && currentCornerIndex < path.corners.Length)
		{
			Gizmos.color = Color.blue;

			for (int i = 0; i < path.corners.Length-1; i++)
			{
				Gizmos.DrawLine(path.corners[i] + Vector3.up * minPathCornerHeight, path.corners[i+1] + Vector3.up * minPathCornerHeight);
			}
		}
	}

	private void StopPathFollowing()
	{
		followPath = false;
		if(OnStopFollowingPath != null) OnStopFollowingPath.Invoke();
	}

	public void SetFollowPath(bool followPath)
	{
		this.followPath = followPath;
		if (!followPath)
		{
			StopPathFollowing();
		}
	}

	public void SetWorldDestination(Vector3 destination)
	{
		worldDestination = destination;

		if (NavMesh.CalculatePath(controller.Rigidbody.position, destination, navMeshMask, path))
		{
			freeFlightDestination = path.status != NavMeshPathStatus.PathComplete;
			currentCornerIndex = 0;
		}
		else
		{
			currentCornerIndex = 0;
			freeFlightDestination = true;
		}
	}

	public bool FollowPath
	{
		get { return followPath; }
	}

	public Vector3 WorldDestination
	{
		get { return worldDestination; }
	}
}

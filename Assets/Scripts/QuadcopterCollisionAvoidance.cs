using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterCollisionAvoidance : MonoBehaviour
{
	[SerializeField] private float maxSeeAhead;
	[SerializeField] private LayerMask checkMask = -1;
	[SerializeField] private float maxAvoidForce = 1;
	[SerializeField] private float checkRadius = 0.1f;
	[SerializeField] private bool enableOnFreeFlight;
	private QuadcopterController controller;
	private QuadcopterPathController pathController;
	private Vector3 avoidanceForce;

	// Use this for initialization
	void Start ()
	{
		controller = GetComponent<QuadcopterController>();
		pathController = GetComponent<QuadcopterPathController>();
		controller.AddCalculateDesiredVelocityListener(CalculateDesiredWorldVelocity,1);
	}

	private void CalculateDesiredWorldVelocity(QuadcopterController.VelocityContext context)
	{
		if(!pathController.FollowPath && !enableOnFreeFlight) return;

		Vector3 ahead = controller.Rigidbody.velocity.normalized;
		if (Mathf.Approximately(ahead.sqrMagnitude,0))
		{
			ahead = controller.transform.forward;
		}
		RaycastHit hit;
		Ray ray = new Ray(controller.Rigidbody.position, ahead * maxSeeAhead);
		if (Physics.SphereCast(ray, checkRadius, out hit, maxSeeAhead, checkMask))
		{
			avoidanceForce = Vector3.ClampMagnitude(hit.normal * (maxSeeAhead / hit.distance) * maxAvoidForce * controller.Rigidbody.velocity.magnitude,maxAvoidForce);
			context.DesiredWorldVelocity += avoidanceForce;
		}
	}

	private void OnDrawGizmos()
	{
		if (controller != null)
		{
			Gizmos.DrawLine(controller.Rigidbody.position, controller.Rigidbody.position + avoidanceForce);
		}
	}
}

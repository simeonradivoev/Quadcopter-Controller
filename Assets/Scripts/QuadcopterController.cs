using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterController : MonoBehaviour
{
	[SerializeField] private Transform centerOfMass;
	[SerializeField] private Transform ForwardRightEngine;
	[SerializeField] private Transform ForwardLeftEngine;
	[SerializeField] private Transform BackRightEngine;
	[SerializeField] private Transform BackLeftEngine;
	[SerializeField] private float throttleSpeed = 10;
	[SerializeField] private float turnSpeed = 10;
	[SerializeField] private float sideSpeed = 10;
	[SerializeField] private float forwardSpeed = 10;
	[SerializeField] private bool realisticSideAdjustment;

	[SerializeField] private PID yPid = new PID(0,0,0);
	[SerializeField] private PID xPid = new PID(0,0,0);
	[SerializeField] private PID sidePid = new PID(0,0,0);

	[SerializeField] private PID yawPid = new PID(0,0,0);
	[SerializeField] private PID pitchPid = new PID(0,0,0);
	[SerializeField] private PID rollPid = new PID(0,0,0);

	[SerializeField] private float maxEngineSpeed = 100;

	private SortedList<int, Action<VelocityContext>> CalculateWorldDesiredVelocity = new SortedList<int, Action<VelocityContext>>();
	private float forwardAmount;
	private float sideAmount;
	private float throttle;
	private VelocityContext velocityContext = new VelocityContext();

	private Rigidbody rigidbody;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.centerOfMass = transform.InverseTransformPoint(centerOfMass.transform.position);
		velocityContext.DesiredDirection = Vector3.forward;
	}

	private void Update()
	{
		AddControlls();
	}

	private void FixedUpdate()
	{
		AddInternalForces();
	}

	private void AddControlls()
	{
		//yawDestination = 0;
		sideAmount = 0;
		forwardAmount = 0;
		throttle = 0;

		var updown = Input.GetAxis("UpDown");
		throttle += Time.deltaTime * throttleSpeed * updown;

		var horizontal = Input.GetAxis("Horizontal");
		var vertical = Input.GetAxis("Vertical");
		forwardAmount += Time.deltaTime * vertical * forwardSpeed;


		if (!Mathf.Approximately(horizontal,0))
		{
			velocityContext.DesiredDirection = Quaternion.AngleAxis(Time.deltaTime * horizontal * turnSpeed, Vector3.up) * velocityContext.DesiredDirection;
		}
		
	}

	public Vector3 CalculateWorldDesiredVelocityInternal()
	{
		return (velocityContext.DesiredDirection * forwardAmount) + Vector3.up * throttle;
	}

	private void AddInternalForces()
	{
		Quaternion inverseDesiredLookRotation = Quaternion.Inverse(Quaternion.LookRotation(velocityContext.DesiredDirection, -Physics.gravity.normalized));

		Vector3 localVelocity = inverseDesiredLookRotation * rigidbody.velocity;
		velocityContext.DesiredWorldVelocity = CalculateWorldDesiredVelocityInternal();
		foreach (var listener in CalculateWorldDesiredVelocity)
		{
			listener.Value.Invoke(velocityContext);
		}

		Vector3 localDesiredVelocity = inverseDesiredLookRotation * velocityContext.DesiredWorldVelocity;

		float newX = Mathf.Clamp((float)xPid.Update(localDesiredVelocity.x, localVelocity.x, Time.deltaTime), -maxEngineSpeed, maxEngineSpeed);
		float newZ = Mathf.Clamp((float)sidePid.Update(localDesiredVelocity.z, localVelocity.z, Time.deltaTime), -maxEngineSpeed, maxEngineSpeed);

		float pitchError = GetPitchError();
		float rollError = GetRollError();
		float yawError = GetYawError();

		float newXRot = (float)pitchPid.Update(0, pitchError, Time.deltaTime);
		float newYRot = (float)yawPid.Update(0, yawError, Time.deltaTime);
		float newZRot = (float)rollPid.Update(0, rollError, Time.deltaTime);

		if(!realisticSideAdjustment)
			rigidbody.AddRelativeForce(new Vector3(newX, 0, 0), ForceMode.VelocityChange);

		rigidbody.AddTorque(new Vector3(newXRot, newYRot, newZRot), ForceMode.VelocityChange);

		float newZAbs = Mathf.Abs(newZ);
		if (newZ > 0)
		{
			AddEngineForce(BackLeftEngine, newZAbs);
			AddEngineForce(BackRightEngine, newZAbs);
		}
		else if(newZ < 0)
		{
			AddEngineForce(ForwardLeftEngine, newZAbs);
			AddEngineForce(ForwardRightEngine, newZAbs);
		}

		if (realisticSideAdjustment)
		{
			if (newX > 0)
			{
				AddEngineForce(BackRightEngine, -newX);
				AddEngineForce(ForwardRightEngine, -newX);
			}
			else if (newX < 0)
			{
				AddEngineForce(BackLeftEngine, newX);
				AddEngineForce(ForwardLeftEngine, newX);
			}
		}


		float newY = Mathf.Clamp((float)yPid.Update(localDesiredVelocity.y, localVelocity.y, Time.deltaTime), -maxEngineSpeed, maxEngineSpeed);

		AddEngineForce(BackLeftEngine, newY);
		AddEngineForce(ForwardLeftEngine, newY);
		AddEngineForce(BackRightEngine, newY);
		AddEngineForce(ForwardRightEngine, newY);

		/*float newXRotAbs = Mathf.Abs(newXRot);
		if (newXRot < 0)
		{
			AddEngineForce(ForwardRightEngine, newXRotAbs);
			AddEngineForce(BackRightEngine, newXRotAbs);
		}
		else if (newXRot > 0)
		{
			AddEngineForce(ForwardLeftEngine, newXRotAbs);
			AddEngineForce(BackLeftEngine, newXRotAbs);
		}*/
	}

	private void AddEngineForce(Transform engine, float force)
	{
		rigidbody.AddForceAtPosition(engine.TransformDirection(Vector3.up* force), engine.position,ForceMode.VelocityChange);
		rigidbody.AddForceAtPosition(engine.TransformDirection(Vector3.up* force), engine.position, ForceMode.VelocityChange);
	}

	//Pitch is rotation around x-axis
	//Returns positive if pitching forward
	private float GetPitchError()
	{
		float getXAngle = AngleSigned(transform.up, Vector3.up, Vector3.left);
		return getXAngle;
	}

	//Roll is rotation around z-axis
	//Returns positive if rolling left
	private float GetRollError()
	{
		float getZAngle = AngleSigned(transform.up, Vector3.up, Vector3.back);
		return getZAngle;
	}

	//Roll is rotation around y-axis
	//Returns positive if rolling left
	private float GetYawError()
	{
		float getYAngle = AngleSigned(transform.forward, velocityContext.DesiredDirection, Vector3.down);
		return getYAngle;
	}

	public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
	{
		return Mathf.Atan2(
			Vector3.Dot(n, Vector3.Cross(v1, v2)),
			Vector3.Dot(v1, v2));
	}

	public void AddCalculateDesiredVelocityListener(Action<VelocityContext> action, int priority)
	{
		if (!CalculateWorldDesiredVelocity.ContainsValue(action))
		{
			CalculateWorldDesiredVelocity.Add(priority,action);
		}
		else
		{
			Debug.Log("Listener present");
		}
	}

	public void RemoveCalculateDesiredVelocityListener(Action<VelocityContext> action)
	{
		CalculateWorldDesiredVelocity.RemoveAt(CalculateWorldDesiredVelocity.IndexOfValue(action));
	}

	[Serializable]
	public class VelocityContext
	{
		private Vector3 desiredDirection;

		public Vector3 DesiredWorldVelocity { get; set; }

		public Vector3 DesiredDirection
		{
			get
			{
				if (desiredDirection == Vector3.zero)
				{
					return Vector3.forward;
				}
				return desiredDirection;
			}
			set
			{
				desiredDirection = value;
			}
		}
	}

	/*private class VelcoityContextListener : IComparable<VelcoityContextListener> 
	{
		int priority;
		internal  action;

		public VelcoityContextListener(Action<VelocityContext> action, int priority)
		{
			this.action = action;
			this.priority = priority;
		}

		public int CompareTo(VelcoityContextListener other)
		{
			return priority.CompareTo(other.priority);
		}

		public void Invoke(VelocityContext context)
		{
			action.Invoke(context);
		}
	}*/

	public Rigidbody Rigidbody
	{
		get { return rigidbody; }
	}

	public float TurnSpeed
	{
		get { return turnSpeed; }
	}

	public Vector3 DesiredDirection { get { return velocityContext.DesiredDirection; } }
}

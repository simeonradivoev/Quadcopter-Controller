using UnityEngine;

[System.Serializable]
public class PID
{
	public double pFactor, iFactor, dFactor;

	double integral;
	double lastError;

	public PID(Vector3 factors)
	{
		this.pFactor = factors.x;
		this.iFactor = factors.y;
		this.dFactor = factors.z;
	}

	public PID(double pFactor, double iFactor, double dFactor)
	{
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
	}


	public double Update(double setpoint, double actual, double timeFrame)
	{
		double present = setpoint - actual;
		integral += present * timeFrame;
		double deriv = (present - lastError) / timeFrame;
		lastError = present;
		return present * pFactor + integral * iFactor + deriv * dFactor;
	}
}

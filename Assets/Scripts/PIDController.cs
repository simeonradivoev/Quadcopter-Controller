using UnityEngine;
using System.Collections;

public class PIDController
{
	float error_old = 0f;
	//The controller will be more robust if you are using a further back sample
	float error_old_2 = 0f;
	float error_sum = 0f;
	//If we want to average an error as input
	float error_sum2 = 0f;

	//PID parameters
	public float gain_P = 0f;
	public float gain_I = 0f;
	public float gain_D = 0f;
	//Sometimes you have to limit the total sum of all errors used in the I
	private float error_sumMax = 20f;

	public float GetFactorFromPIDController(float error)
	{
		float output = CalculatePIDOutput(error);

		return output;
	}

	//Use this when experimenting with PID parameters
	public float GetFactorFromPIDController(float gain_P, float gain_I, float gain_D, float error)
	{
		this.gain_P = gain_P;
		this.gain_I = gain_I;
		this.gain_D = gain_D;

		float output = CalculatePIDOutput(error);

		return output;
	}

	//Use this when experimenting with PID parameters and the gains are stored in a Vector3
	public float GetFactorFromPIDController(Vector3 gains, float error)
	{
		this.gain_P = gains.x;
		this.gain_I = gains.y;
		this.gain_D = gains.z;

		float output = CalculatePIDOutput(error);

		return output;
	}

	private float CalculatePIDOutput(float error)
	{
		//The output from PID
		float output = 0f;


		//P
		output += gain_P * error;


		//I
		error_sum += Time.fixedDeltaTime * error;

		//Clamp the sum 
		this.error_sum = Mathf.Clamp(error_sum, -error_sumMax, error_sumMax);

		//Sometimes better to just sum the last errors
		//float averageAmount = 20f;

		//CTE_sum = CTE_sum + ((CTE - CTE_sum) / averageAmount);

		output += gain_I * error_sum;


		//D
		float d_dt_error = (error - error_old) / Time.fixedDeltaTime;

		//Save the last errors
		this.error_old_2 = error_old;

		this.error_old = error;

		output += gain_D * d_dt_error;


		return output;
	}
}
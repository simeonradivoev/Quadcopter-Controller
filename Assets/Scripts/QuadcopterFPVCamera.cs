using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterFPVCamera : MonoBehaviour
{
	[SerializeField] private Camera camera;
	[SerializeField] private int cameraFpsLimit;
	[SerializeField] private int cameraWidth;
	[SerializeField] private int cameraHeight;
	[SerializeField] private bool render = true;
	private float fpsTimer;
	private RenderTexture renderTexture;

	private void OnGUI()
	{
		GUI.DrawTexture(new Rect(0,Screen.height - cameraHeight, cameraWidth, cameraHeight), renderTexture);
	}

	// Use this for initialization
	void Start ()
	{
		renderTexture = new RenderTexture(cameraWidth, cameraHeight,0,RenderTextureFormat.RGB565);
		camera.targetTexture = renderTexture;
		camera.enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (render)
		{
			if (cameraFpsLimit > 0)
			{
				fpsTimer += Time.deltaTime;
				float timeFrame = 1f / cameraFpsLimit;

				if (fpsTimer >= timeFrame)
				{
					fpsTimer = 0;
					camera.Render();
				}
			}
			else
			{
				camera.Render();
			}
		}
	}

	private void OnDestroy()
	{
		renderTexture.Release();
	}
}

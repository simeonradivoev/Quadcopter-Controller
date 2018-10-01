using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Assets.Scripts
{
	[RequireComponent(typeof(Camera))]
	public class GlitchImageEffect : MonoBehaviour
	{
		/// Provides a shader property that is set in the inspector
		/// and a material instantiated from the shader
		[SerializeField] private Shader shader;
		[SerializeField,Range(0,1)] private float GlitchAmount = 0.1f;
		private Material m_Material;
		private int glitchAmountIndex;

		protected virtual void Start()
		{
			// Disable if we don't support image effects
			if (!SystemInfo.supportsImageEffects)
			{
				enabled = false;
				return;
			}

			// Disable the image effect if the shader can't
			// run on the users graphics card
			if (!shader || !shader.isSupported)
				enabled = false;

			glitchAmountIndex = Shader.PropertyToID("_GlitchAmount");
		}

		private void Update()
		{
			if (m_Material != null)
			{
				m_Material.SetFloat(glitchAmountIndex, GlitchAmount);
			}
		}

		public void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source,destination,material);
		}

		protected Material material
		{
			get
			{
				if (m_Material == null)
				{
					m_Material = new Material(shader);
					m_Material.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_Material;
			}
		}


		protected virtual void OnDisable()
		{
			if (m_Material)
			{
				DestroyImmediate(m_Material);
			}
		}
	}
}
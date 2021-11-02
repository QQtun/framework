using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace HighlightingSystem
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public class HighlightingBase : MonoBehaviour
	{
		#region Static Fields and Constants
		static protected readonly Color colorClear = new Color(0f, 0f, 0f, 0f);
		static protected readonly string renderBufferName = "HighlightingSystem";
		protected const CameraEvent queue = CameraEvent.BeforeImageEffectsOpaque;
	
		#endregion
		
		#region Public Fields
		public int _downsampleFactor = 2;
		[HideInInspector]
        public RenderTexture effectRT
        {
            get
            {
                return highlightingBuffer;
            }
        }

		public bool needDistortion
        {
            get
            {
                return ( _nRenderMode & (1 << (int)Highlighter.Mode.Distortion) ) != 0;
            }
        }

		#endregion
		
		#region Protected Fields
		protected CommandBuffer renderBuffer;

		protected int cachedWidth = -1;
		protected int cachedHeight = -1;
        
		// RenderTargetidentifier for the highlightingBuffer RenderTexture
		protected RenderTargetIdentifier highlightingBufferID;

		// RenderTexture with highlighting buffer
		protected RenderTexture highlightingBuffer = null;

		// Camera reference
		protected Camera cam = null;

//		protected bool _needDistortion = false;

        public static int _nRenderMode = 0;

        static protected bool initialized = false;

		static protected HashSet<Camera> cameras = new HashSet<Camera>();

        /// <summary>
        /// 用于设置里面开启和关闭
        /// </summary>
        public static List<HighlightingBase> highlightingList = new List<HighlightingBase>(); 
		#endregion

		private RenderTextureFormat[] FORMAT_LIST = { RenderTextureFormat.ARGBHalf, RenderTextureFormat.ARGB2101010, RenderTextureFormat.ARGB32 };
    	private RenderTextureFormat rtFormat = RenderTextureFormat.ARGB32;
    	RenderTextureFormat GetVisibleFormat()
    	{
        	for (int i = 0; i < FORMAT_LIST.Length; i++)
        	{
            	bool s = SystemInfo.SupportsRenderTextureFormat(FORMAT_LIST[i]);
            	if (s == true)
                	return FORMAT_LIST[i];
        	}
        	return RenderTextureFormat.ARGB32;
    	}
		#region MonoBehaviour


        void Awake()
        {
            highlightingList.Add(this);
			rtFormat = GetVisibleFormat();
        }
        void OnDestroy()
        {
            highlightingList.Remove(this);
        }
		
        protected virtual void OnEnable()
		{
			Initialize();

            renderBuffer = new CommandBuffer();
			renderBuffer.name = renderBufferName;

			cam = GetComponent<Camera>();

			cameras.Add(cam);

			cam.AddCommandBuffer(queue, renderBuffer);

		}
		
		// 
		protected virtual void OnDisable()
		{
			cameras.Remove(cam);
			if (renderBuffer != null)
			{
				cam.RemoveCommandBuffer(queue, renderBuffer);
				renderBuffer = null;
			}

			if (highlightingBuffer != null && highlightingBuffer.IsCreated())
			{
				highlightingBuffer.Release();
				highlightingBuffer = null;
			}

		}

		// 
		protected virtual void OnPreRender()
		{
			bool updateHighlightingBuffer = false;
			
			updateHighlightingBuffer |= (highlightingBuffer == null || cam.pixelWidth != cachedWidth || cam.pixelHeight != cachedHeight );

            if (updateHighlightingBuffer)
            {
                if (highlightingBuffer != null && highlightingBuffer.IsCreated())
                {
                    highlightingBuffer.Release();
                }

                cachedWidth = cam.pixelWidth;
                cachedHeight = cam.pixelHeight;

                highlightingBuffer = new RenderTexture(cachedWidth / _downsampleFactor, cachedHeight / _downsampleFactor, 16, rtFormat, RenderTextureReadWrite.Default);
                highlightingBuffer.filterMode = FilterMode.Point;
                highlightingBuffer.useMipMap = false;
                highlightingBuffer.wrapMode = TextureWrapMode.Clamp;
                if (!highlightingBuffer.Create())
                {
                    Debug.LogError("HighlightingSystem : UpdateHighlightingBuffer() : Failed to create highlightingBuffer RenderTexture!");
                }

                highlightingBufferID = new RenderTargetIdentifier(highlightingBuffer);
            }

            RebuildCommandBuffer();
        }

		#endregion

		#region Internal
		// 
		static public bool IsHighlightingCamera(Camera cam)
		{
			return cameras.Contains(cam);
		}

		// 
		static protected void Initialize()
		{
			if (initialized) { return; }

			// Initialize shader property constants
			ShaderPropertyID.Initialize();

			initialized = true;
		}


	
		// 
		protected virtual void RebuildCommandBuffer()
		{
            Highlighter.FillRenderers(renderBuffer);
            if (Highlighter._renderMode > 0)
            {
                renderBuffer.Clear();
                // Prepare and clear render target
                renderBuffer.SetRenderTarget(highlightingBufferID, highlightingBufferID);
                renderBuffer.ClearRenderTarget(true, true, colorClear);

                // biuld;
                // Fill buffer with highlighters rendering commands
                Highlighter.FillBuffer(renderBuffer);
            }
            Highlighter._renderMode = HighlightingBase._nRenderMode;
		}

		#endregion
	}
}
using UnityEngine;
using UnityEngine.Profiling;

namespace UnityExtensions
{
    //https://github.com/Unity-Technologies/BoatAttack/blob/e4864ca4381d59e553fe43f3dac6a12500eee8c7/Assets/Scripts/System/MiniProfiler.cs
    #region UnityEngine.Experimental.Rendering
    public class MiniProfiler : MonoBehaviour
    {
        public bool enable;
        private const float AverageStatDuration = 1.0f; // stats refresh each second
        private int _frameCount;
		private float _accDeltaTime;
        private string _statsLabel;
        private GUIStyle _style;

        private readonly float[] _frameTimes = new float[5000];
        private int _totalFrames;
        private float _minFrameTime = 1000f;
        private float _maxFrameTime;

        class RecorderEntry
        {
            public string Name;
            public int CallCount;
            public float AccTime;
            public Recorder Recorder;
        };

		enum Markers
		{
			RenderLoop,
			Culling,
            Shadows,
            Draw,
            Post,
		}

        readonly RecorderEntry[] _recordersList =
        {
			// Warning: Keep this list in the exact same order as the Markers enum
            new RecorderEntry { Name="UnityEngine.CoreModule.dll!UnityEngine.Rendering::RenderPipelineManager.DoRenderLoop_Internal()" },
            new RecorderEntry { Name="CullScriptable" },
            new RecorderEntry { Name="Shadows.ExecuteDrawShadows" },
            new RecorderEntry { Name="RenderLoop.ScheduleDraw" },
            new RecorderEntry { Name="Render PostProcessing Effects" },
        };

        void Awake()
        {
            for (int i = 0; i < _recordersList.Length; i++)
            {
                var sampler = Sampler.Get(_recordersList[i].Name);
                if (sampler.isValid)
                    _recordersList[i].Recorder = sampler.GetRecorder();
            }

            _style = new GUIStyle();
            _style.fontSize = 30;
            _style.normal.textColor = Color.white;

            ResetStats();
        }

        void RazCounters()
        {
            _accDeltaTime = 0.0f;
            _frameCount = 0;
            for (int i = 0; i < _recordersList.Length; i++)
            {
                _recordersList[i].AccTime = 0.0f;
                _recordersList[i].CallCount = 0;
            }
        }

        void ResetStats()
        {
             _statsLabel = "Gathering data...";
             RazCounters();
        }

        void Update()
        {
            if (enable)
            {
                _accDeltaTime += Time.unscaledDeltaTime;
                _frameCount++;

                _frameTimes[(int)Mathf.Repeat(_totalFrames, 5000)] = Time.unscaledDeltaTime;

                int frameFactor = Mathf.Clamp(_totalFrames, 0, 5000);

                float averageFrameTime = 0f;
                
                for (int i = 0; i < frameFactor; i++)
                {
                    averageFrameTime += _frameTimes[i];
                }

                if (_frameCount > 10)
                {
                    _minFrameTime = Time.unscaledDeltaTime < _minFrameTime ? Time.unscaledDeltaTime : _minFrameTime;
                    _maxFrameTime = Time.unscaledDeltaTime > _maxFrameTime ? Time.unscaledDeltaTime : _maxFrameTime;
                }

                // get timing & update average accumulators
                for (int i = 0; i < _recordersList.Length; i++)
                {
                    if (_recordersList[i].Recorder != null)
                    {
                        _recordersList[i].AccTime += _recordersList[i].Recorder.elapsedNanoseconds / 1000000.0f; // acc time in ms
                        _recordersList[i].CallCount += _recordersList[i].Recorder.sampleBlockCount;
                    }
                }

				if (_accDeltaTime >= AverageStatDuration)
				{
					float ooFrameCount = 1.0f / _frameCount;
					float avgLoop = _recordersList[(int)Markers.RenderLoop].AccTime * ooFrameCount;
					float avgCulling = _recordersList[(int)Markers.Culling].AccTime * ooFrameCount;
					float avgShadow = _recordersList[(int)Markers.Shadows].AccTime * ooFrameCount;
					float avgDraw = _recordersList[(int)Markers.Draw].AccTime * ooFrameCount;
					float avgPost = _recordersList[(int)Markers.Post].AccTime * ooFrameCount;

                    var sb = StringBuilderPool.Get();
                    sb.Append("Rendering Loop Main Thread:\t").Append((int)avgLoop).AppendLine("ms");
                    sb.Append("\tCulling:\t\t").Append((int)avgCulling).AppendLine("ms");
                    sb.Append("\tShadows:\t").Append((int)avgShadow).AppendLine("ms");
                    sb.Append("\tDraws:\t\t").AppendFormat("{0:F2}", avgDraw).AppendLine("ms");
                    sb.Append("\tPostProcessing:\t").AppendFormat("{0:F2}", avgPost).AppendLine("ms");
                    sb.Append("Total:\t\t").AppendFormat("{0:F2}", _accDeltaTime * 1000.0f * ooFrameCount)
                        .Append("ms\t(").Append((int)(_frameCount / _accDeltaTime)).AppendLine(" FPS)");
                    
                    sb.Append("Average:\t\t").AppendFormat("{0:F2}", averageFrameTime * 1000f / frameFactor)
                        .Append("ms\t(").Append((int)(frameFactor / averageFrameTime)).AppendLine(" FPS)");
                    sb.Append("Minimum:\t").AppendFormat("{0:F2}", _minFrameTime * 1000f).Append("ms\t(")
                        .Append((int)(1 / _minFrameTime)).AppendLine(" FPS)");
                    sb.Append("Maximum:\t").AppendFormat("{0:F2}", _maxFrameTime * 1000f).Append("ms\t(")
                        .Append((int)(1 / _maxFrameTime)).AppendLine(" FPS)");
                    
                    _statsLabel = sb.ToString();
                    StringBuilderPool.Release(sb);
                    
					RazCounters();
				}
            }

            _totalFrames++;
        }

        void OnGUI()
        {
            if (enable)
            {
                // GUI.skin.label.fontSize = 15;
                GUI.color = Color.white;
                const float w = 356 * 2, h = 356;
                GUILayout.BeginArea(new Rect(32, 50, w, h), "Profiler", GUI.skin.window);
                GUILayout.Label(_statsLabel, _style);
                GUILayout.EndArea();
            }
        }
    }
    #endregion // UnityEngine.Experimental.Rendering
}
using System;
using System.Linq;
using UnityEngine.LowLevel;

namespace PKGE
{
    internal static class RuntimeApplication
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Entities/RuntimeApplication.cs
        #region Unity.Entities
        /// <summary>
        /// Event invoked before a frame update.
        /// </summary>
        public static event Action PreFrameUpdate;

        /// <summary>
        /// Event invoked after a frame update.
        /// </summary>
        public static event Action PostFrameUpdate;

        internal static void InvokePreFrameUpdate() => PreFrameUpdate?.Invoke();
        internal static void InvokePostFrameUpdate() => PostFrameUpdate?.Invoke();

        sealed class UpdatePreFrame { }
        sealed class UpdatePostFrame { }

        internal static void RegisterFrameUpdateToCurrentPlayerLoop()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

#if USING_LINQ
            var playerLoopSystems = playerLoop.subSystemList.ToList();
#else
            bool updatePreFrame = false;
            bool updatePostFrame = false;

            using var _0 = playerLoop.subSystemList.ToListPooled(out var playerLoopSystems);

            for (int i = 0, playerLoopSystemsCount = playerLoopSystems.Count;
                i < playerLoopSystemsCount && !updatePreFrame && !updatePostFrame;
                i++)
            {
                if (playerLoopSystems[i].type == typeof(UpdatePreFrame))
                {
                    updatePreFrame = true;
                }
                
                if (playerLoopSystems[i].type == typeof(UpdatePostFrame))
                {
                    updatePostFrame = true;
                }
            }
#endif // USING_LINQ

#if USING_LINQ
            if (!playerLoopSystems.Any(s => s.type == typeof(UpdatePreFrame)))
#else
            if (!updatePreFrame)
#endif // USING_LINQ
            {
                playerLoopSystems.Insert(0, new PlayerLoopSystem
                {
                    type = typeof(UpdatePreFrame),
                    updateDelegate = InvokePreFrameUpdate
                });
            }

#if USING_LINQ
            if (!playerLoopSystems.Any(s => s.type == typeof(UpdatePostFrame)))
#else
            if (!updatePostFrame)
#endif // USING_LINQ
            {
                playerLoopSystems.Add(new PlayerLoopSystem
                {
                    type = typeof(UpdatePostFrame),
                    updateDelegate = InvokePostFrameUpdate
                });
            }

            playerLoop.subSystemList = playerLoopSystems.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        internal static void UnregisterFrameUpdateToCurrentPlayerLoop()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
#if USING_LINQ
            var playerLoopSystems = playerLoop.subSystemList.ToList();
            playerLoopSystems.RemoveAll(s => s.type == typeof(UpdatePreFrame));
            playerLoopSystems.RemoveAll(s => s.type == typeof(UpdatePostFrame));
#else
            using var _0 = UnityEngine.Pool.ListPool<PlayerLoopSystem>.Get(out var playerLoopSystems);
            playerLoopSystems.EnsureCapacity(playerLoop.subSystemList.Length);
            foreach (var subSystem in playerLoop.subSystemList.AsSpan())
            {
                if (subSystem.type != typeof(UpdatePreFrame)
                    && subSystem.type != typeof(UpdatePostFrame))
                {
                    playerLoopSystems.Add(subSystem);
                }
            }
#endif // USING_LINQ
            playerLoop.subSystemList = playerLoopSystems.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
        }
        #endregion // Unity.Entities
    }
}

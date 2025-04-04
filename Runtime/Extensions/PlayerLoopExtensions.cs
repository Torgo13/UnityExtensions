using System;
using UnityEngine.LowLevel;

namespace UnityExtensions
{
    /// <summary>
    /// A class that contains extension methods used to modify the player loop.
    /// </summary>
    public static class PlayerLoopExtensions
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/PlayerLoopExtensions.cs
        #region Unity.LiveCapture
        /// <summary>
        /// Adds an update callback to the specified subsystem.
        /// </summary>
        /// <remarks>
        /// The subsystem is added to the player loop if it does not already exist.
        /// </remarks>
        /// <param name="update">The update callback to register.</param>
        /// <param name="index">The index to insert the subsystem if it does not already exist. By default, the subsystem is
        /// appended to the end of the subsystem list of the given system.</param>
        /// <returns><see langword="true"/> if the update callback was successfully registered; <see langword="false"/>
        /// if <typeparamref name="TSystem"/> could not be found in the player loop.</returns>
        /// <typeparam name="TSystem">The system to add the subsystem to.</typeparam>
        /// <typeparam name="TSubSystem">The system to add the <paramref name="update"/> callback to.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="update"/> is null.</exception>
        public static bool RegisterUpdate<TSystem, TSubSystem>(PlayerLoopSystem.UpdateFunction update, int index = -1)
        {
            if (update == null)
                throw new ArgumentNullException(nameof(update));

            var loop = PlayerLoop.GetCurrentPlayerLoop();

            if (!loop.TryFindSubSystem<TSystem>(out var system))
            {
                return false;
            }

            if (system.TryFindSubSystem<TSubSystem>(out var subSystem))
            {
                // ensure we do not call the update method multiple times if already registered
                subSystem.updateDelegate -= update;
                subSystem.updateDelegate += update;
                system.TryUpdate(subSystem);
            }
            else
            {
                // if the index is invalid append the subsystem
                var subSystems = system.subSystemList;
                var subSystemCount = subSystems != null ? subSystems.Length : 0;

                if (index < 0 || index > subSystemCount)
                    index = subSystemCount;

                system.AddSubSystem<TSubSystem>(index, update);
            }

            if (!loop.TryUpdate(system))
            {
                return false;
            }

            PlayerLoop.SetPlayerLoop(loop);
            return true;
        }

        /// <summary>
        /// Removes an update callback from the specified subsystem.
        /// </summary>
        /// <param name="update">The update callback to deregister.</param>
        /// <typeparam name="TSubSystem">The system to remove the <paramref name="update"/> callback from.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="update"/> is null.</exception>
        public static void DeregisterUpdate<TSubSystem>(PlayerLoopSystem.UpdateFunction update)
        {
            if (update == null)
                throw new ArgumentNullException(nameof(update));

            var loop = PlayerLoop.GetCurrentPlayerLoop();

            if (!loop.TryFindSubSystem<TSubSystem>(out var subSystem))
            {
                return;
            }

            subSystem.updateDelegate -= update;

            if (!loop.TryUpdate(subSystem))
            {
                return;
            }

            PlayerLoop.SetPlayerLoop(loop);
        }

        /// <summary>
        /// Recursively finds a subsystem of this system by type.
        /// </summary>
        /// <typeparam name="T">The type of subsystem to find.</typeparam>
        /// <param name="system">The system to search.</param>
        /// <param name="result">The returned subsystem.</param>
        /// <returns>True if a subsystem with a matching type was found; otherwise, false.</returns>
        public static bool TryFindSubSystem<T>(this PlayerLoopSystem system, out PlayerLoopSystem result)
        {
            return system.TryFindSubSystem(typeof(T), out result);
        }

        /// <summary>
        /// Recursively finds a subsystem of this system by type.
        /// </summary>
        /// <param name="system">The system to search.</param>
        /// <param name="subsystemType">The type of subsystem to find.</param>
        /// <param name="result">The returned subsystem.</param>
        /// <returns>True if a subsystem with a matching type was found; otherwise, false.</returns>
        public static bool TryFindSubSystem(this PlayerLoopSystem system, Type subsystemType, out PlayerLoopSystem result)
        {
            if (system.type == subsystemType)
            {
                result = system;
                return true;
            }

            if (subsystemType != null && system.subSystemList != null)
            {
                foreach (var subSystem in system.subSystemList)
                {
                    if (subSystem.TryFindSubSystem(subsystemType, out result))
                    {
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Applies changes made to a subsystem to a system.
        /// </summary>
        /// <param name="system">The system to update.</param>
        /// <param name="subSystemToUpdate">The modified subsystem.</param>
        /// <returns>True if the subsystem was successfully updated; otherwise, false.</returns>
        public static bool TryUpdate(this ref PlayerLoopSystem system, PlayerLoopSystem subSystemToUpdate)
        {
            if (system.type == subSystemToUpdate.type)
            {
                system = subSystemToUpdate;
                return true;
            }

            if (system.subSystemList != null)
            {
                for (var i = 0; i < system.subSystemList.Length; i++)
                {
                    if (system.subSystemList[i].TryUpdate(subSystemToUpdate))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a new subsystem to a system.
        /// </summary>
        /// <typeparam name="T">The type of the subsystem to add.</typeparam>
        /// <param name="system">The system to add the subsystem to.</param>
        /// <param name="index">The index of the subsystem in the subsystem array.</param>
        /// <param name="update">The function called to update the new subsystem.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than zero
        /// or larger than the subsystem array length.</exception>
        public static void AddSubSystem<T>(this ref PlayerLoopSystem system, int index, PlayerLoopSystem.UpdateFunction update)
        {
            var subSystems = system.subSystemList;
            var oldLength = subSystems != null ? subSystems.Length : 0;

            if (index < 0 || index > oldLength)
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    "Must be non-negative value no larger than subsystem array length.");

            var newSubSystems = new PlayerLoopSystem[oldLength + 1];

            for (var i = 0; i < oldLength; i++)
            {
                if (i < index)
                {
                    newSubSystems[i] = subSystems[i];
                }
                else if (i >= index)
                {
                    newSubSystems[i + 1] = subSystems[i];
                }
            }

            newSubSystems[index] = new PlayerLoopSystem
            {
                type = typeof(T),
                updateDelegate = update,
            };

            system.subSystemList = newSubSystems;
        }

        /// <summary>
        /// Finds the index of the subsystem in the list of subsystems of the provided system.
        /// </summary>
        /// <typeparam name="T">The type of the subsystem to search for.</typeparam>
        /// <param name="system">The system to use for the search.</param>
        /// <returns>The index of the subsystem if found; otherwise, -1.</returns>
        public static int IndexOf<T>(this ref PlayerLoopSystem system)
        {
            return system.IndexOf(typeof(T));
        }

        /// <summary>
        /// Finds the index of the subsystem in the list of subsystems of the provided system.
        /// </summary>
        /// <param name="system">The system to use for the search.</param>
        /// <param name="subsystemType">The type of the subsystem to search for.</param>
        /// <returns>The index of the subsystem if found; otherwise, -1.</returns>
        public static int IndexOf(this ref PlayerLoopSystem system, Type subsystemType)
        {
            if (subsystemType != null && system.subSystemList != null)
            {
                for (var i = 0; i < system.subSystemList.Length; ++i)
                {
                    if (subsystemType == system.subSystemList[i].type)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
        #endregion // Unity.LiveCapture
    }
}

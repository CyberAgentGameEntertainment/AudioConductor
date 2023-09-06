// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace AudioConductor.Runtime.Core
{
    internal static partial class CustomPlayerLoop
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Setup()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            if (playerLoop.subSystemList == null)
            {
                Debug.LogError("PlayerLoop.subSystemList is null. AudioConductor will not work.");
                return;
            }
            
            var customSubSystem = new PlayerLoopSystem
            {
                type = typeof(CustomUpdate),
                updateDelegate = Update
            };
            
            var registered = false;
            for (var i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                // Register as a subsystem of Update
                var subSystem = playerLoop.subSystemList[i];
                if (subSystem.type == typeof(Update) && subSystem.subSystemList != null)
                {
                    var subSystemList = new PlayerLoopSystem[subSystem.subSystemList.Length + 1];
                    Array.Copy(subSystem.subSystemList, subSystemList, subSystem.subSystemList.Length);
                    subSystemList[^1] = customSubSystem;
                    subSystem.subSystemList = subSystemList;
                    playerLoop.subSystemList[i] = subSystem;
                    registered = true;
                    break;
                }
            }

            if (registered == false)
            {
                Debug.LogError("Can not register CustomPlayerLoop for AudioConductor. AudioConductor will not work.");
                return;
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void Update()
        {
            AudioConductorInternal.Instance.Update(Time.unscaledDeltaTime);
        }

        private readonly struct CustomUpdate
        {
        }
    }
}

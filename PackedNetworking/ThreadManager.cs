// PARTLY FROM: https://github.com/tom-weiland/tcp-udp-networking/blob/tutorial-part2/GameClient/Assets/Scripts/ThreadManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PackedNetworking.Threading
{
    internal class ThreadManager : MonoBehaviour
    {
        private static readonly List<Action> executeOnNextUpdate = new List<Action>();
        private static readonly List<Action> executeOnMainThread = new List<Action>();
        private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
        private static bool actionToExecuteOnMainThread;

        private void Update()
        {
            UpdateMain();
            if(executeOnNextUpdate.Count == 0) return;
            while (executeOnNextUpdate.Count > 0)
            {
                executeOnNextUpdate[0].Invoke();
                executeOnNextUpdate.RemoveAt(0);
            }
        }

        public static void ExecuteOnNextUpdate(Action action)
        {
            if (action == null)
                return;
            
            executeOnNextUpdate.Add(action);
        }
        
        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(Action action)
        {
            if (action == null)
                return;

            lock (executeOnMainThread)
            {
                executeOnMainThread.Add(action);
                actionToExecuteOnMainThread = true;
            }
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        private static void UpdateMain()
        {
            if (!actionToExecuteOnMainThread) return;
            
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
            {
                executeCopiedOnMainThread[i]();
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject dispatcherObject = new GameObject("UnityMainThreadDispatcher");
                instance = dispatcherObject.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(dispatcherObject);
            }
            return instance;
        }
    }

    // Enqueue actions to be run on the main threadđ
    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        // Execute all actions in the queue
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
}

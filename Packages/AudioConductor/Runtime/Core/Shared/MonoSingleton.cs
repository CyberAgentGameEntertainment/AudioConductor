// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace AudioConductor.Runtime.Core.Shared
{
    internal abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

#if UNITY_EDITOR
        private static T _prefabStageInstance;
#endif

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    if (Application.isPlaying == false)
                    {
                        CreateInstanceNonPlaying();
                        return _instance;
                    }
#endif
                    CreateInstance();
                }

                return _instance;
            }
        }

        public static bool ExistInstance => _instance != null;

        protected void Awake() => Initialize(this as T);

        protected void OnDestroy() => Finalize(this as T);

        protected void OnApplicationQuit() => Finalize(this as T);

        private static void CreateInstance()
        {
            if (_instance != null)
                return;

#if UNITY_EDITOR
            if (QuittingState.IsQuitting)
                return;
#endif

            var instance = FindObjectOfType<T>();
            if (instance != null)
                if (Application.isPlaying)
                {
                    Initialize(instance);
                    return;
                }

            var gameObject = new GameObject(typeof(T).FullName);
            instance = gameObject.AddComponent<T>();
            Assert.IsNotNull(instance, $"Instance is null: {typeof(T).FullName}");
            if (instance.IsDontDestroy())
                DontDestroyOnLoad(instance.gameObject);
            Initialize(instance);
        }

#if UNITY_EDITOR
        private static void CreateInstanceNonPlaying()
        {
            if (_instance != null)
                return;

            var instances = Resources.FindObjectsOfTypeAll<T>();
            if (instances != null)
                // May have been reloaded, so recreate.
                foreach (var old in instances)
                    DestroyImmediate(old.gameObject);

            var gameObject = new GameObject(typeof(T).FullName)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var instance = gameObject.AddComponent<T>();
            Assert.IsNotNull(instance, $"Instance is null: {typeof(T).FullName}");
            Initialize(instance);
        }
#endif

        private static void Initialize(T instance)
        {
#if UNITY_EDITOR
            if (IsPrefabStageInstance(instance))
            {
                _prefabStageInstance = instance;
                instance.OnInitialize();
                return;
            }
#endif

            if (_instance == null)
            {
                _instance = instance;
                instance.OnInitialize();
                return;
            }

            if (_instance != instance)
                DestroyImmediate(instance.gameObject);
        }

        private static void Finalize(T instance)
        {
#if UNITY_EDITOR
            if (_prefabStageInstance == instance)
            {
                _prefabStageInstance.OnFinalize();
                _prefabStageInstance = null;
                return;
            }
#endif

            if (_instance == instance)
            {
                _instance.OnFinalize();
                _instance = null;
            }
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnFinalize()
        {
        }

        public static void DestroyInstance()
        {
            if (_instance == null)
                return;

            DestroyImmediate(_instance.gameObject);
            _instance = null;
        }

        protected virtual bool IsDontDestroy() => false;

#if UNITY_EDITOR
        private static bool IsPrefabStageInstance(T instance)
            => PrefabStageUtility.GetPrefabStage(instance.gameObject) != null;
#endif
    }
}

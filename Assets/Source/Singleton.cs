using System;

using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Provides singleton pattern functionality to MonoBehaviours
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T instance;

	private static bool destroyed = false;

	/// <summary>
	/// The singleton instance.
	/// </summary>
	public static T Instance
	{
		get
		{
			if (destroyed)
			{
				return null;
			}

			if (instance == null)
			{
				GameObject prefab = FindResource();
				GameObject go;
				if(prefab != null)
				{
					go = Instantiate(prefab);
					go.name = prefab.name + " Instance";
					instance = go.GetComponent<T>();
					if(instance == null)
					{
						Debug.LogError("Loaded singleton instance is null");
					}
				}
				else
				{
					go = new GameObject();
					go.name = typeof(T).ToString() + " Instance";
					instance = go.AddComponent<T>();
				}
				DontDestroyOnLoad(go);
			}

			return instance;
		}
	}

	/// <summary>
	/// Initialize
	/// </summary>
	protected virtual void Awake()
	{
		SceneManager.sceneLoaded += SceneLoadedHandler;
		OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
	}

	/// <summary>
	/// Initialize
	/// </summary>
	protected virtual void Start()
	{
	}

	/// <summary>
	/// Called when a scene is loaded and when the singleton is first initialized.
	/// </summary>
	protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

	/// <summary>
	/// Called when the singleton is destroyed.
	/// </summary>
	protected virtual void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneLoadedHandler;
		destroyed = true;
	}

	/// <summary>
	/// Search for the predefined instance in the resources folder
	/// </summary>
	/// <returns></returns>
	private static GameObject FindResource()
	{
		GameObject go = (GameObject)Resources.Load(typeof(T).Name, typeof(GameObject));
		if(go != null && go.GetComponent<T>() != null)
		{
			return go;
		}
		return null;
	}

	/// <summary>
	/// Handles the scene loaded event.
	/// </summary>
	private void SceneLoadedHandler(Scene scene, LoadSceneMode mode)
	{
		OnSceneLoaded(scene, mode);
	}
}

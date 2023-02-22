using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardCanvas : MonoBehaviour
{
   public static ScoreboardCanvas Instance;

	void Awake()
	{
		if(Instance)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		Instance = this;
	}
}

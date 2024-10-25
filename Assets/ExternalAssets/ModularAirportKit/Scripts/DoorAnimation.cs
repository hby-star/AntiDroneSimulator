using UnityEngine;
using System.Collections;

public class DoorAnimation : MonoBehaviour
{
	public Animator animator;
	public string onTriggerEnterParameterName;
	public string onTriggerExitParameterName;
	public AudioClip OpenSound;
	public AudioClip CloseSound;


	void Start()
	{
		if(animator == null)
		{
			animator = GetComponent<Animator>();
			if(animator == null)
			{

			}
		}
	}

	void OnTriggerEnter()
	{
		if(onTriggerEnterParameterName != null)
		{
			gameObject.GetComponent<AudioSource>().PlayOneShot(OpenSound);
			animator.SetTrigger(onTriggerEnterParameterName);
		}
	}

	void OnTriggerExit()
	{
		if(onTriggerExitParameterName != null)
		{
			gameObject.GetComponent<AudioSource>().PlayOneShot(CloseSound);
			animator.SetTrigger(onTriggerExitParameterName);
		}
	}
}

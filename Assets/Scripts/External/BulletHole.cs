/// <summary>
/// BulletHole.cs
/// Author: MutantGopher
/// Attach this script to bullet hole prefabs. It handles bullet hole automatic destruction and fading.
/// </summary>

using UnityEngine;
using System.Collections;

public class BulletHole : MonoBehaviour
{
	public GameObject bulletHoleMesh;			// The GameObject that has the actual mesh
	public bool usePooling = true;				// Whether or not the bullet hole will use the pooling system - if false, the bullet hole will be destroyed after lifetime
	public float lifetime = 28.0f;				// The amount of time before the bullet hole disappears entirely
	public float startFadeTime = 10.0f;			// The amount of time before the bullet hole starts to fade
	private float timer;						// A timer to keep track of how long this bullet has been in existence
	public float fadeRate = 0.001f;				// The rate at which the bullet will fade out
	private Color targetColor;					// The color to which the bullet hole wants to change
	


	// Use this for initialization
	void Start()
	{
		// Initialize the timer to 0
		timer = 0.0f;

		// Initialize the targetColor
		targetColor = bulletHoleMesh.GetComponent<Renderer>().material.color;
		targetColor.a = 0.0f;

		// Attach the bullet hole to the hit GameObject ***- no longer used because of the pooling system
		//AttachToParent();
	}
	

	// Update is called once per frame
	void Update()
	{
		if (!usePooling)
			FadeAndDieOverTime();
		
	}

	// This method is called when a bullet hole is moved to a different location/rotation, ready to be used again
	public void Refresh()
	{
		AttachToParent();
	}

	// Make the bullet hole "stick" to the object it hit by parenting it
	private void AttachToParent()
	{
		RaycastHit hit;
		if (Physics.Raycast(bulletHoleMesh.transform.position, -bulletHoleMesh.transform.up, out hit, 0.1f))
		{
			transform.parent = hit.collider.transform;
		}
		else
		{
			Destroy(transform.gameObject);
		}
	}


	private void FadeAndDieOverTime()
	{
		// Update the timer
		timer += Time.deltaTime;

		// If the timer has reached startFadeTime, start fading out
		if (timer >= startFadeTime)
		{
			bulletHoleMesh.GetComponent<Renderer>().material.color = Color.Lerp(bulletHoleMesh.GetComponent<Renderer>().material.color, targetColor, fadeRate * Time.deltaTime);
		}

		// If the timer has reached lifetime, destroy the bullet hole completely
		if (timer >= lifetime)
		{
			Destroy(gameObject);
		}
	}
}


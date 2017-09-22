using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GrappleArm : MonoBehaviour
{
	public float Speed = 5.0f;
	public Transform FireLocation;
	public Transform EndLocation;
	public LayerMask AttachCollisionMask;
	public LayerMask RetractCollisionMask;

	private Transform target;
	private float maxDistanceToTarget = 0.1f;
	private bool isRetracting;
	private bool isAttached;
	private bool hasOwnerReachedHookable;
	private bool isStayingAttached;
	private Vector2 attachedLocation;
	private GameObject hookable;

	public GameObject Owner { get; set; }

	public Vector2 Direction { get; private set; }

	public bool HasOwnerReachedHookable { get { return hasOwnerReachedHookable; } }

	public void Start ()
	{
		HideLinks();
		gameObject.SetActive(false);
	}

	public void Fire ()
	{

		transform.position = FireLocation.position;
		target = EndLocation;
		Direction = EndLocation.position.x - FireLocation.position.x > 0 ? Vector2.right : Vector2.left;
		gameObject.SetActive(true);

	}

	public void Update ()
	{	
		if (isAttached)
		{
			HandleInput();
			HandleAttachment();
			return;
		}
		HandleMovement();
	}

	private void HandleInput ()
	{
		isStayingAttached = false;
		if (Input.GetKey(KeyCode.E))
		{
			isStayingAttached = true;
		} 
	}

	public void OnTriggerEnter2D (Collider2D other)
	{

		/*Debug.Log ("CollisionMask: " + Convert.ToString (AttachCollisionMask.value, 2));
		Debug.Log ("Layer: " + other.gameObject.layer);
		Debug.Log ("left shift operation: " + Convert.ToString (1 << other.gameObject.layer, 2));
		*/

		//do nothing if the grapple is retracting or if it is already attached
		if (isRetracting || isAttached)
		{
			return;
		}

		//if the grapple collisions with something different than a hookabe (e.g. a wall), let's retract it
		if ((RetractCollisionMask.value & (1 << other.gameObject.layer)) != 0)
		{
			target = FireLocation;
			isRetracting = true;
			return;
		}

		//do not attach if it did not collision with anything set in the attach collision mask
		if ((AttachCollisionMask.value & (1 << other.gameObject.layer)) == 0)
		{
			return;
		}

		//it means the grapple  collisioned with any of the layers set in CollisionMask
		isAttached = true;
		hookable = other.gameObject;

	}

	private void HandleAttachment ()
	{
		if (gameObject.transform.parent != hookable.transform)
		{
			gameObject.transform.parent = hookable.transform;
		}

		if (Owner.transform.parent != hookable.transform)
		{
			Owner.transform.parent = hookable.transform;
		}

		if (!hasOwnerReachedHookable)
		{
			//this offset is for putting the player a little behind the grapple, avoiding visual overlap
			float offset = Direction.x * 0.3f;
			attachedLocation = new Vector2 (transform.position.x - offset, Owner.transform.position.y);
			Owner.transform.position = Vector3.MoveTowards (Owner.transform.position, attachedLocation, Speed * Time.deltaTime);
			if (Mathf.Abs (Owner.transform.position.x - attachedLocation.x) == 0)
			{
				hasOwnerReachedHookable = true;
			}
		} 
		else
		{
			//once player has reached the attached location, finish the grapple arm gameplay when player decides to drop
			if (!isStayingAttached)
			{
				Finish();
			}
		}

	}

	private void HandleMovement ()
	{
		transform.position = Vector3.MoveTowards (transform.position, target.position, Speed * Time.deltaTime);

		double distanceSquared = (target.position - transform.position).sqrMagnitude;

		if(distanceSquared >= maxDistanceToTarget * maxDistanceToTarget) {
			return;
		}

		if (isRetracting)
		{
			Finish();
			return;
		}

		target = FireLocation;
		isRetracting = true;
	}

	public void Finish ()
	{
		gameObject.transform.parent = Owner.transform;
		Owner.transform.parent = null;

		isRetracting = false;
		isAttached = false;
		isStayingAttached = false;
		hasOwnerReachedHookable = false;
		HideLinks();
		gameObject.SetActive(false);
		Owner.GetComponent<CharacterController2D>().enabled = true;
	}

	private void HideLinks ()
	{
		List<SpriteRenderer> links =  GetComponentsInChildren<SpriteRenderer>().ToList();
		//the first child is always this game object (the parent). So remove it from the list
		links.RemoveAt(0);

		foreach (SpriteRenderer link in links)
		{
			link.enabled = false;
		}
	}

}


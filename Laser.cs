using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour 
{

	public enum LaserStatus
	{
		SHOOTING,
		FADING,
		SHUTDOWN
	}
	
	public float ShootSpeed = 5;
	public LayerMask CollisionMask;
	public Color BaseColor = Color.red;
	public float RotationSpeed = 0.2f;
	public bool EnableFading = false;
	public float FadeFrequency = 3;
	public float FadingSpeed = 1;
	public float ShutdownDuration = 3;

	[Range(0, 0.01f)]
	public float DistortionMagnitude = 0.0015f;

	private LineRenderer lineRenderer;
	private Vector2 endPosition;
	private BoxCollider2D boxCollider = null;
	private float fadeIn = 0;
	private LaserStatus laserStatus = LaserStatus.SHOOTING;
	private float laserWidth = 0;
		
	public void Start () 
	{
		lineRenderer = GetComponent<LineRenderer>();
		boxCollider = gameObject.GetComponent<BoxCollider2D>();
		endPosition = Vector2.right;
		//lineRenderer.sortingLayerName = "Foreground";
		//lineRenderer.sortingOrder = -1;
		fadeIn = FadeFrequency;
		laserWidth = lineRenderer.widthMultiplier;
	}

	public void Update ()
	{
		lineRenderer.material.SetColor ("_Color", BaseColor);
		lineRenderer.materials [1].SetFloat ("_Magnitude", DistortionMagnitude);

		UpdateColliderDimensions ();

		HandleRotation ();

		HandleFading ();

		if (laserStatus == LaserStatus.SHUTDOWN)
		{
			return;
		}

		Vector2 origin = transform.position;
		float distance = Mathf.Abs(endPosition.x);

		lineRenderer.materials [1].SetFloat ("_LaserDistance", distance);

		//because the laser can be rotated to any direction, let's work with the local coordinates
		Vector3 rightWorldToLocal = transform.InverseTransformDirection(Vector2.right);

		//adjust the laser direction
		Vector2 laserDirection = new Vector2(rightWorldToLocal.x, -rightWorldToLocal.y);

		//Debug.DrawRay (origin, laserDirection * distance, Color.blue);

		RaycastHit2D raycastHit = Physics2D.Raycast(origin, laserDirection, distance, CollisionMask);

		if (!raycastHit)
		{
			lineRenderer.SetPosition (1, endPosition);
			endPosition += Vector2.right * ShootSpeed * Time.deltaTime;
		}
		else
		{
			//this is the absolute position of the laser once it hits with something in the layer mask
			Vector2 absoluteHitPosition = new Vector2(raycastHit.point.x - origin.x, raycastHit.point.y - origin.y);
			//Vector2 absoluteHitPosition = new Vector2(raycastHit.point.x, raycastHit.point.y);
			//let's convert the final position to local coordinates given that the line renderer is in local space
			endPosition = transform.InverseTransformDirection(absoluteHitPosition);

			lineRenderer.SetPosition(1, endPosition);

		}

	}

	private void UpdateColliderDimensions() 
	{
		float distance = Mathf.Abs(endPosition.x);
		boxCollider.offset = new Vector2(distance/2, 0.005f);
		boxCollider.size = new Vector2(distance, lineRenderer.widthMultiplier * 0.09f);
	}

	private void HandleRotation()
	{
		transform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);		
	}

	private void HandleFading ()
	{	

		if (!EnableFading)
		{
			return;
		}

		if (laserStatus == LaserStatus.SHOOTING)
		{
			fadeIn -= Time.deltaTime;

			if (fadeIn <= 0)
			{
				fadeIn = FadeFrequency;
				laserStatus = LaserStatus.FADING;	
			}
		} 
		else if(laserStatus == LaserStatus.FADING)
		{
			StartCoroutine(FadeCoroutine());	
		}
	}

	private IEnumerator FadeCoroutine ()
	{
		lineRenderer.widthMultiplier -= FadingSpeed * Time.deltaTime;

		if (lineRenderer.widthMultiplier > 0)
		{
			yield return null;	
		}
		else
		{	

			lineRenderer.SetPosition (1, Vector2.zero);
			endPosition = Vector2.right;
			laserStatus = LaserStatus.SHUTDOWN;
			lineRenderer.widthMultiplier = 0;

			yield return new WaitForSeconds(ShutdownDuration);

			lineRenderer.widthMultiplier = laserWidth;
			laserStatus = LaserStatus.SHOOTING;
		}




	}

}

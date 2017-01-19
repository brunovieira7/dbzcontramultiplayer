using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RaySpell : MonoBehaviour {

	private Vector2 end;
	private Vector3 origin;
	public GameObject body;
	public GameObject tip;

	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (end != null) {
			if (origin == null) {
				origin = body.transform.position;
			}

			Vector3 scale = body.transform.localScale;
			scale.x += 0.1f;

		
			body.transform.localScale = scale;



			var rend = body.GetComponentInChildren<Renderer> ();

			Vector3 tipPos = tip.transform.position;
			tipPos.x = rend.bounds.size.x - origin.x;
			tipPos.y = rend.bounds.size.y - origin.y;

			tip.transform.position = tipPos;
			Debug.Log ("X: "+ rend.bounds.size);
			Debug.Log ("Y: "+ rend.bounds.max); 
		}
	}

	public void fireSpell(Vector2 end) {
		this.end = end;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		Destroy (gameObject);
	}
}





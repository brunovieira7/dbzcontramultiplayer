using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RaySpell : NetworkBehaviour {

	private Vector2 end;
	private Vector3 origin;
	public GameObject body;
	public GameObject tip;
	public float elapsedTime;
	private BoxCollider2D collider;
	private Rigidbody2D rb2D;

	void Start () {
		elapsedTime = 0f;
		collider = GetComponent<BoxCollider2D> ();
		rb2D= GetComponent<Rigidbody2D> ();
		//Vector3 scale = body.transform.localScale;
		//collider.transform.localScale = scale;
	}
	
	// Update is called once per frame
	void Update () {
		elapsedTime += Time.deltaTime;

		if (elapsedTime > 2f) {
			Destroy (gameObject);
			return;
		}

		if (end != null) {
			if (origin == null) {
				origin = body.transform.position;
				//collider.transform.position = origin;
			}

			Vector3 scale = body.transform.localScale;
			scale.x += 0.2f;

		
			body.transform.localScale = scale;
			collider.transform.localScale = scale;

			var rend = body.GetComponentInChildren<Renderer> ();

			Vector3 tipPos = tip.transform.position;
			tipPos.x = rend.bounds.size.x - origin.x;
			tipPos.y = rend.bounds.size.y - origin.y;

			//tip.transform.position = tipPos;
			//Debug.Log ("X: "+ rend.bounds.size);
			//Debug.Log ("Y: "+ rend.bounds.max); 
		}
	}

	public void fireSpell(Vector2 end) {
		this.end = end;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		GameObject hit = collision.gameObject;

		Debug.Log ("+++" + hit.tag);
		if (hit.tag == "Player") {
			//Debug.Log (hit);
			Player player = hit.GetComponent<Player> ();

			Debug.Log ("===" + player);
			if (player != null) {
				player.CmdTakeDamage (20);
				//Debug.Log ("hELATH" + player.health);
			}
		}
	}
}





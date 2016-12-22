using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {

	private Rigidbody2D rb2D;
	private Vector2 end;
	public float speed;
	public GameObject explosion;
	public AudioClip explosionSound;

	// Use this for initialization
	void Start () {
		rb2D = GetComponent <Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (end != null) {
			rb2D.velocity = end * speed;
		}
	}

	public void fireBullet(Vector2 end) {
		this.end = end;
	}

	void OnCollisionEnter2D(Collision2D collision)
    {
		GameObject hit = collision.gameObject;

		//Debug.Log ("===" + hit.tag);
		if (hit.tag == "Player") {
			Debug.Log (hit);
			Player player = hit.GetComponent<Player> ();

			Debug.Log ("===" + player);
			if (player != null) {
				player.TakeDamage (1);
				//Debug.Log ("hELATH" + player.health);
			}
		} else if (hit.tag == "bullet") {
			CmdExplode (hit.transform.position);

		}
		//Debug.Log("collided" );
		Destroy(gameObject);
    }

	[Command]
	void CmdExplode(Vector3 position) {
		Vector3 start = new Vector3 (position.x, position.y, 0f);
		GameObject instance = Instantiate (explosion, start, Quaternion.identity) as GameObject;

		Debug.Log("---exploding" + instance);
		SoundManager.instance.RandomizeSfx (explosionSound);
		NetworkServer.Spawn (instance);
	}
}

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player : NetworkBehaviour  {

	private Rigidbody2D rb2D;
	public GameObject bullet;
	public Transform bulletspawn;
	public int health;
	private Animator animator;
	public float speed;
	private Skill usingSkill;
	private float timer;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		rb2D = GetComponent <Rigidbody2D> ();
		health = 10;
		timer = 0f;

		if (GetComponent<NetworkView>().isMine) {
			GameObject myCam = GameObject.Find ("Main Camera");
			var script = myCam.GetComponent<CameraController> ();
			script.player = gameObject;
		}
	}

	// Update is called once per frame
	void Update () {
		//Debug.Log ("local:" + isLocalPlayer + " server:" + isServer);
		if (!isLocalPlayer) {
		    return;
		}

		if (usingSkill == null && Input.GetMouseButtonDown(0)) {
			rb2D.velocity = new Vector2 (0f,0f);

			Vector3 position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			position.z = 0f;

			Vector2 direction = position - this.bulletspawn.position;
			direction.Normalize();

			usingSkill = new Skill ("Cast", 0.3f, direction);
			Debug.Log("Mouse Pos " + direction );

 			animator.SetTrigger ("Cast");

			changeDirection(direction.x);


		}
			
		if (usingSkill != null) {
			if (!usingSkill.ready) {
				usingSkill.doSkill (Time.deltaTime);
			} else {
				CmdFire (usingSkill.getPosition ());
				usingSkill = null;
				animator.SetTrigger ("Idle");
			}
		} else {
			tryWalking();
		}

	}

	void tryWalking() {
		if (usingSkill == null) {
			float horizontal = Input.GetAxisRaw ("Horizontal");
			float vertical = Input.GetAxisRaw ("Vertical");

			if (horizontal != 0 || vertical != 0) {
				animator.SetTrigger ("Dash");
			} else {
				animator.SetTrigger ("Idle");
			}

			changeDirection (horizontal);

			rb2D.velocity = new Vector2 (horizontal * speed, vertical * speed);
		} else {
			rb2D.velocity = new Vector2 (0f,0f);
		}
	}
		
	void changeDirection ( float horizontal) {
		if ( (horizontal < 0 && transform.localScale.x > 0) || (horizontal > 0 && transform.localScale.x < 0) ) {
			Vector3 currScale = transform.localScale;
			currScale.x *= -1;
			transform.localScale = currScale;
		}
	}

   public void TakeDamage(int amount)
    {
        health -= amount;
    }

	[Command]
	void CmdFire(Vector3 position) {
		//Debug.Log("p2" + position );

		Vector3 start = new Vector3 (this.bulletspawn.position.x, this.bulletspawn.position.y, 0f);
		GameObject instance = Instantiate (bullet, start, Quaternion.identity) as GameObject;

		var script = instance.GetComponent<Bullet>();
		if( script != null )
		{
			var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
			instance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); 

            // Vector2 direction = position - this.bulletspawn.position;
           //  direction.Normalize();

			script.fireBullet(position);
		}

		NetworkServer.Spawn (instance);
	}
}

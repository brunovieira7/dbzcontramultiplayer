using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour  {

	private Rigidbody2D rb2D;
	public GameObject bullet;
	public GameObject raySpell;
	public Transform bulletspawn;

	private Animator animator;
	public float speed;
	private Skill usingSkill;
	private float timer;
	[SyncVar] public bool facingLeft;

	private bool takingDamage;
	private float timeDmg = 0f;
	private float timeDmgTotal = 0.4f;

	public AudioClip shoot;

	public int maxHealth;
	private int health;
	private GameObject healthBarInfo;
	private GameObject healthBar;
	private bool isDead = false;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		rb2D = GetComponent <Rigidbody2D> ();
		health = maxHealth;
		timer = 0f;
		facingLeft = false;
		takingDamage = false;

		if (isLocalPlayer && GetComponent<NetworkView>().isMine) {
			GameObject myCam = GameObject.Find ("Main Camera");
			var script = myCam.GetComponent<CameraController> ();
			script.player = gameObject;

			healthBarInfo = GameObject.Find ("healthBarInfo");
			healthBar = GameObject.Find ("healthBar");
		}
	}

	// Update is called once per frame
	void Update () {
		if (isDead) {
			return;
		}

		flip ();
		if (!isLocalPlayer) {
		    return;
		}

		if (takingDamage) {
			timeDmg += Time.deltaTime;
			//Debug.Log ("DMG " + timeDmg);
			if (timeDmg > timeDmgTotal) {
				timeDmg = 0f;
				takingDamage = false;
			}
		}

		if (usingSkill == null && Input.GetMouseButtonDown(0) && !takingDamage) {
			rb2D.velocity = new Vector2 (0f,0f);

			Vector3 position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			position.z = 0f;

			Vector2 direction = position - this.bulletspawn.position;
			direction.Normalize();

			usingSkill = new Skill ("Cast", 0.3f, direction);
			//Debug.Log("Mouse Pos " + direction );

 			animator.SetTrigger ("Cast");

			CmdChangeDirection(direction.x);


		}
			
		if (usingSkill != null) {
			if (!usingSkill.ready) {
				usingSkill.doSkill (Time.deltaTime);
			} else {
				CmdFire (usingSkill.getPosition ());
				usingSkill = null;
				animator.SetTrigger ("Idle");
			}
		} else if (!takingDamage) {
			tryWalking();
		}

	}

	void flip() {
		//Debug.Log ("local:" + isLocalPlayer + " server:" + isServer + " flipme:" + facingLeft);
		Vector3 currScale = transform.localScale;
		if ((facingLeft && currScale.x > 0) || (!facingLeft && currScale.x < 0)) {
			//Debug.Log ("local:" + isLocalPlayer + " server:" + isServer + " flipme:" + facingLeft);

			currScale.x *= -1;
			transform.localScale = currScale;
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

			CmdChangeDirection (horizontal);

			rb2D.velocity = new Vector2 (horizontal * speed, vertical * speed);
		} else {
			rb2D.velocity = new Vector2 (0f,0f);
		}
	}

	[Command]
	void CmdChangeDirection ( float horizontal) {
		if ( (horizontal < 0 && transform.localScale.x > 0) || (horizontal > 0 && transform.localScale.x < 0) ) {
			Vector3 currScale = transform.localScale;
			currScale.x *= -1;
			transform.localScale = currScale;
			facingLeft = !facingLeft;
		}
	}

/*   public void TakeDamage(int amount)
    {
		takingDamage = true;
		animator.SetTrigger ("Damage");
        health -= amount;
		rb2D.velocity = new Vector2 (0f,0f);
    }*/


	public void TakeDamage(int dmg) {
		Debug.Log("taking DMG: " + dmg );
		if (isDead) {
			return;
		}

		takingDamage = true;
		animator.SetTrigger ("Damage");
		rb2D.velocity = new Vector2 (0f,0f);

		health -= dmg;

		if (health <= 0) {
			isDead = true;
			animator.SetTrigger ("Death");
			rb2D.gravityScale = 1;
		}

		RectTransform rect = healthBar.GetComponent<RectTransform> ();
		RectTransform rectInfo = healthBarInfo.GetComponent<RectTransform> ();

		float newScale = (float) health / maxHealth;

		Vector3 currScale = rect.localScale;
		currScale.x = newScale;

		Debug.Log("scale: " + currScale.x );

		rect.localScale = currScale;
		rectInfo.localScale = currScale;
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
			SoundManager.instance.RandomizeSfx (shoot);
		}

		NetworkServer.Spawn (instance);
	}

	[Command]
	void CmdFireBullet(Vector3 position) {
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
			SoundManager.instance.RandomizeSfx (shoot);
		}

		NetworkServer.Spawn (instance);
	}
}

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

	private int skillActive;

	// Use this for initialization
	void Start () {
		skillActive = 1;
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

		if (Input.GetKeyDown ("1")) {
			skillActive = 1;
		}

		if (Input.GetKeyDown ("2")) {
			skillActive = 2;
		}

		if (Input.GetKeyDown ("3")) {
			skillActive = 3;
		}
			

		if (takingDamage) {
			timeDmg += Time.deltaTime;
			//Debug.Log ("DMG " + timeDmg);
			if (timeDmg > timeDmgTotal) {
				timeDmg = 0f;
				takingDamage = false;
			}
		}

		if (usingSkill != null && Input.GetMouseButton (0) && !takingDamage &&  (skillActive == 3)) {
			Debug.Log ("=ACTIVE!");
				return;
		}

		if (usingSkill == null && Input.GetMouseButtonDown(0) && !takingDamage) {
			rb2D.velocity = new Vector2 (0f,0f);

			Vector3 position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			position.z = 0f;

			Vector2 direction = position - this.bulletspawn.position;
			direction.Normalize();

			if (skillActive == 1) {
				usingSkill = new Skill ("Casting_1", 0.3f,  0.3f, direction, 1);
			}
			else if (skillActive == 2) {
				usingSkill = new Skill ("Casting_2", 0.3f, 2.3f, direction, 2);
			}
			else if (skillActive == 3) {
				usingSkill = new Skill ("Power", 0.3f, 2.3f, direction, 3);
				animator.SetTrigger (usingSkill.getTrigger());
				return;
			}

			//Debug.Log("Mouse Pos " + direction );

			animator.SetTrigger (usingSkill.getTrigger());

			CmdChangeDirection(direction.x);


		}
			
		if (usingSkill != null) {
			SkillStatus status = usingSkill.doSkill (Time.deltaTime);

			if (status == SkillStatus.READY) {
				if (usingSkill.getSkill() == 1) {
					CmdFireBullet (usingSkill.getPosition ());
				}
				else if (usingSkill.getSkill() == 2) {
					CmdFire (usingSkill.getPosition ());
				}
				usingSkill.skillActive ();
			}
			else if (status == SkillStatus.ENDED) {
				Debug.Log ("ENDED????");
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
		GameObject instance = Instantiate (raySpell, start, Quaternion.identity) as GameObject;

		var script = instance.GetComponent<RaySpell>();
		if( script != null )
		{
			var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
			instance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); 

			// Vector2 direction = position - this.bulletspawn.position;
			//  direction.Normalize();

			script.fireSpell(position);
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

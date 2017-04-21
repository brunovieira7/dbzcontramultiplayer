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

	[SyncVar (hook="LifeChanged")] private int health;
	private GameObject healthBarInfo;
	public GameObject healthBar;
	private bool isDead = false;

	private GameObject[] UIBUttons;

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

			UIBUttons = new GameObject[3];

			UIBUttons[0] = GameObject.Find ("Button1_ac");
			UIBUttons[1] = GameObject.Find ("Button2_ac");
			UIBUttons[2] = GameObject.Find ("Button3_ac");

			UIBUttons[1].SetActive (false);
			UIBUttons[2].SetActive (false);
		}
	}

	int getId() {
		NetworkIdentity ni = GetComponent<NetworkIdentity> ();
		return ni.GetInstanceID();
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

		if (Input.GetKeyDown ("1") || Input.GetKeyDown ("2") || Input.GetKeyDown ("3")) {
			int.TryParse (Input.inputString, out skillActive);
			animator.SetInteger ("SpellSelected", skillActive);
			activateButton (skillActive);
		}
			
		if (takingDamage) {
			timeDmg += Time.deltaTime;
			//Debug.Log ("DMG " + timeDmg);
			if (timeDmg > timeDmgTotal) {
				timeDmg = 0f;
				takingDamage = false;
			}
		}


		if (animator.GetInteger ("SpellSelected") == 3 && animator.GetBool ("SpellReady") == true && Input.GetMouseButton (0)) {
			rb2D.velocity = new Vector2 (0f,0f);
			return;
		}

		if (animator.GetInteger ("SpellSelected") == 3 && animator.GetBool ("SpellReady") == true && Input.GetMouseButtonUp (0)) {
			animator.SetBool ("SpellReady", false);
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


			//Debug.Log("Mouse Pos " + direction );

			animator.SetBool ("SpellReady", true);

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
				//Debug.Log ("ENDED????");
				usingSkill = null;
				//animator.SetTrigger ("Idle");
				animator.SetBool ("SpellReady", false);
			}

		} else if (!takingDamage) {
			tryWalking();
		}

	}

	void activateButton(int active) {
		for (int x = 0; x < UIBUttons.Length; x++) {
			UIBUttons [x].SetActive (false);
		}
		UIBUttons [active-1].SetActive (true);
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

	[Command]
	public void CmdTakeDamage(int dmg) {
		Debug.Log("taking DMG: " + dmg);
		Debug.Log ("local:" + isLocalPlayer + " server:" + isServer + " health:"+health);
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

	}

	void LifeChanged(int value) { 
		RectTransform rect = healthBar.GetComponent<RectTransform> ();


		float newScale = (float) value / maxHealth;

		Vector3 currScale = rect.localScale;
		currScale.x = newScale;

		//Debug.Log("scale: " + currScale.x );

		rect.localScale = currScale;

		if (isLocalPlayer) {
			healthBarInfo = GameObject.Find ("healthBarInfo");
			RectTransform rectInfo = healthBarInfo.GetComponent<RectTransform> ();
			rectInfo.localScale = currScale;
		}

	}

	[Command]
	void CmdFire(Vector3 position) {
		//Debug.Log("p2" + position );

		Vector3 start = new Vector3 (this.bulletspawn.position.x, this.bulletspawn.position.y, 0f);

		var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
		GameObject instance = Instantiate (raySpell, start, Quaternion.AngleAxis(angle, Vector3.forward)) as GameObject;

		SoundManager.instance.RandomizeSfx (shoot);
		NetworkServer.Spawn (instance);
	}

	[Command]
	void CmdFireBullet(Vector3 position) {
		//Debug.Log("p2" + position );

		Vector3 start = new Vector3 (this.bulletspawn.position.x, this.bulletspawn.position.y, 0f);
		var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
		GameObject instance = Instantiate (bullet, start, Quaternion.AngleAxis(angle, Vector3.forward)) as GameObject;

		var script = instance.GetComponent<Bullet>();
		if( script != null )
		{
			script.fireBullet(position);
			SoundManager.instance.RandomizeSfx (shoot);
		}

		NetworkServer.Spawn (instance);
	}
}

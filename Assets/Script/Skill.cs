using UnityEngine;
using System.Collections;

public class Skill  {

	private string trigger;
	private float startTimer;
	private GameObject bullet;
	private float timer;
	private Vector3 position;
	public bool ready;

	// Use this for initialization
	public Skill (string trigger, float startTimer,Vector3 position) {
		this.trigger = trigger;
		this.startTimer = startTimer;
		this.position = position;
		this.timer = 0f;
		ready = false;
	}

	public void doSkill(float deltaTime) {
		timer += deltaTime;

		if (timer >= startTimer) {
			ready = true;
		}
	}

	public Vector3 getPosition(){
		return position;
	}
}

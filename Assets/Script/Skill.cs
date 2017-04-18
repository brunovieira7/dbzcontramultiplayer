using UnityEngine;
using System.Collections;

public class Skill  {

	private string trigger;
	private float startTimer;
	private float endTimer;
	private float timer;
	private Vector3 position;
	public bool ready;
	public bool ended;
	private int skillNumber;
	private SkillStatus status = SkillStatus.INIT;

	// Use this for initialization
	public Skill (string trigger, float startTimer, float endTimer, Vector3 position, int skillNumber) {
		this.trigger = trigger;
		this.startTimer = startTimer;
		this.endTimer = endTimer;
		this.position = position;
		this.timer = 0f;
		this.skillNumber = skillNumber;
	}

	public int getSkill() {
		return skillNumber;
	}

	public SkillStatus doSkill(float deltaTime) {
		timer += deltaTime;

		if (timer >= startTimer && status == SkillStatus.INIT) {
			status = SkillStatus.READY;
		} else if (timer >= endTimer && status == SkillStatus.RUNNING) {
			status = SkillStatus.ENDED;
		}

		return status;
	}

	public void skillActive() {
		status = SkillStatus.RUNNING;
	}

	public Vector3 getPosition(){
		return position;
	}

	public string getTrigger() {
		return trigger;
	}
}

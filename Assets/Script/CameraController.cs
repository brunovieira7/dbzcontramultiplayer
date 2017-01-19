using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public GameObject player;
	private Vector3 offset;

	// Use this for initialization
	void Start () {
		// player comeca 0,0 se mudar tem que mudar isso
		offset = transform.position - player.transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Vector3 pos = transform.position;
		pos.x = player.transform.position.x + offset.x;

		transform.position = pos;
	}
}

using UnityEngine;
using System.Collections;

public class Golfer : MonoBehaviour {

    public RectTransform bar;
    public RectTransform cursor;

    public float shotPower = 10;
    public Vector3 shotVector = new Vector3(0, 1, 0);

    private Rigidbody body;
    private LineRenderer line;

    static int LINESEGMENTS = 100;
    static float LINETICK = 0.05f;
    static float LINESCALE = 20;
    static float LINESPEED = 2;

	// Use this for initialization
	void Start () {
        body = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        line.material.mainTextureOffset = new Vector2(
            line.material.mainTextureOffset.x - Time.deltaTime * LINESPEED,
            line.material.mainTextureOffset.y);
        updateLine();
	}

    void FixedUpdate()
    {
        if (Input.GetButtonDown("Jump"))
        {
            updateLine();
            shoot();
        }
    }

    void updateLine()
    {
        line.material.mainTextureScale = new Vector2(LINESCALE, 1);
        float curTick = 0;
        Vector3 curPoint = transform.position;
        shotVector.Normalize();
        Vector3 curVelocity = shotVector * shotPower;
        line.SetVertexCount(LINESEGMENTS);
        for (int i = 0; i < LINESEGMENTS; i++)
        {
            line.SetPosition(i, curPoint);
            curPoint += curVelocity * LINETICK;
            curVelocity += Physics.gravity * LINETICK;
            curTick += LINETICK;
        }
    }

    void shoot()
    {
        shotVector.Normalize();
        body.AddForce(shotVector * shotPower, ForceMode.VelocityChange);
    }
}

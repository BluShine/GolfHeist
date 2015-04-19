using UnityEngine;
using System.Collections;

public class Golfer : MonoBehaviour {

    public RectTransform bar;
    public RectTransform cursor;
    public Camera camera;
    private Vector3 cameraOffset = new Vector3(0, 0, 0);

    public float shotPower = 10;
    private float currentPower = 100;
    public Vector3 shotVector = new Vector3(0, 1, 0);

    public float rollingResistance = 10;

    private Rigidbody body;
    private LineRenderer line;
    private float radius = 1;

    static int LINESEGMENTS = 100;
    static float LINETICK = 0.03f;
    static float LINESCALE = 20;
    static float LINESPEED = 2;

    private bool isGrounded = false;

	// Use this for initialization
	void Start () {
        body = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
        cameraOffset = camera.transform.position - transform.position;
        body.maxAngularVelocity = 1000000;
	}
	
	// Update is called once per frame
	void Update () {
        //move the camera
        camera.transform.position = transform.position + cameraOffset;

        //draw the line
        line.material.mainTextureOffset = new Vector2(
            line.material.mainTextureOffset.x - Time.deltaTime * LINESPEED,
            line.material.mainTextureOffset.y);
        updateLine();
	}

    void FixedUpdate()
    {
        //check if it's on the ground
        float radius = transform.localScale.x;
        Ray ray = new Ray(transform.position + Vector3.up * radius, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction);
        isGrounded = Physics.SphereCast(ray, radius, radius * 2);
        //bring the ball to a stop
        if (isGrounded)
        {
            //stop it below a certain velocity
            if (body.angularVelocity.magnitude < 3 && body.velocity.magnitude < 1)
            {
                body.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else {
                //apply rolling resistance
                body.AddTorque(-body.angularVelocity.normalized * rollingResistance * Time.fixedDeltaTime, 
                    ForceMode.VelocityChange);
            }
        }

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
        Vector3 curVelocity = shotVector * currentPower;
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
        body.constraints = RigidbodyConstraints.None;
        shotVector.Normalize();
        body.AddForce(shotVector * currentPower, ForceMode.VelocityChange);
    }
}

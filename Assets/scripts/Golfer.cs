using UnityEngine;
using System.Collections;

public class Golfer : MonoBehaviour {

    public RectTransform bar;
    public RectTransform cursor;
    public Camera camera;
    private Vector3 cameraOffset = new Vector3(0, 0, 0);

    //shooting
    static string SHOOTBUTTON = "Jump";
    private enum GolfState { ready, power, accuracy, flying };
    private GolfState state = GolfState.flying;

    public float shotPower = 100;
    public float cycleSpeed = 1;
    private float cycleTimer = 0;
    private float currentPower = 100;

    private Vector3 shotVector = new Vector3(0, 1, 0);
    public float verticalAngle = 45;
    public float aimAngle = 0;
    public float aimSpeed = 180;


    public float rollingResistance = 500;

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
        //PHYSICS

        //check if it's on the ground
        float radius = transform.localScale.x;
        Ray ray = new Ray(transform.position + Vector3.up * radius, Vector3.down);
        //Debug.DrawRay(ray.origin, ray.direction);
        isGrounded = Physics.SphereCast(ray, radius, radius * 2);
        //bring the ball to a stop
        if (isGrounded)
        {
            //stop it below a certain velocity
            if (body.angularVelocity.magnitude < 3 && body.velocity.magnitude < 1)
            {
                body.constraints = RigidbodyConstraints.FreezeRotation;
                if (state == GolfState.flying)
                {
                    state = GolfState.ready;
                }
            }
            else {
                //apply rolling resistance
                body.AddTorque(-body.angularVelocity.normalized * rollingResistance * Time.fixedDeltaTime, 
                    ForceMode.VelocityChange);
            }
        }

        //INPUT

        switch (state)
        {
            case GolfState.ready:
                //aiming
                aimAngle += Input.GetAxis("Horizontal") * aimSpeed;
                shotVector = Quaternion.AngleAxis(-verticalAngle, Vector3.right) * Vector3.forward;
                shotVector = Quaternion.AngleAxis(aimAngle, Vector3.up) * shotVector;
                /*shotVector = shot
                    Quaternion.AngleAxis(verticalAngle, )*/
                //start shooting
                if (Input.GetButtonDown(SHOOTBUTTON)) {
                    currentPower = 0;
                    state = GolfState.power;
                    cycleTimer = 0;
                }
                //ui
                bar.localScale = new Vector3(1, 0, 1);
                cursor.localScale = new Vector3(0, 0, 0);
                line.enabled = true;
                updateLine();
                currentPower = shotPower;
                break;
            case GolfState.power:
                updateLine();
                //bar going up
                if (cycleTimer < cycleSpeed)
                {
                    currentPower = shotPower * (cycleTimer / cycleSpeed);
                    bar.localScale = new Vector3(1, cycleTimer / cycleSpeed, 1);
                }
                //bar going down
                else
                {
                    currentPower = shotPower * ((cycleSpeed * 2 - cycleTimer) / cycleSpeed);
                    bar.localScale = new Vector3(1, (cycleSpeed * 2 - cycleTimer) / cycleSpeed, 1);
                }

                cycleTimer += Time.fixedDeltaTime;
                if (cycleTimer >= cycleSpeed * 2)
                {
                    state = GolfState.ready;
                }
                //shooting
                if (Input.GetButtonDown(SHOOTBUTTON))
                {
                    shoot();
                    state = GolfState.flying;
                }
                
                break;
            case GolfState.accuracy:
                cursor.localScale = new Vector3(1, 1, 1);
                break;
            case GolfState.flying:
                line.enabled = false;
                break;
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

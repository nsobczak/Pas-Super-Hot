﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 200f;
    [SerializeField] private float dashForce = 0f;
    [SerializeField] private float gravityForce = 9.81f;
    [SerializeField] private float flyDelay = 1f;
    [SerializeField] private float multiplierAnimHigh = 0.1f;
    [SerializeField] private float multiplierAnimMid = 0.04f;
    [SerializeField] private float multiplierAnimLow = 0.01f;

    [SerializeField] private Rigidbody rb = default;
    [SerializeField] private Transform playerTransform = default;
    [SerializeField] private GameObject feet = default;
    [SerializeField] private LayerMask groundLayer = default;
    public float rotationSpeed = 50f;

    private float multiplierAnim = 0f;
    private Vector3 gravityDir = Vector3.down;
    private bool isGrounded = true;
    private bool useGravity = true;
    private bool hasPowers = true;
    private bool isDashing = false;
    private Animator[] animators;
    private bool isMoving = false;

    private void Awake()
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        //Debug.Log(platforms.Length);
        animators = new Animator[platforms.Length - 4];//-4 is for 4 walls
        int i = 0;
        foreach (GameObject platform in platforms)
        {
            bool isAnim = platform.TryGetComponent<Animator>(out animators[i]);
            //Debug.Log("isAnim = " + isAnim + (isAnim ? "" : " name" + platform.name));
            if (isAnim) i++;
        }
    }
    private void FixedUpdate()
    {
        // Apply gravity
        if (useGravity)
        {
            if (isGrounded && hasPowers)
                rb.AddRelativeForce(Vector3.down * gravityForce * Time.fixedDeltaTime);
            else
                rb.AddForce(Vector3.down * gravityForce * Time.fixedDeltaTime);
        }
    }

    private void Update()
    {
        playerTransform.Rotate(0f,
            Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime,
            0f);
        if (Physics.OverlapSphere(feet.transform.position, 1f, groundLayer).Length > 0)
            isGrounded = true;
        else
            isGrounded = false;
        if (isGrounded)
        {
            Vector3 newVelocity = speed * Time.deltaTime * (transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical"));

            isMoving = !newVelocity.Equals(Vector3.zero);
            if (isMoving)
                rb.velocity = newVelocity;

        }
        Debug.Log(multiplierAnim);
        ChangeAnimationSpeed();
    }

    private void ChangeAnimationSpeed()
    {
        if (isDashing && multiplierAnim != multiplierAnimHigh)
        {
            multiplierAnim = multiplierAnimHigh;
            foreach (Animator anim in animators)
            {
                anim.speed = multiplierAnim;
            }
        }
        else if (!isDashing && !isMoving && multiplierAnim != multiplierAnimLow)
        {
            multiplierAnim = multiplierAnimLow;
            foreach (Animator anim in animators)
            {
                anim.speed = multiplierAnim;
            }
        }
        else if ((isMoving || !isGrounded) && multiplierAnim != multiplierAnimMid)
        {
            multiplierAnim = multiplierAnimMid;
            foreach (Animator anim in animators)
            {
                anim.speed = multiplierAnim;
            }
        }
    }

    public void Dash(Vector3 dir)
    {
        if (!isGrounded)
            return;

        rb.AddForce(dir * dashForce, ForceMode.Impulse);
        useGravity = false;
        isDashing = true;

        StartCoroutine(nameof(ResetUseGravity), flyDelay);
        StartCoroutine(nameof(ResetIsDashing), 0.1f);
    }

    public void LosePowers()
    {
        hasPowers = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Platform"))
        {
            ContactPoint contactPoint = collision.GetContact(0);
            rb.velocity = Vector3.zero;
            playerTransform.position = contactPoint.point - contactPoint.normal * feet.transform.localPosition.y;
            float switchDir = (Vector3.Angle(collision.collider.transform.up, playerTransform.position - contactPoint.point) < 90) ? 0 : 180; // Depends on the normal
            playerTransform.rotation = Quaternion.Euler(collision.transform.rotation.eulerAngles + Vector3.forward * switchDir);
            //GameObject emptyObject = new GameObject();
            playerTransform.SetParent(collision.transform);
            Vector3 parentScale = collision.transform.localScale;
            playerTransform.localScale = new Vector3(1 / parentScale.x, 1 / parentScale.y, 1 / parentScale.z);
        }
    }

    //private void OnCollisionStay(Collision collision)
    //{
    //    if (!isDashing)
    //    {
    //        if (collision.collider.CompareTag("Platform"))
    //        {
    //            ContactPoint contactPoint = collision.GetContact(0);
    //            float difference = Vector3.Angle(contactPoint.normal, playerTransform.up);
    //            playerTransform.Rotate(Vector3.forward * difference);
    //        }
    //    }
    //}

    private IEnumerator ResetUseGravity(float delay)
    {
        yield return new WaitForSeconds(delay);
        useGravity = true;
    }
    private IEnumerator ResetIsDashing(float delay)
    {
        yield return new WaitForSeconds(delay);
        isDashing = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feet.transform.position, 1f);
    }
}
// Copyright (c) 2017, Timothy Ned Atton.
// All rights reserved.
// nedmakesgames@gmail.com
// This code was written while streaming on twitch.tv/nedmakesgames
//
// This file is part of Raycast Platformer.
//
// Raycast Platformer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Raycast Platformer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Raycast Platformer.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using System.Collections;

public class RaycastEngine : MonoBehaviour {

    private enum JumpState {
        None=0, Holding, 
    }

    [SerializeField]
    private LayerMask platformMask;
    [SerializeField]
    private float parallelInsetLen;
    [SerializeField]
    private float perpendicularInsetLen;
    [SerializeField]
    private float groundTestLen;
    [SerializeField]
    private float gravity;
    [SerializeField]
    private float horizSpeedUpAccel;
    [SerializeField]
    private float horizSpeedDownAccel;
    [SerializeField]
    private float horizSnapSpeed;
    [SerializeField]
    private float horizMaxSpeed;
    [SerializeField]
    private float jumpInputLeewayPeriod;
    [SerializeField]
    private float jumpStartSpeed;
    [SerializeField]
    private float jumpMaxHoldPeriod;
    [SerializeField]
    private float jumpMinSpeed;
    [SerializeField]
    private AudioSource jumpSFX, landSFX, startMoveSFX;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 velocity;

    private RaycastMoveDirection moveDown;
    private RaycastMoveDirection moveLeft;
    private RaycastMoveDirection moveRight;
    private RaycastMoveDirection moveUp;

    private RaycastCheckTouch groundDown;

    private Vector2 lastStandingOnPos;
    private Vector2 lastStandingOnVel;
    private Collider2D lastStandingOn;

    private float jumpStartTimer;
    private float jumpHoldTimer;
    private bool jumpInputDown;
    private JumpState jumpState;
    private bool lastGrounded;

    void Start() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        moveDown = new RaycastMoveDirection(new Vector2(-0.5f, -0.75f), new Vector2(0.5f, -0.75f), Vector2.down, platformMask, 
            Vector2.right * parallelInsetLen, Vector2.up * perpendicularInsetLen);
        moveLeft = new RaycastMoveDirection(new Vector2(-0.5f, -0.75f), new Vector2(-0.5f, 0.75f), Vector2.left, platformMask,
            Vector2.up * parallelInsetLen, Vector2.right * perpendicularInsetLen);
        moveUp = new RaycastMoveDirection(new Vector2(-0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.up, platformMask,
            Vector2.right * parallelInsetLen, Vector2.down * perpendicularInsetLen);
        moveRight= new RaycastMoveDirection(new Vector2(0.5f, -0.75f), new Vector2(0.5f, 0.75f), Vector2.right, platformMask,
            Vector2.up * parallelInsetLen, Vector2.left * perpendicularInsetLen);

        groundDown = new RaycastCheckTouch(new Vector2(-0.5f, -0.75f), new Vector2(0.5f, -0.75f), Vector2.down, platformMask,
            Vector2.right * parallelInsetLen, Vector2.up * perpendicularInsetLen, groundTestLen);
    }

    private int GetSign(float v) {
        if(Mathf.Approximately(v, 0)) {
            return 0;
        } else if(v > 0) {
            return 1;
        } else {
            return -1;
        }
    }

    private void Update() {
        jumpStartTimer -= Time.deltaTime;
        bool jumpBtn = Input.GetButton("Jump");
        if(jumpBtn && jumpInputDown == false) {
            jumpStartTimer = jumpInputLeewayPeriod;
        }
        jumpInputDown = jumpBtn;
    }

    private void FixedUpdate() {

        Collider2D standingOn = groundDown.DoRaycast(transform.position);
        bool grounded = standingOn != null;
        if(grounded && lastGrounded == false) {
            landSFX.Play();
        }
        lastGrounded = grounded;

        switch(jumpState) {
        case JumpState.None:
            if(grounded && jumpStartTimer > 0) {
                jumpStartTimer = 0;
                jumpState = JumpState.Holding;
                jumpHoldTimer = 0;
                velocity.y = jumpStartSpeed;
                jumpSFX.Play();
            }
            break;
        case JumpState.Holding:
            jumpHoldTimer += Time.deltaTime;
            if(jumpInputDown == false || jumpHoldTimer >= jumpMaxHoldPeriod) {
                jumpState = JumpState.None;
                velocity.y = Mathf.Lerp(jumpMinSpeed, jumpStartSpeed, jumpHoldTimer / jumpMaxHoldPeriod);

                // Lerp!
                //float p = jumpHoldTimer / jumpMaxHoldPeriod;
                //velocity.y = jumpMinSpeed + (jumpStartSpeed - jumpMinSpeed) * p;
            }
            break;
        }

        float horizInput = Input.GetAxisRaw("Horizontal");
        int wantedDirection = GetSign(horizInput);
        int velocityDirection = GetSign(velocity.x);

        if(wantedDirection != 0) {
            if(wantedDirection != velocityDirection) {
                velocity.x = horizSnapSpeed * wantedDirection;
                startMoveSFX.Play();
            } else {
                velocity.x = Mathf.MoveTowards(velocity.x, horizMaxSpeed * wantedDirection, horizSpeedUpAccel * Time.deltaTime);
            }
        } else {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, horizSpeedDownAccel * Time.deltaTime);
        }

        if(jumpState == JumpState.None) {
            velocity.y -= gravity * Time.deltaTime;
        }

        Vector2 displacement = Vector2.zero;
        Vector2 wantedDispl = velocity * Time.deltaTime;

        if(standingOn != null) {
            if(lastStandingOn == standingOn) {
                lastStandingOnVel = (Vector2)standingOn.transform.position - lastStandingOnPos;
                wantedDispl += lastStandingOnVel;
            } else if(standingOn == null) {
                velocity += lastStandingOnVel / Time.deltaTime;
                wantedDispl += lastStandingOnVel;
            }
            lastStandingOnPos = standingOn.transform.position;
        }
        lastStandingOn = standingOn;

        if(wantedDispl.x > 0) {
            displacement.x = moveRight.DoRaycast(transform.position, wantedDispl.x);
        } else if(wantedDispl.x < 0) {
            displacement.x = -moveLeft.DoRaycast(transform.position, -wantedDispl.x);
        }
        if(wantedDispl.y > 0) {
            displacement.y = moveUp.DoRaycast(transform.position, wantedDispl.y);
        } else if(wantedDispl.y < 0) {
            displacement.y = -moveDown.DoRaycast(transform.position, -wantedDispl.y);
        }

        if(Mathf.Approximately(displacement.x, wantedDispl.x) == false) {
            velocity.x = 0;
        }
        if(Mathf.Approximately(displacement.y, wantedDispl.y) == false) {
            velocity.y = 0;
        }

        transform.Translate(displacement);

        if(jumpState == JumpState.Holding) {
            animator.Play("Jump");
        } else {
            if(grounded) {
                if(wantedDirection == 0) {
                    animator.Play("Idle");
                } else {
                    animator.Play("Move");
                }
            } else {
                if(velocity.y < 0) {
                    animator.Play("Fall");
                } else {
                    animator.Play("Jump");
                }
            }
        }
        if(wantedDirection != 0) {
            spriteRenderer.flipX = wantedDirection < 0;
        }
    }
}

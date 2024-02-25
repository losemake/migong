using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    public NavMeshAgent nav;
    public float speed = 5f;

    public Transform target;
    public Transform targetHome;
    public Transform targetWalk;

    private Animator animator;
    AnimatorStateInfo stateinfo;
    private CharacterController controller;

    public float angle = 120f;                       //检测前方角度范围

    public float distance = 25f;                    //检测距离
    public float rotatePerSecond = 360f;             //每秒旋转角度

    public float accuracy = 1f;                     //检测精度

    private float chaseTime = 0;
    private float chaseTimeMax = 3;
    private bool ifchase = false;
    private bool ifhome = false;
    private bool ifwalk = false;
    private bool gohome = false;

    private bool ifattack = false;

    private double lookTimeInterval = 1.0;
    private double lookTimeMaxInterval = 0.5;
    private Vector3 position1;
    private Vector3 position2;


    public UnityEngine.UI.Text lifeBlood;


    private string playerTag = "Player";


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("attack", false);
        stateinfo = animator.GetCurrentAnimatorStateInfo(0);

        
    }

    // Update is called once per frame
    void Update()
    {
        if (Look())    //检测到玩家
        {
            ifwalk = false;
            ifhome = false;
            lookTimeInterval = 0;   //检测到玩家时，检测时间间隔归零
            ifchase = true;
            this.nav.SetDestination(this.target.position);
            //UnityEngine.Debug.Log("看见了在追");
        }
        else if(lookTimeInterval < lookTimeMaxInterval)  //在检测时间间隔内，继续追
        {
            lookTimeInterval += Time.deltaTime;  //没检测到，增加间隔时间
            ifchase = true;
            this.nav.SetDestination(this.target.position);
            //UnityEngine.Debug.Log("追在检测");
        }
        else if (ifchase)    //消失后继续追数秒
        {
            chaseTime += Time.deltaTime;
            //UnityEngine.Debug.Log("时间：" + chaseTime);
            this.nav.SetDestination(this.target.position);
            if (chaseTime > chaseTimeMax)
            {
                ifchase = false;
                chaseTime = 0;
            }
            //UnityEngine.Debug.Log("没看见继续追");

        }
        else if (!ifchase)
        {
            if (!ifNeibor(targetHome) && !ifwalk)
            {
                //UnityEngine.Debug.Log("第一个if");
                this.nav.SetDestination(this.targetHome.position);
            }
            else if (ifNeibor(targetHome) && !ifhome)
            {
                //UnityEngine.Debug.Log("d第二个");
                ifhome = true;
                ifwalk = true;
            }
            else if ((ifNeibor(targetHome) || ifNeibor(targetWalk)) || ifwalk)//回家了巡逻
            {
                //UnityEngine.Debug.Log("我在");
                if (gohome)
                {
                    //UnityEngine.Debug.Log("我在回家");
                    this.nav.SetDestination(this.targetHome.position);
                    if (ifNeibor(targetHome))
                    {
                        //UnityEngine.Debug.Log("我到家了");
                        this.nav.SetDestination(this.targetWalk.position); gohome = false;
                    }
                }
                else
                {
                   // UnityEngine.Debug.Log("我在去巡逻");
                    this.nav.SetDestination(this.targetWalk.position);
                    if (ifNeibor(targetWalk))
                    {
                        //UnityEngine.Debug.Log("我巡逻完了");
                        this.nav.SetDestination(this.targetHome.position);
                        gohome = true;
                    }
                }

            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == playerTag)
        {
            animator.SetBool("attack", true);
            UnityEngine.Debug.Log("攻击");
            ifattack = true;
            Vector3 movement = new Vector3(0, 0f, 0);
            transform.Translate(movement * Time.deltaTime * speed);
            stateinfo = animator.GetCurrentAnimatorStateInfo(0);
            //PlayerController.PlayerControll.Attacked();
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == playerTag)
        {
            Vector3 movement = new Vector3(0, 0f, 0);
            transform.Translate(movement * Time.deltaTime * speed);
            stateinfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateinfo.normalizedTime >= 0.5 && stateinfo.IsName("Creature_armature1|attack_1"))
            {
                animator.SetBool("attack", false);
                ifattack = false;
                //UnityEngine.Debug.Log("攻击了:" + stateinfo.normalizedTime);

            }
            //UnityEngine.Debug.Log("走路123");
            stateinfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateinfo.IsName("Creature_armature1|walk"))
            {
                //UnityEngine.Debug.Log("走路");
                animator.SetBool("attack", true);
                if (!ifattack)
                {
                    PlayerController.PlayerControll.Attacked();
                    ifattack = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == playerTag)
        {
            animator.SetBool("attack", false);
           // UnityEngine.Debug.Log("取消攻击");
        }
    }

    private bool Look()
    {
        float subAngle = angle / accuracy;          //每条射线需要检测的角度范围
        for (int i = 0; i < accuracy; i++)
            if (LookAround(Quaternion.Euler(0, -angle / 2 + i * subAngle + Mathf.Repeat(rotatePerSecond * Time.time, subAngle), 0), distance, Color.blue))
                return true;
        return false;
    }
    public bool LookAround(Quaternion eulerAnger, float _lookRange, Color DebugColor)
    {
        position1 = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        position2 = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z+1);
        position1.y += 1;
        UnityEngine.Debug.DrawRay(position1, eulerAnger * position2.normalized * _lookRange, DebugColor);
        RaycastHit hit;
        if (Physics.Raycast(position1, eulerAnger * position2, out hit, _lookRange) && hit.collider.CompareTag(playerTag))
        {
            //UnityEngine.Debug.Log(hit.transform);
            //UnityEngine.Debug.Log("检测到玩家");
            return true;
        }
        return false;
    }



    public bool ifNeibor(Transform targetPosition)
    {
        float x1 = Math.Abs(transform.position.x - targetPosition.position.x);
        float z1 = Math.Abs(transform.position.z - targetPosition.position.z);
        if (Math.Sqrt(Math.Pow(x1,2)+Math.Pow(z1,2))<5)
        {return true;}
        else{return false;}
    }
}

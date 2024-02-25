using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Range(0, 30f)]
    public float Player_Speed = 5f;
    public CharacterController controller;

    public float gravity = -9.81f;
    public Vector3 gravity_vector3;
    public Transform grabity_transform;
    private bool isGround;
    public float gravity_radius = 0.5f;
    public LayerMask gravity_layer;

    private int blood = 3;
    public UnityEngine.UI.Text lifeBlood;
    public GameObject lossText;
    public GameObject winText;
    public string exitTag = "exit";


    private int score = 0;

    public static PlayerController PlayerControll;

    private void Awake()
    {
        PlayerControll = this;
    }


    void Start()
    {
        if (controller == null) {
            controller = GetComponent<CharacterController>();
        }
        lossText.SetActive(false);
        winText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        lifeBlood.text = "血量" + blood + " / 分数" + score;
        float move_x = Input.GetAxis("Horizontal");
        float move_y = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * move_x + transform.forward * move_y) * Player_Speed * Time.deltaTime;
        controller.Move(move);
        
    }


    void Player_Physics() {
        // gravity_vector3.y += gravity;
        // isGround = Physics.CheckSphere(grav)

    }

    public void Attacked()
    {
        blood--;
        
        if (blood <= 0)
        {
            lossText.SetActive(true);
            UnityEngine.Debug.Log("GAME OVER" + blood);
        }
        UnityEngine.Debug.Log(blood);
    }

    public int getBlood()
    {
        return blood;
    }

    public int getScore()
    {
        return score;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == exitTag)
        {
            winText.SetActive(true);
        }
        if (other.tag == "Collection")
        {
            Destroy(other.gameObject);
            score++;
        }
    }

}

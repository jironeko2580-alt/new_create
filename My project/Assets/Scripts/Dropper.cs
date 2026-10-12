using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>マウスX座標に追従し、クリックでボールを投下する</summary>
public class Dropper : MonoBehaviour
{
    [Tooltip("箱の内側の左端X")]
    [SerializeField] float minX = -3f;
    [Tooltip("箱の内側の右端X")]
    [SerializeField] float maxX = 3f;
    [Tooltip("投下してから次のボールが出るまでの秒数")]
    [SerializeField] float cooldown = 0.5f;

    Camera cam;
    Ball held;
    float cooldownTimer;

    void Start()
    {
        cam = Camera.main;
        PrepareNext();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver) return;

        var pointer = Pointer.current;
        if (pointer == null) return;

        if (held == null)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer > 0f) return;
            PrepareNext();
        }

        Vector3 world = cam.ScreenToWorldPoint(pointer.position.ReadValue());
        float r = held.Radius;
        float x = Mathf.Clamp(world.x, minX + r, maxX - r);
        transform.position = new Vector3(x, transform.position.y, 0f);
        held.transform.position = transform.position;

        if (pointer.press.wasPressedThisFrame)
        {
            held.Release();
            held = null;
            cooldownTimer = cooldown;
        }
    }

    void PrepareNext()
    {
        var gm = GameManager.Instance;
        held = gm.SpawnBall(gm.RollDropLevel(), transform.position, false);
    }
}

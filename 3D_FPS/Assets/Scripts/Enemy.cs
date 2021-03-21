using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    /// <summary>
    /// 玩家的變形資訊
    /// </summary>
    private Transform player;
    /// <summary>
    ///  代理器
    /// </summary>
    private NavMeshAgent nav;
    /// <summary>
    /// 動畫控制器
    /// </summary>
    private Animator ani;

    [Header("移動速度"), Range(0, 30)]
    public float speed = 2.5f;
    [Header("攻擊範圍"), Range(2, 100)]
    public float rangeAttack = 5f;
    [Header("生成子彈位置")]
    public Transform point;
    [Header("子彈")]
    public GameObject bullet;
    [Header("子彈速度"), Range(0, 3000)]
    public float speedBullet = 500;
    [Header("開槍間隔"), Range(0f, 5f)]
    public float interval = 0.5f;
    [Header("面向玩家的速度"), Range(0f, 100f)]
    public float speedFace = 5f;
    [Header("彈匣目前數量")]
    public int bulletCount = 30;
    [Header("彈匣數量")]
    public int bulletClip = 30;
    [Header("換彈匣的時間"), Range(0f, 5f)]
    public float addBulletTime = 2.5f;
    [Header("音效")]
    public AudioClip soundFire;
    public AudioClip soundAddBullet;
    public AudioClip soundHeadShot;

    private float timer;
    private bool isAddBullet;
    /// <summary>
    /// 血量
    /// </summary>
    private float hp = 100;

    private GameManager gm;
    private AudioSource aud;

    private void Awake()
    {
        player = GameObject.Find("玩家").transform;       // 取得玩家變形資訊
        aud = GetComponent<AudioSource>();
        ani = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();                 // 取得導覽代理器
        nav.speed = speed;                                  // 速度
        nav.stoppingDistance = rangeAttack;                 // 停止距離

        gm = FindObjectOfType<GameManager>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);                // 指定顏色
        Gizmos.DrawSphere(transform.position, rangeAttack);     // 繪製球體
    }

    private void Update()
    {
        if (isAddBullet) return;
        Track();
    }

    /// <summary>
    /// 追蹤
    /// </summary>
    private void Track()
    {
        nav.SetDestination(player.position);                    // 代理器.設定目的地(角色.座標)

        if (nav.remainingDistance > rangeAttack)                // 如果 剩餘的距離 > 攻擊範圍
        {
            ani.SetBool("跑步開關", true);                      // 就 跑步
        }
        else
        {
            Fire();                                             // 否則 就 開槍
        }
    }

    [Header("攻擊力"), Range(0f, 100f)]
    public float attack = 5f;

    /// <summary>
    /// 開槍
    /// </summary>
    private void Fire()
    {
        ani.SetBool("跑步開關", false);

        if (timer >= interval)                                                      // 如果 計時器 >= 間隔
        {
            aud.pitch = Random.Range(0.8f, 0.9f);
            aud.PlayOneShot(soundFire, Random.Range(0.2f, 0.4f));
            timer = 0;                                                              // 歸零
            GameObject temp = Instantiate(bullet, point.position, point.rotation);  // 暫存 = 生成子彈
            temp.GetComponent<Rigidbody>().AddForce(point.right * -speedBullet);    // 取得子彈 剛體 添加推力(前方 * 速度)
            temp.GetComponent<Bullet>().attack = attack;
            temp.name += name;                                                      // 添加敵人名稱
            ManageBulletCount();
        }
        else
        {
            timer += Time.deltaTime;                                                // 否則 累加時間
            FaceToPlayer();
        }
    }

    /// <summary>
    /// 管理子彈數量
    /// </summary>
    private void ManageBulletCount()
    {
        bulletCount--;

        if (bulletCount <= 0)
        {
            StartCoroutine(AddBullet());
        }
    }

    /// <summary>
    /// 添加子彈
    /// </summary>
    private IEnumerator AddBullet()
    {
        aud.PlayOneShot(soundAddBullet, Random.Range(0.5f, 0.85f));
        ani.SetTrigger("換彈匣觸發");
        isAddBullet = true;
        yield return new WaitForSeconds(addBulletTime);
        isAddBullet = false;
        bulletCount += bulletClip;
    }

    /// <summary>
    /// 面向玩家
    /// </summary>
    private void FaceToPlayer()
    {
        Quaternion faceAngle = Quaternion.LookRotation(player.position - transform.position);                   // 面向向量
        transform.rotation = Quaternion.Lerp(transform.rotation, faceAngle, Time.deltaTime * speedFace);        // 差值(A B 速度)
    }

    /// <summary>
    /// 受傷處理
    /// </summary>
    /// <param name="getDamage">接收到的傷害</param>
    private void Damage(float getDamage)
    {
        if (hp <= 0) return;

        hp -= getDamage;

        if (hp <= 0) Dead();
    }

    private void Dead()
    {
        ani.SetTrigger("死亡觸發");
        GetComponent<SphereCollider>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        enabled = false;

        // 敵人死亡要更新玩家殺敵數
        gm.UpdateDataKill(ref GameManager.killPlayer, gm.textDataPlayer, "玩家", GameManager.deadPlayer);

        if (name == "敵方 1") gm.UpdateDataDead(GameManager.killNpc1, gm.textDataNpc1, "電腦１", ref GameManager.deadNpc1);
        else if (name == "敵方 2") gm.UpdateDataDead(GameManager.killNpc2, gm.textDataNpc2, "電腦２", ref GameManager.deadNpc2);
        else if (name == "敵方 3") gm.UpdateDataDead(GameManager.killNpc3, gm.textDataNpc3, "電腦３", ref GameManager.deadNpc3);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "子彈")
        {
            float damage = collision.gameObject.GetComponent<Bullet>().attack;

            if (collision.contacts[0].thisCollider.GetType().Equals(typeof(SphereCollider)))
            {
                aud.PlayOneShot(soundHeadShot, Random.Range(1f, 1.5f));
                Damage(100);
            }
            else Damage(damage);
        }
    }
}

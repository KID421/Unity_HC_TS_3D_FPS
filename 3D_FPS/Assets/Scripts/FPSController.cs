using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSController : MonoBehaviour
{
    #region 基本欄位
    [Header("移動"), Range(0, 2000)]
    public float speed;
    [Header("旋轉"), Range(0, 2000)]
    public float turn;
    [Header("跳越高度"), Range(0, 2000)]
    public float jump = 100;
    [Header("地板偵測位移")]
    public Vector3 floorOffset;
    [Header("地板偵測半徑"), Range(0, 20)]
    public float floorRadius = 1;
    [Header("血量與血條")]
    public Text textHp;
    public Image imgHp;

    /// <summary>
    /// 血量
    /// </summary>
    private float hp = 100;
    private float hpMax = 100;
    private Animator ani;
    private Rigidbody rig;
    #endregion

    #region 開槍欄位
    [Header("生成子彈的位置")]
    public Transform pointFire;
    [Header("子彈")]
    public GameObject bullet;
    [Header("子彈目前數量")]
    public int bulletCurrent = 30;
    [Header("子彈總數")]
    public int bulletTotal = 150;
    [Header("彈夾裝填子彈數量")]
    public int bulletClip = 30;
    [Header("子彈速度")]
    public int bulletSpeed = 450;
    [Header("文字：子彈目前數量")]
    public Text textBulletCurrent;
    [Header("文字：子彈總數")]
    public Text textBulletTotal;
    [Header("補充子彈時間"), Range(0, 5)]
    public float addBulletTime = 1;
    [Header("開槍音效")]
    public AudioClip soundFire;
    [Header("換彈夾音效")]
    public AudioClip soundAddBullet;
    [Header("開槍間隔時間"), Range(0f, 1f)]
    public float fireInterval = 0.1f;
    [Header("攻擊力"), Range(0f, 100f)]
    public float attack = 5f;

    private AudioSource aud;
    private float timer;

    /// <summary>
    /// 是否在補充子彈
    /// </summary>
    private bool isAddBullet;
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.position + floorOffset, floorRadius);
    }

    private Transform traMain;
    private Transform traCam;

    private void Awake()
    {
        Cursor.visible = false;             // 隱藏滑鼠
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();

        // transform.Find("子物件名稱") - 搜尋子物件
        traMain = transform.Find("Main Camera");
        traCam = transform.Find("Camera");
    }

    private void Update()
    {
        Move();
        Jump();
        Fire();
        AddBullet();
    }

    /// <summary>
    /// 開槍
    /// </summary>
    private void Fire()
    {
        // 按下左鍵 並且 子彈 > 0 並且 不是在補充子彈
        if (Input.GetKey(KeyCode.Mouse0) && bulletCurrent > 0 && !isAddBullet)
        {
            if (timer >= fireInterval)
            {
                ani.SetTrigger("開槍觸發");
                timer = 0;
                aud.PlayOneShot(soundFire, Random.Range(0.7f, 1.2f));
                // 扣除子彈並更新介面
                bulletCurrent--;
                textBulletCurrent.text = bulletCurrent.ToString();
                // 暫存子彈 = 生成(物件，座標，角度)
                GameObject temp = Instantiate(bullet, pointFire.position, pointFire.rotation);
                // 暫存子彈.取得剛體.添加推力(生成點的前方 * 速度)
                temp.GetComponent<Rigidbody>().AddForce(pointFire.up * bulletSpeed);
                // 暫存子彈.取得子彈腳本.傷害 = 玩家傷害
                temp.GetComponent<Bullet>().attack = attack;
            }
            else timer += Time.deltaTime;
        }
    }

    /// <summary>
    /// 補充子彈
    /// </summary>
    private void AddBullet()
    {
        // 條件
        // 1. 按 R
        // 2. 不是在補充子彈
        // 3. 子彈總數 大於 0
        // 4. 目前子彈 小於 彈夾
        if (Input.GetKeyDown(KeyCode.R) && !isAddBullet && bulletTotal > 0 && bulletCurrent < bulletClip) StartCoroutine(AddBulletDelay());
    }

    /// <summary>
    /// 補充子彈協程
    /// </summary>
    private IEnumerator AddBulletDelay()
    {
        ani.SetTrigger("換彈夾觸發");
        aud.PlayOneShot(soundAddBullet, Random.Range(0.8f, 1.1f));

        isAddBullet = true;
        yield return new WaitForSeconds(addBulletTime);
        isAddBullet = false;

        int add = bulletClip - bulletCurrent;               // 計算補幾顆子彈

        if (bulletTotal >= add)                             // 如果總數 大於 要補充的子彈
        {
            bulletCurrent += add;
            bulletTotal -= add;
        }
        else                                                // 否則 將剩餘總數 補充過來
        {
            bulletCurrent += bulletTotal;
            bulletTotal = 0;
        }

        textBulletCurrent.text = bulletCurrent.ToString();
        textBulletTotal.text = bulletTotal.ToString();
    }

    /// <summary>
    /// 跳躍
    /// </summary>
    private void Jump()
    {
        // 3D 模式霧裡碰撞偵測
        // 物理.覆蓋球體(中心點 + 位移，半徑，1 << 圖層編號)
        Collider[] hits = Physics.OverlapSphere(transform.position + floorOffset, floorRadius, 1 << 8);

        // 如果 碰撞物件有一個以上 並且 碰撞物件存在 並且 按下空白鍵
        if (hits.Length > 0 && hits[0] && Input.GetKeyDown(KeyCode.Space))
        {
            // 添加推力(0，跳越高度，0)
            rig.AddForce(0, jump, 0);
        }
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        float v = Input.GetAxis("Vertical");                    // W S 上 下 - 前進 1，後退 -1
        float h = Input.GetAxis("Horizontal");                  // A D 左 右

        rig.MovePosition(transform.position + transform.forward * v * speed * Time.deltaTime + transform.right * h * speed * Time.deltaTime);

        float x = Input.GetAxis("Mouse X");                     // 滑鼠左右的值
        transform.Rotate(0, x * Time.deltaTime * turn, 0);      // 旋轉
    }

    /// <summary>
    /// 受傷處理
    /// </summary>
    /// <param name="getDamage">接收到的傷害</param>
    private void Damage(float getDamage)
    {
        if (hp <= 0) return;            // 如果 死亡 就 跳出

        hp -= getDamage;

        if (hp <= 0) Dead();

        textHp.text = hp.ToString();
        imgHp.fillAmount = hp / hpMax;
    }
    
    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        hp = 0;
        ani.SetTrigger("死亡觸發");
        enabled = false;

        StartCoroutine(MoveCamera());
    }

    private IEnumerator MoveCamera()
    {
        traMain.LookAt(transform);              // 攝影機面向
        traCam.LookAt(transform);

        Vector3 posCam = traMain.position;      // 取得 攝影機 座標

        float yCam = posCam.y;                  // 取得 攝影機的 Y
        float yUp = yCam + 3;                   // 攝影機位移

        // 慢慢往上移 每次移動 0.1

        // 當 攝影機 的 Y 小於 位移 Y
        while (yCam < yUp)
        {
            yCam += 0.05f;                              // 遞增
            posCam.y = yCam;                            // 更新 三維向量

            traMain.position = posCam;                  // 更新 主攝影機 座標
            traCam.position = posCam;       

            yield return new WaitForSeconds(0.02f);     // 等待
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "子彈")
        {
            float damage = collision.gameObject.GetComponent<Bullet>().attack;
            Damage(damage);
        }
    }
}
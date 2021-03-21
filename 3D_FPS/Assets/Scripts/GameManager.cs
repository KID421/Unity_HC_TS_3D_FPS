using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // 一般欄位 載入場景後會恢復預設值
    // 靜態欄位 不會恢復
    // 靜態欄位 不會顯示在屬性面板上

    private static int winPlayer;
    private static int winNpc;

    public static int killPlayer;
    public static int killNpc1;
    public static int killNpc2;
    public static int killNpc3;

    public static int deadPlayer;
    public static int deadNpc1;
    public static int deadNpc2;
    public static int deadNpc3;

    [Header("勝利次數 - 玩家")]
    public Text textPlayer;
    [Header("勝利次數 - 電腦")]
    public Text textNpc;
    [Header("資料 - 玩家")]
    public Text textDataPlayer;
    [Header("資料 - 電腦 1")]
    public Text textDataNpc1;
    [Header("資料 - 電腦 2")]
    public Text textDataNpc2;
    [Header("資料 - 電腦 3")]
    public Text textDataNpc3;
    [Header("結束畫面 群組元件")]
    public CanvasGroup group;

    /// <summary>
    /// 紀錄殺死幾隻敵人
    /// </summary>
    private int enemyCount;
    private bool gameOver;

    // 需要傳送地址的參數可以添加 ref - 呼叫時也要加上 ref

    /// <summary>
    /// 更新殺敵數量
    /// </summary>
    /// <param name="kill">要更新的殺敵數</param>
    /// <param name="textKill">要更新的介面</param>
    /// <param name="content">要顯示的文字內容</param>
    /// <param name="dead">要顯示的死亡數量</param>
    public void UpdateDataKill(ref int kill, Text textKill, string content, int dead)
    {
        kill++;
        textKill.text = content + "　" + kill + "　｜　" + dead;
    }

    /// <summary>
    /// 更新死亡數量
    /// </summary>
    public void UpdateDataDead(int kill, Text textDead, string content,ref int dead)
    {
        dead++;
        textDead.text = content + "　" + kill + "　｜　" + dead;

        if (content == "玩家") 
        {
            winNpc++;
            textNpc.text = "勝利次數：" + winNpc;
            StartCoroutine(ShowFinal());
        }
        // 判斷 死亡的是電腦 並且有 三隻 才結束
        else if (content.Contains("電腦"))
        {
            enemyCount++;

            if (enemyCount == 3) 
            {
                winPlayer++;
                textPlayer.text = "勝利次數：" + winPlayer;
                StartCoroutine(ShowFinal());
            }
        }
    }

    /// <summary>
    /// 顯示結束畫面
    /// </summary>
    private IEnumerator ShowFinal()
    {
        float a = group.alpha;

        while (a < 1)
        {
            a += 0.1f;
            group.alpha = a;
            yield return new WaitForSeconds(0.05f);
        }

        gameOver = true;
    }

    /// <summary>
    /// 重新遊戲
    /// </summary>
    private void Replay()
    {
        if (Input.GetKeyDown(KeyCode.Space) && gameOver) SceneManager.LoadScene("遊戲場景");
    }

    private void Update()
    {
        Replay();
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private int winPlayer;
    private int winNpc;

    public int killPlayer;
    public int killNpc1;
    public int killNpc2;
    public int killNpc3;

    public int deadPlayer;
    public int deadNpc1;
    public int deadNpc2;
    public int deadNpc3;

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

        if (content == "玩家") StartCoroutine(ShowFinal());
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
            yield return new WaitForSeconds(0.2f);
        }
    }
}

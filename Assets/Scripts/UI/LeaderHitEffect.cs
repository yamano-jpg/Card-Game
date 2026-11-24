using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 中央後列のカードが倒れて、リーダーがダメージを受けたときの演出
public class LeaderHitEffect : MonoBehaviour
{
    [SerializeField] private RectTransform effectLayer;   // 演出を出す親(Canvas の直下。全画面)
    [SerializeField] private Image orbPrefab;             // 飛ばす光の玉
    [SerializeField] private TMP_Text damageTextPrefab;   // ダメージの数字
    [SerializeField] private RectTransform shakeTarget;   // 揺らす対象(Canvas の中身をまとめた親。空なら揺らさない)

    [SerializeField] private float flightTime = 0.35f;
    [SerializeField] private float shakeTime = 0.2f;
    [SerializeField] private float shakePower = 14f;

    // 演出を始める。fromWorld: 倒れたカードの位置、leader: 当たる対象のリーダー、damage: ダメージ量
    public void Play(Vector3 fromWorld, RectTransform leader, Image leaderFrame, int damage)
    {
        StartCoroutine(Routine(fromWorld, leader, leaderFrame, damage));
    }

    // 数字と、リーダーの点滅だけの、軽い演出(普通の攻撃・魔法・デッキ切れ用)
    public void PlayDamage(RectTransform leader, Image leaderFrame, int damage)
    {
        StartCoroutine(Flash(leaderFrame));
        StartCoroutine(FloatDamage(leader.position, damage));
    }

    private IEnumerator Routine(Vector3 fromWorld, RectTransform leader, Image leaderFrame, int damage)
    {
        // 光の玉
        Image orb = Instantiate(orbPrefab, effectLayer);
        orb.gameObject.SetActive(true);
        RectTransform orbRect = orb.rectTransform;
        orbRect.position = fromWorld;
        Vector3 start = fromWorld;
        Vector3 end = leader.position;

        float t = 0f;
        while (t < flightTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / flightTime);
            float eased = p * p; // 後半で加速する
            orbRect.position = Vector3.Lerp(start, end, eased);
            orbRect.localScale = Vector3.one * Mathf.Lerp(1f, 1.6f, p);
            yield return null;
        }
        Destroy(orb.gameObject);

        // 当たった瞬間: 揺れ・点滅・数字
        StartCoroutine(Shake());
        StartCoroutine(Flash(leaderFrame));
        StartCoroutine(FloatDamage(leader.position, damage));
    }

    private IEnumerator Shake()
    {
        if (shakeTarget == null) yield break;

        Vector2 origin = shakeTarget.anchoredPosition;
        float t = 0f;
        while (t < shakeTime)
        {
            t += Time.unscaledDeltaTime;
            float fade = 1f - t / shakeTime;
            shakeTarget.anchoredPosition = origin + Random.insideUnitCircle * shakePower * fade;
            yield return null;
        }
        shakeTarget.anchoredPosition = origin;
    }

    private IEnumerator Flash(Image frame)
    {
        if (frame == null) yield break;

        Color original = frame.color;
        float t = 0f;
        const float duration = 0.25f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            frame.color = Color.Lerp(new Color(1f, 0.25f, 0.25f), original, t / duration);
            yield return null;
        }
        frame.color = original;
    }

    private IEnumerator FloatDamage(Vector3 at, int damage)
    {
        TMP_Text text = Instantiate(damageTextPrefab, effectLayer);
        text.gameObject.SetActive(true);
        text.text = "-" + damage;
        RectTransform rect = text.rectTransform;
        rect.position = at;

        float t = 0f;
        const float duration = 0.6f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;
            rect.position = at + Vector3.up * (p * 60f);
            Color c = text.color;
            c.a = 1f - p * p;
            text.color = c;
            yield return null;
        }
        Destroy(text.gameObject);
    }
}
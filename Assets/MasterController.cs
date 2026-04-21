using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ノッチの情報を格納する構造体
[Serializable]
public struct NotchInfo
{
    public NotchType notchType;
    public int notchIndex;
    public float acceleration;
    public float maxSpeed;
    public float brakeForce;

    public static NotchInfo GetNeutralNotch()
    {
        return new NotchInfo
        {
            notchType = NotchType.Neutral,
            notchIndex = 0,
            acceleration = 0f,
            maxSpeed = -1f,
            brakeForce = 0f
        };
    }
}

// ノッチの種類(ニュートラル、動力、ブレーキ)
public enum NotchType
{
    Neutral,
    Powered,
    Braking,
}

public class MasterController : MonoBehaviour
{
    [Header("経路情報")]
    [SerializeField]
    private PathMovement pathMovement;

    [Header("ノッチの情報")]
    [SerializeField]
    private List<NotchInfo> acceleratorNotches = new List<NotchInfo>();
    [SerializeField]
    private List<NotchInfo> brakeNotches = new List<NotchInfo>();

    [Header("現在のノッチ")]
    public NotchInfo currentNotchInfo = NotchInfo.GetNeutralNotch();

    [Header("現在の速度")]
    [SerializeField, ReadOnly]
    private float currentSpeed = 0f;
    
    [Header("ノッチのインデックス")]
    [SerializeField, ReadOnly]

    private int acceleratorNotchIndex = 0;
    [SerializeField, ReadOnly]

    private int brakeNotchIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        // 加速ノッチはインデックス順にソート
        acceleratorNotches.Sort((a, b) => a.notchIndex.CompareTo(b.notchIndex));
        // ブレーキノッチはインデックス順にソート
        brakeNotches.Sort((a, b) => a.notchIndex.CompareTo(b.notchIndex));
    }

    // Update is called once per frame
    void Update()
    {
        // 経路情報がない場合は処理しない
        if (pathMovement == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentNotchInfo = NotchInfo.GetNeutralNotch();
            acceleratorNotchIndex = 0;
            brakeNotchIndex = 0;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            
            if (brakeNotchIndex > 0)
            {
                // ブレーキノッチがある場合はブレーキノッチを1段戻す
                brakeNotchIndex--;
                currentNotchInfo = brakeNotches[brakeNotchIndex];
                acceleratorNotchIndex = 0; // 加速ノッチは0に戻す
            }
            else
            {
                currentNotchInfo = acceleratorNotches[acceleratorNotchIndex];
                // ブレーキノッチがない場合は加速ノッチを1段進める
                if(acceleratorNotchIndex < acceleratorNotches.Count - 1)
                {
                    acceleratorNotchIndex++;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (acceleratorNotchIndex > 0)
            {
                // 加速ノッチがある場合は加速ノッチを1段戻す
                acceleratorNotchIndex--;
                currentNotchInfo = acceleratorNotches[acceleratorNotchIndex];
                brakeNotchIndex = 0; // ブレーキノッチは0に戻す
            }
            else
            {
                currentNotchInfo = brakeNotches[brakeNotchIndex];
                // 加速ノッチがない場合はブレーキノッチを1段進める
                if(brakeNotchIndex < brakeNotches.Count - 1)
                {
                    brakeNotchIndex++;
                }
            }
        }

        // currentRoutePositionを更新
        if (currentNotchInfo.notchType == NotchType.Powered)
        {
            Debug.Log("accelerator");
            if (currentNotchInfo.acceleration > 0)
            {
                currentSpeed += currentNotchInfo.acceleration * Time.deltaTime;
            }
            if (currentNotchInfo.maxSpeed > 0)
            {
                currentSpeed = Mathf.Clamp(currentSpeed, 0, currentNotchInfo.maxSpeed);
            }
        }
        if (currentNotchInfo.notchType == NotchType.Braking)
        {
            Debug.Log("brake");
            if (currentNotchInfo.brakeForce > 0)
            {
                currentSpeed -= currentNotchInfo.brakeForce * Time.deltaTime;
            }
            currentSpeed = Mathf.Max(currentSpeed, 0); // 速度は0以上にする
        }

        pathMovement.currentRoutePosition += currentSpeed * Time.deltaTime;
    }
}


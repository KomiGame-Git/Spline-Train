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

    [Header("ニュートラル減速")]
    [SerializeField]
    private float neutralDeceleration = 0.5f;

    [Header("現在の速度")]
    [SerializeField, ReadOnly]
    private float currentSpeed = 0f;


    [Header("ノッチのインデックス")]
    [SerializeField, ReadOnly]

    private int acceleratorNotchIndex = -1; // -1はニュートラルを表す
    [SerializeField, ReadOnly]

    private int brakeNotchIndex = -1; // -1はニュートラルを表す

    // Start is called before the first frame update
    void Start()
    {
        // 加速ノッチはインデックス順にソート
        acceleratorNotches.Sort((a, b) => a.notchIndex.CompareTo(b.notchIndex));
        // ブレーキノッチはインデックス順にソート
        brakeNotches.Sort((a, b) => a.notchIndex.CompareTo(b.notchIndex));

        currentNotchInfo = NotchInfo.GetNeutralNotch();
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
            acceleratorNotchIndex = -1;
            brakeNotchIndex = -1;
            NotchChanged();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {

            if (brakeNotchIndex >= 0)
            {
                // ブレーキノッチがある場合はブレーキノッチを1段戻す
                brakeNotchIndex--;
                acceleratorNotchIndex = -1;
                NotchChanged();
            }
            else
            {
                // 加速ノッチがある場合は加速ノッチを1段進める
                acceleratorNotchIndex = Mathf.Min(acceleratorNotchIndex + 1, acceleratorNotches.Count - 1);
                NotchChanged();
            }
        }



        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (acceleratorNotchIndex >= 0)
            {
                // 加速ノッチがある場合は加速ノッチを1段戻す
                acceleratorNotchIndex--;
                brakeNotchIndex = -1;
                NotchChanged();
            }
            else
            {
                brakeNotchIndex = Mathf.Min(brakeNotchIndex + 1, brakeNotches.Count - 1);
                NotchChanged();
            }
        }

        // currentRoutePositionを更新
        if (currentNotchInfo.notchType == NotchType.Powered)
        {
            Debug.Log("accelerator");
            if (currentSpeed < currentNotchInfo.maxSpeed)
            {
                if (currentNotchInfo.acceleration > 0)
                {
                    currentSpeed += currentNotchInfo.acceleration * Time.deltaTime;
                }
            }
            else
            {
                currentSpeed -= neutralDeceleration * Time.deltaTime;
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
        if (currentNotchInfo.notchType == NotchType.Neutral)
        {
            Debug.Log("neutral");
            if (currentSpeed > 0)
            {
                currentSpeed -= neutralDeceleration * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0); // 速度は0以上にする
            }
        }

        pathMovement.currentRoutePosition += currentSpeed * Time.deltaTime;


        void NotchChanged()
        {
            if (acceleratorNotchIndex >= 0)
            {
                currentNotchInfo = acceleratorNotches[acceleratorNotchIndex];
            }
            else if (brakeNotchIndex >= 0)
            {
                currentNotchInfo = brakeNotches[brakeNotchIndex];
            }
            else
            {
                currentNotchInfo = NotchInfo.GetNeutralNotch();
            }
        }
    }
}

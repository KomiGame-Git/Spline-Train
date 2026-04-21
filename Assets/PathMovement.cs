using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class PathMovement : MonoBehaviour
{
    // スプライン
    [SerializeField] 
    private SplineContainer _splineContainer;

    // インスペクターから編集可能な経路情報
    [Serializable]
    private struct PathInfo
    {
        // SplineContainerのどのスプラインを使うかをインデックスで指定
        public int splineIndex;
        
        // 上記インデックスのスプラインにおける範囲情報
        public SplineRange range;
    }


    [Header("経路の作成情報")]
    // 経路の作成情報
    [SerializeField] 
    private PathInfo[] _path;

    // 経路上の位置を[0～1]の範囲で表すパラメーター
    private float _t;

    [Header("経路上の位置")]
    public float currentRoutePosition;
    
    [Header("経路の長さ")]
    [SerializeField, ReadOnly] 
    private float routeLength;

    // 実際に使われるスプラインの経路情報
    private SplinePath _splinePath;

    // 初期化
    private void Start()
    {
        // SplinePathインスタンスを予め作成
        OnCreateSplinePath();
    }

    // フレーム更新
    private void Update()
    {
        // スプラインに沿って移動させる
        OnMove();
    }

#if UNITY_EDITOR

    // インスペクターから編集されたとき
    private void OnValidate()
    {
        // SplinePathインスタンスと移動処理を一緒に行う
        OnCreateSplinePath();
        OnMove();
        routeLength = _splinePath != null ? _splinePath.GetLength() : 0;
    }

#endif

    // SplinePathインスタンスを作成する
    private void OnCreateSplinePath()
    {
        if (_splineContainer == null) return;

        // ワールド空間のスプラインとして扱うため、変換行列を指定する
        float4x4 matrix = _splineContainer.transform.localToWorldMatrix;

        // 経路の作成情報からSplinePathインスタンスを作成
        _splinePath = new SplinePath(
            // PathInfoからSplineSlice型のコレクションに変換
            _path.Select(x => new SplineSlice<Spline>(
                    _splineContainer[x.splineIndex],
                    x.range,
                    matrix
                )
            )
        );
    }

    // 予め作成されたSplinePathインスタンスの経路に沿って自身を移動させる
    private void OnMove()
    {
        // 経路情報(SplinePathインスタンス)がない場合は処理しない
        if (_splinePath == null)
        {
            return;
        }

        // 経路上の位置を[0～経路長]の範囲で丸める
        currentRoutePosition = Mathf.Clamp(currentRoutePosition, 0, routeLength);

        _t = currentRoutePosition / routeLength;

        float3 position_f3;
        float3 tangent_f3;
        float3 upVector_f3;
        // スプライン上の位置・向き・上ベクトルを取得
        if (!_splinePath.Evaluate(_t, out position_f3, out tangent_f3, out upVector_f3))
        {
            return;
        }

        Vector3 position = new Vector3(position_f3.x, position_f3.y, position_f3.z);
        Vector3 tangent = new Vector3(tangent_f3.x, tangent_f3.y, tangent_f3.z);
        Vector3 upVector = new Vector3(upVector_f3.x, upVector_f3.y, upVector_f3.z);
        // Transformに反映
        transform.SetPositionAndRotation(
            position,
            Quaternion.LookRotation(tangent, upVector)
        );
    }
}

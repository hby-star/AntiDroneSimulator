using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Walk_Pro_Landform_Control_Data_V2 = KATLegacySDK._3DT.Walk_Pro_Landform_Control_Data_V2;
using Matrix33 = KATLegacySDK._3DT.Matrix33;

public class _3DTerrainPlatform 
{
    /// <summary>
    /// Player GameObject
    /// </summary>
    GameObject player;

    Transform rotateObj;

    /// <summary>
    /// Heartbeat
    /// </summary>
    private int heart;

    /// <summary>
    /// Terrain Angle Ratio
    /// </summary>
    private float Angle_Ratio = 4.8f / 10.0f;


    Walk_Pro_Landform_Control_Data_V2 walk_Pro_Landform_Set;
    Walk_Pro_Landform_Control_Data_V2 walk_Pro_Landform_Get;

    private RaycastHit hit;
    private Quaternion qua;
    private bool action;

    private _3DTerrainPlatform(GameObject player, Transform rotate)
    {
        this.player = player;
        rotateObj = rotate;
        walk_Pro_Landform_Get = new Walk_Pro_Landform_Control_Data_V2();
        walk_Pro_Landform_Set = new Walk_Pro_Landform_Control_Data_V2();

        walk_Pro_Landform_Set.QUIVER = 0;
        walk_Pro_Landform_Set.LIFT = 0;
        walk_Pro_Landform_Set.OVERWEIGHT = 0;
        walk_Pro_Landform_Set.RESET_QUICKLY = 0;
        walk_Pro_Landform_Set.RESET_SLOWLY = 0;
        walk_Pro_Landform_Set.SHAKE_LEVEL = 0;
        walk_Pro_Landform_Set.TREMOR_SHORT = 0;
        walk_Pro_Landform_Set.WEIGHTLESSNESS = 0;

        KATLegacySDK._3DT._3DTInit();
    }

    public static _3DTerrainPlatform instance;

    public static _3DTerrainPlatform Create(GameObject player, Transform rotate)
    {
        if(instance == null)
        {
            instance = new _3DTerrainPlatform(player, rotate);
        }
        else
        {
            Debug.LogWarning("_3DTerrainPlatform Already Created!");
        }

        return instance;
    }

    #region

    /// <summary>
    /// 快速复位，需要内容置1后触发，动作完成后Runtime置0
    /// </summary>
    public int ResetQuickly { set { action = true; walk_Pro_Landform_Set.RESET_QUICKLY = value; } }
    
    /// <summary>
    /// 缓慢复位，需要内容置1后触发，动作完成后Runtime置0
    /// </summary>
     
    public int ResetSlowly { set { action = true; walk_Pro_Landform_Set.RESET_SLOWLY = value; } }
    
    /// <summary>
    /// 超重，需要内容置1后触发，动作完成后Runtime置0
    /// </summary>
    public int Overweight { set { action = true; walk_Pro_Landform_Set.OVERWEIGHT = value; } }

    /// <summary>
    /// 失重，需要内容置1后触发，动作完成后Runtime置0
    /// </summary>
    public int WeightLessness { set { action = true; walk_Pro_Landform_Set.WEIGHTLESSNESS = value; } }
    
    /// <summary>
    /// 短颤，需要内容置1后触发，动作完成后Runtime置0
    /// </summary>
    public int TremorShort { set { action = true; walk_Pro_Landform_Set.TREMOR_SHORT = value; } }

    /// <summary>
    /// 震动，需要内容置震动等级0,1,2
    /// 0为不震动
    /// </summary>
    public int Quiver { get { return walk_Pro_Landform_Set.QUIVER; } set { action = true; walk_Pro_Landform_Set.QUIVER = value; } }

    /// <summary>
    /// 晃动，需要内容置晃动等级,0,1,2,3。
    /// 0为不晃动
    /// </summary>
    public int ShakeLevel { get { return walk_Pro_Landform_Set.SHAKE_LEVEL; } set { action = true; walk_Pro_Landform_Set.SHAKE_LEVEL = value; } }

    /// <summary>
    /// 楼层位置
    /// 五个档位，0档位为初始位置
    /// 2
    /// 1
    /// 0
    /// -1
    /// -2
    /// </summary>
    public int Lift { get { return walk_Pro_Landform_Set.LIFT; } set { action = true; walk_Pro_Landform_Set.LIFT = value; } }
    public Matrix33 MATRIX { get { return walk_Pro_Landform_Set.MATRIX; } }

    bool enable = true;
    /// <summary>
    /// 控制地形模拟是否工作
    /// </summary>
    public bool Enable { get { return enable; } set { enable = value; } }

    public void UpdateData(Transform transform)
    {
        heart++;
        walk_Pro_Landform_Set.HEART_BEAT = heart;
        if (Physics.Raycast(player.transform.position - Vector3.up * 0.5f, -player.transform.up, out hit))
        {
            Debug.DrawRay(player.transform.position - Vector3.up * 0.5f, -player.transform.up, Color.red);
            if (player != null)
            {
                transform.position = hit.point;

                transform.forward = rotateObj.forward;
                transform.up = hit.normal;

                //qua = Quaternion.LookRotation(rotateObj.forward, hit.normal);


                if (enable)
                {
                    qua = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    //qua = Quaternion.LookRotation(rotateObj.forward, hit.normal);
                }
                else
                {
                    qua = Quaternion.identity;
                }
                transform.rotation = qua;
            }
        }


        //Current Player Euler Angles
        float X_Pro = transform.rotation.eulerAngles.x;
        float Y_Pro = transform.rotation.eulerAngles.y;
        float Z_Pro = transform.rotation.eulerAngles.z;



        if (X_Pro > 180)
        {
            X_Pro -= 360;
        }
        X_Pro *= Angle_Ratio;

        if (Y_Pro > 180)
        {
            Y_Pro -= 360;
        }
        Y_Pro *= Angle_Ratio;

        if (Z_Pro > 180)
        {
            Z_Pro -= 360;
        }
        Z_Pro *= Angle_Ratio;

        //Debug.Log("X_Pro="+ X_Pro+ ", Z_Pro="+ Z_Pro+ " Y_Pro="+ Y_Pro);
        transform.eulerAngles = new Vector3(X_Pro, Z_Pro, Y_Pro);

        walk_Pro_Landform_Set.X = X_Pro;
        walk_Pro_Landform_Set.Y = Z_Pro;
        walk_Pro_Landform_Set.Z = Y_Pro;

        Matrix33 matrix = new Matrix33(
            transform.worldToLocalMatrix.m00, transform.worldToLocalMatrix.m01, transform.worldToLocalMatrix.m02,
            transform.worldToLocalMatrix.m10, transform.worldToLocalMatrix.m11, transform.worldToLocalMatrix.m12,
            transform.worldToLocalMatrix.m20, transform.worldToLocalMatrix.m21, transform.worldToLocalMatrix.m22);

        walk_Pro_Landform_Set.MATRIX = matrix;

        transform.rotation = qua;

        KATLegacySDK._3DT.KAT_LANDFORM_CONTROL_DATA_V2_GET(ref walk_Pro_Landform_Get);
        if (!action)
        {
            walk_Pro_Landform_Set.LIFT = walk_Pro_Landform_Get.LIFT;
            walk_Pro_Landform_Set.OVERWEIGHT = walk_Pro_Landform_Get.OVERWEIGHT;
            walk_Pro_Landform_Set.QUIVER = walk_Pro_Landform_Get.QUIVER;
            walk_Pro_Landform_Set.RESET_QUICKLY = walk_Pro_Landform_Get.RESET_QUICKLY;
            walk_Pro_Landform_Set.RESET_SLOWLY = walk_Pro_Landform_Get.RESET_SLOWLY;
            walk_Pro_Landform_Set.SHAKE_LEVEL = walk_Pro_Landform_Get.SHAKE_LEVEL;
            walk_Pro_Landform_Set.TREMOR_SHORT = walk_Pro_Landform_Get.TREMOR_SHORT;
            walk_Pro_Landform_Set.WEIGHTLESSNESS = walk_Pro_Landform_Get.WEIGHTLESSNESS;
        }
        else
        {
            action = false;
        }

        KATLegacySDK._3DT.KAT_LANDFORM_CONTROL_DATA_V2_UPDATE(walk_Pro_Landform_Set);

    }

    #endregion 




}

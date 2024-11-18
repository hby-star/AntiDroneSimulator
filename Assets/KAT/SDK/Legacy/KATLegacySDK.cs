using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class KATLegacySDK
{
    public class _3DT
    {
        /// <summary>
        /// 地形控制数据V2【实时发送】
        /// </summary>
        /// 
        [Serializable]
        public struct Walk_Pro_Landform_Control_Data_V2
        {
            /// <summary>
            /// 心跳包，需要内容每一帧更新
            /// </summary>
            public int HEART_BEAT;

            public float X;
            public float Y;
            public float Z;

            ///特殊动作部分///
            ///同时只生效一个动作///

            /// <summary>
            /// 快速复位，需要内容置1后触发，动作完成后Runtime置0
            /// </summary>
            public int RESET_QUICKLY;

            /// <summary>
            /// 缓慢复位，需要内容置1后触发，动作完成后Runtime置0
            /// </summary>
            public int RESET_SLOWLY;

            /// <summary>
            /// 超重，需要内容置1后触发，动作完成后Runtime置0
            /// </summary>
            public int OVERWEIGHT;

            /// <summary>
            /// 失重，需要内容置1后触发，动作完成后Runtime置0
            /// </summary>
            public int WEIGHTLESSNESS;

            /// <summary>
            /// 短颤，需要内容置1后触发，动作完成后Runtime置0
            /// </summary>
            public int TREMOR_SHORT;


            ///状态部分///
            ///建议震动晃动、只生效一个

            /// <summary>
            /// 震动，需要内容置震动等级0,1,2
            /// 0为不震动
            /// </summary>
            public int QUIVER;

            /// <summary>
            /// 晃动，需要内容置晃动等级,0,1,2,3。
            /// 0为不晃动
            /// </summary>
            public int SHAKE_LEVEL;

            /// <summary>
            /// 楼层位置
            /// 五个档位，0档位为初始位置
            /// 2
            /// 1
            /// 0
            /// -1
            /// -2
            /// </summary>
            public int LIFT;

            /// <summary>
            /// 地形变化，需要内容更新
            /// </summary>
            public Matrix33 MATRIX;

            public Walk_Pro_Landform_Control_Data_V2(int zero = 0, int one = 1)
            {
                HEART_BEAT = zero;
                X = zero;
                Y = zero;
                Z = zero;
                RESET_QUICKLY = zero;
                RESET_SLOWLY = zero;
                OVERWEIGHT = zero;
                WEIGHTLESSNESS = zero;
                TREMOR_SHORT = zero;
                QUIVER = zero;
                SHAKE_LEVEL = zero;
                LIFT = zero;
                MATRIX = new Matrix33();
            }

        }

        /// <summary>
        /// 地形模拟的旋转矩阵
        /// </summary>
        [Serializable]
        public struct Matrix33
        {
            public double m11, m12, m13;
            public double m21, m22, m23;
            public double m31, m32, m33;

            public Matrix33(double m11 = 1, double m12 = 0, double m13 = 0, double m21 = 0, double m22 = 1, double m23 = 0, double m31 = 0, double m32 = 0, double m33 = 1)
            {
                this.m11 = m11; this.m12 = m12; this.m13 = m13;
                this.m21 = m21; this.m22 = m22; this.m23 = m23;
                this.m31 = m31; this.m32 = m32; this.m33 = m33;
            }

            public string GetString()
            {
                string str = $"{this.m11} {this.m12} {this.m13}/n" +
                    $"{this.m21} {this.m22} {this.m23}/n" +
                    $"{this.m31} {this.m32} {this.m33}";

                return str;
            }
        }

        public class KATDevice_Data
        {
            public static string GetString(Walk_Pro_Landform_Control_Data_V2 data)
            {
                string str =
                    $"快速复位: {data.RESET_QUICKLY}/n" +
                    $"缓慢复位: {data.RESET_SLOWLY}/n" +
                    $"超重: {data.OVERWEIGHT}/n" +
                    $"失重: {data.WEIGHTLESSNESS}/n" +
                    $"短颤: {data.TREMOR_SHORT}/n" +
                    $"震动: {data.QUIVER}/n" +
                    $"晃动: {data.SHAKE_LEVEL}/n" +
                    $"楼层: {data.LIFT}/n{data.MATRIX.GetString()}";

                return str;
            }
        }


        public static bool LogOpen;
        public const string TAG = "[KATDevice_Dll]: ";
        #region API
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns>
        /// -1：初始化失败
        /// -2：线程启动失败
        /// 0：初始化成功
        /// 1：已初始化
        /// </returns>
        public static int _3DTInit()
        {
            if (LogOpen)
            {
                Debug.LogWarning($"{TAG} Init_2B call");
                var result = Init_2B();
                Debug.LogWarning($"{TAG} Init_2B result={result}");
                return result;
            }
            else
            {
                return Init_2B();
            }
        }

        public static int KAT_LANDFORM_CONTROL_DATA_V2_UPDATE(Walk_Pro_Landform_Control_Data_V2 data)
        {
            return LANDFORM_CONTROL_DATA_V2_UPDATE(data);
        }

        public static int KAT_LANDFORM_CONTROL_DATA_V2_GET(ref Walk_Pro_Landform_Control_Data_V2 data)
        {
            return LANDFORM_CONTROL_DATA_V2_GET(ref data);
        }


        #endregion API

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns>
        /// -1：初始化失败
        /// -2：线程启动失败
        /// 0：初始化成功
        /// 1：已初始化
        /// </returns>
        [DllImport("WalkerBase_2B", CallingConvention = CallingConvention.Cdecl)]
        extern static int Init_2B();

        [DllImport("WalkerBase_2B", CallingConvention = CallingConvention.Cdecl)]
        extern static int LANDFORM_CONTROL_DATA_V2_UPDATE(Walk_Pro_Landform_Control_Data_V2 data);

        [DllImport("WalkerBase_2B", CallingConvention = CallingConvention.Cdecl)]
        extern static int LANDFORM_CONTROL_DATA_V2_GET(ref Walk_Pro_Landform_Control_Data_V2 data);

        [DllImport("WalkerBase_2B")]
        extern static bool Haptic_Module_Control(int haptic_level, int haptic_time);
    }

    public class WalkC_MiniS
    {
        [DllImport("WalkerBase")]
        extern static bool Haptic_Module_Control(int haptic_level, int haptic_time);
    }
}
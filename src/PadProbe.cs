using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Yx.ModSdk;

namespace YxPadProbe
{
    /// <summary>
    /// 手柄探针（诊断用）：每帧读 Unity 的 Input 手柄状态，把「有变化 / 非零」的打到日志。
    /// 目的是实机确认 Unity 旧版 Input 到底能读到哪些——尤其是右摇杆的轴。
    ///
    /// 只用 UnityEngine.Input，不模拟、不改游戏。按 ILRuntime 约束写（无 LINQ / 无插值 / 数字带 InvariantCulture）。
    /// </summary>
    public sealed class PadProbeMod : YxMod
    {
        // Unity 的 KeyCode.JoystickButton0 = 330；按到 Joystick8Button19 一共很多，这里覆盖到 409（任意手柄 + 前两只）。
        const int JoyButtonFirst = 330;
        const int JoyButtonLast = 409;

        // 要探测的轴名：GetAxis 对「没在 InputManager 里定义的轴名」会抛异常，
        // 所以逐个 try —— 不抛的就是游戏定义了的轴（顺带 dump 出游戏到底暴露了哪些轴）。
        static readonly string[] AxisNames = new string[]
        {
            "Horizontal", "Vertical", "Mouse X", "Mouse Y", "Mouse ScrollWheel",
            "Fire1", "Fire2", "Fire3", "Jump", "Submit", "Cancel",
            "RightStickHorizontal", "RightStickVertical", "Right Horizontal", "Right Vertical",
            "RHorizontal", "RVertical", "Horizontal2", "Vertical2", "DPADHorizontal", "DPADVertical",
            "1st axis", "2nd axis", "3rd axis", "4th axis", "5th axis", "6th axis",
            "7th axis", "8th axis", "9th axis", "10th axis",
        };

        readonly List<string> _definedAxes = new List<string>();
        int _frame;
        bool _axesDumped;

        public override void OnLoad(ModContext ctx)
        {
            ctx.Log.Info("手柄探针已加载：插上手柄，推摇杆 / 按键看日志。先在这里 dump 一次游戏定义的轴。");
            DumpDefinedAxes(ctx);
        }

        public override void OnUpdate()
        {
            _frame++;
            // 大约每 12 帧（约 5 次/秒）打一次，别刷屏。
            if (_frame % 12 != 0) return;

            // 按下的按钮（任意手柄）。
            string buttons = DownButtons();
            if (buttons.Length > 0) Context.Log.Info("按钮按下：" + buttons);

            // 明显偏离中心的轴（|v|>0.3）——推摇杆时这里应该出现对应轴。
            string axes = ActiveAxes();
            if (axes.Length > 0) Context.Log.Info("轴：" + axes);
        }

        void DumpDefinedAxes(ModContext ctx)
        {
            if (_axesDumped) return;
            _axesDumped = true;
            for (int i = 0; i < AxisNames.Length; i++)
            {
                string name = AxisNames[i];
                try
                {
                    float v = Input.GetAxisRaw(name);
                    _definedAxes.Add(name);
                    // 读到了（没抛）= 游戏定义了这个轴。当前值一并记下。
                    ctx.Log.Info("定义了轴：" + name + " = " + v.ToString("0.00", CultureInfo.InvariantCulture));
                }
                catch (Exception)
                {
                    // 没定义这个轴名，跳过。
                }
            }
            string count = _definedAxes.Count.ToString(CultureInfo.InvariantCulture);
            ctx.Log.Info("游戏一共定义了 " + count + " 个可读轴（右摇杆若在其中，推动它会在上面出现非零值）。");
        }

        string DownButtons()
        {
            string result = "";
            for (int code = JoyButtonFirst; code <= JoyButtonLast; code++)
            {
                bool down;
                try { down = Input.GetKey((KeyCode)code); }
                catch (Exception) { down = false; }
                if (!down) continue;
                if (result.Length > 0) result = result + ", ";
                result = result + code.ToString(CultureInfo.InvariantCulture);
            }
            return result;
        }

        string ActiveAxes()
        {
            string result = "";
            for (int i = 0; i < _definedAxes.Count; i++)
            {
                string name = _definedAxes[i];
                float v;
                try { v = Input.GetAxisRaw(name); }
                catch (Exception) { continue; }
                if (v > -0.3f && v < 0.3f) continue;
                if (result.Length > 0) result = result + ", ";
                result = result + name + "=" + v.ToString("0.00", CultureInfo.InvariantCulture);
            }
            return result;
        }
    }
}

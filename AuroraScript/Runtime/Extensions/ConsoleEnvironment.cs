﻿using AuroraScript.Runtime.Base;
using AuroraScript.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AuroraScript.Runtime.Extensions
{
    internal class ConsoleEnvironment : ScriptObject
    {
        /// <summary>
        /// 用于计时功能的秒表对象
        /// </summary>
        private static Stopwatch _stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// 存储计时标记的字典，键为计时标记名称，值为开始时间
        /// </summary>
        private static Dictionary<string, long> _times = new();

        public ConsoleEnvironment()
        {
            // 在控制台对象中注册日志、计时和计时结束函数
            Define("log", new ClrFunction(LOG), writeable: false, enumerable: false);
            Define("time", new ClrFunction(TIME), writeable: false, enumerable: false);
            Define("timeEnd", new ClrFunction(TIMEEND), writeable: false, enumerable: false);
        }



        /// <summary>
        /// 日志输出函数，在脚本中通过console.log或debug调用
        /// </summary>
        /// <param name="domain">脚本域</param>
        /// <param name="thisObject">调用对象（this）</param>
        /// <param name="args">参数数组</param>
        /// <returns>空对象</returns>
        public static ScriptObject LOG(ExecuteContext context, ScriptObject thisObject, ScriptObject[] args)
        {
            Console.WriteLine(string.Join(", ", args));
            return Null;
        }



        /// <summary>
        /// 开始计时函数，在脚本中通过console.time调用
        /// </summary>
        /// <param name="domain">脚本域</param>
        /// <param name="thisObject">调用对象（this）</param>
        /// <param name="args">参数数组，第一个参数为计时标记名称</param>
        /// <returns>空对象</returns>
        public static ScriptObject TIME(ExecuteContext context, ScriptObject thisObject, ScriptObject[] args)
        {
            if (args.Length == 1 && args[0] is StringValue stringValue)
            {
                // 记录当前时间作为计时开始点
                _times[stringValue.Value] = _stopwatch.ElapsedMilliseconds;
            }
            return Null;
        }

        /// <summary>
        /// 结束计时函数，在脚本中通过console.timeEnd调用
        /// 计算并输出从开始计时到结束计时的时间间隔
        /// </summary>
        /// <param name="domain">脚本域</param>
        /// <param name="thisObject">调用对象（this）</param>
        /// <param name="args">参数数组，第一个参数为计时标记名称</param>
        /// <returns>空对象</returns>
        public static ScriptObject TIMEEND(ExecuteContext context, ScriptObject thisObject, ScriptObject[] args)
        {
            if (args.Length == 1 && args[0] is StringValue stringValue && _times.TryGetValue(stringValue.Value, out var value))
            {
                // 计算时间间隔
                var time = _stopwatch.ElapsedMilliseconds - value;
                // 移除计时标记
                _times.Remove(stringValue.Value);
                // 输出计时结果

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{stringValue.Value} Used {time}ms");
                Console.ResetColor();

            }
            return Null;
        }


    }
}

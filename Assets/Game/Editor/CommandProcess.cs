using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

// コマンドプロセス
public class CommandProcess {
    //--------------------------------------------------------------------- 開始
    // コマンドプロセスの開始
    public int Start(string command, object arguments = null, string cwd = null, Dictionary<string,string> env = null) {
        // 環境変数フラグ
        var isNonBlock = false;
        if (env != null && env.ContainsKey("&NONBLOCK")) {
            isNonBlock = (env["&NONBLOCK"] == "1");
            env.Remove("&NONBLOCK");
        }
        var isUseShell = (Application.platform == RuntimePlatform.WindowsEditor);
        if (env != null && env.ContainsKey("&USESHELL")) {
            isUseShell = (env["&USESHELL"] == "1");
            env.Remove("&USESHELL");
        }
        var isAutoClose = true;
        if (env != null && env.ContainsKey("&NOCLOSE")) {
            isAutoClose = !(env["&NOCLOSE"] == "1");
            env.Remove("&NOCLOSE");
        }

        // カレントディレクトリ確認
        if (!string.IsNullOrEmpty(cwd)) {
            if (!Directory.Exists(cwd)) {
                Debug.Log("Directory not found. (" + cwd +")");
                return 1;
            }
        }

        // コマンド実行
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow  = false;
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            // コマンドを作成
            var commandString = "^\"" + ShellEscape(command) + "^\"";

            // 引数を作成
            var argumentsString = "";
            if (arguments is string) {
                argumentsString = arguments as string;
            } else if (arguments is List<string>) {
                var argumentsList = arguments as List<string>;
                foreach (var argument in argumentsList) {
                    argumentsString += (string.IsNullOrEmpty(argumentsString))? "" : " ";
                    argumentsString += "^\"" + ShellEscape(argument) + "^\"";
                }
            }

            // コマンド実行
            startInfo.UseShellExecute = isUseShell;
            if (!isAutoClose) {
                startInfo.FileName  = "cmd.exe";
                startInfo.Arguments = "/K \"" + commandString + " " + argumentsString + "\"";
            } else {
                startInfo.FileName  = "cmd.exe";
                startInfo.Arguments = "/V:ON /C \"" + commandString + " " + argumentsString + " "
                                    + "& set E=!ERRORLEVEL!"
                                    + "& (if !E! equ 0 timeout 3)"
                                    + "& (if !E! neq 0 timeout 10)"
                                    + "& exit /b !E!"
                                    + "\"";
            }
        } else {
            // コマンドを作成
            var commandString = ShellEscape(command);

            // 引数を作成
            var argumentsString = "";
            if (arguments is string) {
                argumentsString = arguments as string;
            } else if (arguments is List<string>) {
                var argumentsList = arguments as List<string>;
                foreach (var argument in argumentsList) {
                    argumentsString += (string.IsNullOrEmpty(argumentsString))? "" : " ";
                    argumentsString += ShellEscape(argument);
                }
            }

            // コマンド実行
            startInfo.UseShellExecute        = isUseShell;
            startInfo.FileName               = commandString;
            startInfo.Arguments              = argumentsString;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError  = true;
            startInfo.RedirectStandardInput  = false;
        }
        if (!string.IsNullOrEmpty(cwd)) {
            startInfo.WorkingDirectory = cwd;
        }
        var exitCode = 0;
        var restoreEnv = default(Dictionary<string,string>);
        if (env != null) {
            restoreEnv = new Dictionary<string,string>();
            SetEnvironmentVariables(env, restoreEnv);
        }
        try {
            using(Process exeProcess = Process.Start(startInfo)) {
                if (!isNonBlock) {
                    var stdout = "";
                    var stderr = "";
                    if (startInfo.RedirectStandardOutput) { stdout = exeProcess.StandardOutput.ReadToEnd(); }
                    if (startInfo.RedirectStandardError)  { stderr = exeProcess.StandardError.ReadToEnd(); }
                    exeProcess.WaitForExit();
                    if (!string.IsNullOrEmpty(stdout)) { Debug.Log(stdout);      }
                    if (!string.IsNullOrEmpty(stderr)) { Debug.LogError(stderr); }
                    exitCode = exeProcess.ExitCode;
                }
            }
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            exitCode = 1;
        }
        if (restoreEnv != null) {
            SetEnvironmentVariables(restoreEnv);
        }
        return exitCode;
    }

    //--------------------------------------------------------------------- 内部処理
    // 環境変数の設定
    void SetEnvironmentVariables(Dictionary<string,string> env, Dictionary<string,string> restoreEnv = null) {
        if (restoreEnv != null) {
            restoreEnv.Clear();
        }
        foreach (var pair in env) {
            var last = Environment.GetEnvironmentVariable(pair.Key);
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            if (restoreEnv != null) {
                restoreEnv[pair.Key] = last;
            }
        }
    }

    // シェルエスケープ
    string ShellEscape(string path) {
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            return ShellEscapeWin(path);
        }
        return ShellEscapeOther(path);
    }

    // シェルエスケープ (Win)
    string ShellEscapeWin(string path) {
        path = path.Replace(" ", "^ ");
        path = path.Replace("(", "^(");
        path = path.Replace(")", "^)");
        return path;
    }

    // シェルエスケープ (その他)
    string ShellEscapeOther(string path) {
        // ■ 注意
        // 今のところ処理なし
        return path;
    }
}

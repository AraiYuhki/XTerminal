using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Xeon.UniTerminal.UnityCommands
{
    /// <summary>
    /// GameObjectのTransformを操作するコマンド
    /// </summary>
    [Command("transform", "Manipulate GameObject Transform (set, add, sub)")]
    public class TransformCommand : ICommand
    {
        #region Options

        [Option("position", "p", Description = "World position (x,y,z)")]
        public string Position;

        [Option("local-position", "P", Description = "Local position (x,y,z)")]
        public string LocalPosition;

        [Option("rotation", "r", Description = "World rotation in euler angles (x,y,z)")]
        public string Rotation;

        [Option("local-rotation", "R", Description = "Local rotation in euler angles (x,y,z)")]
        public string LocalRotation;

        [Option("scale", "s", Description = "Local scale (x,y,z)")]
        public string Scale;

        [Option("parent", "", Description = "Set parent object (use '/' or 'null' to unparent)")]
        public string Parent;

        [Option("world", "w", Description = "Maintain world position when changing parent")]
        public bool WorldPositionStays = true;

        #endregion

        #region ICommand

        public string CommandName => "transform";
        public string Description => "Manipulate GameObject Transform";

        public async Task<ExitCode> ExecuteAsync(CommandContext context, CancellationToken ct)
        {
            if (context.PositionalArguments.Count == 0)
            {
                await context.Stderr.WriteLineAsync("transform: missing path argument", ct);
                await context.Stderr.WriteLineAsync("Usage: transform [set|add|sub] <path> [options]", ct);
                return ExitCode.UsageError;
            }

            var firstArg = context.PositionalArguments[0].ToLower();

            // サブコマンドを判定
            if (IsSubCommand(firstArg))
            {
                var args = context.PositionalArguments.Skip(1).ToList();
                return firstArg switch
                {
                    "set" => await SetAsync(context, args, ct),
                    "add" => await AddAsync(context, args, ct),
                    "sub" => await SubAsync(context, args, ct),
                    _ => await UnknownSubCommandAsync(context, firstArg, ct)
                };
            }

            // 後方互換性: サブコマンドなしの場合はset/表示として扱う
            return await SetOrDisplayAsync(context, context.PositionalArguments.ToList(), ct);
        }

        public IEnumerable<string> GetCompletions(CompletionContext context)
        {
            var token = context.CurrentToken ?? "";

            if (context.TokenIndex == 1)
            {
                // サブコマンドまたはパスの補完
                var subCommands = new[] { "set", "add", "sub" };
                var matchingSubCommands = subCommands.Where(cmd => cmd.StartsWith(token, StringComparison.OrdinalIgnoreCase));

                if (!token.StartsWith("-"))
                    return matchingSubCommands.Concat(GameObjectPath.GetCompletions(token));

                return matchingSubCommands;
            }

            if (context.TokenIndex == 2 && !token.StartsWith("-"))
                return GameObjectPath.GetCompletions(token);

            return Array.Empty<string>();
        }

        #endregion

        #region Subcommands

        private static bool IsSubCommand(string arg)
        {
            return arg == "set" || arg == "add" || arg == "sub";
        }

        private async Task<ExitCode> SetOrDisplayAsync(CommandContext context, List<string> args, CancellationToken ct)
        {
            if (args.Count == 0)
            {
                await context.Stderr.WriteLineAsync("transform: missing path argument", ct);
                return ExitCode.UsageError;
            }

            var path = args[0];
            var go = GameObjectPath.Resolve(path);

            if (go == null)
            {
                await context.Stderr.WriteLineAsync($"transform: '{path}': GameObject not found", ct);
                return ExitCode.RuntimeError;
            }

            var transform = go.transform;
            bool modified = false;

            await context.Stdout.WriteLineAsync($"Transform: {go.name}", ct);

            var result = await ApplyTransformOptions(context, transform, TransformOperation.Set, ct);
            if (result.exitCode != ExitCode.Success)
                return result.exitCode;
            modified = result.modified;

            // 親の変更はsetのみ
            var parentResult = await ApplyParentOption(context, transform, ct);
            if (parentResult.exitCode != ExitCode.Success)
                return parentResult.exitCode;
            modified |= parentResult.modified;

            if (!modified)
                await DisplayTransformInfoAsync(context, go, ct);

            return ExitCode.Success;
        }

        private async Task<ExitCode> SetAsync(CommandContext context, List<string> args, CancellationToken ct)
        {
            return await SetOrDisplayAsync(context, args, ct);
        }

        private async Task<ExitCode> AddAsync(CommandContext context, List<string> args, CancellationToken ct)
        {
            return await ApplyArithmeticAsync(context, args, TransformOperation.Add, ct);
        }

        private async Task<ExitCode> SubAsync(CommandContext context, List<string> args, CancellationToken ct)
        {
            return await ApplyArithmeticAsync(context, args, TransformOperation.Subtract, ct);
        }

        private async Task<ExitCode> ApplyArithmeticAsync(CommandContext context, List<string> args, TransformOperation operation, CancellationToken ct)
        {
            var opName = operation == TransformOperation.Add ? "add" : "sub";

            if (args.Count == 0)
            {
                await context.Stderr.WriteLineAsync($"transform {opName}: missing path argument", ct);
                return ExitCode.UsageError;
            }

            var path = args[0];
            var go = GameObjectPath.Resolve(path);

            if (go == null)
            {
                await context.Stderr.WriteLineAsync($"transform: '{path}': GameObject not found", ct);
                return ExitCode.RuntimeError;
            }

            var transform = go.transform;
            await context.Stdout.WriteLineAsync($"Transform: {go.name}", ct);

            var result = await ApplyTransformOptions(context, transform, operation, ct);
            if (result.exitCode != ExitCode.Success)
                return result.exitCode;

            if (!result.modified)
            {
                await context.Stderr.WriteLineAsync($"transform {opName}: no options specified", ct);
                return ExitCode.UsageError;
            }

            return ExitCode.Success;
        }

        private async Task<ExitCode> UnknownSubCommandAsync(CommandContext context, string subCommand, CancellationToken ct)
        {
            await context.Stderr.WriteLineAsync($"transform: unknown subcommand '{subCommand}'", ct);
            await context.Stderr.WriteLineAsync("Subcommands: set, add, sub", ct);
            return ExitCode.UsageError;
        }

        #endregion

        #region Transform Operations

        private enum TransformOperation { Set, Add, Subtract }

        private async Task<(ExitCode exitCode, bool modified)> ApplyTransformOptions(
            CommandContext context, Transform transform, TransformOperation operation, CancellationToken ct)
        {
            bool modified = false;

            var posResult = await ApplyPositionOptions(context, transform, operation, ct);
            if (posResult.exitCode != ExitCode.Success)
                return (posResult.exitCode, false);
            modified |= posResult.modified;

            var rotResult = await ApplyRotationOptions(context, transform, operation, ct);
            if (rotResult.exitCode != ExitCode.Success)
                return (rotResult.exitCode, false);
            modified |= rotResult.modified;

            var scaleResult = await ApplyScaleOption(context, transform, operation, ct);
            if (scaleResult.exitCode != ExitCode.Success)
                return (scaleResult.exitCode, false);
            modified |= scaleResult.modified;

            return (ExitCode.Success, modified);
        }

        private async Task<(ExitCode exitCode, bool modified)> ApplyPositionOptions(
            CommandContext context, Transform transform, TransformOperation operation, CancellationToken ct)
        {
            bool modified = false;

            // ワールド位置
            if (!string.IsNullOrEmpty(Position))
            {
                if (!TryParseVector3(Position, out var pos))
                {
                    await context.Stderr.WriteLineAsync($"transform: invalid position: '{Position}'", ct);
                    return (ExitCode.UsageError, false);
                }

                var oldPos = transform.position;
                transform.position = ApplyVector3Operation(oldPos, pos, operation);
                await WriteTransformChange(context, "Position", oldPos, transform.position, pos, operation, ct);
                modified = true;
            }

            // ローカル位置
            if (!string.IsNullOrEmpty(LocalPosition))
            {
                if (!TryParseVector3(LocalPosition, out var pos))
                {
                    await context.Stderr.WriteLineAsync($"transform: invalid local-position: '{LocalPosition}'", ct);
                    return (ExitCode.UsageError, false);
                }

                var oldPos = transform.localPosition;
                transform.localPosition = ApplyVector3Operation(oldPos, pos, operation);
                await WriteTransformChange(context, "Local Position", oldPos, transform.localPosition, pos, operation, ct);
                modified = true;
            }

            return (ExitCode.Success, modified);
        }

        private async Task<(ExitCode exitCode, bool modified)> ApplyRotationOptions(
            CommandContext context, Transform transform, TransformOperation operation, CancellationToken ct)
        {
            bool modified = false;

            // ワールド回転
            if (!string.IsNullOrEmpty(Rotation))
            {
                if (!TryParseVector3(Rotation, out var rot))
                {
                    await context.Stderr.WriteLineAsync($"transform: invalid rotation: '{Rotation}'", ct);
                    return (ExitCode.UsageError, false);
                }

                var oldRot = transform.eulerAngles;
                transform.eulerAngles = ApplyVector3Operation(oldRot, rot, operation);
                await WriteTransformChange(context, "Rotation", oldRot, transform.eulerAngles, rot, operation, ct);
                modified = true;
            }

            // ローカル回転
            if (!string.IsNullOrEmpty(LocalRotation))
            {
                if (!TryParseVector3(LocalRotation, out var rot))
                {
                    await context.Stderr.WriteLineAsync($"transform: invalid local-rotation: '{LocalRotation}'", ct);
                    return (ExitCode.UsageError, false);
                }

                var oldRot = transform.localEulerAngles;
                transform.localEulerAngles = ApplyVector3Operation(oldRot, rot, operation);
                await WriteTransformChange(context, "Local Rotation", oldRot, transform.localEulerAngles, rot, operation, ct);
                modified = true;
            }

            return (ExitCode.Success, modified);
        }

        private async Task<(ExitCode exitCode, bool modified)> ApplyScaleOption(
            CommandContext context, Transform transform, TransformOperation operation, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(Scale))
                return (ExitCode.Success, false);

            if (!TryParseVector3(Scale, out var scale))
            {
                await context.Stderr.WriteLineAsync($"transform: invalid scale: '{Scale}'", ct);
                return (ExitCode.UsageError, false);
            }

            var oldScale = transform.localScale;
            transform.localScale = ApplyVector3Operation(oldScale, scale, operation);
            await WriteTransformChange(context, "Scale", oldScale, transform.localScale, scale, operation, ct);
            return (ExitCode.Success, true);
        }

        private static Vector3 ApplyVector3Operation(Vector3 current, Vector3 operand, TransformOperation operation)
        {
            return operation switch
            {
                TransformOperation.Set => operand,
                TransformOperation.Add => current + operand,
                TransformOperation.Subtract => current - operand,
                _ => current
            };
        }

        private async Task WriteTransformChange(
            CommandContext context, string propName, Vector3 oldValue, Vector3 newValue, Vector3 operand,
            TransformOperation operation, CancellationToken ct)
        {
            if (operation == TransformOperation.Set)
            {
                await context.Stdout.WriteLineAsync($"  {propName}: {FormatVector3(oldValue)} -> {FormatVector3(newValue)}", ct);
            }
            else
            {
                var symbol = operation == TransformOperation.Add ? "+" : "-";
                await context.Stdout.WriteLineAsync($"  {propName}: {FormatVector3(oldValue)} {symbol} {FormatVector3(operand)} = {FormatVector3(newValue)}", ct);
            }
        }

        private async Task<(ExitCode exitCode, bool modified)> ApplyParentOption(
            CommandContext context, Transform transform, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(Parent))
                return (ExitCode.Success, false);

            string oldParentPath = transform.parent != null
                ? GameObjectPath.GetPath(transform.parent.gameObject)
                : "(none)";

            // 親を解除
            if (Parent == "/" || Parent.ToLower() == "null" || Parent.ToLower() == "none")
            {
                transform.SetParent(null, WorldPositionStays);
                await context.Stdout.WriteLineAsync($"  Parent: {oldParentPath} -> (none)", ct);
                return (ExitCode.Success, true);
            }

            // 親を解決
            var parentGo = GameObjectPath.Resolve(Parent);
            if (parentGo == null)
            {
                await context.Stderr.WriteLineAsync($"transform: '{Parent}': Parent not found", ct);
                return (ExitCode.RuntimeError, false);
            }

            // 循環参照チェック
            if (parentGo.transform == transform || parentGo.transform.IsChildOf(transform))
            {
                await context.Stderr.WriteLineAsync("transform: cannot set parent to self or descendant", ct);
                return (ExitCode.RuntimeError, false);
            }

            transform.SetParent(parentGo.transform, WorldPositionStays);
            string newParentPath = GameObjectPath.GetPath(parentGo);
            await context.Stdout.WriteLineAsync($"  Parent: {oldParentPath} -> {newParentPath}", ct);
            return (ExitCode.Success, true);
        }

        #endregion

        #region Display

        private async Task DisplayTransformInfoAsync(CommandContext context, GameObject go, CancellationToken ct)
        {
            var t = go.transform;

            await context.Stdout.WriteLineAsync($"  World Position:  {FormatVector3(t.position)}", ct);
            await context.Stdout.WriteLineAsync($"  Local Position:  {FormatVector3(t.localPosition)}", ct);
            await context.Stdout.WriteLineAsync($"  World Rotation:  {FormatVector3(t.eulerAngles)}", ct);
            await context.Stdout.WriteLineAsync($"  Local Rotation:  {FormatVector3(t.localEulerAngles)}", ct);
            await context.Stdout.WriteLineAsync($"  Local Scale:     {FormatVector3(t.localScale)}", ct);

            string parentPath = t.parent != null ? GameObjectPath.GetPath(t.parent.gameObject) : "(none)";
            await context.Stdout.WriteLineAsync($"  Parent:          {parentPath}", ct);
            await context.Stdout.WriteLineAsync($"  Children:        {t.childCount}", ct);
            await context.Stdout.WriteLineAsync($"  Sibling Index:   {t.GetSiblingIndex()}", ct);
        }

        #endregion

        #region Utility

        private bool TryParseVector3(string input, out Vector3 result)
        {
            result = Vector3.zero;
            if (string.IsNullOrEmpty(input))
                return false;

            var parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                return parts.Length switch
                {
                    1 => ParseSingle(parts[0], out result),
                    2 => ParseTwo(parts, out result),
                    3 => ParseThree(parts, out result),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private static bool ParseSingle(string value, out Vector3 result)
        {
            var single = float.Parse(value);
            result = new Vector3(single, single, single);
            return true;
        }

        private static bool ParseTwo(string[] parts, out Vector3 result)
        {
            result = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), 0);
            return true;
        }

        private static bool ParseThree(string[] parts, out Vector3 result)
        {
            result = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            return true;
        }

        private static string FormatVector3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        #endregion
    }
}

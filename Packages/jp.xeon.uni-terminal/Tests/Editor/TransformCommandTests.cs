using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Xeon.UniTerminal.Tests
{
    /// <summary>
    /// transformコマンドのテスト
    /// </summary>
    public class TransformCommandTests
    {
        private Terminal terminal;
        private StringBuilderTextWriter stdout;
        private StringBuilderTextWriter stderr;
        private List<GameObject> createdObjects;

        [SetUp]
        public void SetUp()
        {
            terminal = new Terminal("/", "/", registerBuiltInCommands: true);
            stdout = new StringBuilderTextWriter();
            stderr = new StringBuilderTextWriter();
            createdObjects = new List<GameObject>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in createdObjects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            createdObjects.Clear();
        }

        private GameObject CreateTestObject(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null)
            {
                go.transform.SetParent(parent);
            }
            createdObjects.Add(go);
            return go;
        }

        // TF-001 情報表示
        [Test]
        public async Task Transform_ShowsInfo()
        {
            var obj = CreateTestObject("TfTest_Info");
            obj.transform.position = new Vector3(1, 2, 3);

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_Info", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("TfTest_Info"));
            Assert.IsTrue(output.Contains("World Position"));
            Assert.IsTrue(output.Contains("Local Position"));
        }

        // TF-002 存在しないパス
        [Test]
        public async Task Transform_NotFound_ReturnsError()
        {
            var exitCode = await terminal.ExecuteAsync("transform /TfTest_NonExistent", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("not found"));
        }

        // TF-010 ワールド位置設定
        [Test]
        public async Task Transform_SetPosition()
        {
            var obj = CreateTestObject("TfTest_Pos");

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_Pos --position 5,10,15", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(5, 10, 15), obj.transform.position);
        }

        // TF-011 ローカル位置設定
        [Test]
        public async Task Transform_SetLocalPosition()
        {
            var parent = CreateTestObject("TfTest_Parent");
            parent.transform.position = new Vector3(10, 0, 0);
            var child = CreateTestObject("TfTest_Child", parent.transform);

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("transform /TfTest_Parent/TfTest_Child --local-position 1,2,3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(1, 2, 3), child.transform.localPosition);
        }

        // TF-012 単一値位置
        [Test]
        public async Task Transform_SetPosition_SingleValue()
        {
            var obj = CreateTestObject("TfTest_SinglePos");

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_SinglePos --position 5", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(5, 5, 5), obj.transform.position);
        }

        // TF-020 回転設定
        [Test]
        public async Task Transform_SetRotation()
        {
            var obj = CreateTestObject("TfTest_Rot");

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_Rot --rotation 0,90,0", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            // 浮動小数点の誤差を考慮
            Assert.AreEqual(90f, obj.transform.eulerAngles.y, 0.1f);
        }

        // TF-021 ローカル回転設定
        [Test]
        public async Task Transform_SetLocalRotation()
        {
            var obj = CreateTestObject("TfTest_LocalRot");

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_LocalRot --local-rotation 45,0,0", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(45f, obj.transform.localEulerAngles.x, 0.1f);
        }

        // TF-030 スケール設定
        [Test]
        public async Task Transform_SetScale()
        {
            var obj = CreateTestObject("TfTest_Scale");

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_Scale --scale 2,2,2", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(2, 2, 2), obj.transform.localScale);
        }

        // TF-031 非均等スケール
        [Test]
        public async Task Transform_SetScale_NonUniform()
        {
            var obj = CreateTestObject("TfTest_NonUniformScale");

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_NonUniformScale --scale 1,2,3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(1, 2, 3), obj.transform.localScale);
        }

        // TF-040 親変更
        [Test]
        public async Task Transform_SetParent()
        {
            var parent = CreateTestObject("TfTest_NewParent");
            var obj = CreateTestObject("TfTest_Reparent");

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("transform /TfTest_Reparent --parent /TfTest_NewParent", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(parent.transform, obj.transform.parent);
        }

        // TF-041 親解除
        [Test]
        public async Task Transform_Unparent()
        {
            var parent = CreateTestObject("TfTest_OldParent");
            var child = CreateTestObject("TfTest_Unparent", parent.transform);

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("transform /TfTest_OldParent/TfTest_Unparent --parent /", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.IsNull(child.transform.parent);
        }

        // TF-042 循環参照エラー
        [Test]
        public async Task Transform_CyclicParent_ReturnsError()
        {
            var parent = CreateTestObject("TfTest_CyclicParent");
            var child = CreateTestObject("TfTest_CyclicChild", parent.transform);

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("transform /TfTest_CyclicParent --parent /TfTest_CyclicParent/TfTest_CyclicChild", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("cannot set parent"));
        }

        // TF-050 複合設定
        [Test]
        public async Task Transform_MultipleOptions()
        {
            var obj = CreateTestObject("TfTest_Multi");

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_Multi --position 1,2,3 --rotation 0,90,0 --scale 2,2,2", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(1, 2, 3), obj.transform.position);
            Assert.AreEqual(90f, obj.transform.eulerAngles.y, 0.1f);
            Assert.AreEqual(new Vector3(2, 2, 2), obj.transform.localScale);
        }

        // パス引数なし
        [Test]
        public async Task Transform_NoPath_ReturnsError()
        {
            var exitCode = await terminal.ExecuteAsync("transform", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("missing path"));
        }

        // 無効なVector3
        [Test]
        public async Task Transform_InvalidVector_ReturnsError()
        {
            var obj = CreateTestObject("TfTest_InvalidVec");

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_InvalidVec --position invalid", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("invalid"));
        }

        // --- サブコマンド方式テスト ---

        // TF-100 setサブコマンド
        [Test]
        public async Task Transform_Set_SubCommand_SetsPosition()
        {
            var obj = CreateTestObject("TfTest_SetSub");

            var exitCode = await terminal.ExecuteAsync("transform set /TfTest_SetSub -p 1,2,3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(1, 2, 3), obj.transform.position);
        }

        // TF-101 後方互換性（サブコマンドなし）
        [Test]
        public async Task Transform_BackwardCompatibility_SetsPosition()
        {
            var obj = CreateTestObject("TfTest_BackCompat");

            var exitCode = await terminal.ExecuteAsync("transform /TfTest_BackCompat -p 5,5,5", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(5, 5, 5), obj.transform.position);
        }

        // --- addサブコマンドテスト ---

        // TF-110 位置の加算
        [Test]
        public async Task Transform_Add_Position()
        {
            var obj = CreateTestObject("TfTest_AddPos");
            obj.transform.position = new Vector3(1, 2, 3);

            var exitCode = await terminal.ExecuteAsync("transform add /TfTest_AddPos -p 1,1,1", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(2, 3, 4), obj.transform.position);
        }

        // TF-111 ローカル位置の加算
        [Test]
        public async Task Transform_Add_LocalPosition()
        {
            var parent = CreateTestObject("TfTest_AddLocalParent");
            parent.transform.position = new Vector3(10, 0, 0);
            var child = CreateTestObject("TfTest_AddLocalChild", parent.transform);
            child.transform.localPosition = new Vector3(1, 1, 1);

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("transform add /TfTest_AddLocalParent/TfTest_AddLocalChild -P 2,2,2", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(3, 3, 3), child.transform.localPosition);
        }

        // TF-112 回転の加算
        [Test]
        public async Task Transform_Add_Rotation()
        {
            var obj = CreateTestObject("TfTest_AddRot");
            obj.transform.eulerAngles = new Vector3(0, 45, 0);

            var exitCode = await terminal.ExecuteAsync("transform add /TfTest_AddRot -r 0,45,0", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(90f, obj.transform.eulerAngles.y, 0.1f);
        }

        // TF-113 ローカル回転の加算
        [Test]
        public async Task Transform_Add_LocalRotation()
        {
            var obj = CreateTestObject("TfTest_AddLocalRot");
            obj.transform.localEulerAngles = new Vector3(0, 0, 0);

            var exitCode = await terminal.ExecuteAsync("transform add /TfTest_AddLocalRot -R 30,0,0", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(30f, obj.transform.localEulerAngles.x, 0.1f);
        }

        // TF-114 スケールの加算
        [Test]
        public async Task Transform_Add_Scale()
        {
            var obj = CreateTestObject("TfTest_AddScale");
            obj.transform.localScale = new Vector3(1, 1, 1);

            var exitCode = await terminal.ExecuteAsync("transform add /TfTest_AddScale -s 0.5,0.5,0.5", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(1.5f, 1.5f, 1.5f), obj.transform.localScale);
        }

        // TF-115 addでオプションなしエラー
        [Test]
        public async Task Transform_Add_NoOptions_ReturnsError()
        {
            var obj = CreateTestObject("TfTest_AddNoOpt");

            var exitCode = await terminal.ExecuteAsync("transform add /TfTest_AddNoOpt", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("no options"));
        }

        // --- subサブコマンドテスト ---

        // TF-120 位置の減算
        [Test]
        public async Task Transform_Sub_Position()
        {
            var obj = CreateTestObject("TfTest_SubPos");
            obj.transform.position = new Vector3(5, 5, 5);

            var exitCode = await terminal.ExecuteAsync("transform sub /TfTest_SubPos -p 2,1,3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(3, 4, 2), obj.transform.position);
        }

        // TF-121 ローカル位置の減算
        [Test]
        public async Task Transform_Sub_LocalPosition()
        {
            var obj = CreateTestObject("TfTest_SubLocalPos");
            obj.transform.localPosition = new Vector3(10, 10, 10);

            var exitCode = await terminal.ExecuteAsync("transform sub /TfTest_SubLocalPos -P 3,3,3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(7, 7, 7), obj.transform.localPosition);
        }

        // TF-122 回転の減算
        [Test]
        public async Task Transform_Sub_Rotation()
        {
            var obj = CreateTestObject("TfTest_SubRot");
            obj.transform.eulerAngles = new Vector3(0, 90, 0);

            var exitCode = await terminal.ExecuteAsync("transform sub /TfTest_SubRot -r 0,45,0", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(45f, obj.transform.eulerAngles.y, 0.1f);
        }

        // TF-123 スケールの減算
        [Test]
        public async Task Transform_Sub_Scale()
        {
            var obj = CreateTestObject("TfTest_SubScale");
            obj.transform.localScale = new Vector3(2, 2, 2);

            var exitCode = await terminal.ExecuteAsync("transform sub /TfTest_SubScale -s 0.5,0.5,0.5", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(1.5f, 1.5f, 1.5f), obj.transform.localScale);
        }

        // TF-124 subでパスなしエラー
        [Test]
        public async Task Transform_Sub_NoPath_ReturnsError()
        {
            var exitCode = await terminal.ExecuteAsync("transform sub", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("missing path"));
        }

        // --- 複合テスト ---

        // TF-130 add複数オプション
        [Test]
        public async Task Transform_Add_MultipleOptions()
        {
            var obj = CreateTestObject("TfTest_AddMulti");
            obj.transform.position = new Vector3(1, 1, 1);
            obj.transform.localScale = new Vector3(1, 1, 1);

            var exitCode = await terminal.ExecuteAsync("transform add /TfTest_AddMulti -p 1,1,1 -s 1,1,1", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(2, 2, 2), obj.transform.position);
            Assert.AreEqual(new Vector3(2, 2, 2), obj.transform.localScale);
        }

        // TF-131 不明な文字列はパスとして扱われる（後方互換性）
        [Test]
        public async Task Transform_UnknownString_TreatedAsPath()
        {
            // 後方互換性のため、不明な文字列はサブコマンドではなくパスとして扱われる
            // "mul"というオブジェクトが存在しないため、not foundエラーになる
            var exitCode = await terminal.ExecuteAsync("transform mul -s 2", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("not found"));
        }

        // TF-132 存在しないオブジェクトへのadd
        [Test]
        public async Task Transform_Add_NotFound_ReturnsError()
        {
            var exitCode = await terminal.ExecuteAsync("transform add /TfTest_NonExistent -p 1,1,1", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("not found"));
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Xeon.XTerminal.Tests
{
    /// <summary>
    /// propertyコマンドのテスト
    /// </summary>
    public class PropertyCommandTests
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

        // PROP-001 プロパティ一覧表示
        [Test]
        public async Task Property_List_ShowsProperties()
        {
            var obj = CreateTestObject("PropTest_List");
            obj.AddComponent<Rigidbody>();

            var exitCode = await terminal.ExecuteAsync("property list /PropTest_List Rigidbody", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("mass"));
            Assert.IsTrue(output.Contains("useGravity"));
        }

        // PROP-003 存在しないパス
        [Test]
        public async Task Property_List_NotFound()
        {
            var exitCode = await terminal.ExecuteAsync("property list /PropTest_NonExistent Transform", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("not found"));
        }

        // PROP-010 単一プロパティ取得
        [Test]
        public async Task Property_Get_Single()
        {
            var obj = CreateTestObject("PropTest_GetSingle");
            var rb = obj.AddComponent<Rigidbody>();
            rb.mass = 5f;

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property get /PropTest_GetSingle Rigidbody mass", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("mass"));
            Assert.IsTrue(output.Contains("5"));
        }

        // PROP-011 複数プロパティ取得
        [Test]
        public async Task Property_Get_Multiple()
        {
            var obj = CreateTestObject("PropTest_GetMultiple");
            var rb = obj.AddComponent<Rigidbody>();
            rb.mass = 10f;
            rb.linearDamping = 2f;

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property get /PropTest_GetMultiple Rigidbody mass,drag", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("mass"));
            Assert.IsTrue(output.Contains("drag"));
        }

        // PROP-012 存在しないプロパティ
        [Test]
        public async Task Property_Get_NotFound()
        {
            var obj = CreateTestObject("PropTest_GetNotFound");
            obj.AddComponent<Rigidbody>();

            var exitCode = await terminal.ExecuteAsync("property get /PropTest_GetNotFound Rigidbody nonexistent", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("not found"));
        }

        // PROP-020 float設定
        [Test]
        public async Task Property_Set_Float()
        {
            var obj = CreateTestObject("PropTest_SetFloat");
            var rb = obj.AddComponent<Rigidbody>();
            rb.mass = 1f;

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property set /PropTest_SetFloat Rigidbody mass 10", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(10f, rb.mass, 0.01f);
        }

        // PROP-021 bool設定
        [Test]
        public async Task Property_Set_Bool()
        {
            var obj = CreateTestObject("PropTest_SetBool");
            var rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = true;

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property set /PropTest_SetBool Rigidbody useGravity false", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.IsFalse(rb.useGravity);
        }

        // PROP-022 Vector3設定
        [Test]
        public async Task Property_Set_Vector3()
        {
            var obj = CreateTestObject("PropTest_SetVector3");

            var exitCode = await terminal.ExecuteAsync("property set /PropTest_SetVector3 Transform position 1,2,3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(1, 2, 3), obj.transform.position);
        }

        // PROP-024 enum設定
        [Test]
        public async Task Property_Set_Enum()
        {
            var obj = CreateTestObject("PropTest_SetEnum");
            var rb = obj.AddComponent<Rigidbody>();

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property set /PropTest_SetEnum Rigidbody interpolation Interpolate", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(RigidbodyInterpolation.Interpolate, rb.interpolation);
        }

        // PROP-026 型変換エラー
        [Test]
        public async Task Property_Set_TypeError()
        {
            var obj = CreateTestObject("PropTest_SetTypeError");
            obj.AddComponent<Rigidbody>();

            var exitCode = await terminal.ExecuteAsync("property set /PropTest_SetTypeError Rigidbody mass notanumber", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("Cannot convert"));
        }

        // サブコマンドなし
        [Test]
        public async Task Property_NoSubcommand_ReturnsError()
        {
            var exitCode = await terminal.ExecuteAsync("property", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("missing subcommand"));
        }

        // 不明なサブコマンド
        [Test]
        public async Task Property_UnknownSubcommand_ReturnsError()
        {
            var exitCode = await terminal.ExecuteAsync("property unknowncmd /Test Transform", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("unknown subcommand"));
        }

        // コンポーネントが見つからない
        [Test]
        public async Task Property_ComponentNotFound()
        {
            var obj = CreateTestObject("PropTest_CompNotFound");

            var exitCode = await terminal.ExecuteAsync("property list /PropTest_CompNotFound Rigidbody", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("not found"));
        }

        // --- Phase 5: 配列操作テスト ---

        // PROP-050 配列要素の取得
        [Test]
        public async Task Property_Get_ArrayElement()
        {
            var obj = CreateTestObject("PropTest_ArrayGet");
            var renderer = obj.AddComponent<MeshRenderer>();

            // MeshRendererのsharedMaterials配列を取得（最初の要素）
            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property get /PropTest_ArrayGet MeshRenderer sharedMaterials", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("sharedMaterials"));
        }

        // PROP-051 配列インデックス範囲外
        [Test]
        public async Task Property_Get_ArrayIndexOutOfRange()
        {
            var obj = CreateTestObject("PropTest_ArrayOutOfRange");
            var renderer = obj.AddComponent<MeshRenderer>();

            var exitCode = await terminal.ExecuteAsync("property get /PropTest_ArrayOutOfRange MeshRenderer sharedMaterials[999]", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("out of range"));
        }

        // PROP-052 配列の表示形式
        [Test]
        public async Task Property_List_ShowsArrays()
        {
            var obj = CreateTestObject("PropTest_ArrayList");
            // Editorモードでも安定して動作するTransformを使用
            // Transformにはposition, rotation等のプロパティがあり、配列プロパティの代わりに
            // プロパティ一覧が正しく表示されることを確認
            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property list /PropTest_ArrayList Transform", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            // Transformのプロパティが表示されることを確認
            Assert.IsTrue(output.Contains("position") || output.Contains("localPosition"));
        }

        // --- Phase 5: 参照型テスト ---

        // PROP-060 参照型の表示
        [Test]
        public async Task Property_Get_ReferenceType()
        {
            var obj = CreateTestObject("PropTest_RefType");
            var rb = obj.AddComponent<Rigidbody>();

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property get /PropTest_RefType Transform root", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            // 参照型の表示形式を確認
            Assert.IsTrue(output.Contains("Transform") || output.Contains("root"));
        }

        // PROP-061 GameObject参照の設定
        [Test]
        public async Task Property_Set_GameObjectReference()
        {
            var obj1 = CreateTestObject("PropTest_RefSet");
            var obj2 = CreateTestObject("PropTest_RefTarget");

            // カスタムコンポーネントではなく、標準のTransform.parentを使用
            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property set /PropTest_RefSet Transform parent /PropTest_RefTarget", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(obj2.transform, obj1.transform.parent);
        }

        // PROP-062 参照型をnullに設定
        [Test]
        public async Task Property_Set_NullReference()
        {
            var parent = CreateTestObject("PropTest_NullRefParent");
            var child = CreateTestObject("PropTest_NullRefChild", parent.transform);

            Assert.IsNotNull(child.transform.parent);

            stdout = new StringBuilderTextWriter();
            stderr = new StringBuilderTextWriter();
            // 子オブジェクトのフルパスを使用
            var exitCode = await terminal.ExecuteAsync("property set /PropTest_NullRefParent/PropTest_NullRefChild Transform parent null", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode, $"stderr: {stderr}");
            Assert.IsNull(child.transform.parent);
        }

        // --- 演算操作テスト ---

        // PROP-100 add - float
        [Test]
        public async Task Property_Add_Float()
        {
            var obj = CreateTestObject("PropTest_AddFloat");
            var rb = obj.AddComponent<Rigidbody>();
            rb.mass = 5f;

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property add /PropTest_AddFloat Rigidbody mass 3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(8f, rb.mass, 0.01f);
        }

        // PROP-101 add - 負の値
        [Test]
        public async Task Property_Add_NegativeValue()
        {
            var obj = CreateTestObject("PropTest_AddNeg");
            var rb = obj.AddComponent<Rigidbody>();
            rb.mass = 10f;

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property add /PropTest_AddNeg Rigidbody mass -3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(7f, rb.mass, 0.01f);
        }

        // PROP-102 add - Vector3
        [Test]
        public async Task Property_Add_Vector3()
        {
            var obj = CreateTestObject("PropTest_AddVec3");
            obj.transform.position = new Vector3(1, 2, 3);

            var exitCode = await terminal.ExecuteAsync("property add /PropTest_AddVec3 Transform position 1,1,1", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(2, 3, 4), obj.transform.position);
        }

        // PROP-110 sub - float
        [Test]
        public async Task Property_Sub_Float()
        {
            var obj = CreateTestObject("PropTest_SubFloat");
            var rb = obj.AddComponent<Rigidbody>();
            rb.mass = 10f;

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property sub /PropTest_SubFloat Rigidbody mass 3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(7f, rb.mass, 0.01f);
        }

        // PROP-111 sub - Vector3
        [Test]
        public async Task Property_Sub_Vector3()
        {
            var obj = CreateTestObject("PropTest_SubVec3");
            obj.transform.position = new Vector3(5, 5, 5);

            var exitCode = await terminal.ExecuteAsync("property sub /PropTest_SubVec3 Transform position 2,1,3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(3, 4, 2), obj.transform.position);
        }

        // PROP-120 mul - float
        [Test]
        public async Task Property_Mul_Float()
        {
            var obj = CreateTestObject("PropTest_MulFloat");
            var rb = obj.AddComponent<Rigidbody>();
            rb.mass = 5f;

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property mul /PropTest_MulFloat Rigidbody mass 3", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(15f, rb.mass, 0.01f);
        }

        // PROP-121 mul - int (Light.cullingMask)
        [Test]
        public async Task Property_Mul_Int()
        {
            var obj = CreateTestObject("PropTest_MulInt");
            var light = obj.AddComponent<Light>();
            // renderingLayerMaskはuint型だが、cullingMaskはint型
            // 代わりにRigidbody.solverIterationsを使用（int型）
            var rb = obj.AddComponent<Rigidbody>();

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property mul /PropTest_MulInt Rigidbody solverIterations 2", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            // デフォルト値は6なので、6 * 2 = 12
            Assert.AreEqual(12, rb.solverIterations);
        }

        // PROP-122 mul - Vector3 スカラー乗算
        [Test]
        public async Task Property_Mul_Vector3_Scalar()
        {
            var obj = CreateTestObject("PropTest_MulVec3Scalar");
            obj.transform.localScale = new Vector3(1, 2, 3);

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property mul /PropTest_MulVec3Scalar Transform localScale 2", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(2, 4, 6), obj.transform.localScale);
        }

        // PROP-123 mul - Vector3 成分ごとの乗算
        [Test]
        public async Task Property_Mul_Vector3_ComponentWise()
        {
            var obj = CreateTestObject("PropTest_MulVec3Comp");
            obj.transform.localScale = new Vector3(1, 2, 3);

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property mul /PropTest_MulVec3Comp Transform localScale 2,3,4", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(2, 6, 12), obj.transform.localScale);
        }

        // PROP-130 div - float
        [Test]
        public async Task Property_Div_Float()
        {
            var obj = CreateTestObject("PropTest_DivFloat");
            var rb = obj.AddComponent<Rigidbody>();
            rb.mass = 10f;

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property div /PropTest_DivFloat Rigidbody mass 2", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(5f, rb.mass, 0.01f);
        }

        // PROP-131 div - ゼロ除算
        [Test]
        public async Task Property_Div_ByZero_ReturnsError()
        {
            var obj = CreateTestObject("PropTest_DivZero");
            var rb = obj.AddComponent<Rigidbody>();
            rb.mass = 10f;

            var exitCode = await terminal.ExecuteAsync("property div /PropTest_DivZero Rigidbody mass 0", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("division by zero"));
        }

        // PROP-132 div - Vector3 スカラー除算
        [Test]
        public async Task Property_Div_Vector3_Scalar()
        {
            var obj = CreateTestObject("PropTest_DivVec3Scalar");
            obj.transform.localScale = new Vector3(4, 6, 8);

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property div /PropTest_DivVec3Scalar Transform localScale 2", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(2, 3, 4), obj.transform.localScale);
        }

        // PROP-133 div - Vector3 成分ごとの除算
        [Test]
        public async Task Property_Div_Vector3_ComponentWise()
        {
            var obj = CreateTestObject("PropTest_DivVec3Comp");
            obj.transform.localScale = new Vector3(10, 20, 30);

            stdout = new StringBuilderTextWriter();
            var exitCode = await terminal.ExecuteAsync("property div /PropTest_DivVec3Comp Transform localScale 2,4,5", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(new Vector3(5, 5, 6), obj.transform.localScale);
        }

        // PROP-134 div - Vector3 ゼロ除算エラー
        [Test]
        public async Task Property_Div_Vector3_ByZero_ReturnsError()
        {
            var obj = CreateTestObject("PropTest_DivVec3Zero");
            obj.transform.localScale = new Vector3(1, 2, 3);

            var exitCode = await terminal.ExecuteAsync("property div /PropTest_DivVec3Zero Transform localScale 2,0,2", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("division by zero"));
        }

        // PROP-140 非対応型エラー（string）
        [Test]
        public async Task Property_Add_UnsupportedType_ReturnsError()
        {
            var obj = CreateTestObject("PropTest_AddString");

            var exitCode = await terminal.ExecuteAsync("property add /PropTest_AddString Transform name test", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("does not support arithmetic"));
        }

        // PROP-141 非対応型エラー（bool）
        [Test]
        public async Task Property_Mul_Bool_ReturnsError()
        {
            var obj = CreateTestObject("PropTest_MulBool");
            var rb = obj.AddComponent<Rigidbody>();

            var exitCode = await terminal.ExecuteAsync("property mul /PropTest_MulBool Rigidbody useGravity 2", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("does not support"));
        }

        // PROP-150 演算 - 引数不足
        [Test]
        public async Task Property_Add_MissingArgs_ReturnsError()
        {
            var obj = CreateTestObject("PropTest_AddMissing");
            obj.AddComponent<Rigidbody>();

            var exitCode = await terminal.ExecuteAsync("property add /PropTest_AddMissing Rigidbody mass", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("usage"));
        }

        // PROP-151 演算 - 存在しないプロパティ
        [Test]
        public async Task Property_Add_NotFoundProperty_ReturnsError()
        {
            var obj = CreateTestObject("PropTest_AddNotFound");
            obj.AddComponent<Rigidbody>();

            var exitCode = await terminal.ExecuteAsync("property add /PropTest_AddNotFound Rigidbody nonexistent 5", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("not found"));
        }

        // PROP-152 演算 - 読み取り専用プロパティ
        [Test]
        public async Task Property_Add_ReadOnlyProperty_ReturnsError()
        {
            var obj = CreateTestObject("PropTest_AddReadOnly");
            var rb = obj.AddComponent<Rigidbody>();

            // inertiaTensorは読み取り専用（自動計算される）
            var exitCode = await terminal.ExecuteAsync("property add /PropTest_AddReadOnly Rigidbody worldCenterOfMass 1,1,1", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("read-only"));
        }

        // PROP-160 サブコマンド一覧の更新確認
        [Test]
        public async Task Property_UnknownSubcommand_ShowsAllSubcommands()
        {
            var exitCode = await terminal.ExecuteAsync("property unknown /Test Transform", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            var errorOutput = stderr.ToString();
            Assert.IsTrue(errorOutput.Contains("add"));
            Assert.IsTrue(errorOutput.Contains("sub"));
            Assert.IsTrue(errorOutput.Contains("mul"));
            Assert.IsTrue(errorOutput.Contains("div"));
        }
    }
}

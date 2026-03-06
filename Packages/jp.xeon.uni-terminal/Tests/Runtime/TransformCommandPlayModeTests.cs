using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Xeon.XTerminal.Tests.Runtime
{
    /// <summary>
    /// transformコマンドのPlayModeテスト
    /// </summary>
    public class TransformCommandPlayModeTests
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
                    Object.Destroy(go);
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

        [UnityTest]
        public IEnumerator Transform_Position_SetsWorldPosition()
        {
            var target = CreateTestObject("PlayMode_TransPos");

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransPos -p 1,2,3", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(1, 2, 3), target.transform.position);
        }

        [UnityTest]
        public IEnumerator Transform_LocalPosition_SetsLocalPosition()
        {
            var parent = CreateTestObject("PlayMode_TransParent");
            parent.transform.position = new Vector3(10, 10, 10);
            var child = CreateTestObject("PlayMode_TransChild", parent.transform);

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransParent/PlayMode_TransChild -P 1,1,1", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(1, 1, 1), child.transform.localPosition);
        }

        [UnityTest]
        public IEnumerator Transform_Rotation_SetsWorldRotation()
        {
            var target = CreateTestObject("PlayMode_TransRot");

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransRot -r 0,90,0", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(90f, target.transform.eulerAngles.y, 0.1f);
        }

        [UnityTest]
        public IEnumerator Transform_LocalRotation_SetsLocalRotation()
        {
            var parent = CreateTestObject("PlayMode_TransRotParent");
            parent.transform.rotation = Quaternion.Euler(0, 45, 0);
            var child = CreateTestObject("PlayMode_TransRotChild", parent.transform);

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransRotParent/PlayMode_TransRotChild -R 0,45,0", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(45f, child.transform.localEulerAngles.y, 0.1f);
        }

        [UnityTest]
        public IEnumerator Transform_Scale_SetsLocalScale()
        {
            var target = CreateTestObject("PlayMode_TransScale");

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransScale -s 2,3,4", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(2, 3, 4), target.transform.localScale);
        }

        [UnityTest]
        public IEnumerator Transform_Parent_SetsParent()
        {
            var parent = CreateTestObject("PlayMode_TransNewParent");
            var child = CreateTestObject("PlayMode_TransOrphan");

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransOrphan --parent /PlayMode_TransNewParent", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(parent.transform, child.transform.parent);
        }

        [UnityTest]
        public IEnumerator Transform_Parent_Null_UnparentsObject()
        {
            var parent = CreateTestObject("PlayMode_TransUnparentParent");
            var child = CreateTestObject("PlayMode_TransUnparentChild", parent.transform);

            Assert.IsNotNull(child.transform.parent);

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransUnparentParent/PlayMode_TransUnparentChild --parent null", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.IsNull(child.transform.parent);
        }

        [UnityTest]
        public IEnumerator Transform_Combined_SetsMultipleProperties()
        {
            var target = CreateTestObject("PlayMode_TransCombined");

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransCombined -p 5,5,5 -r 45,45,45 -s 2,2,2", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(5, 5, 5), target.transform.position);
            Assert.AreEqual(new Vector3(2, 2, 2), target.transform.localScale);
            Assert.AreEqual(45f, target.transform.eulerAngles.x, 0.1f);
        }

        [UnityTest]
        public IEnumerator Transform_NoOptions_ShowsCurrentTransform()
        {
            var target = CreateTestObject("PlayMode_TransInfo");
            target.transform.position = new Vector3(1, 2, 3);
            target.transform.localScale = new Vector3(2, 2, 2);

            yield return null;

            stdout = new StringBuilderTextWriter();
            var task = terminal.ExecuteAsync("transform /PlayMode_TransInfo", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("position") || output.Contains("Position"));
        }

        [UnityTest]
        public IEnumerator Transform_WorldPositionStays_MaintainsWorldPosition()
        {
            var parent = CreateTestObject("PlayMode_TransWorldParent");
            parent.transform.position = new Vector3(10, 0, 0);
            var child = CreateTestObject("PlayMode_TransWorldChild");
            child.transform.position = new Vector3(5, 5, 5);

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransWorldChild --parent /PlayMode_TransWorldParent -w", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(parent.transform, child.transform.parent);
            // ワールド座標が維持されている
            Assert.AreEqual(5f, child.transform.position.x, 0.1f);
            Assert.AreEqual(5f, child.transform.position.y, 0.1f);
            Assert.AreEqual(5f, child.transform.position.z, 0.1f);
        }

        // --- addサブコマンドテスト ---

        [UnityTest]
        public IEnumerator Transform_Add_Position_AddsToCurrentPosition()
        {
            var target = CreateTestObject("PlayMode_TransAddPos");
            target.transform.position = new Vector3(1, 2, 3);

            yield return null;

            var task = terminal.ExecuteAsync("transform add /PlayMode_TransAddPos -p 1,1,1", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(2, 3, 4), target.transform.position);
        }

        [UnityTest]
        public IEnumerator Transform_Add_LocalPosition_AddsToLocalPosition()
        {
            var parent = CreateTestObject("PlayMode_TransAddLocalParent");
            parent.transform.position = new Vector3(10, 0, 0);
            var child = CreateTestObject("PlayMode_TransAddLocalChild", parent.transform);
            child.transform.localPosition = new Vector3(1, 1, 1);

            yield return null;

            var task = terminal.ExecuteAsync("transform add /PlayMode_TransAddLocalParent/PlayMode_TransAddLocalChild -P 2,2,2", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(3, 3, 3), child.transform.localPosition);
        }

        [UnityTest]
        public IEnumerator Transform_Add_Rotation_AddsToRotation()
        {
            var target = CreateTestObject("PlayMode_TransAddRot");
            target.transform.eulerAngles = new Vector3(0, 45, 0);

            yield return null;

            var task = terminal.ExecuteAsync("transform add /PlayMode_TransAddRot -r 0,45,0", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(90f, target.transform.eulerAngles.y, 0.1f);
        }

        [UnityTest]
        public IEnumerator Transform_Add_Scale_AddsToScale()
        {
            var target = CreateTestObject("PlayMode_TransAddScale");
            target.transform.localScale = new Vector3(1, 1, 1);

            yield return null;

            var task = terminal.ExecuteAsync("transform add /PlayMode_TransAddScale -s 0.5,0.5,0.5", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(1.5f, 1.5f, 1.5f), target.transform.localScale);
        }

        [UnityTest]
        public IEnumerator Transform_Add_MultipleOptions_AppliesAll()
        {
            var target = CreateTestObject("PlayMode_TransAddMulti");
            target.transform.position = new Vector3(1, 1, 1);
            target.transform.localScale = new Vector3(1, 1, 1);

            yield return null;

            var task = terminal.ExecuteAsync("transform add /PlayMode_TransAddMulti -p 1,1,1 -s 1,1,1", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(2, 2, 2), target.transform.position);
            Assert.AreEqual(new Vector3(2, 2, 2), target.transform.localScale);
        }

        // --- subサブコマンドテスト ---

        [UnityTest]
        public IEnumerator Transform_Sub_Position_SubtractsFromPosition()
        {
            var target = CreateTestObject("PlayMode_TransSubPos");
            target.transform.position = new Vector3(5, 5, 5);

            yield return null;

            var task = terminal.ExecuteAsync("transform sub /PlayMode_TransSubPos -p 2,1,3", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(3, 4, 2), target.transform.position);
        }

        [UnityTest]
        public IEnumerator Transform_Sub_Rotation_SubtractsFromRotation()
        {
            var target = CreateTestObject("PlayMode_TransSubRot");
            target.transform.eulerAngles = new Vector3(0, 90, 0);

            yield return null;

            var task = terminal.ExecuteAsync("transform sub /PlayMode_TransSubRot -r 0,45,0", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(45f, target.transform.eulerAngles.y, 0.1f);
        }

        [UnityTest]
        public IEnumerator Transform_Sub_Scale_SubtractsFromScale()
        {
            var target = CreateTestObject("PlayMode_TransSubScale");
            target.transform.localScale = new Vector3(2, 2, 2);

            yield return null;

            var task = terminal.ExecuteAsync("transform sub /PlayMode_TransSubScale -s 0.5,0.5,0.5", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(1.5f, 1.5f, 1.5f), target.transform.localScale);
        }

        // --- setサブコマンドと後方互換性テスト ---

        [UnityTest]
        public IEnumerator Transform_Set_Subcommand_SetsPosition()
        {
            var target = CreateTestObject("PlayMode_TransSetSub");

            yield return null;

            var task = terminal.ExecuteAsync("transform set /PlayMode_TransSetSub -p 1,2,3", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(1, 2, 3), target.transform.position);
        }

        [UnityTest]
        public IEnumerator Transform_BackwardCompatibility_WithoutSubcommand()
        {
            var target = CreateTestObject("PlayMode_TransBackCompat");

            yield return null;

            var task = terminal.ExecuteAsync("transform /PlayMode_TransBackCompat -p 5,5,5", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.Success, task.Result);
            Assert.AreEqual(new Vector3(5, 5, 5), target.transform.position);
        }

        // --- エラーケーステスト ---

        [UnityTest]
        public IEnumerator Transform_Add_NoOptions_ReturnsError()
        {
            var target = CreateTestObject("PlayMode_TransAddNoOpt");

            yield return null;

            stderr = new StringBuilderTextWriter();
            var task = terminal.ExecuteAsync("transform add /PlayMode_TransAddNoOpt", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.UsageError, task.Result);
            Assert.IsTrue(stderr.ToString().Contains("no options"));
        }

        [UnityTest]
        public IEnumerator Transform_Sub_NotFound_ReturnsError()
        {
            yield return null;

            stderr = new StringBuilderTextWriter();
            var task = terminal.ExecuteAsync("transform sub /PlayMode_NonExistent -p 1,1,1", stdout, stderr);
            while (!task.IsCompleted) yield return null;

            Assert.AreEqual(ExitCode.RuntimeError, task.Result);
            Assert.IsTrue(stderr.ToString().Contains("not found"));
        }
    }
}

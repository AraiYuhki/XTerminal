using System;
using NUnit.Framework;
using UnityEngine;

namespace Xeon.UniTerminal.Tests
{
    /// <summary>
    /// ValueConverterの演算メソッドのテスト
    /// </summary>
    public class ValueConverterArithmeticTests
    {

        [Test]
        public void IsArithmeticType_Int_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsArithmeticType(typeof(int)));
        }

        [Test]
        public void IsArithmeticType_Float_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsArithmeticType(typeof(float)));
        }

        [Test]
        public void IsArithmeticType_Double_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsArithmeticType(typeof(double)));
        }

        [Test]
        public void IsArithmeticType_Long_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsArithmeticType(typeof(long)));
        }

        [Test]
        public void IsArithmeticType_Byte_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsArithmeticType(typeof(byte)));
        }

        [Test]
        public void IsArithmeticType_Short_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsArithmeticType(typeof(short)));
        }

        [Test]
        public void IsArithmeticType_String_ReturnsFalse()
        {
            Assert.IsFalse(ValueConverter.IsArithmeticType(typeof(string)));
        }

        [Test]
        public void IsArithmeticType_Bool_ReturnsFalse()
        {
            Assert.IsFalse(ValueConverter.IsArithmeticType(typeof(bool)));
        }

        [Test]
        public void IsArithmeticType_Vector3_ReturnsFalse()
        {
            Assert.IsFalse(ValueConverter.IsArithmeticType(typeof(Vector3)));
        }

        [Test]
        public void IsVectorType_Vector2_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsVectorType(typeof(Vector2)));
        }

        [Test]
        public void IsVectorType_Vector3_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsVectorType(typeof(Vector3)));
        }

        [Test]
        public void IsVectorType_Vector4_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsVectorType(typeof(Vector4)));
        }

        [Test]
        public void IsVectorType_Vector2Int_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsVectorType(typeof(Vector2Int)));
        }

        [Test]
        public void IsVectorType_Vector3Int_ReturnsTrue()
        {
            Assert.IsTrue(ValueConverter.IsVectorType(typeof(Vector3Int)));
        }

        [Test]
        public void IsVectorType_Int_ReturnsFalse()
        {
            Assert.IsFalse(ValueConverter.IsVectorType(typeof(int)));
        }

        [Test]
        public void IsVectorType_Quaternion_ReturnsFalse()
        {
            Assert.IsFalse(ValueConverter.IsVectorType(typeof(Quaternion)));
        }

        [Test]
        public void Add_Int_ReturnsCorrectSum()
        {
            var result = ValueConverter.Add(5, 3, typeof(int));
            Assert.AreEqual(8, result);
        }

        [Test]
        public void Add_Float_ReturnsCorrectSum()
        {
            var result = ValueConverter.Add(2.5f, 1.5f, typeof(float));
            Assert.AreEqual(4.0f, (float)result, 0.001f);
        }

        [Test]
        public void Add_Double_ReturnsCorrectSum()
        {
            var result = ValueConverter.Add(2.5, 1.5, typeof(double));
            Assert.AreEqual(4.0, (double)result, 0.001);
        }

        [Test]
        public void Add_Long_ReturnsCorrectSum()
        {
            var result = ValueConverter.Add(1000000L, 2000000L, typeof(long));
            Assert.AreEqual(3000000L, result);
        }

        [Test]
        public void Add_NegativeNumbers_ReturnsCorrectSum()
        {
            var result = ValueConverter.Add(10, -3, typeof(int));
            Assert.AreEqual(7, result);
        }

        [Test]
        public void Add_Vector2_ReturnsCorrectSum()
        {
            var a = new Vector2(1, 2);
            var b = new Vector2(3, 4);
            var result = (Vector2)ValueConverter.Add(a, b, typeof(Vector2));
            Assert.AreEqual(new Vector2(4, 6), result);
        }

        [Test]
        public void Add_Vector3_ReturnsCorrectSum()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(4, 5, 6);
            var result = (Vector3)ValueConverter.Add(a, b, typeof(Vector3));
            Assert.AreEqual(new Vector3(5, 7, 9), result);
        }

        [Test]
        public void Add_Vector4_ReturnsCorrectSum()
        {
            var a = new Vector4(1, 2, 3, 4);
            var b = new Vector4(5, 6, 7, 8);
            var result = (Vector4)ValueConverter.Add(a, b, typeof(Vector4));
            Assert.AreEqual(new Vector4(6, 8, 10, 12), result);
        }

        [Test]
        public void Add_Vector2Int_ReturnsCorrectSum()
        {
            var a = new Vector2Int(1, 2);
            var b = new Vector2Int(3, 4);
            var result = (Vector2Int)ValueConverter.Add(a, b, typeof(Vector2Int));
            Assert.AreEqual(new Vector2Int(4, 6), result);
        }

        [Test]
        public void Add_Vector3Int_ReturnsCorrectSum()
        {
            var a = new Vector3Int(1, 2, 3);
            var b = new Vector3Int(4, 5, 6);
            var result = (Vector3Int)ValueConverter.Add(a, b, typeof(Vector3Int));
            Assert.AreEqual(new Vector3Int(5, 7, 9), result);
        }

        [Test]
        public void Subtract_Int_ReturnsCorrectDifference()
        {
            var result = ValueConverter.Subtract(10, 3, typeof(int));
            Assert.AreEqual(7, result);
        }

        [Test]
        public void Subtract_Float_ReturnsCorrectDifference()
        {
            var result = ValueConverter.Subtract(5.5f, 2.5f, typeof(float));
            Assert.AreEqual(3.0f, (float)result, 0.001f);
        }

        [Test]
        public void Subtract_NegativeResult_ReturnsCorrect()
        {
            var result = ValueConverter.Subtract(3, 10, typeof(int));
            Assert.AreEqual(-7, result);
        }

        [Test]
        public void Subtract_Vector3_ReturnsCorrectDifference()
        {
            var a = new Vector3(5, 7, 9);
            var b = new Vector3(1, 2, 3);
            var result = (Vector3)ValueConverter.Subtract(a, b, typeof(Vector3));
            Assert.AreEqual(new Vector3(4, 5, 6), result);
        }

        [Test]
        public void Subtract_Vector2Int_ReturnsCorrectDifference()
        {
            var a = new Vector2Int(10, 20);
            var b = new Vector2Int(3, 5);
            var result = (Vector2Int)ValueConverter.Subtract(a, b, typeof(Vector2Int));
            Assert.AreEqual(new Vector2Int(7, 15), result);
        }

        [Test]
        public void Multiply_Int_ReturnsCorrectProduct()
        {
            var result = ValueConverter.Multiply(5, 3, typeof(int));
            Assert.AreEqual(15, result);
        }

        [Test]
        public void Multiply_Float_ReturnsCorrectProduct()
        {
            var result = ValueConverter.Multiply(2.5f, 4.0f, typeof(float));
            Assert.AreEqual(10.0f, (float)result, 0.001f);
        }

        [Test]
        public void Multiply_Double_ReturnsCorrectProduct()
        {
            var result = ValueConverter.Multiply(2.5, 4.0, typeof(double));
            Assert.AreEqual(10.0, (double)result, 0.001);
        }

        [Test]
        public void Multiply_ByZero_ReturnsZero()
        {
            var result = ValueConverter.Multiply(100, 0, typeof(int));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void Multiply_Vector3_ThrowsNotSupported()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(2, 2, 2);

            Assert.Throws<NotSupportedException>(() =>
                ValueConverter.Multiply(a, b, typeof(Vector3)));
        }

        [Test]
        public void Divide_Int_ReturnsCorrectQuotient()
        {
            var result = ValueConverter.Divide(10, 2, typeof(int));
            Assert.AreEqual(5, result);
        }

        [Test]
        public void Divide_Float_ReturnsCorrectQuotient()
        {
            var result = ValueConverter.Divide(10.0f, 4.0f, typeof(float));
            Assert.AreEqual(2.5f, (float)result, 0.001f);
        }

        [Test]
        public void Divide_Double_ReturnsCorrectQuotient()
        {
            var result = ValueConverter.Divide(10.0, 4.0, typeof(double));
            Assert.AreEqual(2.5, (double)result, 0.001);
        }

        [Test]
        public void Divide_ByZero_ThrowsDivideByZero()
        {
            Assert.Throws<DivideByZeroException>(() =>
                ValueConverter.Divide(10, 0, typeof(int)));
        }

        [Test]
        public void Divide_Vector3_ThrowsNotSupported()
        {
            var a = new Vector3(4, 6, 8);
            var b = new Vector3(2, 2, 2);

            Assert.Throws<NotSupportedException>(() =>
                ValueConverter.Divide(a, b, typeof(Vector3)));
        }

        [Test]
        public void Add_String_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                ValueConverter.Add("hello", "world", typeof(string)));
        }

        [Test]
        public void Subtract_Bool_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                ValueConverter.Subtract(true, false, typeof(bool)));
        }

        [Test]
        public void Multiply_Quaternion_ThrowsNotSupported()
        {
            var a = Quaternion.identity;
            var b = Quaternion.Euler(0, 90, 0);

            Assert.Throws<NotSupportedException>(() =>
                ValueConverter.Multiply(a, b, typeof(Quaternion)));
        }

    }
}

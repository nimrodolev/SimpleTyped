using System;
using System.Collections.Generic;
using System.Reflection;
using SimplyTyped.Serialization;
using Xunit;

namespace SimplyTyped.Tests
{
    class InlineRangeAttribute : Xunit.Sdk.DataAttribute
    {
        private long _start;
        private long _end;

        public InlineRangeAttribute(long start, long end)
        {
            _start = start;
            _end = end;
        }

        public InlineRangeAttribute(int start, int end)
        {
            _start = start;
            _end = end;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            for (var i = _start; i <= _end; i++)
            {
                yield return new object[] { i };
            }
        }
    }

    public class SerializationTests
    {
        [Theory]
        [InlineData(typeof(byte))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(int))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(long))]
        [InlineData(typeof(ulong))]
        public void PrimitiveAttributeSerializer_SerializeNumber_PaddingLengthCorrect(Type type)
        {
            // arrange
            var serializer = new PrimitiveAttributeSerializer();
            FieldInfo fi = type.GetField("MaxValue");
            object maxValue = fi.GetRawConstantValue();
            object value = Convert.ChangeType(1, type);

            // act
            var serialized = serializer.Serialize(value);

            // assert
            Assert.Equal(maxValue.ToString().Length, serialized.Length);
        }


        [Fact]
        public void PrimitiveAttributeSerializer_SerializeByte_PaddingLengthCorrect()
        {
            // arrange
            var serializer = new PrimitiveAttributeSerializer();

            byte value = 1;
            // act
            var serialized = serializer.Serialize(value);
            // assert
            Assert.Equal(byte.MaxValue.ToString().Length, serialized.Length);
        }

        [Fact]
        public void PrimitiveAttributeSerializer_SerializeShort_PaddingLengthCorrect()
        {
            // arrange

            var serializer = new PrimitiveAttributeSerializer();
            short value = 1;
            // act
            var serialized = serializer.Serialize(value);
            // assert
            Assert.Equal(short.MaxValue.ToString().Length, serialized.Length);
        }

        [Fact]
        public void PrimitiveAttributeSerializer_SerializeInt_PaddingLengthCorrect()
        {
            // arrange
            var serializer = new PrimitiveAttributeSerializer();
            int value = 1;
            // act
            var serialized = serializer.Serialize(value);
            // assert
            Assert.Equal(int.MaxValue.ToString().Length, serialized.Length);
        }

        [Theory]
        [InlineRange(1, 5)]
        public void PrimitiveAttributeSerializer_SerializeLong_PaddingLengthCorrect(long value)
        {
            // arrange
            var serializer = new PrimitiveAttributeSerializer();
            //long value = 1;

            // act
            var serialized = serializer.Serialize(value);
            // assert
            Assert.Equal(long.MaxValue.ToString().Length, serialized.Length);
        }
    }
}
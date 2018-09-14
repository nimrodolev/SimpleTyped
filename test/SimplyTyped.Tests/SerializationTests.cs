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

        private void PrimitiveAttributeSerializer_SerializeNumber_RoundtripSuccessfull(object value, Type type)
        {
            // arrange
            var serializer = new PrimitiveAttributeSerializer();
            // act
            var str = serializer.Serialize(value);
            var roundtripped = serializer.Deserialize(str, type);
        
            // assert
            Assert.Equal(value, roundtripped);
        }

        [Theory]
        [InlineData(Byte.MinValue)]
        [InlineData(Byte.MaxValue)]
        [InlineData((Byte)0)]
        public void PrimitiveAttributeSerializer_SerializeByte_RoundtripSuccessfull(Byte value)
        {
            PrimitiveAttributeSerializer_SerializeNumber_RoundtripSuccessfull(value, typeof(Byte));
        }

        [Theory]
        [InlineData(SByte.MinValue)]
        [InlineData(SByte.MaxValue)]
        [InlineData((SByte)0)]
        public void PrimitiveAttributeSerializer_SerializeSignedByte_RoundtripSuccessfull(SByte value)
        {
            PrimitiveAttributeSerializer_SerializeNumber_RoundtripSuccessfull(value, typeof(SByte));
        }

        [Theory]
        [InlineData(Int16.MinValue)]
        [InlineData(Int16.MaxValue)]
        [InlineData((Int16)0)]
        public void PrimitiveAttributeSerializer_SerializeSignedInt16_RoundtripSuccessfull(Int16 value)
        {
            PrimitiveAttributeSerializer_SerializeNumber_RoundtripSuccessfull(value, typeof(Int16));
        }

        [Theory]
        [InlineData(UInt16.MinValue)]
        [InlineData(UInt16.MaxValue)]
        [InlineData((UInt16)0)]
        public void PrimitiveAttributeSerializer_SerializeUnsignedInt16_RoundtripSuccessfull(UInt16 value)
        {
            PrimitiveAttributeSerializer_SerializeNumber_RoundtripSuccessfull(value, typeof(UInt16));
        }

        [Theory]
        [InlineData(Int32.MinValue)]
        [InlineData(Int32.MaxValue)]
        [InlineData((Int32)0)]
        public void PrimitiveAttributeSerializer_SerializeSignedInt32_RoundtripSuccessfull(Int32 value)
        {
            PrimitiveAttributeSerializer_SerializeNumber_RoundtripSuccessfull(value, typeof(Int32));
        }

        [Theory]
        [InlineData(UInt32.MinValue)]
        [InlineData(UInt32.MaxValue)]
        [InlineData((UInt32)0)]
        public void PrimitiveAttributeSerializer_SerializeUnsignedInt32_RoundtripSuccessfull(UInt32 value)
        {
            PrimitiveAttributeSerializer_SerializeNumber_RoundtripSuccessfull(value, typeof(UInt32));
        }

        [Theory]
        [InlineData(Int64.MinValue)]
        [InlineData(Int64.MaxValue)]
        [InlineData((Int64)0)]
        public void PrimitiveAttributeSerializer_SerializeSignedInt64_RoundtripSuccessfull(Int64 value)
        {
            PrimitiveAttributeSerializer_SerializeNumber_RoundtripSuccessfull(value, typeof(Int64));
        }

        [Theory]
        [InlineData(UInt64.MinValue)]
        [InlineData(UInt64.MaxValue)]
        [InlineData((UInt64)0)]
        public void PrimitiveAttributeSerializer_SerializeUnsignedInt64_RoundtripSuccessfull(UInt64 value)
        {
            PrimitiveAttributeSerializer_SerializeNumber_RoundtripSuccessfull(value, typeof(UInt64));
        }
    }
}
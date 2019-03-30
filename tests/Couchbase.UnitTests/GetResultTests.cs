using System.Collections.Generic;
using Couchbase.Core.IO.Converters;
using Couchbase.Core.IO.Operations;
using Couchbase.Core.IO.Operations.Legacy.SubDocument;
using Couchbase.Core.IO.Operations.SubDocument;
using Couchbase.Core.IO.Transcoders;
using Couchbase.UnitTests.Helpers;
using Newtonsoft.Json;
using Xunit;

namespace Couchbase.UnitTests
{
    public class GetResultTests
    {
        readonly byte[] _lookupInPacket = new byte[901]
            {
                0x81, 0xd0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x6d, 0x00, 0x00, 0x00, 0x19, 0x15,
                0x87, 0x10, 0x16, 0x4e, 0xf7, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x22, 0x45, 0x6d, 0x6d,
                0x79, 0x2d, 0x6c, 0x6f, 0x75, 0x20, 0x44, 0x69, 0x63, 0x6b, 0x65, 0x72, 0x73, 0x6f, 0x6e, 0x22, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x02, 0x32, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x5b, 0x22, 0x63, 0x61,
                0x74, 0x22, 0x2c, 0x20, 0x22, 0x64, 0x6f, 0x67, 0x22, 0x2c, 0x20, 0x22, 0x70, 0x61, 0x72, 0x72, 0x6f,
                0x74, 0x22, 0x5d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x22, 0x64, 0x6f, 0x67, 0x22, 0x00, 0x00, 0x00,
                0x00, 0x01, 0x58, 0x7b, 0x0d, 0x0a, 0x09, 0x09, 0x22, 0x68, 0x61, 0x69, 0x72, 0x22, 0x3a, 0x20, 0x22,
                0x62, 0x72, 0x6f, 0x77, 0x6e, 0x22, 0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x22, 0x64, 0x69, 0x6d, 0x65, 0x6e,
                0x73, 0x69, 0x6f, 0x6e, 0x73, 0x22, 0x3a, 0x20, 0x7b, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x22, 0x68, 0x65,
                0x69, 0x67, 0x68, 0x74, 0x22, 0x3a, 0x20, 0x36, 0x37, 0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x22, 0x77,
                0x65, 0x69, 0x67, 0x68, 0x74, 0x22, 0x3a, 0x20, 0x31, 0x37, 0x35, 0x0d, 0x0a, 0x09, 0x09, 0x7d, 0x2c,
                0x0d, 0x0a, 0x09, 0x09, 0x22, 0x68, 0x6f, 0x62, 0x62, 0x69, 0x65, 0x73, 0x22, 0x3a, 0x20, 0x5b, 0x7b,
                0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x22, 0x74, 0x79, 0x70, 0x65, 0x22, 0x3a, 0x20, 0x22, 0x77, 0x69,
                0x6e, 0x74, 0x65, 0x72, 0x20, 0x73, 0x70, 0x6f, 0x72, 0x74, 0x73, 0x22, 0x2c, 0x0d, 0x0a, 0x09, 0x09,
                0x09, 0x09, 0x22, 0x6e, 0x61, 0x6d, 0x65, 0x22, 0x3a, 0x20, 0x22, 0x63, 0x75, 0x72, 0x6c, 0x69, 0x6e,
                0x67, 0x22, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x7d, 0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x7b, 0x0d, 0x0a,
                0x09, 0x09, 0x09, 0x09, 0x22, 0x74, 0x79, 0x70, 0x65, 0x22, 0x3a, 0x20, 0x22, 0x73, 0x75, 0x6d, 0x6d,
                0x65, 0x72, 0x20, 0x73, 0x70, 0x6f, 0x72, 0x74, 0x73, 0x22, 0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09,
                0x22, 0x6e, 0x61, 0x6d, 0x65, 0x22, 0x3a, 0x20, 0x22, 0x77, 0x61, 0x74, 0x65, 0x72, 0x20, 0x73, 0x6b,
                0x69, 0x69, 0x6e, 0x67, 0x22, 0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x22, 0x64, 0x65, 0x74, 0x61,
                0x69, 0x6c, 0x73, 0x22, 0x3a, 0x20, 0x7b, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x09, 0x22, 0x6c, 0x6f,
                0x63, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x22, 0x3a, 0x20, 0x7b, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x09,
                0x09, 0x22, 0x6c, 0x61, 0x74, 0x22, 0x3a, 0x20, 0x34, 0x39, 0x2e, 0x32, 0x38, 0x32, 0x37, 0x33, 0x30,
                0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x22, 0x6c, 0x6f, 0x6e, 0x67, 0x22, 0x3a, 0x20,
                0x2d, 0x31, 0x32, 0x33, 0x2e, 0x31, 0x32, 0x30, 0x37, 0x33, 0x35, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09,
                0x09, 0x7d, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x7d, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x7d, 0x0d, 0x0a,
                0x09, 0x09, 0x5d, 0x0d, 0x0a, 0x09, 0x7d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x22, 0x62, 0x72, 0x6f,
                0x77, 0x6e, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2a, 0x7b, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x22, 0x68,
                0x65, 0x69, 0x67, 0x68, 0x74, 0x22, 0x3a, 0x20, 0x36, 0x37, 0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x22,
                0x77, 0x65, 0x69, 0x67, 0x68, 0x74, 0x22, 0x3a, 0x20, 0x31, 0x37, 0x35, 0x0d, 0x0a, 0x09, 0x09, 0x7d,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x36, 0x37, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x31, 0x37, 0x35,
                0x00, 0x00, 0x00, 0x00, 0x00, 0xf3, 0x5b, 0x7b, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x22, 0x74, 0x79,
                0x70, 0x65, 0x22, 0x3a, 0x20, 0x22, 0x77, 0x69, 0x6e, 0x74, 0x65, 0x72, 0x20, 0x73, 0x70, 0x6f, 0x72,
                0x74, 0x73, 0x22, 0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x22, 0x6e, 0x61, 0x6d, 0x65, 0x22, 0x3a,
                0x20, 0x22, 0x63, 0x75, 0x72, 0x6c, 0x69, 0x6e, 0x67, 0x22, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x7d, 0x2c,
                0x0d, 0x0a, 0x09, 0x09, 0x09, 0x7b, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x22, 0x74, 0x79, 0x70, 0x65,
                0x22, 0x3a, 0x20, 0x22, 0x73, 0x75, 0x6d, 0x6d, 0x65, 0x72, 0x20, 0x73, 0x70, 0x6f, 0x72, 0x74, 0x73,
                0x22, 0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x22, 0x6e, 0x61, 0x6d, 0x65, 0x22, 0x3a, 0x20, 0x22,
                0x77, 0x61, 0x74, 0x65, 0x72, 0x20, 0x73, 0x6b, 0x69, 0x69, 0x6e, 0x67, 0x22, 0x2c, 0x0d, 0x0a, 0x09,
                0x09, 0x09, 0x09, 0x22, 0x64, 0x65, 0x74, 0x61, 0x69, 0x6c, 0x73, 0x22, 0x3a, 0x20, 0x7b, 0x0d, 0x0a,
                0x09, 0x09, 0x09, 0x09, 0x09, 0x22, 0x6c, 0x6f, 0x63, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x22, 0x3a, 0x20,
                0x7b, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x22, 0x6c, 0x61, 0x74, 0x22, 0x3a, 0x20, 0x34,
                0x39, 0x2e, 0x32, 0x38, 0x32, 0x37, 0x33, 0x30, 0x2c, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09,
                0x22, 0x6c, 0x6f, 0x6e, 0x67, 0x22, 0x3a, 0x20, 0x2d, 0x31, 0x32, 0x33, 0x2e, 0x31, 0x32, 0x30, 0x37,
                0x33, 0x35, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x09, 0x7d, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x7d,
                0x0d, 0x0a, 0x09, 0x09, 0x09, 0x7d, 0x0d, 0x0a, 0x09, 0x09, 0x5d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0f,
                0x22, 0x77, 0x69, 0x6e, 0x74, 0x65, 0x72, 0x20, 0x73, 0x70, 0x6f, 0x72, 0x74, 0x73, 0x22, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x0e, 0x22, 0x77, 0x61, 0x74, 0x65, 0x72, 0x20, 0x73, 0x6b, 0x69, 0x69, 0x6e, 0x67,
                0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3d, 0x7b, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x22,
                0x6c, 0x61, 0x74, 0x22, 0x3a, 0x20, 0x34, 0x39, 0x2e, 0x32, 0x38, 0x32, 0x37, 0x33, 0x30, 0x2c, 0x0d,
                0x0a, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x22, 0x6c, 0x6f, 0x6e, 0x67, 0x22, 0x3a, 0x20, 0x2d, 0x31,
                0x32, 0x33, 0x2e, 0x31, 0x32, 0x30, 0x37, 0x33, 0x35, 0x0d, 0x0a, 0x09, 0x09, 0x09, 0x09, 0x09, 0x7d,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x0b, 0x2d, 0x31, 0x32, 0x33, 0x2e, 0x31, 0x32, 0x30, 0x37, 0x33, 0x35
            };

        private List<OperationSpec> _lookupInSpecs = new List<OperationSpec>
        {
            new OperationSpec {Path = "name"},
            new OperationSpec {Path = "age"},
            new OperationSpec {Path = "animals"},
            new OperationSpec {Path = "animals[1]"},
            new OperationSpec {Path = "attributes"},
            new OperationSpec {Path = "attributes.hair"},
            new OperationSpec {Path = "attributes.dimensions"},
            new OperationSpec {Path = "attributes.dimensions.height"},
            new OperationSpec {Path = "attributes.dimensions.weight"},
            new OperationSpec {Path = "attributes.hobbies"},
            new OperationSpec {Path = "attributes.hobbies[0].type"},
            new OperationSpec {Path = "attributes.hobbies[1].name"},
            new OperationSpec {Path = "attributes.hobbies[1].details.location"},
            new OperationSpec {Path = "attributes.hobbies[1].details.location.long"}
        };

        public class Dimensions
        {
            public int height { get; set; }
            public int weight { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double @long { get; set; }
        }

        public class Details
        {
            public Location location { get; set; }
        }

        public class Hobby
        {
            public string type { get; set; }
            public string name { get; set; }
            public Details details { get; set; }
        }

        public class Attributes
        {
            public string hair { get; set; }
            public Dimensions dimensions { get; set; }
            public List<Hobby> hobbies { get; set; }
        }

        public class Person
        {
            public string name { get; set; }
            public int age { get; set; }
            public List<string> animals { get; set; }
            public Attributes attributes { get; set; }
        }

        [Fact]
        public void Test_Projection()
        {
            var getRequest = new MultiLookup<byte[]>();
            getRequest.ReadAsync(new FakeMemoryOwner<byte>(_lookupInPacket));

            var readResult = new GetResult(new FakeMemoryOwner<byte>(_lookupInPacket),
                new DefaultTranscoder(new DefaultConverter()),
                _lookupInSpecs)
            {
                OpCode = OpCode.MultiLookup,
                Flags = getRequest.Flags,
                Header = getRequest.Header
            };

            var result = readResult.ContentAs<dynamic>();
            Assert.Equal("Emmy-lou Dickerson",result.name.Value);
        }

        [Fact]
        public void Test_Projection_With_Poco()
        {
            var getRequest = new MultiLookup<byte[]>();
            getRequest.ReadAsync(new FakeMemoryOwner<byte>(_lookupInPacket));

            var readResult = new GetResult(new FakeMemoryOwner<byte>(_lookupInPacket),
                new DefaultTranscoder(new DefaultConverter()),
                _lookupInSpecs)
            {
                OpCode = OpCode.MultiLookup,
                Flags = getRequest.Flags,
                Header = getRequest.Header
            };

            var result = readResult.ContentAs<Person>();
            Assert.Equal("Emmy-lou Dickerson",result.name);
        }

        [Fact]
        public void Test_Projection_With_Dictionary()
        {
            var getRequest = new MultiLookup<byte[]>();
            getRequest.ReadAsync(new FakeMemoryOwner<byte>(_lookupInPacket));

            var readResult = new GetResult(new FakeMemoryOwner<byte>(_lookupInPacket),
                new DefaultTranscoder(new DefaultConverter()),
                _lookupInSpecs)
            {
                OpCode = OpCode.MultiLookup,
                Flags = getRequest.Flags,
                Header = getRequest.Header
            };

            var result = readResult.ContentAs<Dictionary<string, dynamic>>();
            Assert.Equal(result["name"], "Emmy-lou Dickerson");
        }
    }
}

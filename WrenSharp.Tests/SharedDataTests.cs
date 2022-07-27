namespace WrenSharp.Tests
{
    public class WrenSharedDataTests
    {
        [Fact]
        public void Add_Contains_Pass()
        {
            var table = new WrenSharedDataTable();
            var handle = table.Add("hello world");
            Assert.True(table.Contains(handle));
        }

        [Fact]
        public void Contains_Fail()
        {
            var table = new WrenSharedDataTable();
            var handle = table.Add("hello world");
            Assert.False(table.Contains(handle + 1));
        }

        [Fact]
        public void Remove_Pass()
        {
            var table = new WrenSharedDataTable();
            var handle = table.Add("hello world");
            table.Remove(handle);
            Assert.False(table.Contains(handle));
        }

        [Fact]
        public void Clear_Pass()
        {
            var table = new WrenSharedDataTable();
            var handle1 = table.Add("hello world");
            var handle2 = table.Add("foobar");
            
            table.Clear();

            Assert.Equal(0, table.Count);
            Assert.False(table.Contains(handle1));
            Assert.False(table.Contains(handle2));
        }

        [Fact]
        public void Add_Remove_SingleHandle_EnsureHandleReuse()
        {
            var table = new WrenSharedDataTable();
            var handle1 = table.Add("hello world");
            table.Remove(handle1);

            var handle2 = table.Add("foobar");

            Assert.Equal(handle1, handle2);
        }

        [Fact]
        public void Add_Remove_MultipleHandles_EnsureHandleReuse()
        {
            var table = new WrenSharedDataTable();
            
            var handle1 = table.Add("hello world");
            var handle2 = table.Add("foobar");
            var handle3 = table.Add("quick brown fox");

            table.Remove(handle2);
            table.Remove(handle1);

            var handle4 = table.Add("jumps over");
            var handle5 = table.Add("the lazy dog");

            // Last handle release should be reused
            Assert.Equal(handle4, handle1);
            Assert.Equal(handle5, handle2);

            table.Remove(handle3);

            var handle6 = table.Add("lorem ipsum dolor");
            Assert.Equal(handle6, handle3);
        }

        [Fact]
        public void Get_ValidHandle_Pass()
        {
            var table = new WrenSharedDataTable();
            var value = "hello world";
            var handle = table.Add(value);
            var retrievedValue = table.Get(handle);
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void Get_FreedHandle_ThrowInvalidHandleException()
        {
            var table = new WrenSharedDataTable();
            var value = "hello world";
            var handle = table.Add(value);

            // Remove the handle
            table.Remove(handle);

            // Attempt to get the value with the now invalid handle
            // This should throw an invalid handle exception
            Assert.Throws<InvalidOperationException>(() => table.Get(handle));
        }

        [Fact]
        public void Get_InvalidHandle_ThrowInvalidHandleException()
        {
            var table = new WrenSharedDataTable();
            table.Add("hello world");

            Assert.Throws<InvalidOperationException>(() => table.Get(WrenSharedDataHandle.Invalid));
        }

        [Fact]
        public void Set_ValidHandle_Pass()
        {
            var table = new WrenSharedDataTable();
            var value = "hello world";
            var handle = table.Add(value);

            // Change the value at the handle's address
            table.Set(handle, "foobar");
            
            Assert.Equal("foobar", (string)table.Get(handle));
        }

        [Fact]
        public void Set_InvalidHandle_ThrowInvalidHandleException()
        {
            var table = new WrenSharedDataTable();
            var handle = (WrenSharedDataHandle)10;
            Assert.Throws<InvalidOperationException>(() => table.Set(handle, "foobar"));
        }

        [Fact]
        public void Set_FreedHandle_ThrowInvalidHandleException()
        {
            var table = new WrenSharedDataTable();
            var handle = table.Add("hello world");
            table.Remove(handle);

            Assert.Throws<InvalidOperationException>(() => table.Set(handle, "foobar"));
        }
    }
}
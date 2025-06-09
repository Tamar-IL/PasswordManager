using MyProject.Common;
using Microsoft.Extensions.Options;
using Xunit;
using BL;
namespace TestProject
{
    public class UnitTest1
    {
        [Fact]
        public void �����_����_�������_������_�����()
        {
            // Arrange (����): ���� ������� �� ����� �����
            var settings = Options.Create(new MySetting
            {
                graphOrder = 3,
                keySize = 256,
                BlockSize = 6,
                subBlockSize = 13
            });

            var key = Enumerable.Range(0, 256).ToArray(); // ���� ����
            var matrix = new int[3, 3]; // ������ ����� 3x3
            var generator = new GenerateKeyEncryption(key, matrix, settings);

            string message = "hello world"; // ����� ������

            // Act (����): �������� ������ �� ������� ���������
            (int[][,] subKeys, List<int> vector) = generator.GenerateSubKeysForEncryption(message);

            // Assert (������): ����� �������� ������
            Assert.NotNull(subKeys); // ����� ��� null
            Assert.NotNull(vector);
            Assert.Equal(subKeys.Length, vector.Count); // ����� ��� ������� ��� ����

            foreach (var subKey in subKeys)
            {
                Assert.Equal(3, subKey.GetLength(0)); // �����
                Assert.Equal(3, subKey.GetLength(1)); // ������
            }
        }

    }
}
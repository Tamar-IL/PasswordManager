using MyProject.Common;
using Microsoft.Extensions.Options;
using Xunit;
using BL;
namespace TestProject
{
    public class UnitTest1
    {
        [Fact]
        public void בדיקה_שתתי_המפתחות_נוצרים_כראוי()
        {
            // Arrange (הכנה): נוצר אובייקט עם נתוני בדיקה
            var settings = Options.Create(new MySetting
            {
                graphOrder = 3,
                keySize = 256,
                BlockSize = 6,
                subBlockSize = 13
            });

            var key = Enumerable.Range(0, 256).ToArray(); // מפתח פשוט
            var matrix = new int[3, 3]; // מטריצה בגודל 3x3
            var generator = new GenerateKeyEncryption(key, matrix, settings);

            string message = "hello world"; // הודעה לבדיקה

            // Act (הרצה): הפונקציה מחזירה את המפתחות והמיקומים
            (int[][,] subKeys, List<int> vector) = generator.GenerateSubKeysForEncryption(message);

            // Assert (בדיקות): בדיקה שהתוצאות תקינות
            Assert.NotNull(subKeys); // לוודא שלא null
            Assert.NotNull(vector);
            Assert.Equal(subKeys.Length, vector.Count); // לוודא שיש תת־מפתח לכל בלוק

            foreach (var subKey in subKeys)
            {
                Assert.Equal(3, subKey.GetLength(0)); // שורות
                Assert.Equal(3, subKey.GetLength(1)); // עמודות
            }
        }

    }
}
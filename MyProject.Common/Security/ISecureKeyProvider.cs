namespace MyProject.Common.Security
{
    // ממשק לספק מפתחות מאובטח
    public interface ISecureKeyProvider
    {
        // מחזיר את המפתח הראשי להצפנה
        byte[] GetMasterKey();

        // מחזיר מטריצת אתחול מאובטחת
        int[,] GetInitializationMatrix();

        // יוצר מפתח ראשי חדש
        void RegenerateMasterKey();

        // בודק אם המפתח קיים
        bool KeyExists();
    }
}
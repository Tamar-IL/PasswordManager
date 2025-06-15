namespace MyProject.Common.Security
{
    /// <summary>
    /// ממשק לספק מפתחות מאובטח
    /// </summary>
    public interface ISecureKeyProvider
    {
        /// <summary>
        /// מחזיר את המפתח הראשי להצפנה
        /// </summary>
        byte[] GetMasterKey();

        /// <summary>
        /// מחזיר מטריצת אתחול מאובטחת
        /// </summary>
        int[,] GetInitializationMatrix();

        /// <summary>
        /// יוצר מפתח ראשי חדש
        /// </summary>
        void RegenerateMasterKey();

        /// <summary>
        /// בודק אם המפתח קיים
        /// </summary>
        bool KeyExists();
    }
}
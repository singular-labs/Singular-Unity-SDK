namespace Singular
{
    public class Utilities
    {
        public static string[][] DelimitedStringsArrayToArrayOfArrayOfString(string[] delimitedStringsArray, char delimiter)
        {
            if (delimitedStringsArray == null || delimitedStringsArray.Length == 0)
            {
                SingularUnityLogger.LogDebug("push notification paths strings array is null or empty. skipping.");
                return null;
            }
            
            string[][] arrayOfArrayOfString = new string[delimitedStringsArray.Length][];
            for (int i = 0; i < delimitedStringsArray.Length; i++)
            {
                arrayOfArrayOfString[i] = delimitedStringsArray[i].Split(delimiter);
            }
            
            return arrayOfArrayOfString;
        }
    }
}
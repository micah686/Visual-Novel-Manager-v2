﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VnManager.Helpers
{
    public static class CheckWriteAccess
    {
        /// <summary>
        /// Checks to see if you can write in a specified directory
        /// Creates an empty file with a GUID as its' name then deletes it.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static bool CheckWrite(string dirPath)
        {
            var testFile = Path.Combine(dirPath, Guid.NewGuid().ToString());
            try
            {
                File.WriteAllText(testFile, "");
                File.Delete(testFile);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

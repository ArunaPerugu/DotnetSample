// <Snippet1>
using System;
using System.IO;

class Test 
{
	
    public static void Main() 
    {
        string path = @"c:\temp\MyTest.txt";

            if (File.Exists(path)) 
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path)) 
            {
                sw.WriteLine("This");
                sw.WriteLine("is some text");
                sw.WriteLine("to test");
                sw.WriteLine("Reading");
            }

            using (StreamReader sr = new StreamReader(path)) 
            {
                //This allows you to do one Read operation.
                Console.WriteLine(sr.ReadToEnd());
            }
    }
}
// </Snippet1>

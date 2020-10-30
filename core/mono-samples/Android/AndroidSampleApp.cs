// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

public static class AndroidSampleApp
{
    [DllImport("native-lib")]
    private extern static int myNum();

    public static int Main(string[] args)
    {
        Console.WriteLine("Hello, Android!"); // logcat
        return myNum();
    }
}

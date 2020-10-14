// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

// it's not part of the BCL but runtime needs it for native-to-managed callbacks in AOT
// To be replaced with NativeCallableAttribute
public class MonoPInvokeCallbackAttribute : Attribute
{
    public MonoPInvokeCallbackAttribute(Type delegateType) { }
}

public static class Program
{
    // Defined in main.m
    [DllImport("__Internal")]
    private extern static void ios_set_text(string value);

    // [DllImport("__Internal")]
    // private extern static void ios_greet_name(string value);

    [DllImport("__Internal")]
    private extern static void ios_register_counter_increment(Action action);

    // [DllImport("__Internal")]
    // private extern static void ios_register_name_greet(Action action);

    private static Action incrementHandler = null;
    // private static Action nameHandler = null;

    private static int counter = 1;

    // Called by native code, see main.m
    [MonoPInvokeCallback(typeof(Action))]
    private static void IncrementCounter()
    {
        ios_set_text("Clicked " + counter++ + " times!");
    }

    // [MonoPInvokeCallback(typeof(Action))]
    // private static void GreetName()
    // {
    //     ios_greet_name("Mitch");
    // }

    public static async Task Main(string[] args)
    {
        // Register a managed callback (will be called by UIButton, see main.m)
        // Also, keep the handler alive so GC won't collect it.
        ios_register_counter_increment(incrementHandler = IncrementCounter);
        // ios_register_name_greet(nameHandler = GreetName);

        const string msg = "Hello World!";
        for (int i = 0; i < msg.Length; i++)
        {
            // a kind of an animation
            ios_set_text(msg.Substring(0, i + 1));
            await Task.Delay(100);
        }

        Console.WriteLine("Done!");
        await Task.Delay(-1);
    }
}

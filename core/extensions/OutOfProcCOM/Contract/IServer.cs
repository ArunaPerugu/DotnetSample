﻿using System;
using System.Runtime.InteropServices;

[ComVisible(true)]
[Guid(Contract.Constants.ServerInterface)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IServer
{
    /// <summary>
    /// Compute the value of the constant Pi.
    /// </summary>
    double ComputePi();
}

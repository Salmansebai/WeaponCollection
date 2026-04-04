using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using WeaponCollection.Services;

namespace WeaponCollection.ViewModels;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    internal DeviceOrientationService? MyScanner;

    public void Dispose()
    {
        MyScanner?.ClosePort();
        _cts.Cancel();
        _cts.Dispose();
        
    }
}
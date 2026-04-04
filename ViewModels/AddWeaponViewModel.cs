using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MongoDB.Bson;
using WeaponCollection.Models;
using WeaponCollection.Services;

namespace WeaponCollection.ViewModels;

public partial class AddWeaponViewModel : ViewModelBase
{
    public IRelayCommand BackCommand { get; }
    private readonly DeviceOrientationService _scanner = new();

    [ObservableProperty] private string? name;
    [ObservableProperty] private string? type;
    [ObservableProperty] private int damage;
    [ObservableProperty] private double weight;
    [ObservableProperty] private double price;
    [ObservableProperty] private string? description;
    [ObservableProperty] private string? era;
    [ObservableProperty] private string? country;
    [ObservableProperty] private string? imageUrl;
    [ObservableProperty] private string? scannedId;
    [ObservableProperty] private bool isScanning;
    [ObservableProperty] private string scanStatus = "En attente du scanner...";
    [ObservableProperty] private string scanButtonText = "📷 Scanner";
    [ObservableProperty] private string scanButtonColor = "DodgerBlue";
    [ObservableProperty] private Bitmap? previewImage;
    [ObservableProperty] private bool hasPreviewImage;
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool hasStatus;
    [ObservableProperty] private bool isSuccess;

    public AddWeaponViewModel(IRelayCommand backCommand)
    {
        BackCommand = backCommand;
    }

    [RelayCommand]
    private async Task PickImage()
    {
        var window = (Avalonia.Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (window == null) return;

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choisir une image",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Images")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" }
                }
            }
        });

        if (files.Count > 0)
        {
            var path = files[0].Path.LocalPath;
            ImageUrl = path;
            try
            {
                PreviewImage = new Bitmap(path);
                HasPreviewImage = true;
            }
            catch
            {
                PreviewImage = null;
                HasPreviewImage = false;
            }
        }
    }

    [RelayCommand]
    private void RemoveImage()
    {
        PreviewImage = null;
        HasPreviewImage = false;
        ImageUrl = null;
    }

    [RelayCommand]
    private void ToggleScan()
    {
        if (IsScanning)
        {
            _scanner.ClosePort();
            _scanner.SerialBuffer.Changed -= OnDataReceived;
            IsScanning = false;
            ScanButtonText = "📷 Scanner";
            ScanButtonColor = "DodgerBlue";
            ScanStatus = "";
        }
        else
        {
            try
            {
                _scanner.SerialBuffer.Changed += OnDataReceived;
                _scanner.OpenPort();
                IsScanning = true;
                ScanButtonText = "⏹ Stop";
                ScanButtonColor = "Red";
                ScanStatus = "En attente du scanner...";
            }
            catch (Exception ex)
            {
                ScanStatus = $"Erreur : {ex.Message}";
                IsScanning = false;
            }
        }
    }

    private void OnDataReceived(object? sender, EventArgs e)
    {
        if (_scanner.SerialBuffer.Count > 0)
        {
            var data = _scanner.SerialBuffer.Dequeue()?.ToString();
            if (!string.IsNullOrWhiteSpace(data))
            {
                ScannedId = data.Trim();
                ScanStatus = $"ID scanné : {ScannedId}";
                _scanner.ClosePort();
                _scanner.SerialBuffer.Changed -= OnDataReceived;
                IsScanning = false;
                ScanButtonText = "📷 Scanner";
                ScanButtonColor = "DodgerBlue";
            }
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "⚠ Le nom est obligatoire !";
            IsSuccess = false;
            HasStatus = true;
            return;
        }

        var weapon = new Weapon
        {
            Id = string.IsNullOrWhiteSpace(ScannedId)
                ? ObjectId.GenerateNewId()
                : new ObjectId(ScannedId),
            Name = Name ?? "",
            Type = Type ?? "",
            Damage = Damage,
            Weight = Weight,
            Price = Price,
            Description = Description ?? "",
            Era = Era ?? "",
            Country = Country ?? "",
            PictureUrl = ImageUrl ?? "",
            Picture = PreviewImage
        };

        MyGlobals.MyWeapons.Add(weapon);

        var service = new WeaponService();
        await service.SaveWeaponsAsync(new List<Weapon>(MyGlobals.MyWeapons));

        if (IsScanning)
            _scanner.ClosePort();

        // Message de succès
        StatusMessage = $"✅ \"{weapon.Name}\" ajouté avec succès !";
        IsSuccess = true;
        HasStatus = true;

        await Task.Delay(1500);
        BackCommand.Execute(null);
    }
}

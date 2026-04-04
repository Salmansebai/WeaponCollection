using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MongoDB.Bson;
using WeaponCollection.Models;
using WeaponCollection.Services;

namespace WeaponCollection.ViewModels;

public partial class EditWeaponViewModel : ViewModelBase
{
    public IRelayCommand BackCommand { get; }
    private readonly Weapon _originalWeapon;

    [ObservableProperty] private string? name;
    [ObservableProperty] private string? type;
    [ObservableProperty] private int damage;
    [ObservableProperty] private double weight;
    [ObservableProperty] private double price;
    [ObservableProperty] private string? description;
    [ObservableProperty] private string? era;
    [ObservableProperty] private string? country;
    [ObservableProperty] private string? imageUrl;
    [ObservableProperty] private Bitmap? previewImage;
    [ObservableProperty] private bool hasPreviewImage;
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool hasStatus;

    public EditWeaponViewModel(ObjectId id, IRelayCommand backCommand)
    {
        BackCommand = backCommand;
        _originalWeapon = MyGlobals.MyWeapons.First(w => w.Id == id);

        Name = _originalWeapon.Name;
        Type = _originalWeapon.Type;
        Damage = _originalWeapon.Damage;
        Weight = _originalWeapon.Weight;
        Price = _originalWeapon.Price;
        Description = _originalWeapon.Description;
        Era = _originalWeapon.Era;
        Country = _originalWeapon.Country;
        ImageUrl = _originalWeapon.PictureUrl;
        PreviewImage = _originalWeapon.Picture as Bitmap;
        HasPreviewImage = PreviewImage != null;
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
    private async Task Save()
    {
        // Mise à jour de l'arme
        _originalWeapon.Name = Name ?? "";
        _originalWeapon.Type = Type ?? "";
        _originalWeapon.Damage = Damage;
        _originalWeapon.Weight = Weight;
        _originalWeapon.Price = Price;
        _originalWeapon.Description = Description ?? "";
        _originalWeapon.Era = Era ?? "";
        _originalWeapon.Country = Country ?? "";
        _originalWeapon.PictureUrl = ImageUrl ?? "";
        _originalWeapon.Picture = PreviewImage;

        // Sauvegarde sur le serveur
        var service = new WeaponService();
        await service.SaveWeaponsAsync(new List<Weapon>(MyGlobals.MyWeapons));

        // Message de succès
        StatusMessage = $"✅ \"{_originalWeapon.Name}\" modifié avec succès !";
        HasStatus = true;

        // Attendre 1.5 secondes puis retourner vers Details
        await Task.Delay(1500);
        BackCommand.Execute(null);
    }
}

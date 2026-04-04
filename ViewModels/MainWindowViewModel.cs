using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MongoDB.Bson;
using WeaponCollection.Models;
using WeaponCollection.Services;

namespace WeaponCollection.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    public IRelayCommand GoToCsvCommand { get; }

    public MainWindowViewModel()
    {
        GoToCsvCommand = new RelayCommand(GoToCsv);
        CurrentPage = new HomeViewModel(
            GoToCollectionCommand,
            GoToDetailsFromHomeCommand,
            LoadWeaponsCommand);
    }

    [RelayCommand]
    private async Task LoadWeapons()
    {
        MyGlobals.MyWeapons.Clear();
        var service = new WeaponService();
        var weapons = await service.LoadWeaponsAsync();

        if (weapons.Count == 0)
        {
            weapons = new List<Weapon>
            {
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "AK-47", Type = "Assault Rifle", Damage = 80, Weight = 4, Era = "Cold War", Country = "Russia", Description = "Fusil d'assaut soviétique légendaire.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/aka47.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Battle Axe", Type = "Axe", Damage = 60, Weight = 7, Era = "Medieval", Country = "Europe", Description = "Hache de guerre médiévale.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/axes.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Sword", Type = "Melee", Damage = 50, Weight = 3, Era = "Ancient", Country = "Europe", Description = "Épée longue à double tranchant.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/sword.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "M16", Type = "Assault Rifle", Damage = 100, Weight = 6, Era = "Modern", Country = "USA", Description = "Fusil d'assaut américain.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/m16.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Katana", Type = "Melee", Damage = 70, Weight = 1.5, Era = "Feudal Japan", Country = "Japan", Description = "Sabre japonais des samouraïs.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/katana.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Desert Eagle", Type = "Pistol", Damage = 75, Weight = 2, Era = "Modern", Country = "USA", Description = "Pistolet semi-automatique puissant.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/desert_eagle.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Longbow", Type = "Bow", Damage = 45, Weight = 2.5, Era = "Medieval", Country = "England", Description = "Arc long anglais.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/longbow.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "RPG-7", Type = "Rocket Launcher", Damage = 200, Weight = 7, Era = "Cold War", Country = "Russia", Description = "Lance-roquettes antichar.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/rpg7.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Glock 17", Type = "Pistol", Damage = 55, Weight = 0.7, Era = "Modern", Country = "Austria", Description = "Pistolet léger et fiable.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/glock17.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Crossbow", Type = "Bow", Damage = 65, Weight = 3.5, Era = "Medieval", Country = "Europe", Description = "Arbalète médiévale.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/crossbow.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Spear", Type = "Melee", Damage = 55, Weight = 2, Era = "Ancient", Country = "Greece", Description = "Lance grecque des hoplites.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/spear.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Sniper AWP", Type = "Sniper Rifle", Damage = 150, Weight = 6.5, Era = "Modern", Country = "UK", Description = "Fusil de précision britannique.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/sniper_awp.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Flail", Type = "Melee", Damage = 65, Weight = 4, Era = "Medieval", Country = "Europe", Description = "Fléau d'armes médiéval.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/flail.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "MP5", Type = "Submachine Gun", Damage = 60, Weight = 2.5, Era = "Modern", Country = "Germany", Description = "Pistolet-mitrailleur allemand.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/mp5.png" },
                new Weapon { Id = ObjectId.GenerateNewId(), Name = "Gladius", Type = "Melee", Damage = 48, Weight = 1.2, Era = "Ancient", Country = "Rome", Description = "Épée courte romaine.", PictureUrl = "https://raw.githubusercontent.com/Salmansebai/WeaponCollection/main/gladius.png" }
            };
            await service.SaveWeaponsAsync(weapons);
        }

        foreach (var weapon in weapons)
            MyGlobals.MyWeapons.Add(weapon);
    }

    partial void OnCurrentPageChanging(ViewModelBase? oldValue, ViewModelBase? newValue)
    {
        oldValue?.Dispose();
    }

    [RelayCommand]
    private void GoToCollection()
    {
        CurrentPage = new CollectionViewModel(GoToDetailsFromCollectionCommand, GoToAddCommand, GoToHomeCommand, GoToCsvCommand);
    }

    [RelayCommand]
    private void GoToHome()
    {
        CurrentPage = new HomeViewModel(GoToCollectionCommand, GoToDetailsFromHomeCommand, LoadWeaponsCommand);
    }

    [RelayCommand]
    private void GoToDetailsFromHome(ObjectId weaponId)
    {
        CurrentPage = new CollectionDetailsViewModel(weaponId, GoToHomeCommand, GoToEditFromHomeCommand);
    }

    [RelayCommand]
    private void GoToDetailsFromCollection(ObjectId weaponId)
    {
        CurrentPage = new CollectionDetailsViewModel(weaponId, GoToCollectionCommand, GoToEditFromCollectionCommand);
    }

    // Edit depuis Home → Back revient vers Details de Home
    [RelayCommand]
    private void GoToEditFromHome(ObjectId weaponId)
    {
        CurrentPage = new EditWeaponViewModel(weaponId, new RelayCommand(() =>
        {
            CurrentPage = new CollectionDetailsViewModel(weaponId, GoToHomeCommand, GoToEditFromHomeCommand);
        }));
    }

    // Edit depuis Collection → Back revient vers Details de Collection
    [RelayCommand]
    private void GoToEditFromCollection(ObjectId weaponId)
    {
        CurrentPage = new EditWeaponViewModel(weaponId, new RelayCommand(() =>
        {
            CurrentPage = new CollectionDetailsViewModel(weaponId, GoToCollectionCommand, GoToEditFromCollectionCommand);
        }));
    }

    [RelayCommand]
    private void GoToAdd()
    {
        CurrentPage = new AddWeaponViewModel(GoToCollectionCommand);
    }

    private void GoToCsv()
    {
        CurrentPage = new CsvViewModel(GoToCollectionCommand);
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MongoDB.Bson;
using WeaponCollection.Models;

namespace WeaponCollection.ViewModels;

public partial class CollectionViewModel : ViewModelBase
{
    public IRelayCommand<ObjectId> FromParentCommand { get; set; }
    public ObservableCollection<Weapon> MyObservableWeapons => MyGlobals.MyWeapons;
    public IRelayCommand GoToAddCommand { get; }
    public IRelayCommand BackCommand { get; }
    public IRelayCommand GoToCsvCommand { get; }

    [ObservableProperty]
    private Weapon? _selectedWeapon;

    public CollectionViewModel(
        IRelayCommand<ObjectId> fromParentCommand,
        IRelayCommand goToAddCommand,
        IRelayCommand backCommand,
        IRelayCommand goToCsvCommand)
    {
        FromParentCommand = fromParentCommand;
        GoToAddCommand = goToAddCommand;
        BackCommand = backCommand;
        GoToCsvCommand = goToCsvCommand;
    }
}

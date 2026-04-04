using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MongoDB.Bson;
using WeaponCollection.Models;
using WeaponCollection.Services;

namespace WeaponCollection.ViewModels;

public partial class CollectionDetailsViewModel : ViewModelBase
{
    [ObservableProperty]
    private Weapon _myWeapon;

    [ObservableProperty]
    private bool _showConfirmDelete;

    [ObservableProperty]
    private bool _showDeleteSuccess;

    [ObservableProperty]
    private string _deletedWeaponName = "";

    public IRelayCommand BackCommand { get; }
    public IRelayCommand<ObjectId> EditCommand { get; }
    public IRelayCommand ShowDeleteConfirmCommand { get; }
    public IRelayCommand CancelDeleteCommand { get; }
    public IRelayCommand ConfirmDeleteCommand { get; }

    public CollectionDetailsViewModel(ObjectId id, IRelayCommand backCommand, IRelayCommand<ObjectId> editCommand)
    {
        BackCommand = backCommand;
        EditCommand = editCommand;
        MyWeapon = MyGlobals.MyWeapons.First(w => w.Id == id);

        ShowDeleteConfirmCommand = new RelayCommand(() => ShowConfirmDelete = true);
        CancelDeleteCommand = new RelayCommand(() => ShowConfirmDelete = false);
        ConfirmDeleteCommand = new AsyncRelayCommand(ConfirmDelete);
    }

    private async Task ConfirmDelete()
    {
        ShowConfirmDelete = false;
        DeletedWeaponName = MyWeapon.Name;
        MyGlobals.MyWeapons.Remove(MyWeapon);

        var service = new WeaponService();
        await service.SaveWeaponsAsync(new List<Weapon>(MyGlobals.MyWeapons));

        // Affiche le message de succès
        ShowDeleteSuccess = true;

        // Attendre 3s puis retourner
        await Task.Delay(3000);
        BackCommand.Execute(null);
    }
}

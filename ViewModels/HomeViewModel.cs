using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MongoDB.Bson;
using WeaponCollection.Models;

namespace WeaponCollection.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public IRelayCommand GoToCollectionCommand { get; }
    public IRelayCommand<ObjectId> GoToDetailsCommand { get; }
    public IRelayCommand LoadWeaponsCommand { get; }

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private ObservableCollection<Weapon> _filteredWeapons = new();

    // Montant total de la collection
    [ObservableProperty]
    private double _totalPrice;

    // Message pour l'utilisateur
    [ObservableProperty]
    private bool _isCollectionEmpty = true;

    public HomeViewModel(
        IRelayCommand goToCollectionCommand,
        IRelayCommand<ObjectId> goToDetailsCommand,
        IRelayCommand loadWeaponsCommand)
    {
        GoToCollectionCommand = goToCollectionCommand;
        GoToDetailsCommand = goToDetailsCommand;
        LoadWeaponsCommand = loadWeaponsCommand;

        MyGlobals.MyWeapons.CollectionChanged += (s, e) => FilterWeapons();
        FilterWeapons();
    }

    partial void OnSearchTextChanged(string? value)
    {
        FilterWeapons();
    }

    private void FilterWeapons()
    {
        FilteredWeapons.Clear();
        var query = SearchText?.ToLower() ?? "";
        var results = string.IsNullOrWhiteSpace(query)
            ? MyGlobals.MyWeapons
            : MyGlobals.MyWeapons.Where(w =>
                w.Name.ToLower().Contains(query) ||
                w.Type.ToLower().Contains(query) ||
                w.Era.ToLower().Contains(query) ||
                w.Country.ToLower().Contains(query));

        foreach (var weapon in results)
            FilteredWeapons.Add(weapon);

        // Calcul du montant total
        TotalPrice = MyGlobals.MyWeapons.Sum(w => w.Price);

        // Message si collection vide
        IsCollectionEmpty = MyGlobals.MyWeapons.Count == 0;
    }
}

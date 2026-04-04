using System.Collections.ObjectModel;
using WeaponCollection.Models;

namespace WeaponCollection;

public static class MyGlobals
{
    public static ObservableCollection<Weapon> MyWeapons { get; } = new();
}

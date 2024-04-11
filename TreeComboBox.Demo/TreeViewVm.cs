using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TreeComboBox.Demo;

public class TreeViewVm : ObservableObject
{
    public ObservableCollection<TreeViewItemVm> Items { get; set; }

    public ObservableCollection<FirstItem>? MultipleLevelItems { get; set; }

    private ObservableCollection<TreeViewItemVm> _items;

    public TreeViewVm()
    {
        _items = new ObservableCollection<TreeViewItemVm>()
        {
            new TreeViewItemVm() { Name = "Item 1", Id = "1", leaf = 0 },
            new TreeViewItemVm() { Name = "Item 2", Id = "2", leaf = 0 },
            new TreeViewItemVm()
            {
                Name = "Item 3", Id = "3", leaf = 0,
                Items = new ObservableCollection<TreeViewItemVm>()
                {
                    new TreeViewItemVm() { Name = "Item 3.1", Id = "3.1", leaf = 1 },
                    new TreeViewItemVm() { Name = "Item 3.2", Id = "3.2", leaf = 1 },
                    new TreeViewItemVm() { Name = "Item 3.3", Id = "3.3", leaf = 1 },
                },
            },
        };
        Items = _items;
        MultipleLevelItems = new();
        for (int i = 1; i < 6; i++)
        {
            FirstItem firstItem = new FirstItem { Id = i, Name = $"FirstItem {i}" };
            firstItem.SecondItems = new();
            for (int j = 1; j < 6; j++)
            {
                SecondItem secondItem = new SecondItem { Id = j, Name = $"SecondItem {j}" };
                secondItem.ThirdItemItems = new();
                for (int k = 1; k < 6; k++)
                {
                    ThirdItem thirdItem = new ThirdItem { Id = k, Name = $"ThirdItem {k}" };
                    secondItem.ThirdItemItems.Add(thirdItem);
                }

                firstItem.SecondItems.Add(secondItem);
            }

            MultipleLevelItems.Add(firstItem);
        }
    }

    public void Filter(string name)
    {
        Items = Filter(_items.ToList(), name);
        OnPropertyChanged(nameof(Items));
    }

    private ObservableCollection<TreeViewItemVm> Filter(List<TreeViewItemVm> list, string name)
    {
        ObservableCollection<TreeViewItemVm> result = new ObservableCollection<TreeViewItemVm>();
        foreach (var item in list)
        {
            if (item.Items != null && item.Items.Any())
            {
                //再过滤子集的子集有没有数据
                var r = Filter(item.Items.ToList(), name);
                if (r.Any())
                {
                    item.Items = r;
                    result.Add(item);
                }
            }
            else
            {
                if (item.Name.Contains(name))
                {
                    result.Add(item);
                }
            }
        }

        return result;
    }


    public partial class TreeViewItemVm : ObservableObject
    {
        public ObservableCollection<TreeViewItemVm> Items { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }

        public int leaf { get; set; }
    }

    public class ItemBase
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class FirstItem : ItemBase
    {
        public ObservableCollection<SecondItem>? SecondItems { get; set; }
    }

    public class SecondItem : ItemBase
    {
        public ObservableCollection<ThirdItem>? ThirdItemItems { get; set; }
    }

    public class ThirdItem : ItemBase
    {
    }
}
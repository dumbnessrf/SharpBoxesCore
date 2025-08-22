using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.Mvvm;

public class VMBase : INotifyPropertyChanged, INotifyPropertyChanging
{
    public event PropertyChangingEventHandler PropertyChanging;
    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetProperty<T>(
        ref T storage,
        T value,
        [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null
    )
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        OnPropertyChanging(propertyName);
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanging(
        [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null
    )
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    protected void OnPropertyChanged(
        [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null
    )
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(
        ref T storage,
        T value,
        Action<T> onChanged,
        [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null
    )
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        OnPropertyChanging(propertyName);
        storage = value;
        onChanged?.Invoke(value);
        OnPropertyChanged(propertyName);
        return true;
    }


}

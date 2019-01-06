using System.ComponentModel;

namespace ID3TagEditLib
{
    public interface ISync : INotifyPropertyChanged
    {
        bool IsSync { get; }
    }
}

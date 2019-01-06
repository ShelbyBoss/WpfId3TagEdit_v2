using ID3TagLib;
using System.ComponentModel;

namespace ID3TagEditLib
{
    public class EditFrame : INotifyPropertyChanged
    {
        protected Frame parent;

        public string FrameId { get { return parent.FrameId; } }

        public EditFrame(Frame parent)
        {
            this.parent = parent;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static implicit operator Frame(EditFrame frame)
        {
            return frame.parent;
        }

        public static implicit operator EditFrame(Frame frame)
        {
            if (frame is TextFrame) return new EditTextFrame((TextFrame)frame);

            return new EditFrame(frame);
        }
    }
}

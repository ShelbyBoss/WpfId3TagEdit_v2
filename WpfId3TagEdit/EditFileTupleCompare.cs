using ID3TagEditLib;
using System;
using System.Collections.Generic;

namespace WpfId3TagEdit
{
    class EditFileTupleCompare : IComparer<Tuple<EditID3File, IsUnsynchronizedDetector>>
    {
        public int Compare(Tuple<EditID3File, IsUnsynchronizedDetector> x, Tuple<EditID3File, IsUnsynchronizedDetector> y)
        {
            return string.Compare(x.Item1.FileName.FileName, y.Item1.FileName.FileName);
        }
    }
}

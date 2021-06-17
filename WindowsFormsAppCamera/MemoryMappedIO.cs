using System.Windows.Forms;
using System.IO.MemoryMappedFiles;

namespace WindowsFormsAppCamera
{
    class MMIo
    {
        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;

        public MMIo()
        {
            _mmf = MemoryMappedFile.CreateNew("DivGrindName", 30);
            _accessor = _mmf.CreateViewAccessor();
        }

        public void Write(string s)
        {
            _accessor.WriteArray(0, s.ToCharArray(), 0, s.Length);
        }

        public void Close()
        {
            _accessor.Dispose();
            _mmf.Dispose();
        }
    }
}

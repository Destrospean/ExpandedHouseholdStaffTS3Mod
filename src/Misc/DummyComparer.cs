using System.Collections.Generic;

namespace zoeoeAndDestrospean.Misc
{
    public class DummyComparer : IComparer<string>
    {
        public int Compare(string a, string b)
        {
            return 1;
        }
    }
}

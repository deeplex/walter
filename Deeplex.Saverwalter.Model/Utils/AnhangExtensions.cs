namespace Deeplex.Saverwalter.Model
{
    public static class AnhangExtensions
    {
        public static string getPath(this Anhang a, string root)
        {
            return System.IO.Path.Combine(root, a.AnhangId + System.IO.Path.GetExtension(a.FileName));
        }
    }
}

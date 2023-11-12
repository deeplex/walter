namespace Deeplex.Saverwalter.Model
{
    public class HKVO
    {
        public int HKVOId { get; set; }

        public double? HKVO_P7 { get; set; }
        public double? HKVO_P8 { get; set; }
        public HKVO_P9A2? HKVO_P9 { get; set; }

        public string? Notiz { get; set; }
    }

    public enum HKVO_P9A2
    {
        Satz_1 = 1,
        Satz_2 = 2,
        Satz_4 = 4,
    }
}

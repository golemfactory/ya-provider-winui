using System;
using System.Collections.Generic;

namespace GolemUI
{
    public class NameGen
    {
        private static List<string> _elvenPrefixes = new List<string> { "Ael", "Aer", "Af", "Ah", "Al", "Am", "Ama", "An", "Ang", "Ansr", "Ar", "Ari", "Arn", "Aza", "Bael", "Bes", "Cael", "Cal", "Cas", "Cla", "Cor", "Cy", "Dae", "Dho", "Dre", "Du", "Eil", "Eir", "El", "Er", "Ev", "Fera", "Fi", "Fir", "Fis", "Gael", "Gar", "Gil", "Ha", "Hu", "Ia", "Il", "Ja", "Jar", "Ka", "Kan", "Ker", "Keth", "Koeh", "Kor", "La", "Laf", "Lam", "Lue", "Ly", "Mai", "Mal", "Mara", "My", "Na", "Nai", "Nim", "Nu", "Ny", "Py", "Raer", "Re", "Ren", "Rhy", "Ru", "Rua", "Rum", "Rid", "Sae", "Seh", "Sel", "Sha", "She", "Si", "Sim", "Sol", "Sum", "Syl", "Ta", "Tahl", "Tha", "Tho", "Ther", "Thro", "Tia", "Tra", "Ty", "Uth", "Ver", "Vil", "Von", "Ya", "Za", "Zy" };
        private static List<string> _elvenSuffixes = new List<string> { "ae", "ael", "aer", "aias", "ah", "aith", "al", "ali", "am", "an", "ar", "ari", "aro", "as", "ath", "avel", "brar", "dar", "deth", "dre", "drim", "dul", "ean", "el", "emar", "en", "er", "ess", "evar", "fel", "hal", "har", "hel", "ian", "iat", "ik", "il", "im", "in", "ir", "is", "ith", "kash", "ki", "lan", "lam", "lar", "las", "lian", "lis", "lyn", "mah", "mil", "mus", "nal", "nes", "nin", "nis", "on", "or", "oth", "que", "quis", "rah", "rad", "rail", "ran", "reth", "ro", "ruil", "sal", "san", "sar", "sel", "sha", "spar", "tae", "tas", "ten", "thal", "thar", "ther", "thi", "thus", "ti", "tril", "ual", "uath", "us", "van", "var", "vain", "via", "vin", "wyn", "ya", "yr", "yth", "zair" };


        private static List<string> _dwarvenPrefixes = new List<string> { "Blain", "Baraz", "Nithi", "Gim", "Fund", "Onarr", "Vargr", "Suthrin", "Skjald", "Kheled", "Ty", "Uth", "Ver", "Vil", "Von", "Ya", "Za", "Nyr", "Oinn" };
        private static List<string> _dwarvenSuffixes = new List<string> { "in", "emar", "nala", "ri", "ari", "sha", "u", "hle", "mim", "andu" };

        private readonly Random _random;

        public NameGen()
        {
            _random = new Random(Environment.TickCount);
        }
        public static string GetRandom(List<string> items, Random rnd)
        {
            return items[rnd.Next(items.Count)];
        }

        public string GenerateElvenName()
        {
            var optional = _random.Next(2) == 0 ? GetRandom(_elvenSuffixes, _random) : string.Empty;
            if (optional.Length > 3)
            {
                optional = "";
            }
            return $"{GetRandom(_elvenPrefixes, _random)}{GetRandom(_elvenSuffixes, _random)}{optional}";
        }
        public string GenerateDwarvenName()
        {
            var optional = _random.Next(2) == 0 ? GetRandom(_dwarvenSuffixes, _random) : string.Empty;
            if (optional.Length > 3)
            {
                optional = "";
            }
            return $"{GetRandom(_dwarvenPrefixes, _random)}{GetRandom(_dwarvenSuffixes, _random)}{optional}";
        }
    }
}

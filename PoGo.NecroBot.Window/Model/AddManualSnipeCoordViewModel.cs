using POGOProtos.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PoGo.NecroBot.Window.Model
{
    public class AddManualSnipeCoordViewModel : ViewModelBase
    {
        [Range(-90, 90)]
        public double Latitude { get; set; }
        [Range(-180, 180)]
        public double Longitude { get; set; }
        public PokemonId PokemonId { get; set; }

        public string Move1 { get; set; }
        public string Move2 { get; set; }
        public double IV { get; set; }
        private string freeInput;
        public string InputText
        {
            get { return freeInput; }
            set
            {
                freeInput = value;
                Parse(value);
                RaisePropertyChanged("InputText");
            }
        }

        public void Clear()
        {
            Longitude = 0;
            Latitude = 0;
            PokemonId = PokemonId.Missingno;
            InputText = "";
            RaisePropertyChanged("Latitude");
            RaisePropertyChanged("Longitude");
            RaisePropertyChanged("PokemonId");
            RaisePropertyChanged("InputText");
        }

        public void Parse(string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            var pid = LookPokemonId(content);
            if (pid == PokemonId.Missingno || pid == PokemonId.Mewtwo || pid == PokemonId.Mew) return;

            PokemonId = pid;

            IV = LookIV(content);

            var coord = LookCoord(content);

            if (coord == null) return;


            Latitude = coord.Item1;
            Longitude = coord.Item2;

            var moves = LookMoves(content);
            if (moves != null)
            {
                Move1 = moves.Item1.ToString();
                Move2 = moves.Item2.ToString();
            }

            RaisePropertyChanged("Latitude");
            RaisePropertyChanged("Longitude");
            RaisePropertyChanged("PokemonId");
            RaisePropertyChanged("InputText");
            RaisePropertyChanged("IV");
            RaisePropertyChanged("Move1");
            RaisePropertyChanged("Move2");

        }

        private Tuple<PokemonMove, PokemonMove> LookMoves(string content)
        {
            string pattern = @"(\w*\s?\w*)(\s?[\s]*[\/\-]\s?[\s]*)(\w*\s?\w*)";

            var match = Regex.Match(content, pattern, RegexOptions.Multiline);
            if (match != null && match.Groups.Count >= 3)
            {
                PokemonMove move1 = PokemonMove.MoveUnset;
                PokemonMove move2 = PokemonMove.MoveUnset;
                Enum.TryParse(match.Groups[1].Value.Replace(" ", string.Empty).Trim(), true, out move1);
                Enum.TryParse(match.Groups[3].Value.Replace(" ", string.Empty).Trim(), true, out move2);

                if (move1 != PokemonMove.MoveUnset && move2 != PokemonMove.MoveUnset)
                {
                    return new Tuple<PokemonMove, PokemonMove>(move1, move2);
                }

            }
            return null;
        }

        private double LookIV(string content)
        {
#pragma warning disable IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
            string[] patterns = new string[]
            {
               @"(\d{1,2}\.?\d*?)%"   ,
               @"(\d*\.?\d*)iv"
            };
            foreach (var p in patterns)
            {
                var match = Regex.Match(content, p);
                double x;
                if (match != null && !string.IsNullOrEmpty(match.Value))
                {
                    double.TryParse(match.Groups[1].Value, out x);
                    return x;
                }
            }
            if (content.Contains("💯")) return 100.0;
            return 0;
#pragma warning restore IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
        }

        private Tuple<double, double> LookCoord(string content)
        {
            var regexPattern = @"(\-?\d{1,3}.\d{5,})([,;\s]*)(\-?\d{1,3}.\d{5,})";

            var match = Regex.Match(content, regexPattern);
            if (match != null && match.Groups.Count >= 3)
            {

                var lat = Convert.ToDouble(match.Groups[1].Value, CultureInfo.InvariantCulture);
                var lng = Convert.ToDouble(match.Groups[3].Value, CultureInfo.InvariantCulture);
                return new Tuple<double, double>(lat, lng);
            }
            return null;
        }

        private PokemonId LookPokemonId(string content)
        {
            foreach (var pid in Enum.GetValues(typeof(PokemonId)))
            {
                string name = ((PokemonId)pid).ToString();
                if (content.ToLower().Contains(name.ToLower()))
                    return (PokemonId)pid;
            }
            return PokemonId.Missingno;
        }
    }
}

using POGOProtos.Enums;
using RocketBot2.Helpers;
using System;
using System.Windows.Forms;

namespace RocketBot2.Forms
{
    public partial class PokeDex : System.Windows.Forms.Form
    {
        public PokeDex()
        {
            InitializeComponent();
        }

        private void PokeDex_Load(object sender, EventArgs e)
        {
            foreach (PokemonId x in Enum.GetValues(typeof(PokemonId)))
            {
                if (x == PokemonId.Missingno || (int)x > 251) continue;
                var pic = new PictureBox();
                pic.Image = ResourceHelper.ResizeImage(ResourceHelper.GetPokemonImage((int)x), pic, true);
                flpdex.Controls.Add(pic);
            }
        }
    }
}

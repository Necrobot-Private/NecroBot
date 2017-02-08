using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class BaseConfig
    {
        public BaseConfig()
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var d = prop.GetCustomAttribute<DefaultValueAttribute>();

                if (d != null)
                {

                    if (prop.PropertyType == typeof(List<PokemonId>))
                    {
                        var arr = d.Value.ToString().Split(new char[] { ';' });
                        var list = new List<PokemonId>();
                        foreach (var pname in arr)
                        {
                            PokemonId pi = PokemonId.Missingno;
                            if (Enum.TryParse<PokemonId>(pname, true, out pi))
                            {
                                list.Add(pi);
                            }
                        }
                        prop.SetValue(this, list);
                    }
                    else
                        prop.SetValue(this, d.Value);
                }
            }
        }
    }
}
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using PoGo.NecroBot.Logic.Model.Settings;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMasterConfig
{
    public class ExcelConfigHelper
    {
        public static void MigrateFromObject(GlobalSettings setting, string sourceTemplate, string destination)
        {
            using (var package = new ExcelPackage(new FileInfo(sourceTemplate)))
            {
                var pokemonFilter = package.Workbook.Worksheets["Pokemons"];
                pokemonFilter.Protection.IsProtected = true;
                pokemonFilter.Cells["J2:AE153"].Style.Locked = false;
                MigrateCatchPokemonFilter(pokemonFilter, setting);
                MigrateTransferPokemonFilter(pokemonFilter, setting);
                MigrateUpgradePokemonFilter(pokemonFilter, setting);
                MigrateEvolvePokemonFilter(pokemonFilter, setting);
                MigrateSnipePokemonFilter(pokemonFilter, setting);

                foreach (var item in setting.GetType().GetFields())
                {
                    var att = item.GetCustomAttributes(typeof(ExcelConfigAttribute), true).FirstOrDefault();
                    if (att != null)
                    {
                        ExcelConfigAttribute excelAtt = att as ExcelConfigAttribute;
                        ExcelWorksheet workSheet = package.Workbook.Worksheets[excelAtt.SheetName];
                        if (workSheet == null)
                        {
                            workSheet = package.Workbook.Worksheets.Add(excelAtt.SheetName);
                            workSheet.Cells[1, 1].Value = excelAtt.SheetName;
                            workSheet.Cells[2, 1].Value = excelAtt.Description;

                            workSheet.Cells["A1:C1"].Merge = true; ;
                            workSheet.Cells["A1:C1"].Style.Font.Size = 16;
                            workSheet.Row(1).CustomHeight = true;
                            workSheet.Row(1).Height = 30;

                            workSheet.Cells["A1:C1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            workSheet.Cells["A1:C1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;

                            workSheet.Cells[4, 1].Value = "Key";
                            workSheet.Cells[4, 2].Value = "Value";
                            workSheet.Cells[4, 3].Value = "Description";
                        }

                        var configProp = item.GetValue(setting);

                        foreach (var cfg in configProp.GetType().GetFields())
                        {
                            var att2 = cfg.GetCustomAttributes(typeof(ExcelConfigAttribute), true).FirstOrDefault();
                            if (att2 != null)
                            {
                                var exAtt = att2 as ExcelConfigAttribute;
                                string configKey = string.IsNullOrEmpty(exAtt.Key) ? cfg.Name : exAtt.Key;
                                var propValue = cfg.GetValue(configProp);
                                workSheet.Cells[exAtt.Position + 4, 1].Value = configKey;
                                workSheet.Cells[exAtt.Position + 4, 2].Value = propValue;
                                workSheet.Cells[exAtt.Position + 4, 2].Style.Locked = false;
                                workSheet.Cells[exAtt.Position + 4, 2].Style.Font.Bold = true;
                                workSheet.Cells[exAtt.Position + 4, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                workSheet.Cells[exAtt.Position + 4, 3].Value = exAtt.Description;
                                workSheet.Cells[exAtt.Position + 4, 3].Style.Locked = false;
                                workSheet.Cells[exAtt.Position + 4, 1].AutoFitColumns();
                                workSheet.Cells[exAtt.Position + 4, 2].AutoFitColumns();
                                workSheet.Cells[exAtt.Position + 4, 3].AutoFitColumns();

                                if (propValue is Boolean)
                                {
                                    var validation = workSheet.DataValidations.AddListValidation($"B{exAtt.Position + 4}");
                                    validation.ShowErrorMessage = true;
                                    validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                                    validation.Error = "Please select from list";
                                    validation.ErrorTitle = $"{configKey} Validation";
                                    validation.Formula.Values.Add("TRUE");
                                    validation.Formula.Values.Add("FALSE");
                                    validation.PromptTitle = "Boolean only";
                                    validation.Prompt = "Only TRUE or FALSE are accepted";
                                    validation.ShowInputMessage = true;
                                    //data validation
                                }

                                if (propValue is int || propValue is double)
                                {
                                    var validation = workSheet.DataValidations.AddIntegerValidation($"B{exAtt.Position + 4}");
                                    validation.ShowErrorMessage = true;
                                    validation.Error = "Please enter a valid number";
                                    validation.ErrorTitle = $"{configKey} Validation";
                                    validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                                    validation.PromptTitle = "Enter a integer value here";
                                    validation.Prompt = "Please enter a negative number here";
                                    validation.ShowInputMessage = true;
                                    validation.ShowErrorMessage = true;
                                    validation.Operator = ExcelDataValidationOperator.between;
                                    validation.Formula.Value = 0;
                                    validation.Formula2.Value = int.MaxValue;
                                    var range = cfg.GetCustomAttributes(typeof(RangeAttribute), true).Cast<RangeAttribute>().FirstOrDefault();
                                    if(range != null)
                                    {
                                        validation.Formula.Value = (int)range.Minimum;
                                        validation.Formula2.Value = (int)range.Maximum;
                                        validation.Prompt = $"Please enter a valid number from {validation.Formula.Value} to {validation.Formula2.Value}";
                                        validation.Error = $"Please enter a valid number from {validation.Formula.Value} to {validation.Formula2.Value}";
                                    }
                                }
                                if(propValue is string)
                                {
                                    var maxLength = cfg.GetCustomAttributes(typeof(MaxLengthAttribute), true).Cast<MaxLengthAttribute>().FirstOrDefault();
                                    var minLength = cfg.GetCustomAttributes(typeof(MinLengthAttribute), true).Cast<MinLengthAttribute>().FirstOrDefault();
                                    if (maxLength != null && minLength != null)
                                    {
                                        var validation = workSheet.DataValidations.AddTextLengthValidation($"B{exAtt.Position + 4}");
                                        validation.ErrorTitle = $"{configKey} Validation";
                                        validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                                        validation.PromptTitle = "String Validation";
                                        validation.ShowInputMessage = true;
                                        validation.ShowErrorMessage = true;

                                        validation.Error = $"Please enter a string from {minLength.Length} to {maxLength.Length} characters";
                                        validation.Prompt = $"Please enter a string from {minLength.Length} to {maxLength.Length} characters";

                                        validation.Operator = ExcelDataValidationOperator.between;
                                        validation.Formula.Value = minLength.Length;
                                        validation.Formula2.Value = maxLength.Length;
                                    }
                                    else
                                    {
                                        if ( minLength != null)
                                        {
                                            var validation = workSheet.DataValidations.AddTextLengthValidation($"B{exAtt.Position + 4}");
                                            validation.ErrorTitle = $"{configKey} Validation";
                                            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                                            validation.PromptTitle = "String Validation";
                                            validation.ShowInputMessage = true;
                                            validation.ShowErrorMessage = true;

                                            validation.Error = $"Please enter a string atleast {minLength.Length} characters";
                                            validation.Prompt = $"Please enter a string atleast {minLength.Length} characters";

                                            validation.Operator = ExcelDataValidationOperator.greaterThan;
                                            validation.Formula.Value = minLength.Length;
                                        }
                                    }

                                }
                                var enumDataType = cfg.GetCustomAttributes(typeof(EnumDataTypeAttribute), true).Cast<EnumDataTypeAttribute>().FirstOrDefault();
                                if(enumDataType != null)
                                {
                                    var validation = workSheet.DataValidations.AddListValidation($"B{exAtt.Position + 4}");
                                    validation.ErrorTitle = $"{configKey} Validation";
                                    validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                                    validation.PromptTitle = $"{configKey} Validation";
                                    validation.ShowInputMessage = true;
                                    validation.ShowErrorMessage = true;

                                    List<string> values = new List<string>();
                                     foreach( var v in Enum.GetValues(enumDataType.EnumType)) {
                                        validation.Formula.Values.Add(v.ToString());
                                        values.Add(v.ToString());
                                    }
                                    string value = String.Join(",", values);
                                    validation.Error = $"Please select data from a list: {value}";
                                    validation.Prompt = $"Please select data from a list: {value}";
                                }
                            }
                        }
                        workSheet.Protection.IsProtected = true;
                    }

                }
                package.SaveAs(new FileInfo(destination));
            }
        }

        private static void MigrateEvolvePokemonFilter(ExcelWorksheet pokemonFilter, GlobalSettings setting)
        {
            for (int i = 3; i < 153; i++)
            {
                var id = Convert.ToInt32(pokemonFilter.Cells[$"A{i}"].Value);
                var pid = (PokemonId)id;
                pokemonFilter.Cells[$"AF{i}"].AddComment(pid.ToString(), "necrobot2");
                if (setting.PokemonsToEvolve.Contains(pid))
                {
                    pokemonFilter.Cells[$"AF{i}"].Value = true;
                }
                else
                {
                    pokemonFilter.Cells[$"AF{i}"].Value = false;
                }
            }
            AddListValidation(pokemonFilter, $"AF:AF", "Evolve Transfer [Boolean] validation", "Boolean value only", "TRUE", "FALSE");
        }

        private static void MigrateSnipePokemonFilter(ExcelWorksheet pokemonFilter, GlobalSettings setting)
        {
            for (int i = 3; i < 153; i++)
            {
                var id = Convert.ToInt32(pokemonFilter.Cells[$"A{i}"].Value);
                var pid = (PokemonId)id;
                pokemonFilter.Cells[$"AH{i}"].AddComment(pid.ToString(), "necrobot2");
                if (setting.PokemonToSnipe.Pokemon.Contains(pid))
                {
                    pokemonFilter.Cells[$"AH{i}"].Value = true;
                }
                else
                {
                    pokemonFilter.Cells[$"AH{i}"].Value = false;
                }
            }
            AddListValidation(pokemonFilter, $"AH:AH", "Allow sniper [Boolean] validation", "Boolean value only", "TRUE", "FALSE");
        }

        private static void MigrateTransferPokemonFilter(ExcelWorksheet pokemonFilter, GlobalSettings setting)
        {
            for (int i = 3; i < 153; i++)
            {
                var id = Convert.ToInt32(pokemonFilter.Cells[$"A{i}"].Value);
                var pid = (PokemonId)id;
                if(setting.PokemonsNotToTransfer.Contains(pid))
                {
                    pokemonFilter.Cells[$"P{i}"].Value = "FALSE";
                }
                else
                {
                    pokemonFilter.Cells[$"P{i}"].Value = "TRUE";
                    if(setting.PokemonsTransferFilter.ContainsKey(pid))
                    {
                        var p = setting.PokemonsTransferFilter[pid];

                        pokemonFilter.Cells[$"Q{i}"].Value = p.KeepMinIvPercentage;
                        pokemonFilter.Cells[$"R{i}"].Value = p.KeepMinLvl;
                        pokemonFilter.Cells[$"S{i}"].Value = p.KeepMinCp;
                        pokemonFilter.Cells[$"U{i}"].Value = p.KeepMinDuplicatePokemon;
                        pokemonFilter.Cells[$"V{i}"].Value = p.KeepMinOperator;
                    }
                }
            }
            AddListValidation(pokemonFilter, $"P3:P153", "Allow Transfer [Boolean] validation", "Boolean value only", "TRUE", "FALSE");
            AddListValidation(pokemonFilter, $"V3:V153", "Transfer Operator- Validation", "OR or AND only", "OR", "AND");
            AddNumberValidation(pokemonFilter, $"Q3:Q153", "MinIV - Transfer Validation", "IV : 0 -> 100",0,100);
            AddNumberValidation(pokemonFilter, $"R3:R153", "MinCP - Transfer Validation", "CP : 0 -> 5000", 0, 5000);
            AddNumberValidation(pokemonFilter, $"S3:S153", "MinLV - Transfer Validation", "LV : 0 -> 50", 0, 50);
            AddNumberValidation(pokemonFilter, $"U3:U153", "Keep duplication - Transfer Validation", "LV : 0 -> 1000", 0, 1000);
        }

        private static void MigrateUpgradePokemonFilter(ExcelWorksheet pokemonFilter, GlobalSettings setting)
        {
            for (int i = 3; i < 153; i++)
            {
                var id = Convert.ToInt32(pokemonFilter.Cells[$"A{i}"].Value);
                var pid = (PokemonId)id;
                if (!setting.PokemonUpgradeFilters.ContainsKey(pid))
                {
                    pokemonFilter.Cells[$"W{i}"].Value = "FALSE";
                }
                else
                {
                    pokemonFilter.Cells[$"W{i}"].Value = "TRUE";
                   
                    var p = setting.PokemonUpgradeFilters[pid];

                    pokemonFilter.Cells[$"X{i}"].Value = p.UpgradePokemonIvMinimum;
                    pokemonFilter.Cells[$"Y{i}"].Value = p.UpgradePokemonCpMinimum;
                    pokemonFilter.Cells[$"AB{i}"].Value = p.OnlyUpgradeFavorites;
                    pokemonFilter.Cells[$"AD{i}"].Value = p.UpgradePokemonMinimumStatsOperator;
                    pokemonFilter.Cells[$"AE{i}"].Value = p.LevelUpByCPorIv;
                    
                }
            }
            AddListValidation(pokemonFilter, $"W3:W153", "Allow Upgrade [Boolean] validation", "Boolean value only", "TRUE", "FALSE");
            AddListValidation(pokemonFilter, $"AD3:AD3", "Upgrade Operator- Validation", "OR or AND only", "OR", "AND");
            AddNumberValidation(pokemonFilter, $"X3:X153", "MinIV - Upgrade Validation", "IV : 0 -> 100", 0, 100);
            AddNumberValidation(pokemonFilter, $"Y3:Y153", "MinCP - Upgrade Validation", "CP : 0 -> 5000", 0, 5000);
            AddNumberValidation(pokemonFilter, $"Z3:Z153", "MinLV - Upgrade Validation", "LV : 0 -> 50", 0, 50);
            AddNumberValidation(pokemonFilter, $"AA3:AA153", "Min Candy - Upgrade Validation", "LV : 0 -> 10000", 0, 10000);

            AddListValidation(pokemonFilter, $"AB3:AB153", "Upgrade favorite only - Validation", "Boolean value only", "TRUE", "FALSE");
            AddListValidation(pokemonFilter, $"AE3:AE153", "Upgrade priority - Validation", "IV or CP ", "IV", "CP");
        }


        public static void AddListValidation(ExcelWorksheet pokemonFilter, string address, string errorTitle, string promptTitle, params string[] values)
        {
            var validation = pokemonFilter.DataValidations.AddListValidation(address);
            validation.ShowErrorMessage = true;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            validation.Error = "Please select from list";
            validation.ErrorTitle = errorTitle;
            foreach (var item in values)
            {
                validation.Formula.Values.Add(item);
            }

            validation.PromptTitle = promptTitle;
            validation.Prompt = $"ONLY {string.Join(",", values) } are accepted";
            validation.ShowInputMessage = true;
        }

        public static void AddNumberValidation(ExcelWorksheet workSheet, string address, string errorTitle, string promptTitle, int? minValue, int? maxValue)
        {
            var validation = workSheet.DataValidations.AddIntegerValidation(address);
            validation.ShowErrorMessage = true;
            validation.Error = "Please enter a valid number";
            validation.ErrorTitle = errorTitle;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            validation.PromptTitle = promptTitle;
            validation.Prompt = "Please enter a negative number here";
            validation.ShowInputMessage = true;
            validation.ShowErrorMessage = true;
            validation.Operator = ExcelDataValidationOperator.between;
            validation.Formula.Value = 0;
            validation.Formula2.Value = int.MaxValue;
            if(minValue.HasValue)
            {
                validation.Formula.Value = (int)minValue.Value;
            }
            if (maxValue.HasValue)
            {
                validation.Formula2.Value = (int)maxValue.Value;

            }

            if (maxValue.HasValue  || minValue.HasValue)

            {
                validation.Prompt = $"Please enter a valid number from {validation.Formula.Value} to {validation.Formula2.Value}";
                validation.Error = $"Please enter a valid number from {validation.Formula.Value} to {validation.Formula2.Value}";
            }

        }

        private static void MigrateCatchPokemonFilter(ExcelWorksheet pokemonFilter, GlobalSettings setting)
        {

            for (int i = 3; i < 153; i++)
            {
                var id = Convert.ToInt32(pokemonFilter.Cells[$"A{i}"].Value);
                var pid = (PokemonId)id;
                if(setting.PokemonsToIgnore.Contains(pid))
                {
                    pokemonFilter.Cells[$"J{i}"].Value = "FALSE";
                }
                else
                {
                    pokemonFilter.Cells[$"J{i}"].Value = "TRUE";
                    pokemonFilter.Cells[$"K{i}"].Value = 10;
                    pokemonFilter.Cells[$"L{i}"].Value = 10;
                    pokemonFilter.Cells[$"M{i}"].Value = 1;
                    pokemonFilter.Cells[$"O{i}"].Value = "AND";
                }
                if(setting.PokemonsTransferFilter.ContainsKey(pid))
                {
                    var tfilter = setting.PokemonsTransferFilter[pid];
                    if(tfilter.CatchOnlyPokemonMeetTransferCriteria)
                    {
                        pokemonFilter.Cells[$"J{i}"].Value = "TRUE";
                        pokemonFilter.Cells[$"K{i}"].Value = tfilter.KeepMinIvPercentage;
                        pokemonFilter.Cells[$"L{i}"].Value = tfilter.KeepMinCp;
                        pokemonFilter.Cells[$"M{i}"].Value = tfilter.KeepMinLvl;
                        pokemonFilter.Cells[$"O{i}"].Value = tfilter.KeepMinOperator.ToUpper();
                    }
                }
            }

            AddListValidation(pokemonFilter, $"J3:J153","Allow Catch - [Boolean] validation","Boolean value only" , "TRUE", "FALSE");

            AddListValidation(pokemonFilter, $"O3:O153", "Catch Operator- Validation", "OR or AND only", "OR", "AND");

            AddNumberValidation(pokemonFilter, $"K3:K153", "MinIV - Catch Validation", "IV : 0 -> 100", 0, 100);
            AddNumberValidation(pokemonFilter, $"L3:L153", "MinCP - Catch Validation", "CP : 0 -> 5000", 0, 5000);
            AddNumberValidation(pokemonFilter, $"M3:M153", "MinLV - Catch Validation", "LV : 0 -> 50", 0, 50);

        }
    }
}

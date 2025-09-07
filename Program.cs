using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace GreedSimulation
{
    public class simulationSettings
    {
        public double preyGrowthRate;
        public double predatorGrowthRate;
        public double preyCarryingCapacity;
        public double predatorCarryingCapacity;
        public double competitionCoefficient12;
        public double competitionCoefficient21;
        public double initialPreyPopulation;
        public double initialPredatorPopulation;
        public double timeStep;
        public double simulationTime;
        public bool displayGraph;
    }

    public class timeFrame
    {
        public double time;
        public double preyPopulation;
        public double predatorPopulation;
    }

    public class valueBounds
    {
        public double lower;
        public double upper;
    }

    internal class Program
    {
        static List<timeFrame> Simulate(double preyGrowthRate, double predatorGrowthRate, double preyCarryingCapacity, double predatorCarryingCapacity, double competitionCoefficient12, double competitionCoesfficient21, double initialPreyPopulation, double initialPredatorPopulation, double timeStep, double simulationTime)
        {
            List<timeFrame> results = new List<timeFrame>();
            double preyPopulation = initialPreyPopulation;
            double predatorPopulation = initialPredatorPopulation;

            results.Add(new timeFrame { time = 0, preyPopulation = preyPopulation, predatorPopulation = predatorPopulation });

            for (double time = timeStep; time <= simulationTime; time += timeStep)
            {
                time = Math.Round(time, 3);
                double dx = preyGrowthRate * preyPopulation * (1 - (preyPopulation + competitionCoefficient12 * predatorPopulation) / preyCarryingCapacity);
                double dy = predatorGrowthRate * predatorPopulation * (1 - (predatorPopulation + competitionCoesfficient21 * preyPopulation) / predatorCarryingCapacity);

                preyPopulation += dx * timeStep;
                predatorPopulation += dy * timeStep;

                preyPopulation = Math.Max(preyPopulation, 0);
                predatorPopulation = Math.Max(predatorPopulation, 0);

                preyPopulation = Math.Round(preyPopulation, 3);
                predatorPopulation = Math.Round(predatorPopulation, 3);

                Console.WriteLine($"Time: {Math.Round(time, 3)}, Prey: {preyPopulation}, Predators: {predatorPopulation}");

                results.Add(new timeFrame { time = Math.Round(time, 3), preyPopulation = Math.Round(preyPopulation, 3), predatorPopulation = Math.Round(predatorPopulation, 3) });

                if (preyPopulation <= 1 || predatorPopulation <= 1)
                {
                    break;
                }
            }

            return results;
        }

        static void DisplayGraph(List<timeFrame> results)
        {
            Form graphForm = new Form
            {
                Text = "Greed Simulation Results",
                Width = 800,
                Height = 600
            };

            Chart chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            double minTime = Math.Floor(results.Min(result => result.time));
            double maxTime = results.Max(result => result.time);

            double range = maxTime - minTime;
            double interval = Math.Max(1, Math.Ceiling(range / 5));
            if (interval % 5 != 0 && interval != 1) interval = Math.Ceiling(interval / 5) * 5;

            ChartArea chartArea = new ChartArea
            {
                Name = "MainArea",
                AxisX = { 
                    Title = "Time",
                    Minimum = minTime,
                    Maximum = maxTime,
                    Interval = interval,
                    IntervalAutoMode = IntervalAutoMode.FixedCount,
                    LabelStyle = { Format = "0" }
                },
                AxisY = { 
                    Title = "Population"
                }
            };
            chart.ChartAreas.Add(chartArea);

            Legend legend = new Legend
            {
                Name = "MainLegend",
                BackColor = Color.White,
                Docking = Docking.Top,
                Alignment = StringAlignment.Center,
                LegendStyle = LegendStyle.Row
            };
            chart.Legends.Add(legend);

            Series preySeries = new Series
            {
                Name = "Prey",
                Color = System.Drawing.Color.Blue,
                ChartType = SeriesChartType.Line,
                Legend = "MainLegend"
            };

            Series predatorSeries = new Series
            {
                Name = "Predator",
                Color = System.Drawing.Color.Orange,
                ChartType = SeriesChartType.Line,
                Legend = "MainLegend"
            };

            foreach (var result in results)
            {
                preySeries.Points.AddXY(result.time, result.preyPopulation);
                predatorSeries.Points.AddXY(result.time, result.predatorPopulation);
            }

            chart.Series.Add(preySeries);
            chart.Series.Add(predatorSeries);

            graphForm.Controls.Add(chart);

            Button saveButton = new Button
            {
                Text = "Save Graph",
                Dock = DockStyle.Bottom
            };

            saveButton.Click += (sender, e) =>
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png",
                    Title = "Save Graph as a .png"
                };
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        chart.SaveImage(saveDialog.FileName, ChartImageFormat.Png);
                        MessageBox.Show("Graph saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving graph: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            graphForm.Controls.Add(saveButton);

            System.Windows.Forms.Application.Run(graphForm);
        }

        static void WriteColouredText(string text)
        {
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '%' && i + 2 < text.Length)
                {
                    string marker = text.Substring(i, 3);
                    switch (marker)
                    {
                        case "%bk": Console.ForegroundColor = ConsoleColor.Black; i += 3; break;
                        case "%db": Console.ForegroundColor = ConsoleColor.DarkBlue; i += 3; break;
                        case "%dg": Console.ForegroundColor = ConsoleColor.DarkGreen; i += 3; break;
                        case "%dc": Console.ForegroundColor = ConsoleColor.DarkCyan; i += 3; break;
                        case "%dr": Console.ForegroundColor = ConsoleColor.DarkRed; i += 3; break;
                        case "%dm": Console.ForegroundColor = ConsoleColor.DarkMagenta; i += 3; break;
                        case "%dy": Console.ForegroundColor = ConsoleColor.DarkYellow; i += 3; break;
                        case "%gy": Console.ForegroundColor = ConsoleColor.Gray; i += 3; break;
                        case "%dx": Console.ForegroundColor = ConsoleColor.DarkGray; i += 3; break;
                        case "%bl": Console.ForegroundColor = ConsoleColor.Blue; i += 3; break;
                        case "%gn": Console.ForegroundColor = ConsoleColor.Green; i += 3; break;
                        case "%cy": Console.ForegroundColor = ConsoleColor.Cyan; i += 3; break;
                        case "%rd": Console.ForegroundColor = ConsoleColor.Red; i += 3; break;
                        case "%mg": Console.ForegroundColor = ConsoleColor.Magenta; i += 3; break;
                        case "%yl": Console.ForegroundColor = ConsoleColor.Yellow; i += 3; break;
                        case "%wh": Console.ForegroundColor = ConsoleColor.White; i += 3; break;
                        default:
                            Console.ResetColor();
                            i++;
                            break;
                    }

                    while (i < text.Length && text[i] != ' ')
                    {
                        Console.Write(text[i]);
                        i++;
                    }
                    Console.ResetColor();
                } else
                {
                    Console.Write(text[i]);
                    i++;
                }
            }
        }

        static int GetPlainLength(string text)
        {
            int length = 0;
            int i = 0;

            while (i < text.Length)
            {
                if (text[i] == '%' && i + 2 < text.Length)
                {
                    string marker = text.Substring(i, 3);

                    if (marker == "%bk" || marker == "%db" || marker == "%dg" || marker == "%dc" ||
                        marker == "%dr" || marker == "%dm" || marker == "%dy" || marker == "%gy" ||
                        marker == "%dx" || marker == "%bl" || marker == "%gn" || marker == "%cy" ||
                        marker == "%rd" || marker == "%mg" || marker == "%yl" || marker == "%wh")
                    {
                        i += 3;

                        while (i < text.Length && text[i] != ' ')
                        {
                            length++;
                            i++;
                        }
                    } else
                    {
                        length++;
                        i++;
                    }
                } else
                {
                    length++;
                    i++;
                }
            }

            return length;
        }

        static void CentreText(string text)
        {
            int consoleWidth = Console.WindowWidth;
            int consoleHeight = Console.WindowHeight;
            if (text.Length >= consoleWidth)
            {
                Console.Write(text);
                return;
            }

            int leftPadding = (int)Math.Floor((double)(consoleWidth - text.Length) / 2);
            int topPadding = (int)Math.Floor((double)consoleHeight / 2);

            Console.SetCursorPosition(leftPadding, topPadding);
            Console.Write(text);
        }

        static void PrintMenuLine(string originalText, int leftPadding, int rightPadding, int leftMargin, int row, bool isHighlighted)
        {
            string fullText = (isHighlighted ? "> " : "  ") + new string(' ', leftPadding) + originalText + new string(' ', rightPadding) + (isHighlighted ? " <" : "  ");
            Console.SetCursorPosition(leftMargin, row);
            WriteColouredText(fullText);
        }

        static int Menu(string[] options)
        {
            if (options.Length == 0) return -1;

            int currentOption = 0;
            int longestOptionLength = options.Max(option => GetPlainLength(option));

            int consoleWidth = Console.WindowWidth;
            int consoleHeight = Console.WindowHeight;
            int startRow = (consoleHeight / 2) - (options.Length / 2);

            int[] leftPaddings = new int[options.Length];
            int[] rightPaddings = new int[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                int plainLength = GetPlainLength(options[i]);
                leftPaddings[i] = (int)Math.Floor((double)(longestOptionLength - plainLength) / 2);
                rightPaddings[i] = (int)Math.Ceiling((double)(longestOptionLength - plainLength) / 2);
            }

            for (int i = 0; i < options.Length; i++)
            {
                int leftMargin = (consoleWidth - (GetPlainLength(options[i]) + leftPaddings[i] + rightPaddings[i] + 4)) / 2;
                PrintMenuLine(options[i], leftPaddings[i], rightPaddings[i], leftMargin, startRow + i, false);
            }

            {
                int leftMargin = (consoleWidth - (GetPlainLength(options[currentOption]) + leftPaddings[currentOption] + rightPaddings[currentOption] + 4)) / 2;
                PrintMenuLine(options[currentOption], leftPaddings[currentOption], rightPaddings[currentOption], leftMargin, startRow + currentOption, true);
            }

            string instructions = "Use arrow keys to navigate, Enter to select.";
            int instructionsLeftMargin = (int)Math.Floor((double)(consoleWidth - instructions.Length) / 2);
            int instructionsRow = startRow + options.Length + 4;
            Console.SetCursorPosition(instructionsLeftMargin, instructionsRow);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(instructions);
            Console.ResetColor();

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                int previousOption = currentOption;

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    currentOption--;
                    if (currentOption < 0) currentOption = options.Length - 1;
                } else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    currentOption++;
                    if (currentOption >= options.Length) currentOption = 0;
                } else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return currentOption;
                }

                if (currentOption != previousOption)
                {
                    {
                        int leftMargin = (consoleWidth - (GetPlainLength(options[previousOption]) + leftPaddings[previousOption] + rightPaddings[previousOption] + 4)) / 2;
                        PrintMenuLine(options[previousOption], leftPaddings[previousOption], rightPaddings[previousOption], leftMargin, startRow + previousOption, false);
                    }

                    {
                        int leftMargin = (consoleWidth - (GetPlainLength(options[currentOption]) + leftPaddings[currentOption] + rightPaddings[currentOption] + 4)) / 2;
                        PrintMenuLine(options[currentOption], leftPaddings[currentOption], rightPaddings[currentOption], leftMargin, startRow + currentOption, true);
                    }
                }
            }
        }

        static simulationSettings GetDefaultSettings()
        {
            simulationSettings settings = new simulationSettings();
            settings.preyGrowthRate = 0.8;
            settings.predatorGrowthRate = 0.6;
            settings.preyCarryingCapacity = 100;
            settings.predatorCarryingCapacity = 80;
            settings.competitionCoefficient12 = 0.5;
            settings.competitionCoefficient21 = 0.4;
            settings.initialPreyPopulation = 50;
            settings.initialPredatorPopulation = 30;
            settings.timeStep = 0.1;
            settings.simulationTime = 50;
            settings.displayGraph = true;
            return settings;
        }

        static simulationSettings LoadSettings(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return GetDefaultSettings();
            }

            simulationSettings settings = new simulationSettings();

            using (FileStream fileStream = File.OpenRead(filePath))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    settings.preyGrowthRate = reader.ReadDouble();
                    settings.predatorGrowthRate = reader.ReadDouble();
                    settings.preyCarryingCapacity = reader.ReadDouble();
                    settings.predatorCarryingCapacity = reader.ReadDouble();
                    settings.competitionCoefficient12 = reader.ReadDouble();
                    settings.competitionCoefficient21 = reader.ReadDouble();
                    settings.initialPreyPopulation = reader.ReadDouble();
                    settings.initialPredatorPopulation = reader.ReadDouble();
                    settings.timeStep = reader.ReadDouble();
                    settings.simulationTime = reader.ReadDouble();
                    settings.displayGraph = reader.ReadBoolean();
                }
            }

            return settings;
        }

        static bool SaveSettings(simulationSettings settings, string filePath)
        {
            try
            {
                using (FileStream fileStream = File.Create(filePath))
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                    {
                        writer.Write(settings.preyGrowthRate);
                        writer.Write(settings.predatorGrowthRate);
                        writer.Write(settings.preyCarryingCapacity);
                        writer.Write(settings.predatorCarryingCapacity);
                        writer.Write(settings.competitionCoefficient12);
                        writer.Write(settings.competitionCoefficient21);
                        writer.Write(settings.initialPreyPopulation);
                        writer.Write(settings.initialPredatorPopulation);
                        writer.Write(settings.timeStep);
                        writer.Write(settings.simulationTime);
                        writer.Write(settings.displayGraph);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        static void LoadSimulationPresets(ref double preyGrowthRate, ref double predatorGrowthRate, ref double preyCarryingCapacity, ref double predatorCarryingCapacity, ref double competitionCoefficient12, ref double competitionCoefficient21, ref double initialPreyPopulation, ref double initialPredatorPopulation, ref double timeStep, ref double simulationTime, ref bool displayGraph)
        {
            while (true)
            {
                Console.Clear();
                string[] options = { "Default", "Oscillatory Coexistence", "Competitive Exclusion", "Stable Coexistence", "%rdBack" };
                int selectedOption = Menu(options);

                switch (selectedOption)
                {
                    case 0:
                        {
                            simulationSettings settings = LoadSettings("Default.bin");

                            preyGrowthRate = settings.preyGrowthRate;
                            predatorGrowthRate = settings.predatorGrowthRate;
                            preyCarryingCapacity = settings.preyCarryingCapacity;
                            predatorCarryingCapacity = settings.predatorCarryingCapacity;
                            competitionCoefficient12 = settings.competitionCoefficient12;
                            competitionCoefficient21 = settings.competitionCoefficient21;
                            initialPreyPopulation = settings.initialPreyPopulation;
                            initialPredatorPopulation = settings.initialPredatorPopulation;
                            timeStep = settings.timeStep;
                            simulationTime = settings.simulationTime;
                            displayGraph = settings.displayGraph;

                            Console.Clear();
                            CentreText("Loaded the Default settings.");
                            System.Threading.Thread.Sleep(1500);
                            return;
                        }
                    case 1:
                        {
                            simulationSettings settings = LoadSettings("OscillatoryCoexistence.bin");

                            preyGrowthRate = settings.preyGrowthRate;
                            predatorGrowthRate = settings.predatorGrowthRate;
                            preyCarryingCapacity = settings.preyCarryingCapacity;
                            predatorCarryingCapacity = settings.predatorCarryingCapacity;
                            competitionCoefficient12 = settings.competitionCoefficient12;
                            competitionCoefficient21 = settings.competitionCoefficient21;
                            initialPreyPopulation = settings.initialPreyPopulation;
                            initialPredatorPopulation = settings.initialPredatorPopulation;
                            timeStep = settings.timeStep;
                            simulationTime = settings.simulationTime;
                            displayGraph = settings.displayGraph;

                            Console.Clear();
                            CentreText("Loaded the Oscillatory Coexistence preset.");
                            System.Threading.Thread.Sleep(1500);
                            return;
                        }
                    case 2:
                        {
                            simulationSettings settings = LoadSettings("CompetitiveExclusion.bin");

                            preyGrowthRate = settings.preyGrowthRate;
                            predatorGrowthRate = settings.predatorGrowthRate;
                            preyCarryingCapacity = settings.preyCarryingCapacity;
                            predatorCarryingCapacity = settings.predatorCarryingCapacity;
                            competitionCoefficient12 = settings.competitionCoefficient12;
                            competitionCoefficient21 = settings.competitionCoefficient21;
                            initialPreyPopulation = settings.initialPreyPopulation;
                            initialPredatorPopulation = settings.initialPredatorPopulation;
                            timeStep = settings.timeStep;
                            simulationTime = settings.simulationTime;
                            displayGraph = settings.displayGraph;

                            Console.Clear();
                            CentreText("Loaded the Competitive Exclusion preset.");
                            System.Threading.Thread.Sleep(1500);
                            return;
                        }
                    case 3:
                        {
                            simulationSettings settings = LoadSettings("StableCoexistence.bin");

                            preyGrowthRate = settings.preyGrowthRate;
                            predatorGrowthRate = settings.predatorGrowthRate;
                            preyCarryingCapacity = settings.preyCarryingCapacity;
                            predatorCarryingCapacity = settings.predatorCarryingCapacity;
                            competitionCoefficient12 = settings.competitionCoefficient12;
                            competitionCoefficient21 = settings.competitionCoefficient21;
                            initialPreyPopulation = settings.initialPreyPopulation;
                            initialPredatorPopulation = settings.initialPredatorPopulation;
                            timeStep = settings.timeStep;
                            simulationTime = settings.simulationTime;
                            displayGraph = settings.displayGraph;

                            Console.Clear();
                            CentreText("Loaded the Stable Coexistence preset.");
                            System.Threading.Thread.Sleep(1500);
                            return;
                        }
                    case 4:
                        {
                            Console.Clear();
                            return;
                        }
                    default:
                        Console.Clear();
                        CentreText("Unknown option selected.");
                        break;
                }
            }
        }

        static void SimulationSettings(ref double preyGrowthRate, ref double predatorGrowthRate, ref double preyCarryingCapacity, ref double predatorCarryingCapacity, ref double competitionCoefficient12, ref double competitionCoefficient21, ref double initialPreyPopulation, ref double initialPredatorPopulation, ref double timeStep, ref double simulationTime, ref bool displayGraph)
        {
            Dictionary<string, valueBounds> bounds = new Dictionary<string, valueBounds>
            {
                { "preyGrowthRate", new valueBounds { lower = 0.1, upper = 2 } },
                { "predatorGrowthRate", new valueBounds { lower = 0.1, upper = 2 } },
                { "preyCarryingCapacity", new valueBounds { lower = 50, upper = 1000 } },
                { "predatorCarryingCapacity", new valueBounds { lower = 50, upper = 1000 } },
                { "competitionCoefficient12", new valueBounds { lower = 0.1, upper = 1.5 } },
                { "competitionCoefficient21", new valueBounds { lower = 0.1, upper = 1.5 } },
                { "initialPreyPopulation", new valueBounds { lower = 10, upper = 500 } },
                { "initialPredatorPopulation", new valueBounds { lower = 10, upper = 500 } },
                { "timeStep", new valueBounds { lower = 0.01, upper = 1 } },
                { "simulationTime", new valueBounds { lower = 10, upper = 100 } },
            };

            while (true)
            {
                Console.Clear();
                string[] options = { $"Prey Growth Rate: {preyGrowthRate}", $"Predator Growth Rate: {predatorGrowthRate}", $"Prey Carrying Capacity: {preyCarryingCapacity}", $"Predator Carrying Capacity: {predatorCarryingCapacity}", $"Competition Coeffiecient 12: {competitionCoefficient12}", $"Competition Coeffiecient 21: {competitionCoefficient21}", $"Initial Prey Population: {initialPreyPopulation}", $"Initial Predator Population: {initialPredatorPopulation}", $"Time Step: {timeStep}", $"Simulation Time: {simulationTime}", $"Display Graph: {displayGraph}", "%gnSave %gnSettings", "%rdCancel", "%ylLoad %ylPreset %ylSettings" };
                int selectedOption = Menu(options);

                switch (selectedOption)
                {
                    case 0:
                        Console.Clear();
                        CentreText("Enter new Prey Growth Rate: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                preyGrowthRate = newValue;

                                if (newValue < bounds["preyGrowthRate"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["preyGrowthRate"].lower} for the Prey Growth Rate.");
                                    System.Threading.Thread.Sleep(1500);
                                } else if (newValue > bounds["preyGrowthRate"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["preyGrowthRate"].upper} for the Prey Growth Rate.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Prey Growth Rate: ");
                            }
                        }
                        break;
                    case 1:
                        Console.Clear();
                        CentreText("Enter new Predator Growth Rate: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                predatorGrowthRate = newValue;

                                if (newValue < bounds["predatorGrowthRate"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["predatorGrowthRate"].lower} for the Predator Growth Rate.");
                                    System.Threading.Thread.Sleep(1500);
                                }
                                else if (newValue > bounds["predatorGrowthRate"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["predatorGrowthRate"].upper} for the Predator Growth Rate.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Predator Growth Rate: ");
                            }
                        }
                        break;
                    case 2:
                        Console.Clear();
                        CentreText("Enter new Prey Carrying Capacity: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                preyCarryingCapacity = newValue;

                                if (newValue < bounds["preyCarryingCapacity"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["preyCarryingCapacity"].lower} for the Prey Carrying Capacity.");
                                    System.Threading.Thread.Sleep(1500);
                                }
                                else if (newValue > bounds["preyCarryingCapacity"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["preyCarryingCapacity"].upper} for the Prey Carrying Capacity.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Prey Carrying Capacity: ");
                            }
                        }
                        break;
                    case 3:
                        Console.Clear();
                        CentreText("Enter new Predator Carrying Capacity: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                predatorCarryingCapacity = newValue;

                                if (newValue < bounds["predatorCarryingCapacity"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["predatorCarryingCapacity"].lower} for the Predator Carrying Capacity.");
                                    System.Threading.Thread.Sleep(1500);
                                }
                                else if (newValue > bounds["predatorCarryingCapacity"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["predatorCarryingCapacity"].upper} for the Predator Carrying Capacity.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Predator Carrying Capacity: ");
                            }
                        }
                        break;
                    case 4:
                        Console.Clear();
                        CentreText("Enter new Competition Coefficient 12: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                competitionCoefficient12 = newValue;

                                if (newValue < bounds["competitionCoefficient12"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["competitionCoefficient12"].lower} for Competition Coefficient 12.");
                                    System.Threading.Thread.Sleep(1500);
                                }
                                else if (newValue > bounds["competitionCoefficient12"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["competitionCoefficient12"].upper} for Competition Coefficient 12.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Competition Coefficient 12: ");
                            }
                        }
                        break;
                    case 5:
                        Console.Clear();
                        CentreText("Enter new Competition Coefficient 21: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                competitionCoefficient21 = newValue;

                                if (newValue < bounds["competitionCoefficient21"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["competitionCoefficient21"].lower} for Competition Coefficient 21.");
                                    System.Threading.Thread.Sleep(1500);
                                }
                                else if (newValue > bounds["competitionCoefficient21"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["competitionCoefficient21"].upper} for Competition Coefficient 21.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Competition Coefficient 21: ");
                            }
                        }
                        break;
                    case 6:
                        Console.Clear();
                        CentreText("Enter new Initial Prey Population: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                initialPreyPopulation = newValue;

                                if (newValue < bounds["initialPreyPopulation"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["initialPreyPopulation"].lower} for the Initial Prey Population.");
                                    System.Threading.Thread.Sleep(1500);
                                }
                                else if (newValue > bounds["initialPreyPopulation"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["initialPreyPopulation"].upper} for the Initial Prey Population.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Initial Prey Population: ");
                            }
                        }
                        break;
                    case 7:
                        Console.Clear();
                        CentreText("Enter new Initial Predator Population: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                initialPredatorPopulation = newValue;

                                if (newValue < bounds["initialPredatorPopulation"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["initialPredatorPopulation"].lower} for the Initial Predator Population.");
                                    System.Threading.Thread.Sleep(1500);
                                }
                                else if (newValue > bounds["initialPredatorPopulation"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["initialPredatorPopulation"].upper} for the Initial Predator Population.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Initial Predator Population: ");
                            }
                        }
                        break;
                    case 8:
                        Console.Clear();
                        CentreText("Enter new Time Step: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                timeStep = newValue;

                                if (newValue < bounds["timeStep"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["timeStep"].lower} for the Time Step.");
                                    System.Threading.Thread.Sleep(1500);
                                }
                                else if (newValue > bounds["timeStep"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["timeStep"].upper} for the Time Step.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Time Step: ");
                            }
                        }
                        break;
                    case 9:
                        Console.Clear();
                        CentreText("Enter new Simulation Time: ");
                        while (true)
                        {
                            string input = Console.ReadLine();
                            if (double.TryParse(input, out double newValue) && newValue >= 0)
                            {
                                simulationTime = newValue;

                                if (newValue < bounds["simulationTime"].lower)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is lower than our recommended of {bounds["simulationTime"].lower} for the Simulation Time.");
                                    System.Threading.Thread.Sleep(1500);
                                }
                                else if (newValue > bounds["simulationTime"].upper)
                                {
                                    Console.Clear();
                                    CentreText($"{newValue} is higher than our recommended of {bounds["simulationTime"].upper} for the Simulation Time.");
                                    System.Threading.Thread.Sleep(1500);
                                }

                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid Simulation Time: ");
                            }
                        }
                        break;
                    case 10:
                        Console.Clear();
                        CentreText("Should the graph display: (Y/Yes/True | N/No/False) ");
                        while (true)
                        {
                            string input = Console.ReadLine().ToLower();
                            if (input == "y" || input == "yes" || input == "true" || input == "n" || input == "no" || input == "false")
                            {
                                if (input == "y" || input == "yes" || input == "true")
                                {
                                    displayGraph = true;
                                }
                                else
                                {
                                    displayGraph = false;
                                }
                                break;
                            }
                            else
                            {
                                CentreText("Invalid input. Please enter a valid option: (Y/Yes/True | N/No/False) ");
                            }
                        }
                        break;
                    case 11:
                        {
                            Console.Clear();

                            simulationSettings settings = new simulationSettings
                            {
                                preyGrowthRate = preyGrowthRate,
                                predatorGrowthRate = predatorGrowthRate,
                                preyCarryingCapacity = preyCarryingCapacity,
                                predatorCarryingCapacity = predatorCarryingCapacity,
                                competitionCoefficient12 = competitionCoefficient12,
                                competitionCoefficient21 = competitionCoefficient21,
                                initialPreyPopulation = initialPreyPopulation,
                                initialPredatorPopulation = initialPredatorPopulation,
                                timeStep = timeStep,
                                simulationTime = simulationTime,
                                displayGraph = displayGraph
                            };

                            if (SaveSettings(settings, "settings.bin"))
                            {
                                CentreText("Settings saved successfully.");
                            }
                            else
                            {
                                CentreText("Failed to save settings.");
                            }

                            System.Threading.Thread.Sleep(1500);
                            return;
                        }
                    case 12:
                        {
                            Console.Clear();
                            CentreText("Cancelling settings changes.");

                            simulationSettings settings = LoadSettings("settings.bin");

                            preyGrowthRate = settings.preyGrowthRate;
                            predatorGrowthRate = settings.predatorGrowthRate;
                            preyCarryingCapacity = settings.preyCarryingCapacity;
                            predatorCarryingCapacity = settings.predatorCarryingCapacity;
                            competitionCoefficient12 = settings.competitionCoefficient12;
                            competitionCoefficient21 = settings.competitionCoefficient21;
                            initialPreyPopulation = settings.initialPreyPopulation;
                            initialPredatorPopulation = settings.initialPredatorPopulation;
                            timeStep = settings.timeStep;
                            simulationTime = settings.simulationTime;
                            displayGraph = settings.displayGraph;

                            System.Threading.Thread.Sleep(1500);
                            return;
                        }
                    case 13:
                        {
                            LoadSimulationPresets(ref preyGrowthRate, ref predatorGrowthRate, ref preyCarryingCapacity, ref predatorCarryingCapacity, ref competitionCoefficient12, ref competitionCoefficient21, ref initialPreyPopulation, ref initialPredatorPopulation, ref timeStep, ref simulationTime, ref displayGraph);
                            break;
                        }
                    default:
                        Console.Clear();
                        CentreText("Unknown option selected.");
                        return;
                }
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            simulationSettings settings = LoadSettings("settings.bin");

            double preyGrowthRate = settings.preyGrowthRate;
            double predatorGrowthRate = settings.predatorGrowthRate;
            double preyCarryingCapacity = settings.preyCarryingCapacity;
            double predatorCarryingCapacity = settings.predatorCarryingCapacity;
            double competitionCoefficient12 = settings.competitionCoefficient12;
            double competitionCoefficient21 = settings.competitionCoefficient21;
            double initialPreyPopulation = settings.initialPreyPopulation;
            double initialPredatorPopulation = settings.initialPredatorPopulation;
            double timeStep = settings.timeStep;
            double simulationTime = settings.simulationTime;
            bool displayGraph = settings.displayGraph;

            Console.Title = "Greed Simulation";
            Console.CursorVisible = false;
            Console.Clear();
            CentreText("Welcome to the Greed Simulation!");
            System.Threading.Thread.Sleep(2000);

            while (true)
            {
                List<timeFrame> results = new List<timeFrame>();

                Console.Title = "Greed Simulation - Menu";
                Console.Clear();

                string[] options = { "%gnStart %gnSimulation", "%blSimulation %blSettings", "%rdExit %rdProgram" };
                int selectedOption = Menu(options);

                switch (selectedOption)
                {
                    case 0:
                        Console.Title = "Greed Simulation - Simulation";
                        Console.Clear();
                        results = Simulate(preyGrowthRate, predatorGrowthRate, preyCarryingCapacity, predatorCarryingCapacity, competitionCoefficient12, competitionCoefficient21, initialPreyPopulation, initialPredatorPopulation, timeStep, simulationTime);
                        
                        if (displayGraph)
                        {
                            CentreText("Displaying the graph.");
                            DisplayGraph(results);
                            System.Threading.Thread.Sleep(1000);
                        } else
                        {
                            CentreText("Press any key to continue.");
                            Console.ReadKey();
                        }

                        Console.Clear();
                        CentreText("Press any key to return to the menu.");
                        Console.ReadKey();
                        break;
                    case 1:
                        Console.Title = "Greed Simulation - Settings";
                        Console.Clear();
                        SimulationSettings(ref preyGrowthRate, ref predatorGrowthRate, ref preyCarryingCapacity, ref predatorCarryingCapacity, ref competitionCoefficient12, ref competitionCoefficient21, ref initialPreyPopulation, ref initialPredatorPopulation, ref timeStep, ref simulationTime, ref displayGraph);
                        break;
                    case 2:
                        Console.Title = "Greed Simulation - Exiting";
                        Console.Clear();
                        CentreText("Exiting program...");
                        System.Threading.Thread.Sleep(1000);
                        return;
                    default:
                        Console.Title = "Greed Simulation";
                        Console.Clear();
                        CentreText("Unknown option selected.");
                        System.Threading.Thread.Sleep(1000);
                        break;
                }
            }
        }
    }
}
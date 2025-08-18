using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

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

    internal class Program
    {
        static void CentreText(string text)
        {
            int consoleWidth = Console.WindowWidth, consoleHeight = Console.WindowHeight;
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

        static List<timeFrame> Simulate(double preyGrowthRate, double predatorGrowthRate, double preyCarryingCapacity, double predatorCarryingCapacity, double competitionCoefficient12, double competitionCoesfficient21, double initialPreyPopulation, double initialPredatorPopulation, double timeStep, double simulationTime)
        {
            List<timeFrame> results = new List<timeFrame>();
            double preyPopulation = initialPreyPopulation;
            double predatorPopulation = initialPredatorPopulation;

            results.Add(new timeFrame { time = 0, preyPopulation = preyPopulation, predatorPopulation = predatorPopulation });

            for (double time = 0; time <= simulationTime; time += timeStep)
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
                Dock = DockStyle.Fill
            };

            ChartArea chartArea = new ChartArea
            {
                Name = "MainArea",
                AxisX = { Title = "Time" },
                AxisY = { Title = "Population" }
            };
            chart.ChartAreas.Add(chartArea);

            Series preySeries = new Series
            {
                Name = "Prey",
                Color = System.Drawing.Color.Blue,
                ChartType = SeriesChartType.Line
            };

            Series predatorSeries = new Series
            {
                Name = "Predator",
                Color = System.Drawing.Color.Orange,
                ChartType = SeriesChartType.Line
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

        static int Menu(string[] options)
        {
            int currentOption = 0;
            int longestOptionLength = options.Max(option => option.Length);

            while (true)
            {
                Console.Clear();
                
                int consoleWidth = Console.WindowWidth, consoleHeight = Console.WindowHeight;
                int menuWidth = 2 + longestOptionLength + 2;
                int startRow = (consoleHeight / 2) - (options.Length / 2);

                for (int i = 0; i < options.Length; i++)
                {
                    string text = options[i];

                    int leftPaddingInside = (int)Math.Floor((double)(longestOptionLength - text.Length) / 2);
                    int rightPaddingInside = (int)Math.Ceiling((double)(longestOptionLength - text.Length) / 2);

                    string paddedText = new string(' ', leftPaddingInside) + text + new string(' ', rightPaddingInside);
                    string displayText;

                    if (i == currentOption)
                    {
                        displayText = $"> {paddedText} <";
                    }
                    else
                    {
                        displayText = $"  {paddedText}  ";
                    }

                    int leftMargin = (consoleWidth - displayText.Length) / 2;

                    Console.SetCursorPosition(leftMargin, startRow + i);
                    if (i == currentOption)
                    {
                        Console.WriteLine(displayText);
                    } else
                    {
                        Console.WriteLine(displayText);
                    }
                }

                string instructions = "Use ↑ and ↓ to navigate, Enter to select.";
                int instructionsLeftMargin = (int)Math.Floor((double)(consoleWidth - instructions.Length) / 2);
                int instructionsRow = startRow + options.Length + 4;

                Console.SetCursorPosition(instructionsLeftMargin, instructionsRow);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(instructions);
                Console.ResetColor();

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    currentOption--;
                    if (currentOption < 0) currentOption = options.Length - 1;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    currentOption++;
                    if (currentOption >= options.Length) currentOption = 0;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return currentOption;
                }
            }
        }

        static simulationSettings GetDefaultSettings()
        {
            simulationSettings settings = new simulationSettings();
            settings.preyGrowthRate = 0.5;
            settings.predatorGrowthRate = 0.3;
            settings.preyCarryingCapacity = 100;
            settings.predatorCarryingCapacity = 80;
            settings.competitionCoefficient12 = 0.6;
            settings.competitionCoefficient21 = 0.8;
            settings.initialPreyPopulation = 40;
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

        static void SimulationSettings(ref double preyGrowthRate, ref double predatorGrowthRate, ref double preyCarryingCapacity, ref double predatorCarryingCapacity, ref double competitionCoefficient12, ref double competitionCoefficient21, ref double initialPreyPopulation, ref double initialPredatorPopulation, ref double timeStep, ref double simulationTime, ref bool displayGraph)
        {
            while (true)
            {
                string[] options = { $"Prey Growth Rate: {preyGrowthRate}", $"Predator Growth Rate: {predatorGrowthRate}", $"Prey Carrying Capacity: {preyCarryingCapacity}", $"Predator Carrying Capacity: {predatorCarryingCapacity}", $"Competition Coeffiecient 12: {competitionCoefficient12}", $"Competition Coeffiecient 21: {competitionCoefficient21}", $"Initial Prey Population: {initialPreyPopulation}", $"Initial Predator Population: {initialPredatorPopulation}", $"Time Step: {timeStep}", $"Simulation Time: {simulationTime}", $"Display Graph: {displayGraph}", "Save Settings", "Cancel" };
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

            double preyGrowthRate = settings.preyGrowthRate, predatorGrowthRate = settings.predatorGrowthRate;
            double preyCarryingCapacity = settings.preyCarryingCapacity, predatorCarryingCapacity = settings.predatorCarryingCapacity;
            double competitionCoefficient12 = settings.competitionCoefficient12, competitionCoefficient21 = settings.competitionCoefficient21;
            double initialPreyPopulation = settings.initialPreyPopulation, initialPredatorPopulation = settings.initialPredatorPopulation;
            double timeStep = settings.timeStep;
            double simulationTime = settings.simulationTime;
            bool displayGraph = settings.displayGraph;

            Console.Title = "Greed Simulation";
            Console.Clear();
            CentreText("Welcome to the Greed Simulation!");
            System.Threading.Thread.Sleep(2000);

            while (true)
            {
                List<timeFrame> results = new List<timeFrame>();

                string[] options = { "Start Simulation", "Simulation Settings", "Exit Program" };
                int selectedOption = Menu(options);

                switch (selectedOption)
                {
                    case 0:
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
                        Console.Clear();
                        SimulationSettings(ref preyGrowthRate, ref predatorGrowthRate, ref preyCarryingCapacity, ref predatorCarryingCapacity, ref competitionCoefficient12, ref competitionCoefficient21, ref initialPreyPopulation, ref initialPredatorPopulation, ref timeStep, ref simulationTime, ref displayGraph);
                        break;
                    case 2:
                        Console.Clear();
                        CentreText("Exiting program...");
                        System.Threading.Thread.Sleep(1000);
                        return;
                    default:
                        Console.Clear();
                        CentreText("Unknown option selected.");
                        return;
                }
            }
        }
    }
}
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VoltageAnalyzer;
using Wisp.Comtrade;
using Wisp.Comtrade.Models;
using static SkiaSharp.HarfBuzz.SKShaper;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace WinFormsApp6
{


    public partial class Form1 : Form
    {
        private ConfigurationHandler configuration;

        private ThreePhaseVoltageAnalyzer voltageAnalyzer;
        public Form1()
        {
            InitializeComponent();
        }

        private void файлToolStripMenuItem_Click1(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {



                openFileDialog.Filter = "COMTRADE files (*.cfg)|*.cfg|All files (*.*)|*.*";
                openFileDialog.Title = "Open COMTRADE File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {

                        LoadComtradeFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }
        private void LoadComtradeFile(string filePath)
        {


            InfoToolStripStatusLabel.Text = $"Loading file: {Path.GetFileName(filePath)}";


            Application.DoEvents();

            try
            {


                voltageAnalyzer = new ThreePhaseVoltageAnalyzer(filePath, 10, 1);
                this.Text = $"Oscilloscope: {Path.GetFileName(filePath)}";


                string phaseInfo = voltageAnalyzer.GetPhaseIndicesInfo();
                InfoToolStripStatusLabel.Text = $"Start time: {voltageAnalyzer.TimeStampsOsc[0]} | {phaseInfo}";

                // Plot the data
                PlotVoltageData();
                PlotFreqData();
                PlotHarmData();
                PlotRMSData();
            }
            catch (Exception ex)
            {
                InfoToolStripStatusLabel.Text = "Error loading file.";
                MessageBox.Show($"Error processing COMTRADE file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void PlotHarmData()
        {
            if (voltageAnalyzer == null)
                return;


            harmPlot.Plot.Clear();

            // Подготавливаем для отображения средние за все время спектры по каждой из фаз
            int harmPoints = voltageAnalyzer.PhaseAHarmonicsAmplitudes[0].Count; // Количество гармоник по фазе
            int dataPoints = voltageAnalyzer.PhaseAHarmonicsAmplitudes.Count; // Количество временных отсчетов

            double[] harmonics = new double[harmPoints];


            double[] phaseA = new double[harmPoints];


            // Для каждой гармоники вычисляем ее среднее значение на всем протяжении измерений
            for (int i = 0; i < harmPoints; i++)
            {
                harmonics[i] = i + 1;

                // Вычисление среднего значения
                for (int j = 0; j < dataPoints; j++)
                {
                    phaseA[i] = phaseA[i] + voltageAnalyzer.PhaseAHarmonicsAmplitudes[j][i];

                }


                phaseA[i] = phaseA[i] / dataPoints;

            }

            var phA = harmPlot.Plot.Add.Bars(harmonics, phaseA);
            phA.LegendText = "Ua";

            harmPlot.Plot.Axes.AutoScale();


            harmPlot.Plot.ShowLegend(Edge.Right);


            harmPlot.Refresh();
        }

        private double FIR_filter(double[] phaseA1, int windowsize, int i)
        //Метод реализующий фильтр FIR
        {
            double phaseA2 = 0;
            double result;
            if (i < windowsize)
            {
                for (int k = 0; k <= i; k++)
                {
                    phaseA2 = phaseA2 + phaseA1[i - k];

                }
                result = phaseA2 / (i + 1);
            }
            else
            {
                for (int k = 0; k < windowsize; k++)
                {
                    phaseA2 = phaseA2 + phaseA1[i - k];

                }
                result = phaseA2 / windowsize;
            }


            return result;
        }
        private void PlotVoltageData()
        {
            if (voltageAnalyzer == null)
                return;
            OscPlot.Plot.Clear();

            int dataPoints = voltageAnalyzer.PhaseA.Count;



            double[] timeSeconds = new double[dataPoints];
            DateTime startTime = voltageAnalyzer.TimeStampsOsc[0];

            double[] phaseA = new double[dataPoints];
            double[] phaseA1 = new double[dataPoints];



            for (int i = 0; i < dataPoints; i++)
            {
                timeSeconds[i] = (voltageAnalyzer.TimeStampsOsc[i] - startTime).TotalSeconds;
                phaseA[i] = voltageAnalyzer.PhaseAU[i];


            }


            var phA = OscPlot.Plot.Add.SignalXY(timeSeconds, phaseA);
            phA.LegendText = "Ua";





            OscPlot.Plot.Axes.AutoScale();

            OscPlot.Plot.ShowLegend(Edge.Right);

            OscPlot.Refresh();
        }

        private void файлToolStripMenuItem_Click_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {



                openFileDialog.Filter = "COMTRADE files (*.cfg)|*.cfg|All files (*.*)|*.*";
                openFileDialog.Title = "Open COMTRADE File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {

                        LoadComtradeFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }
        private void PlotFreqData()
        {
            if (voltageAnalyzer == null)
                return;



            FreqPlot.Plot.Clear();

            int dataPoints = this.voltageAnalyzer.PhaseAFreq.Count;


            double[] timeSeconds = new double[dataPoints];
            DateTime startTime = voltageAnalyzer.TimeStampsFreq[0];


            double[] phaseA = new double[dataPoints];
            double[] phaseA1 = new double[dataPoints];
            for (int i = 0; i < dataPoints; i++)
            {
                timeSeconds[i] = (voltageAnalyzer.TimeStampsFreq[i] - startTime).TotalSeconds;
                phaseA[i] = voltageAnalyzer.PhaseAFreq[i];


            }

            var phA = FreqPlot.Plot.Add.SignalXY(timeSeconds, phaseA);
            phA.LegendText = "Ua";




            FreqPlot.Plot.Axes.AutoScale();

            FreqPlot.Plot.ShowLegend(Edge.Right);


            FreqPlot.Refresh();
        }
        private void PlotU_Diff()
        {



            double nominalU = 6000;
            double[] avgAarray = voltageAnalyzer.PhaseARms.ToArray();
            double avgA = Math.Sqrt(avgAarray.Average(x => x * x));

            int samplingRate = (int)voltageAnalyzer.GetSamplingRate();
            int windowSizePoints = (int)(samplingRate * 10 / 50);
            int stepSizePoints = (int)(samplingRate / 50);


            double rmsValuesPerSecond = (double)samplingRate / stepSizePoints;

            double[] timeSeconds = new double[avgAarray.Length];
            double[] phaseA = new double[avgAarray.Length];

            int rmsValuesIn3s = (int)(rmsValuesPerSecond * 600);//10 минут или 3 с
            int numberOf3secIntervals = avgAarray.Length / rmsValuesIn3s;
            double[] avgA3sec = new double[numberOf3secIntervals];

            for (int i = 0; i < numberOf3secIntervals; i++)
            {
                double[] windowNSeconds = new double[rmsValuesIn3s];
                Array.Copy(avgAarray, i * (rmsValuesIn3s), windowNSeconds, 0, rmsValuesIn3s);
                avgA3sec[i] = Math.Sqrt(windowNSeconds.Average(x => x * x));
                for (int j = i * rmsValuesIn3s; j < rmsValuesIn3s * (i + 1); j++)
                {
                    phaseA[j] = (avgA3sec[i] - nominalU) * 100 / nominalU;
                    timeSeconds[j] = (voltageAnalyzer.TimeStampsRms[j] - voltageAnalyzer.TimeStampsRms[0]).TotalSeconds;
                }

            }
            Array.Resize(ref timeSeconds, numberOf3secIntervals * rmsValuesIn3s);
            Array.Resize(ref phaseA, numberOf3secIntervals * rmsValuesIn3s);
           
            var phA = PlotUotkl.Plot.Add.SignalXY(timeSeconds, phaseA);
            phA.LegendText = "deltaUa";
           


            PlotUotkl.Plot.Axes.AutoScale();

            PlotUotkl.Plot.ShowLegend(Edge.Right);


            PlotUotkl.Refresh();


        }
        private void Plot_f_Diff()
        {

            double NominalFreq = voltageAnalyzer.GetNominalFrequency();
            double[] avgAarray = voltageAnalyzer.PhaseAFreq.ToArray();



            int samplingRate = (int)voltageAnalyzer.GetSamplingRate();
            int windowSizePoints = (int)(samplingRate * 10 / 50);
            int stepSizePoints = (int)(samplingRate / 50);


            double FreqValuesPerSecond = (double)samplingRate / stepSizePoints;


            int ValuesIn10sec = (int)(samplingRate * 10 / stepSizePoints);


            int numberOf10SecIntervals = avgAarray.Length / ValuesIn10sec;
            double[] avgAIn10sec = new double[numberOf10SecIntervals];



            int totalPoints = numberOf10SecIntervals * ValuesIn10sec;
            double[] timeSeconds = new double[totalPoints]; 
            double[] phaseAFreqDiff = new double[totalPoints];

            for (int i = 0; i < numberOf10SecIntervals; i++)
            {

                double[] windowNSeconds = new double[ValuesIn10sec];
                Array.Copy(avgAarray, i * (ValuesIn10sec), windowNSeconds, 0, ValuesIn10sec);
                avgAIn10sec[i] = windowNSeconds.Average();
               
                    for (int j = i * windowNSeconds.Length; j < windowNSeconds.Length * (i + 1); j++)
                    {
                        phaseAFreqDiff[j] = (avgAIn10sec[i] - 50);
                        timeSeconds[j] = (voltageAnalyzer.TimeStampsFreq[j] - voltageAnalyzer.TimeStampsFreq[0]).TotalSeconds;
                    }
                

            }
            var phA = Plotfotkl.Plot.Add.SignalXY(timeSeconds, phaseAFreqDiff);
            phA.LegendText = "delta_fa";

            Array.Resize(ref timeSeconds, numberOf10SecIntervals * ValuesIn10sec);
            Array.Resize(ref phaseAFreqDiff, numberOf10SecIntervals * ValuesIn10sec);

            Plotfotkl.Plot.Axes.AutoScale();

            Plotfotkl.Plot.ShowLegend(Edge.Right);


            Plotfotkl.Refresh();
        }
        private void PlotRMSData()
        {
            if (voltageAnalyzer == null)
                return;



            DeistvPlot.Plot.Clear();

            int dataPoints = this.voltageAnalyzer.PhaseARms.Count;


            double[] timeSeconds = new double[dataPoints];



            double[] phaseA = new double[dataPoints];
            for (int i = 0; i < dataPoints; i++)
            {
                timeSeconds[i] = (voltageAnalyzer.TimeStampsRms[i] - voltageAnalyzer.TimeStampsRms[0]).TotalSeconds;
                phaseA[i] = voltageAnalyzer.PhaseARms[i];


            }

            var phA = DeistvPlot.Plot.Add.SignalXY(timeSeconds, phaseA);
            phA.LegendText = "Ua";



            DeistvPlot.Plot.Axes.AutoScale();

            DeistvPlot.Plot.ShowLegend(Edge.Right);


            DeistvPlot.Refresh();
        }


        private void осциллограммаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {



                openFileDialog.Filter = "COMTRADE files (*.cfg)|*.cfg|All files (*.*)|*.*";
                openFileDialog.Title = "Open COMTRADE File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {

                        LoadComtradeFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        private void частотаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {

                openFileDialog.Filter = "COMTRADE files (*.cfg)|*.cfg|All files (*.*)|*.*";
                openFileDialog.Title = "Open COMTRADE File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {

                        LoadComtradeFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }


        private void результатыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int CountDiff_dfp = 0;
            int CountDiff_dfn = 0;

            double NominalFreq = voltageAnalyzer.GetNominalFrequency();




            double[] avgAarray = voltageAnalyzer.PhaseAFreq.ToArray();


            int samplingRate = (int)voltageAnalyzer.GetSamplingRate();
            int windowSizePoints = (int)(samplingRate * 10 / 50);
            int stepSizePoints = (int)(samplingRate * 5 / 50);


            double FreqValuesPerSecond = (double)samplingRate / stepSizePoints;


            int ValuesIn10sec = (int)(samplingRate * 10);


            int numberOf10SecIntervals = avgAarray.Length / ValuesIn10sec;
            double[] avgAIn10sec = new double[numberOf10SecIntervals];


            for (int i = 0; i < numberOf10SecIntervals; i++)
            {

                double[] windowNSeconds = new double[ValuesIn10sec];
                Array.Copy(avgAarray, i * (ValuesIn10sec), windowNSeconds, 0, ValuesIn10sec);
                avgAIn10sec[i] = windowNSeconds.Average();

                if (avgAIn10sec[i] - 50 > 0.2)
                {
                    CountDiff_dfp++;
                }
                if (avgAIn10sec[i] - 50 < -0.2)
                {
                    CountDiff_dfn++;
                }
            }
            double avgA = avgAIn10sec.Average();
            string TextToWrite = $"\n Отклонение частоты {(avgA - NominalFreq):F4} Гц \r\n";
            File.AppendAllText("results.txt", TextToWrite);


            MessageBox.Show($"частота фазы А {avgA:F4};\n отклонение частоты фазы А {(avgA - NominalFreq):F4}Гц;\n", "Усредненная частота для фазы А и отклонение частоты:",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void действующиеЗначенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int CountDiff_Up = 0;
            int CountDiff_Un = 0;
            double nominalU = 6000;
            double[] avgAarray = voltageAnalyzer.PhaseARms.ToArray();
            double avgA = Math.Sqrt(avgAarray.Average(x => x * x));

            int samplingRate = (int)voltageAnalyzer.GetSamplingRate();
            int windowSizePoints = (int)(samplingRate * 10 / 50);
            int stepSizePoints = (int)(samplingRate / 50);


            double rmsValuesPerSecond = (double)samplingRate / stepSizePoints;


            int rmsValuesIn10min = (int)(rmsValuesPerSecond * 600);
            int numberOf10minIntervals = avgAarray.Length / rmsValuesIn10min;
            double[] avgA10min = new double[numberOf10minIntervals];
            for (int i = 0; i < numberOf10minIntervals; i++)
            {
                double[] windowNSeconds = new double[rmsValuesIn10min];
                Array.Copy(avgAarray, i * (rmsValuesIn10min), windowNSeconds, 0, rmsValuesIn10min);
                avgA10min[i] = Math.Sqrt(windowNSeconds.Average(x => x * x));
                if (avgA10min[i] > 1.1 * nominalU)
                {
                    CountDiff_Up++;
                }
                if (avgA10min[i] < 0.9 * nominalU)
                {
                    CountDiff_Un++;
                }

            }

            double avgA10min1 = Math.Sqrt(avgA10min.Average(x => x * x));

            string TextToWrite = $"\n Отклонение U {((avgA10min1 - nominalU) * 100 / nominalU):F2} % \r\n";

            File.AppendAllText("results.txt", TextToWrite);

            MessageBox.Show($"Отклонение действующего значения напряжения фазы А: \n Отклонение U {((-nominalU + avgA10min1) * 100 / nominalU):F2} %;\n отклонение больше +10% было {CountDiff_Up * 10} минут \n Отклонение меньше -10% было {CountDiff_Un * 10} минут \n", "Действующие значения U для фазы А и отклонение частоты:",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

        }



        private void показатьГрафикToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PlotU_Diff();

        }

        private void показатьГрафикToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Plot_f_Diff();
        }
    }
}

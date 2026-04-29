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
        private ThreePhaseVoltageAnalyzer voltageAnalyzerFreq;
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
    
                voltageAnalyzer = new ThreePhaseVoltageAnalyzer(filePath, 10, 10);//для FFT
                voltageAnalyzerFreq = new ThreePhaseVoltageAnalyzer(filePath, 500, 500);//для freq
                Text = $"Oscilloscope: {Path.GetFileName(filePath)}";


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
                phaseA[i] = voltageAnalyzer.PhaseA[i];


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
            if (voltageAnalyzerFreq == null)
                return;



            FreqPlot.Plot.Clear();

            int dataPoints = voltageAnalyzerFreq.PhaseAFreq.Count;


            double[] timeSeconds = new double[dataPoints];
            DateTime startTime = voltageAnalyzerFreq.TimeStampsFreq[0];


            double[] phaseA = new double[dataPoints];
            for (int i = 0; i < dataPoints; i++)
            {
                timeSeconds[i] = (voltageAnalyzerFreq.TimeStampsFreq[i] - startTime).TotalSeconds;
                phaseA[i] = voltageAnalyzerFreq.PhaseAFreq[i];


            }

            var phA = FreqPlot.Plot.Add.SignalXY(timeSeconds, phaseA);
            phA.LegendText = "Ua";




            FreqPlot.Plot.Axes.AutoScale();

            FreqPlot.Plot.ShowLegend(Edge.Right);


            FreqPlot.Refresh();
        }
        private void Plot_Voltage_Difference()
        {

            double nominalU = 6000;
            double[] avgAarray = voltageAnalyzer.PhaseARms.ToArray();//3300 значений

            double[] timeSeconds = new double[avgAarray.Length];
            double[] phaseA = new double[avgAarray.Length];

            for (int i = 0; i < avgAarray.Length; i++)
            {
                phaseA[i] = (avgAarray[i] - nominalU) * 100 / nominalU;
                timeSeconds[i] = (voltageAnalyzer.TimeStampsRms[i] - voltageAnalyzer.TimeStampsRms[0]).TotalSeconds;
            }
        

            var phA = PlotUotkl.Plot.Add.SignalXY(timeSeconds, phaseA);
            phA.LegendText = "deltaUa";
           


            PlotUotkl.Plot.Axes.AutoScale();

            PlotUotkl.Plot.ShowLegend(Edge.Right);


            PlotUotkl.Refresh();


        }
        private void Plot_frequency_Difference()
        {
            double NominalFreq = voltageAnalyzerFreq.GetNominalFrequency();
            int dataPoints = voltageAnalyzerFreq.PhaseAFreq.Count;


            double[] timeSeconds = new double[dataPoints];
            DateTime startTime = voltageAnalyzerFreq.TimeStampsFreq[0];


            double[] phaseA = new double[dataPoints];
            for (int i = 0; i < dataPoints; i++)
            {
                timeSeconds[i] = (voltageAnalyzerFreq.TimeStampsFreq[i] - startTime).TotalSeconds;
                phaseA[i] = voltageAnalyzerFreq.PhaseAFreq[i]- NominalFreq;


            }

            var phA = Plotfotkl.Plot.Add.SignalXY(timeSeconds, phaseA);
            phA.LegendText = "Ua";



            Plotfotkl.Plot.Axes.AutoScale();

            Plotfotkl.Plot.ShowLegend(Edge.Right);


            Plotfotkl.Refresh();
        }
        private void PlotRMSData()
        {
            if (voltageAnalyzer == null)
                return;

            RMS_Plot.Plot.Clear();

            int dataPoints = voltageAnalyzer.PhaseARms.Count;

            double[] timeSeconds = new double[dataPoints];

            double[] phaseA = new double[dataPoints];

            for (int i = 0; i < dataPoints; i++)
            {
                timeSeconds[i] = (voltageAnalyzer.TimeStampsRms[i] - voltageAnalyzer.TimeStampsRms[0]).TotalSeconds;
                phaseA[i] = voltageAnalyzer.PhaseARms[i];


            }

            var phA = RMS_Plot.Plot.Add.SignalXY(timeSeconds, phaseA);
            phA.LegendText = "Ua";



            RMS_Plot.Plot.Axes.AutoScale();

            RMS_Plot.Plot.ShowLegend(Edge.Right);


            RMS_Plot.Refresh();
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
            int stepSizePoints = (int)(samplingRate * 10 / 50);
            int estimatesPer10sec = (int)(10.0 / (10.0 / NominalFreq));
            double FreqValuesPerSecond = (double)samplingRate / stepSizePoints;


            long PeriodsIn10sec = (int)(estimatesPer10sec);


            long numberOf10SecIntervals = voltageAnalyzer.PhaseAFIR.Length / (10 * samplingRate);

            double[] avgAIn10sec = new double[numberOf10SecIntervals];


            for (int i = 0; i < numberOf10SecIntervals; i++)
            {

                double[] windowNSeconds = new double[PeriodsIn10sec ];
                Array.Copy(avgAarray, i * (PeriodsIn10sec ), windowNSeconds, 0, PeriodsIn10sec);
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
            int samplingRate = (int)voltageAnalyzer.GetSamplingRate();
            int CountDiff_Up = 0;
            int CountDiff_Un = 0;
            double nominalU = 6000;
            double[] avgAarray = voltageAnalyzer.PhaseARms.ToArray();
            int rmsValuesIn10min = (int)(5 * voltageAnalyzer.PhaseA.Count/ (samplingRate));
            Debug.WriteLine(voltageAnalyzer.PhaseARms.Count + " voltageAnalyzer.PhaseARms.Count ?=6.600.000");
            Debug.WriteLine(rmsValuesIn10min + " rmsValuesIn10min - 660*5??");
            int numberOf10minIntervals = (int)voltageAnalyzer.PhaseA.Count / (600*samplingRate);
            double[] avgA10min = new double[numberOf10minIntervals];
            Debug.WriteLine(numberOf10minIntervals + " numberOf10minIntervals - 1??");
            for (int i = 0; i < numberOf10minIntervals; i++)
            {
                Debug.WriteLine(1);
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
            Plot_Voltage_Difference();

        }

        private void показатьГрафикToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Plot_frequency_Difference();
        }
    }
}

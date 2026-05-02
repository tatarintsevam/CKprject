using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
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
        private Quality_Calculator quality_Calculator;
        private FrequencyCalculator frequencyCalculator;
        private HarmnicsCalculator harmnicsCalculator;
        private RMS_Calculator rMS_Calculator;

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

                voltageAnalyzer = new ThreePhaseVoltageAnalyzer(filePath, 10, 10);
                voltageAnalyzerFreq = new ThreePhaseVoltageAnalyzer(filePath, 500, 500);
                harmnicsCalculator = new HarmnicsCalculator(voltageAnalyzer);
                rMS_Calculator = new RMS_Calculator(voltageAnalyzer);
                frequencyCalculator = new FrequencyCalculator(voltageAnalyzerFreq);
                quality_Calculator = new Quality_Calculator(voltageAnalyzer,  frequencyCalculator, harmnicsCalculator, rMS_Calculator);
                
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
            int harmPoints = harmnicsCalculator.PhaseAHarmonicsAmplitudes[0].Count; // Количество гармоник по фазе
            int dataPoints = harmnicsCalculator.PhaseAHarmonicsAmplitudes.Count; // Количество временных отсчетов

            double[] harmonics = new double[harmPoints];


            double[] phaseA = new double[harmPoints];


            // Для каждой гармоники вычисляем ее среднее значение на всем протяжении измерений
            for (int i = 0; i < harmPoints; i++)
            {
                harmonics[i] = i + 1;

                // Вычисление среднего значения
                for (int j = 0; j < dataPoints; j++)
                {
                    phaseA[i] = phaseA[i] + harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][i];

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
            if (frequencyCalculator == null)
                return;



            FreqPlot.Plot.Clear();

            int dataPoints = frequencyCalculator.PhaseAFreq.Count;


            double[] timeSeconds = new double[dataPoints];
            DateTime startTime = frequencyCalculator.TimeStampsFreq[0];


            double[] phaseA = new double[dataPoints];
            for (int i = 0; i < dataPoints; i++)
            {
                timeSeconds[i] = (frequencyCalculator.TimeStampsFreq[i] - startTime).TotalSeconds;
                phaseA[i] = frequencyCalculator.PhaseAFreq[i];


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
            double[] avgAarray = rMS_Calculator.PhaseARms.ToArray();//3300 значений

            double[] timeSeconds = new double[avgAarray.Length];
            double[] phaseA = new double[avgAarray.Length];

            for (int i = 0; i < avgAarray.Length; i++)
            {
                phaseA[i] = (avgAarray[i] - nominalU) * 100 / nominalU;
                timeSeconds[i] = (rMS_Calculator.TimeStampsRms[i] - rMS_Calculator.TimeStampsRms[0]).TotalSeconds;
            }


            var phA = PlotUotkl.Plot.Add.SignalXY(timeSeconds, phaseA);
            phA.LegendText = "deltaUa, %";



            PlotUotkl.Plot.Axes.AutoScale();

            PlotUotkl.Plot.ShowLegend(Edge.Right);


            PlotUotkl.Refresh();


        }

        private void Plot_frequency_Difference()
        {
            double NominalFreq = voltageAnalyzer.GetNominalFrequency();
            int dataPoints = frequencyCalculator.PhaseAFreq.Count;


            double[] timeSeconds = new double[dataPoints];
            DateTime startTime = frequencyCalculator.TimeStampsFreq[0];


            double[] phaseA = new double[dataPoints];
            for (int i = 0; i < dataPoints; i++)
            {
                timeSeconds[i] = (frequencyCalculator.TimeStampsFreq[i] - startTime).TotalSeconds;
                phaseA[i] = frequencyCalculator.PhaseAFreq[i] - NominalFreq;


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

            int dataPoints = rMS_Calculator.PhaseARms.Count;

            double[] timeSeconds = new double[dataPoints];

            double[] phaseA = new double[dataPoints];

            for (int i = 0; i < dataPoints; i++)
            {
                timeSeconds[i] = (rMS_Calculator.TimeStampsRms[i] - rMS_Calculator.TimeStampsRms[0]).TotalSeconds;
                phaseA[i] = rMS_Calculator.PhaseARms[i];


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

            double avgA = quality_Calculator.Calculate_Frequency_Quality();


            MessageBox.Show($"частота фазы А {avgA:F4};\n отклонение частоты фазы А {(avgA - quality_Calculator.NominalFreq):F4}Гц;\n Отклонение частоты более +-0,2Гц длилось {quality_Calculator.CountDiff_df_02 *10} секунд; \n Отклонение частоты более +-0,4Гц длилось {quality_Calculator.CountDiff_df_04 * 10} секунд", "Усредненная частота для фазы А и отклонение частоты:",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void действующиеЗначенияToolStripMenuItem_Click(object sender, EventArgs e)
        {


            double avgA10min1 = quality_Calculator.Calculate_Voltage_quality();



            MessageBox.Show($"Отклонение действующего значения напряжения фазы А: \n Отклонение U {((-quality_Calculator.nominalU + avgA10min1) * 100 / quality_Calculator.nominalU):F2} %;\n отклонение больше +10% было {quality_Calculator.CountDiff_Up * 10} минут \n Отклонение меньше -10% было {quality_Calculator.CountDiff_Un * 10} минут \n", "Действующие значения U для фазы А и отклонение частоты:",
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

        private void отчетToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (voltageAnalyzer == null || frequencyCalculator == null)
            {
                MessageBox.Show("нет данных", "Ошибка",
                    MessageBoxButtons.OK);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "HTML файлы (*.html)|*.html|Все файлы (*.*)|*.*";
                saveFileDialog.Title = "Сохранить отчет";
                saveFileDialog.FileName = $"Отчет_дата:{DateTime.Now:yyyyMMdd_HHmmss}.html";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var reportGenerator = new HTMLReportGenerator(voltageAnalyzer,  quality_Calculator, frequencyCalculator, harmnicsCalculator, rMS_Calculator, saveFileDialog.FileName);
                        reportGenerator.GenerateReport();
                        reportGenerator.OpenInBrowser();

                        MessageBox.Show($"Отчет сохранен в файл:\n{saveFileDialog.FileName}",
                            "Отчет", MessageBoxButtons.OK);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"не получилось создать отчет: {ex.Message}",
                            "Ошбка", MessageBoxButtons.OK);
                    }
                }
            }
        }
    }
}

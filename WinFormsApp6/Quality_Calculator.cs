using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VoltageAnalyzer;
using static Plotly.NET.StyleParam;

namespace WinFormsApp6
{
    public class Quality_Calculator
    {
        public int CountDiff_Up = 0;
        public int CountDiff_Un = 0;

        public int CountDiff_df_02 = 0;
        public int CountDiff_df_04 = 0;

        public double nominalU = 6000;

       

        public double[] GetTHDSy_Array() => THDSy_Array;

        private readonly ThreePhaseVoltageAnalyzer _voltageAnalyzer;
        private readonly FrequencyCalculator _frequencyCalculator;
        private readonly HarmnicsCalculator _harmnicsCalculator;
        private readonly RMS_Calculator _rMS_Calculator;
        protected double[] THDSy_Array;

        public Quality_Calculator(ThreePhaseVoltageAnalyzer voltageAnalyzer,
                                    FrequencyCalculator frequencyCalculator,
                                    HarmnicsCalculator harmnicsCalculator,
                                    RMS_Calculator rMS_Calculator)


        {
            _voltageAnalyzer = voltageAnalyzer;
            _frequencyCalculator = frequencyCalculator;
            _harmnicsCalculator= harmnicsCalculator;
            _rMS_Calculator = rMS_Calculator;
        }
        public double Calculate_Harmonics_THDSy()
        {
            int dataPoints = _harmnicsCalculator.PhaseAHarmonicsAmplitudes.Count; // Количество временных отсчетов по 10 периодов 
            THDSy_Array = new double[dataPoints];
           
            for (int j = 0; j < dataPoints; j++)
            {
                double Ysg_1 = Math.Sqrt(_harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][9] * _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][9] + _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][11] * _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][11] + _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][10] * _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][10]);
                double YhY1 = 0;
                for (int i = 2; i < 40; i++)
                {
                        double Ysg_h =Math.Sqrt(_harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][i * 10 + 1] * _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][i * 10 + 1] + _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][i * 10] * _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][i * 10] + _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][i * 10 - 1] * _harmnicsCalculator.PhaseAHarmonicsAmplitudes[j][i * 10 - 1]);
                       
                        YhY1 = YhY1 + (Math.Pow(Ysg_h,2) / Math.Pow(Ysg_1, 2)); 

                }
                THDSy_Array[j] = Math.Sqrt(YhY1);
                WriteCSV_THDSy(j, THDSy_Array[j]);

            }
            double THDSy_Sum = Math.Sqrt(THDSy_Array.Average(x => x * x));//все таки RMS
            //double THDSy_Sum = THDSy_Array.Average(); //RMS или просто .Average?
            //double THDSy_Sum = Math.Sqrt(THDSy_Array.Average(x => x * x));
            return THDSy_Sum;
        
        
        }

        private void WriteCSV_THDSy(int Count, double THDSy)
        {
            string csvFilePath = "outputTHDSy.csv";

            try
            {
                bool fileExists = File.Exists(csvFilePath);
                // Запись данных
                using (StreamWriter sw = new StreamWriter(csvFilePath, true, Encoding.UTF8))
                {

                    if (!fileExists)
                    {
                        sw.WriteLine("Номер интервала 10 периодов; Значение THDSy на интервале 10 периодов");
                        sw.WriteLine($"{Count }; {THDSy.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)}");
                    }
                    else
                    {
                        sw.WriteLine($"{Count }; {THDSy.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)}");
                    }

                }



            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при записи в файл: {ex.Message}");
            }

        }

        public double Calculate_Voltage_quality()
        {


            int samplingRate = (int)_voltageAnalyzer.GetSamplingRate();
            double[] avgAarray = _rMS_Calculator.PhaseARms.ToArray();
            int rmsValuesIn10min = (int)(5 * _voltageAnalyzer.PhaseA.Count/ (samplingRate));

            int numberOf10minIntervals = (int)_voltageAnalyzer.PhaseA.Count / (600 * samplingRate);
            double[] avgA10min = new double[numberOf10minIntervals];

            if (numberOf10minIntervals==0) 
            {
                throw new Exception("Запись меньше 10 минут, не получится определить этот показатель качества ЭЭ");
            }
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
            return avgA10min1;
        }

        public double Calculate_Frequency_Quality()
        {

         

            double[] avgAarray = _frequencyCalculator.PhaseAFreq.ToArray();

            int numberOf10SecIntervals = _frequencyCalculator.PhaseAFreq.Count;

            for (int i = 0; i < numberOf10SecIntervals; i++)
            {


                if (Math.Abs(avgAarray[i] - _voltageAnalyzer.GetNominalFrequency()) > 0.2 && (Math.Abs(avgAarray[i] - _voltageAnalyzer.GetNominalFrequency()) < 0.4) )
                {
                    CountDiff_df_02++;
                }
                if (Math.Abs(avgAarray[i] - _voltageAnalyzer.GetNominalFrequency()) > 0.4 )
                {
                    CountDiff_df_04++;
                }
            }
            double avgA = avgAarray.Average();
            return avgA;
        }

    }
}

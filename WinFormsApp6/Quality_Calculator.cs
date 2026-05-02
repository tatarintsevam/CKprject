using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoltageAnalyzer;

namespace WinFormsApp6
{
    public class Quality_Calculator
    {
        public int CountDiff_Up = 0;
        public int CountDiff_Un = 0;

        public int CountDiff_df_02 = 0;
        public int CountDiff_df_04 = 0;

        public double nominalU = 6000;
        public double NominalFreq = 50;

        private readonly ThreePhaseVoltageAnalyzer _voltageAnalyzer;
        private readonly FrequencyCalculator _frequencyCalculator;
        private readonly HarmnicsCalculator _harmnicsCalculator;
        private readonly RMS_Calculator _rMS_Calculator;


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

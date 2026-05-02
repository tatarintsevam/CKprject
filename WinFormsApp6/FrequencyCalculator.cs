using ComtradeHandler.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoltageAnalyzer;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;
using WinFormsApp6;
using Wisp.Comtrade;
using Wisp.Comtrade.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WinFormsApp6
{
   public class FrequencyCalculator
    {
        private readonly ThreePhaseVoltageAnalyzer _voltageAnalyzer;
        public List<double> PhaseAFreq { get; private set; }
        public List<DateTime> TimeStampsFreq { get; private set; }

        public FrequencyCalculator(ThreePhaseVoltageAnalyzer voltageAnalyzer)
        {
            _voltageAnalyzer = voltageAnalyzer;
            CalculateFrequencies();
        }
        
        public void CalculateFrequencies()//вычисление частот на промежутках в 10 периодов
        {
            TimeStampsFreq = new List<DateTime>();
            PhaseAFreq = new List<double>(); 
            

            double _nominalFrequency = _voltageAnalyzer.GetNominalFrequency();


            PhaseAFreq.Clear(); 

            int samplingRate = (int)_voltageAnalyzer.GetSamplingRate();

            int windowSizePoints = (int)(samplingRate * _voltageAnalyzer.GetWindowSizePeriods() / _nominalFrequency);

            int stepSizePoints = (int)(samplingRate * _voltageAnalyzer.GetStepSizePeriods() / _nominalFrequency);

            //double[] PhaseA1 = PhaseA.ToArray();
            double[] PhaseA1 = _voltageAnalyzer.GetPhaseAFIR();//щас тут будет null надо вынести FIR в отдельный метод

            for (int startPoint = 0; startPoint <= PhaseA1.Length - windowSizePoints; startPoint += stepSizePoints)
            {
                // Получение данных для текущего окна
                double[] phaseAWindow = _voltageAnalyzer.GetDataWindow(PhaseA1, startPoint, windowSizePoints);

                // Расчет частоты для каждой фазы
                double frequencyA = CalculateFrequencyByZeroCrossings2(phaseAWindow, samplingRate, _nominalFrequency);


                // Расчет временной метки для середины окна
                int middlePoint = startPoint + windowSizePoints / 2;
                DateTime timeStamp = _voltageAnalyzer.GetComtradeData().Configuration.StartTime.AddSeconds((double)middlePoint / samplingRate);

                // Сохранение результатов
                TimeStampsFreq.Add(timeStamp);

                // Сохранение результатов
                PhaseAFreq.Add(frequencyA);//массив на выходе 6600000/2000=3300 точек

            }


        }
        protected double CalculateFrequencyByZeroCrossings2(double[] data, int samplingRate, double _nominalFrequency)
        {

            int minPoints = 3;

            List<double> zeroCrossingApprx = new List<double>();

            for (int i = 1; i < data.Length; i++)
            {

                if (data[i - 1] <= 0 && data[i] > 0)
                {
                    double x1 = i - 1;
                    double y1 = data[i - 1];
                    double x2 = i;
                    double y2 = data[i];
                    double exactCrossing = x1 - y1 * (x2 - x1) / (y2 - y1);
                    zeroCrossingApprx.Add(exactCrossing);
                    //zeroCrossingIndices.Add(i);
                    //zeroCrossingApprx.Add(i)
                }
            }

            if (zeroCrossingApprx.Count < minPoints)
            {

                return _nominalFrequency;
            }


            double totalPeriods = 0;
            int periodCount = 0;

            for (int i = 1; i < zeroCrossingApprx.Count; i++)
            {
                double samplesBetweenCrossings = zeroCrossingApprx[i] - zeroCrossingApprx[i - 1];


                double estimatedFreq = (double)samplingRate / samplesBetweenCrossings;
                if (estimatedFreq >= 0.5 * _nominalFrequency && estimatedFreq <= 2.0 * _nominalFrequency)
                {
                    totalPeriods += samplesBetweenCrossings;
                    periodCount++;
                }

                // Если нет ни одного валидного периода, возвращаем номинальную частоту
                if (periodCount == 0)
                {
                    return _nominalFrequency;
                }
            }

            // Вычисляем среднюю длительность периода в сэмплах
            double averagePeriodInSamples = totalPeriods / periodCount;

            // Пересчитываем в Герцы: частота = частота дискретизации / количество отсчетов в периоде
            double frequency = samplingRate / averagePeriodInSamples;

            return frequency;
        }

    }
}

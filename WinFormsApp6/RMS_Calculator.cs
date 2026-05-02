using ComtradeHandler.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoltageAnalyzer;

namespace WinFormsApp6
{
    public class RMS_Calculator
    {
        private readonly ThreePhaseVoltageAnalyzer _voltageAnalyzer;
        public List<DateTime> TimeStampsRms { get; private set; }
        public List<double> PhaseARms { get; private set; }
        public RMS_Calculator(ThreePhaseVoltageAnalyzer voltageAnalyzer)

        {
            _voltageAnalyzer = voltageAnalyzer;
   
            InitializeResultLists();
            CalculateRmsValues();
        }
        protected double CalculateRmsValue(double[] data)
        {
            // Квадратный корень из среднего квадрата значений
            double sumOfSquares = data.Sum(x => x * x);
            double rmsValue = Math.Sqrt(sumOfSquares / data.Length);
            return rmsValue;
        }
        protected void CalculateRmsValues()
        {
            // Инициализация списков для RMS значений
            TimeStampsRms = new List<DateTime>();
            PhaseARms = new List<double>();



            // Получение частоты дискретизации
            int samplingRate = (int)_voltageAnalyzer.GetConfiguration().SampleRates[0].SamplingFrequency;
            double durationSeconds = _voltageAnalyzer.GetPhaseAData().Count / samplingRate;
            // Расчет размера окна и шага в точках
            int windowSizePoints = (int)(samplingRate * _voltageAnalyzer.GetWindowSizePeriods() / _voltageAnalyzer.GetNominalFrequency());
            int stepSizePoints = (int)(samplingRate * _voltageAnalyzer.GetStepSizePeriods() / _voltageAnalyzer.GetNominalFrequency());

            double[] phaseAWindow1 = _voltageAnalyzer.PhaseA.ToArray();
            // Расчет для каждого окна
            for (int startPoint = 0; startPoint <= _voltageAnalyzer.PhaseA.Count - windowSizePoints; startPoint += stepSizePoints)
            {
                // Получение данных для текущего окна
                double[] phaseAWindow = _voltageAnalyzer.GetDataWindow(phaseAWindow1, startPoint, windowSizePoints);


                // Расчет RMS значений для каждой фазы
                double rmsA = CalculateRmsValue(phaseAWindow);


                // Расчет временной метки для середины окна
                int middlePoint = startPoint + windowSizePoints / 2;
                DateTime timeStamp = _voltageAnalyzer.GetComtradeData().Configuration.StartTime.AddSeconds((double)middlePoint / samplingRate);

                // Сохранение результатов
                TimeStampsRms.Add(timeStamp);
                PhaseARms.Add(rmsA);


            }


        }
        protected void InitializeResultLists()
        {


            TimeStampsRms = new List<DateTime>();
            PhaseARms = new List<double>();


        }

 
    
      
    }
}

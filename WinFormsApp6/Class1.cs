using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinFormsApp6;
using Wisp.Comtrade;
using Wisp.Comtrade.Models;

namespace VoltageAnalyzer
{

    public class ThreePhaseVoltageAnalyzer
    {
        public double[] PhaseAFIR;
        private List<PhaseVoltageCandidate> _phaseVoltageCandidates;

        private const double TWO_PI = 2 * Math.PI;
        public double[] PhaseAU;

        private Wisp.Comtrade.RecordReader _comtradeData;
        private ConfigurationHandler configuration;

        private int _phaseAIndex;
        private int _phaseBIndex;
        private int _phaseCIndex;
        private double _nominalFrequency;
        public double GetNominalFrequency() => configuration.Frequency;
        public string GetStationName() => configuration.StationName;    
        public double GetSamplingRate() => (int)configuration.SampleRates[0].SamplingFrequency;
        public List<DateTime> TimeStampsOsc { get; private set; }

        public List<double> PhaseA { get; private set; }

        public List<double> PhaseB { get; private set; }

        public List<double> PhaseC { get; private set; }


        private int _windowSizePeriods;
        private int _stepSizePeriods;

        public List<DateTime> TimeStampsFreq { get; private set; }
        public List<double> PhaseAFreq { get; private set; }
        public List<double> PhaseBFreq { get; private set; }
        public List<double> PhaseCFreq { get; private set; }

        // Добавляем новые списки для хранения RMS значений
        public List<DateTime> TimeStampsRms { get; private set; }
        public List<double> PhaseARms { get; private set; }
        public List<double> PhaseBRms { get; private set; }
        public List<double> PhaseCRms { get; private set; }

        // Новые свойства для хранения результатов гармоник
        public List<DateTime> TimeStampsHarmonics { get; private set; }
        public List<List<double>> PhaseAHarmonicsAmplitudes { get; private set; }
        public List<List<double>> PhaseBHarmonicsAmplitudes { get; private set; }
        public List<List<double>> PhaseCHarmonicsAmplitudes { get; private set; }
        public List<List<double>> PhaseAHarmonicsPhases { get; private set; }
        public List<List<double>> PhaseBHarmonicsPhases { get; private set; }
        public List<List<double>> PhaseCHarmonicsPhases { get; private set; }

        public ThreePhaseVoltageAnalyzer(string comtradeFilePath, int windowSizePeriods, int stepSizePeriods)
        {
            _windowSizePeriods = windowSizePeriods;
            _stepSizePeriods = stepSizePeriods;


           configuration = new ConfigurationHandler(comtradeFilePath);

     
            _comtradeData = new RecordReader(comtradeFilePath);
 
            FindPhaseVoltageIndices();

          
            _nominalFrequency = configuration.Frequency;

         
            InitializeOscLists();

    
            
            InitializeResultLists();
            ReadAllVoltages();

            CalculateAll();
        }
      
        private void ClearResults()
        {
            TimeStampsOsc.Clear();

            PhaseA.Clear();
          
        }
        private void InitializeResultLists()
        {
            TimeStampsFreq = new List<DateTime>();
            PhaseAFreq = new List<double>();
    

            TimeStampsRms = new List<DateTime>();
            PhaseARms = new List<double>();
     

            TimeStampsHarmonics = new List<DateTime>();
            PhaseAHarmonicsAmplitudes = new List<List<double>>();
       
            PhaseAHarmonicsPhases = new List<List<double>>();
          
        }
        private void CalculateAll()
        {
            CalculateFrequencies();

            CalculateRmsValues();

            CalculateHarmonics();


        }
        
        
        private void CalculateRmsValues()
        {
            // Инициализация списков для RMS значений
            TimeStampsRms = new List<DateTime>();
            PhaseARms = new List<double>();


            
        // Получение частоты дискретизации
            int samplingRate = (int)configuration.SampleRates[0].SamplingFrequency;
            double durationSeconds = PhaseA.Count / samplingRate;
            // Расчет размера окна и шага в точках
            int windowSizePoints = (int)(samplingRate * _windowSizePeriods / _nominalFrequency);
            int stepSizePoints = (int)(samplingRate * _stepSizePeriods / _nominalFrequency);
            
            double[] phaseAWindow1 = PhaseAU.ToArray();
            // Расчет для каждого окна
            for (int startPoint = 0; startPoint <= PhaseA.Count - windowSizePoints; startPoint += stepSizePoints)
            {
                // Получение данных для текущего окна
                double[] phaseAWindow = GetDataWindow(phaseAWindow1, startPoint, windowSizePoints);
              

                // Расчет RMS значений для каждой фазы
                double rmsA = CalculateRmsValue(phaseAWindow);
             

                // Расчет временной метки для середины окна
                int middlePoint = startPoint + windowSizePoints / 2;
                DateTime timeStamp = _comtradeData.Configuration.StartTime.AddSeconds((double)middlePoint / samplingRate);

                // Сохранение результатов
                TimeStampsRms.Add(timeStamp);
                PhaseARms.Add(rmsA);
          

            }


        }
        private (List<double> amplitudes, List<double> phases) CalculateHarmonicsForWindow(double[] data,
    int samplingRate)
        {
            Complex[] dft = PerformFFT(data);
            List<double> amplitudes = new List<double>();
            List<double> phases = new List<double>();
            // Рассчитываем до 50-й гармоники (можно адаптировать)
            int maxHarmonic = (int)(samplingRate / (2 * _nominalFrequency));
            for (int i = 1; i <= maxHarmonic; i++)
            {
                int index = (int)(i * _nominalFrequency * data.Length / samplingRate);
                if (index >= dft.Length) break;

                amplitudes.Add(Math.Sqrt(dft[index].Real * dft[index].Real +
                    dft[index].Imaginary * dft[index].Imaginary) * 2 / data.Length);
                phases.Add(Math.Atan2(dft[index].Imaginary, dft[index].Real));
            }

            return (amplitudes, phases);
        }
        private void CalculateHarmonics()
        {
            TimeStampsHarmonics.Clear();
            PhaseAHarmonicsAmplitudes.Clear();
    
            PhaseAHarmonicsPhases.Clear();
   

            int samplingRate = (int)configuration.SampleRates[0].SamplingFrequency;
            int windowSizePoints = (int)(samplingRate * _windowSizePeriods / _nominalFrequency);
            int stepSizePoints = (int)(samplingRate * _stepSizePeriods / _nominalFrequency);
            double[] phaseAWindow1 = PhaseAU.ToArray();
            for (int startPoint = 0; startPoint <= PhaseA.Count - windowSizePoints; startPoint += stepSizePoints)
            {
                var phaseAWindow = GetDataWindow(phaseAWindow1, startPoint, windowSizePoints);
               

                var (amplitudesA, phaseAU) = CalculateHarmonicsForWindow(phaseAWindow, samplingRate);
              

                int middlePoint = startPoint + windowSizePoints / 2;
                DateTime timeStamp = _comtradeData.Configuration.StartTime.AddSeconds(
                    (double)middlePoint / samplingRate);

                TimeStampsHarmonics.Add(timeStamp);
                PhaseAHarmonicsAmplitudes.Add(amplitudesA);
               
                PhaseAHarmonicsPhases.Add(phaseAU);
            
            }


        }
        private Complex[] PerformDFT(double[] data)
        {
            int N = data.Length;
            Complex[] result = new Complex[N];
            for (int k = 0; k < N; k++)
            {
                for (int n = 0; n < N; n++)
                {
                    double angle = -2 * Math.PI * k * n / N;
                    result[k] += data[n] * new Complex(Math.Cos(angle), Math.Sin(angle));
                }
            }
            return result;
        }
        public double CalculateRmsValue(double[] data)
        {
            // Квадратный корень из среднего квадрата значений
            double sumOfSquares = data.Sum(x => x * x);
            double rmsValue = Math.Sqrt(sumOfSquares / data.Length);
            return rmsValue;
        }
        private Complex[] PerformFFT(double[] data)//метод в котором реализовано БПФ, статический потому что не будет объекта класса
        {
            int N = data.Length;
            int nextPowerOfTwo = 1;
            while (nextPowerOfTwo < N)
            {
                nextPowerOfTwo <<= 1; // Умножаем на 2 до тех пор, пока не получим степень двойки
            }

            // Если нужно дополнение, создаем новый массив
            if (nextPowerOfTwo != N)
            {
                double[] addedData = new double[nextPowerOfTwo];
                Array.Copy(data, addedData, N); // Копируем исходные данные
                                                // Остальные элементы автоматически заполнены 0
                data = addedData;
                N = nextPowerOfTwo;
            }
            if (N == 1)//выход из рекурсии
                return new Complex[] { data[0] };
            double[] odd = new double[N / 2];//создание массива четного и нечетного X(m)
            double[] even = new double[N / 2];
            for (int i = 0; i < N / 2; i++)//заполнение массивов данными
            {
                odd[i] = data[2 * i + 1];
                even[i] = data[2 * i];

            }
            Complex[] BPFodd = PerformFFT(odd);//шаг рекурсии
            Complex[] BPFeven = PerformFFT(even);

            Complex[] result = new Complex[N];//создание массива результатов

            for (int i = 0; i < N / 2; i++)//цикл для вычисления финального результата по формулам 
                                           //X(i)=even[]+(W^i)*odd[] и X(i+N/2)=even[]-(W^i)*odd[] 
            {
                Complex W = Complex.FromPolarCoordinates(1, -2 * Math.PI * i / N);//вычисление экспоненты, которая отвечает за сдвиг угла

                Complex WFFT = W * BPFodd[i];

                result[i] = BPFeven[i] + WFFT;
                result[i + N / 2] = BPFeven[i] - WFFT;
            }

            return result;
        }


        private void CalculateFrequencies()
        {
            PhaseAFreq.Clear();
      
            int samplingRate = (int)configuration.SampleRates[0].SamplingFrequency;
           
            int windowSizePoints = (int)(samplingRate * _windowSizePeriods / _nominalFrequency);
       
            int stepSizePoints = (int)(samplingRate * _stepSizePeriods / _nominalFrequency);
           
            //double[] PhaseA1 = PhaseA.ToArray();
            double[] PhaseA1 = PhaseAFIR;
            for (int startPoint = 0; startPoint <= PhaseA.Count - windowSizePoints; startPoint += stepSizePoints)
            {
                // Получение данных для текущего окна
                double[] phaseAWindow = GetDataWindow(PhaseA1, startPoint, windowSizePoints);
               
                // Расчет частоты для каждой фазы
                double frequencyA = CalculateFrequencyByZeroCrossings2(phaseAWindow, samplingRate);
               

                // Расчет временной метки для середины окна
                int middlePoint = startPoint + windowSizePoints / 2;
                DateTime timeStamp = _comtradeData.Configuration.StartTime.AddSeconds((double)middlePoint / samplingRate);

                // Сохранение результатов
                TimeStampsFreq.Add(timeStamp);

                // Сохранение результатов
                PhaseAFreq.Add(frequencyA);
               
            }

        }
        private double CalculateFrequencyByZeroCrossings2(double[] data, int samplingRate)
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
        private double[] GetDataWindow(double[] data, int startIndex, int length)
        {
            double[] window = new double[length];
            Array.Copy(data, startIndex, window, 0, length);
            return window;
            
        }
        public string GetPhaseIndicesInfo()
        {
            return $"A (idx:{_phaseAIndex + 1}))";
        }
       
        private void FindPhaseVoltageIndices()
        {
          
            List<PhaseVoltageCandidate> phaseVoltageCandidates = new List<PhaseVoltageCandidate>();

          
            var phaseARegex = new Regex(@"(?:UA|VA|U[_\s]*A|V[_\s]*A|Ua|Va|Phase[\s_]*A)", RegexOptions.IgnoreCase);
            


            for (int i = 0; i < configuration.AnalogChannelsCount; i++)
            {

                var channel1 = configuration.AnalogChannelInformationList[i];
                var channel = configuration.AnalogChannelInformationList[i];
                bool isVoltage = channel.Units.ToLower().Contains("v") ||
                channel.Units.ToLower().Contains("в");

                if (!isVoltage)
                    continue;

               
                string channelName = channel.Name;

                PhaseType detectedPhase = PhaseType.Unknown;

                if (phaseARegex.IsMatch(channelName))
                {
                    detectedPhase = PhaseType.PhaseA;
                }
              

              
                if (detectedPhase != PhaseType.Unknown)
                {
                    phaseVoltageCandidates.Add(new PhaseVoltageCandidate
                    {
                        ChannelIndex = i,
                        PhaseType = detectedPhase,
                        ChannelName = channelName
                    });
                }
            }
            if (phaseVoltageCandidates.Count == 0)
            {
                
                var voltageChannels = new List<int>();

                for (int i = 0; i < configuration.AnalogChannelsCount; i++)
                {
                    var channel = configuration.AnalogChannelInformationList[i];
                    bool isVoltage = channel.Units.ToLower().Contains("v") ||
                    channel.Units.ToLower().Contains("в");

                    if (isVoltage)
                    {
                        voltageChannels.Add(i);
                    }
                }

               
                if (voltageChannels.Count >= 3)
                {
              
                    phaseVoltageCandidates.Add(new PhaseVoltageCandidate
                    {
                        ChannelIndex = voltageChannels[0],
                        PhaseType = PhaseType.PhaseA,
                        ChannelName = configuration.AnalogChannelInformationList[voltageChannels[0]].Name
                    });

                   
                }
            }
            var phaseA = phaseVoltageCandidates.FirstOrDefault(c => c.PhaseType == PhaseType.PhaseA);
           

            if (phaseA != null /*&& phaseB != null && phaseC != null*/)
            {
                _phaseAIndex = phaseA.ChannelIndex;
               
            }
            else
            {
                throw new Exception("не получилось определить индексы всех трех напряжений");
            }

        }
       
        public void ReadAllVoltages()
        {
            PhaseA = _comtradeData.GetAnalogPrimaryChannel(_phaseAIndex).ToList();
            PhaseAFIR = CalculatePhaseAFIR(PhaseA.ToArray());
            PhaseAU = CalculatePhaseAFIR(PhaseA.ToArray());
            double samplingRate = configuration.SampleRates[0].SamplingFrequency;
            for (int i = 0; i < PhaseA.Count; i++)
            {
                DateTime timeStamp = configuration.StartTime.AddSeconds(i / samplingRate);
                TimeStampsOsc.Add(timeStamp);

              
            }
        }
        private double[] CalculatePhaseAFIR( double[] _PhaseAToFIR)
        {
            int dataPoints = _PhaseAToFIR.Length;
            double[] phaseA = new double[dataPoints];
            double[] phaseA1 = new double[dataPoints];

            for (int i = 0; i < dataPoints; i++)
            {
               
                phaseA1[i] = _PhaseAToFIR[i];

            }

            for (int i = 0; i < dataPoints; i++)
            {


                //phaseA[i] = voltageAnalyzer.PhaseAFreq[i];
                phaseA[i] = FIRfltr(phaseA1, 20, i);

            }
            return phaseA;

        }
        private double FIRfltr(double[] phaseA1, int windowsize, int i)
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

        private void InitializeOscLists()
        {
            TimeStampsOsc = new List<DateTime>();

            PhaseA = new List<double>();
           
        }


    }

    class PhaseVoltageCandidate
    {
        public int ChannelIndex { get; set; }

        public PhaseType PhaseType { get; set; }

        public string ChannelName { get; set; }

    }
    enum PhaseType
    {
        Unknown,
        PhaseA,
    
    }



}

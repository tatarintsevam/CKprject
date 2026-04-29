using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;
using WinFormsApp6;
using Wisp.Comtrade;
using Wisp.Comtrade.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VoltageAnalyzer
{

    public class ThreePhaseVoltageAnalyzer
    {



        public double[] PhaseAFIR;
        private List<PhaseVoltageCandidate> _phaseVoltageCandidates;

        protected const double TWO_PI = 2 * Math.PI;
        protected double[] PhaseAU;

        protected Wisp.Comtrade.RecordReader _comtradeData;
        protected ConfigurationHandler configuration;

        protected int _phaseAIndex;
        protected double _nominalFrequency;
        public double GetNominalFrequency() => configuration.Frequency;
        public string GetStationName() => configuration.StationName;    
        public double GetSamplingRate() => (int)configuration.SampleRates[0].SamplingFrequency;
        public List<DateTime> TimeStampsOsc { get; private set; }

        public List<double> PhaseA { get; private set; }

        public List<double> PhaseB { get; private set; }

        public List<double> PhaseC { get; private set; }


        protected int _windowSizePeriods;
        protected int _stepSizePeriods;

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

        protected void ClearResults()
        {
            TimeStampsOsc.Clear();

            PhaseA.Clear();
          
        }
        protected void InitializeResultLists()
        {
            TimeStampsFreq = new List<DateTime>();
            PhaseAFreq = new List<double>();
    

            TimeStampsRms = new List<DateTime>();
            PhaseARms = new List<double>();
     

            TimeStampsHarmonics = new List<DateTime>();
            PhaseAHarmonicsAmplitudes = new List<List<double>>();
       
            PhaseAHarmonicsPhases = new List<List<double>>();
          
        }
        protected void CalculateAll()
        {
            CalculateFrequencies();

            CalculateRmsValues();

            CalculateHarmonics();


        }


        protected void CalculateRmsValues()
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
            
            double[] phaseAWindow1 = PhaseA.ToArray();
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
        protected (List<double> amplitudes, List<double> phases) CalculateHarmonicsForWindow(double[] data,
    int samplingRate)
        {
            Complex[] dft = PerformFFT(data);
            //var dft = data.Select(x => new Complex(x, 0)).ToArray(); Fourier.Forward(dft, FourierOptions.Default);
            List<double> amplitudes = new List<double>();
            List<double> phases = new List<double>();
            // Рассчитываем до 50-й гармоники
            int maxHarmonic = (int)(samplingRate / (2 * _nominalFrequency));
            for (int i = 1; i <= maxHarmonic; i++)
            {
                int index = (int)(i * _nominalFrequency * data.Length / samplingRate);
                if (index >= dft.Length) break;

                amplitudes.Add((Math.Sqrt(dft[index].Real * dft[index].Real +
                    dft[index].Imaginary * dft[index].Imaginary) * 2 / dft.Length));
                phases.Add(Math.Atan2(dft[index].Imaginary, dft[index].Real));
            }

            return (amplitudes, phases);
        }
        protected void CalculateHarmonics()
        {
            TimeStampsHarmonics.Clear();
            PhaseAHarmonicsAmplitudes.Clear();

            PhaseAHarmonicsPhases.Clear();


            int samplingRate = (int)configuration.SampleRates[0].SamplingFrequency;
            int windowSizePoints = (int)(samplingRate * _windowSizePeriods / _nominalFrequency);
            int stepSizePoints = (int)(samplingRate * _stepSizePeriods / _nominalFrequency);
            double[] phaseAWindow1 = PhaseA.ToArray();


            for (int startPoint = 0; startPoint <= PhaseA.Count - windowSizePoints; startPoint += stepSizePoints)
            {
                var phaseAWindow = GetDataWindow(phaseAWindow1, startPoint, windowSizePoints);


                var (amplitudesA, phaseA) = CalculateHarmonicsForWindow(phaseAWindow, samplingRate);


                int middlePoint = startPoint + windowSizePoints / 2;
                DateTime timeStamp = _comtradeData.Configuration.StartTime.AddSeconds(
                    (double)middlePoint / samplingRate);

                TimeStampsHarmonics.Add(timeStamp);
                PhaseAHarmonicsAmplitudes.Add(amplitudesA);

                PhaseAHarmonicsPhases.Add(phaseA);

            }


        }
        protected bool IsPowerOfTwo(int x)
        {
            return x > 0 && (x & (x - 1)) == 0;
        }

        protected double CalculateRmsValue(double[] data)
        {
            // Квадратный корень из среднего квадрата значений
            double sumOfSquares = data.Sum(x => x * x);
            double rmsValue = Math.Sqrt(sumOfSquares / data.Length);
            return rmsValue;
        }
        protected Complex[] CalculateFFT(double[] data)//выполняет FFT преобразование
        {
            int N = data.Length;

            if (N == 1)//выход из рекурсии
                return new Complex[] { data[0] };
            double[] odd = new double[N / 2];//создание массива четного и нечетного X(m)
            double[] even = new double[N / 2];
            for (int i = 0; i < N / 2; i++)//заполнение массивов данными
            {
                odd[i] = data[2 * i + 1];
                even[i] = data[2 * i];

            }
            Complex[] BPFodd = CalculateFFT(odd);//шаг рекурсии
            Complex[] BPFeven = CalculateFFT(even);

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
        protected Complex[] PerformFFT(double[] data1)//Преобразует исходный массив, меняет его размер
        {

            int nextPowerOfTwo1 = 1;
            while (nextPowerOfTwo1 < data1.Length)
            {
                nextPowerOfTwo1 <<= 1; // Умножаем на 2 до тех пор, пока не получим степень двойки
            }
            Complex[] result = new Complex[nextPowerOfTwo1];//создание массива результатов
            if (!IsPowerOfTwo(data1.Length))
            {
                double[] data = new double[nextPowerOfTwo1];
                double[] originalArguments = new double[data1.Length];
                for (int i = 0; i < data1.Length; i++)
                {
                    originalArguments[i] = i;
                }

                double[] newArguments = new double[nextPowerOfTwo1];
                for (int i = 0; i < nextPowerOfTwo1; i++)
                {
                    newArguments[i] = i * (double)(data1.Length - 1) / (nextPowerOfTwo1 - 1);
                }

                for (int i = 0; i < nextPowerOfTwo1; i++)
                {
                    data[i] = Perform_interpolation(data1, (int)Math.Round(newArguments[i]), originalArguments);
                }
                result = CalculateFFT(data);
            }
            else
            {
                result = CalculateFFT(data1);
            }


            return result;

        }

        protected Complex[] PerformDFT(double[] data)
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
        protected void CalculateFrequencies()//вычисление частот на промежутках в 10 периодов
        {
            
           
            PhaseAFreq.Clear();
      
            int samplingRate = (int)configuration.SampleRates[0].SamplingFrequency;
           
            int windowSizePoints = (int)(samplingRate * _windowSizePeriods / _nominalFrequency);
       
            int stepSizePoints = (int)(samplingRate * _stepSizePeriods/ _nominalFrequency);

            //double[] PhaseA1 = PhaseA.ToArray();
            double[] PhaseA1 = PhaseAFIR;

            for (int startPoint = 0; startPoint <= PhaseA1.Length - windowSizePoints; startPoint += stepSizePoints)
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
                PhaseAFreq.Add(frequencyA);//массив на выходе 6600000/2000=3300 точек
               
            }


        }
        protected double Perform_interpolation(double[] data, int ReqNumOfPoints, double[] arguments)
        {
            double x = ReqNumOfPoints;
            int n = data.Length;


            int idx = Array.BinarySearch(arguments, x);
            if (idx < 0) idx = ~idx - 1;
            if (idx < 0) idx = 0;
            if (idx >= n - 1) idx = n - 2;

            double x0 = arguments[idx];
            double x1 = arguments[idx + 1];
            double y0 = data[idx];
            double y1 = data[idx + 1];


            static double[] GetSecondDerivatives(double[] y, double[] x)
            {
                int N = y.Length;
                double[] h = new double[N - 1];
                for (int i = 0; i < N - 1; i++)
                    h[i] = x[i + 1] - x[i];

                double[] a = new double[N];
                double[] b = new double[N];
                double[] c = new double[N];
                double[] d = new double[N];

                for (int i = 1; i < N - 1; i++)
                {
                    a[i] = h[i - 1];
                    b[i] = 2 * (h[i - 1] + h[i]);
                    c[i] = h[i];
                    d[i] = 6 * ((y[i + 1] - y[i]) / h[i] - (y[i] - y[i - 1]) / h[i - 1]);
                }


                b[0] = 1;
                c[0] = 0;
                d[0] = 0;
                a[N - 1] = 0;
                b[N - 1] = 1;
                d[N - 1] = 0;


                double[] m = new double[N];
                double[] alpha = new double[N];
                double[] beta = new double[N];
                alpha[0] = -c[0] / b[0];
                beta[0] = d[0] / b[0];
                for (int i = 1; i < N; i++)
                {
                    double denom = b[i] + a[i] * alpha[i - 1];
                    alpha[i] = -c[i] / denom;
                    beta[i] = (d[i] - a[i] * beta[i - 1]) / denom;
                }
                m[N - 1] = beta[N - 1];
                for (int i = N - 2; i >= 0; i--)
                    m[i] = alpha[i] * m[i + 1] + beta[i];

                return m;
            }


            double[] secondDerivatives;
            if (!_cache.ContainsKey(arguments))
            {
                secondDerivatives = GetSecondDerivatives(data, arguments);
                _cache[arguments] = secondDerivatives;
            }
            else
            {
                secondDerivatives = (double[])_cache[arguments];
            }

            double m0 = secondDerivatives[idx];
            double m1 = secondDerivatives[idx + 1];
            double h = x1 - x0;


            double t = (x - x0) / h;
            double a00 = 2 * t * t * t - 3 * t * t + 1;
            double a10 = t * t * t - 2 * t * t + t;
            double a01 = -2 * t * t * t + 3 * t * t;
            double a11 = t * t * t - t * t;

            return a00 * y0 + a10 * h * m0 + a01 * y1 + a11 * h * m1;
        }

        protected static Dictionary<object, object> _cache = new Dictionary<object, object>();
        protected double CalculateFrequencyByZeroCrossings2(double[] data, int samplingRate)
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
        protected double[] GetDataWindow(double[] data, int startIndex, int length)
        {
            double[] window = new double[length];
            Array.Copy(data, startIndex, window, 0, length);
            return window;
            
        }
        public string GetPhaseIndicesInfo()
        {
            return $"A (idx:{_phaseAIndex + 1}))";
        }

        protected void FindPhaseVoltageIndices()
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
           

            if (phaseA != null )
            {
                _phaseAIndex = phaseA.ChannelIndex;
               
            }
            else
            {
                throw new Exception("не получилось определить индексы всех трех напряжений");
            }

        }

        protected void ReadAllVoltages()
        {

            PhaseA = _comtradeData.GetAnalogPrimaryChannel(_phaseAIndex).ToList();
            PhaseAFIR = CalculatePhaseAFIR(PhaseA.ToArray());
            PhaseAU =CalculatePhaseAFIR(PhaseA.ToArray());
            double samplingRate = configuration.SampleRates[0].SamplingFrequency;
            for (int i = 0; i < PhaseA.Count; i++)
            {
                DateTime timeStamp = configuration.StartTime.AddSeconds(i / samplingRate);
                TimeStampsOsc.Add(timeStamp);

              
            }
        }
        protected double[] CalculatePhaseAFIR(double[] _PhaseAToFIR)
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
        protected double FIRfltr(double[] phaseA1, int windowsize, int i)
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

        protected void InitializeOscLists()
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

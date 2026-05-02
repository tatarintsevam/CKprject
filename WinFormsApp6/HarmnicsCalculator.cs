using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoltageAnalyzer;

namespace WinFormsApp6
{
    public class HarmnicsCalculator
    {
        private readonly ThreePhaseVoltageAnalyzer _voltageAnalyzer;
        public List<DateTime> TimeStampsHarmonics { get; private set; }
        public List<List<double>> PhaseAHarmonicsAmplitudes { get; private set; }

        public List<List<double>> PhaseAHarmonicsPhases { get; private set; }
        public HarmnicsCalculator(ThreePhaseVoltageAnalyzer voltageAnalyzer)
         
        {
            _voltageAnalyzer = voltageAnalyzer;
            InitializeResultLists();
            CalculateHarmonics();
        }
        protected (List<double> amplitudes, List<double> phases) CalculateHarmonicsForWindow(double[] data,
   int samplingRate)
        {
            Complex[] dft = PerformFFT(data);
            //var dft = data.Select(x => new Complex(x, 0)).ToArray(); Fourier.Forward(dft, FourierOptions.Default);
            List<double> amplitudes = new List<double>();
            List<double> phases = new List<double>();
            // Рассчитываем до 50-й гармоники
            int maxHarmonic = (int)(samplingRate / (2 * _voltageAnalyzer.GetNominalFrequency()));
            for (int i = 1; i <= maxHarmonic; i++)
            {
                int index = (int)(i * _voltageAnalyzer.GetNominalFrequency() * data.Length / samplingRate);
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


            int samplingRate = (int)_voltageAnalyzer.GetConfiguration().SampleRates[0].SamplingFrequency;
            int windowSizePoints = (int)(samplingRate * _voltageAnalyzer.GetWindowSizePeriods() / _voltageAnalyzer.GetNominalFrequency());
            int stepSizePoints = (int)(samplingRate * _voltageAnalyzer.GetStepSizePeriods() / _voltageAnalyzer.GetNominalFrequency());
            double[] phaseAWindow1 = _voltageAnalyzer.GetPhaseAData().ToArray();


            for (int startPoint = 0; startPoint <= _voltageAnalyzer.GetPhaseAData().Count - windowSizePoints; startPoint += stepSizePoints)
            {
                var phaseAWindow = _voltageAnalyzer.GetDataWindow(phaseAWindow1, startPoint, windowSizePoints);


                var (amplitudesA, phaseA) = CalculateHarmonicsForWindow(phaseAWindow, samplingRate);


                int middlePoint = startPoint + windowSizePoints / 2;
                DateTime timeStamp = _voltageAnalyzer.GetComtradeData().Configuration.StartTime.AddSeconds(
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
        protected void InitializeResultLists()
        {



            TimeStampsHarmonics = new List<DateTime>();
            PhaseAHarmonicsAmplitudes = new List<List<double>>();

            PhaseAHarmonicsPhases = new List<List<double>>();

        }
    }
}

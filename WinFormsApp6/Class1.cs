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



        protected double[] PhaseAFIR;
        private List<PhaseVoltageCandidate> _phaseVoltageCandidates;

        protected const double TWO_PI = 2 * Math.PI;

        protected RecordReader _comtradeData;
        protected ConfigurationHandler configuration;

        protected int _phaseAIndex;
        protected double _nominalFrequency;
        public double GetNominalFrequency() => configuration.Frequency;
        public string GetStationName() => configuration.StationName;    
        public double GetSamplingRate() => (int)configuration.SampleRates[0].SamplingFrequency;
        public List<double> GetPhaseAData() => PhaseA;

        public double[] GetPhaseAFIR() => PhaseAFIR;
        public RecordReader GetComtradeData() => _comtradeData;
        public ConfigurationHandler GetConfiguration() => configuration;
        public int GetWindowSizePeriods() => _windowSizePeriods;
        public int GetStepSizePeriods() => _stepSizePeriods;

        public List<double> PhaseA { get; private set; }


        protected int _windowSizePeriods;
        protected int _stepSizePeriods;



        // Добавляем новые списки для хранения RMS значений

        public List<DateTime> TimeStampsOsc { get; private set; }
  

        // Новые свойства для хранения результатов гармоник




        public ThreePhaseVoltageAnalyzer(string comtradeFilePath, int windowSizePeriods, int stepSizePeriods)
        {

            _windowSizePeriods = windowSizePeriods;
            _stepSizePeriods = stepSizePeriods;


            configuration = new ConfigurationHandler(comtradeFilePath);

     
            _comtradeData = new RecordReader(comtradeFilePath);
 
            FindPhaseVoltageIndices();

          
            _nominalFrequency = configuration.Frequency;

            InitializeOscLists();
            ClearResults();

            ReadAllVoltages();

            //CalculateAll();

        }

        //protected void ClearResults()
        //{
        //    TimeStampsOsc.Clear();

        //    PhaseA.Clear();

        //}

        //protected void CalculateAll()
        //{

        //    CalculateRmsValues();


        //}


        protected void ClearResults()
        {
            TimeStampsOsc.Clear();

            PhaseA.Clear();

        }


        protected void InitializeOscLists()
        {
            TimeStampsOsc = new List<DateTime>();

            PhaseA = new List<double>();

        }

        public double[] GetDataWindow(double[] data, int startIndex, int length)
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

            double samplingRate = configuration.SampleRates[0].SamplingFrequency;
            for (int i = 0; i < PhaseA.Count; i++)
            {
                DateTime timeStamp = configuration.StartTime.AddSeconds(i / samplingRate);
                TimeStampsOsc.Add(timeStamp);

              
            }
        }
        public double[] CalculatePhaseAFIR(double[] _PhaseAToFIR)
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
                phaseA[i] = FIRfltr(phaseA1, 20, i);

            }
            return phaseA;

        }
        public double FIRfltr(double[] phaseA1, int windowsize, int i)
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

        //protected void InitializeOscLists()
        //{
        //    TimeStampsOsc = new List<DateTime>();

        //    PhaseA = new List<double>();
           
        //}


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

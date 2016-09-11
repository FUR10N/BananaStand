using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BananaStand
{
    public class LightChangedEventArgs
    {
        public readonly int Headphone;

        public readonly int Ambient;

        public readonly bool UseHeadphones;

        public LightChangedEventArgs(int headphone, int ambient, bool useHeadphones)
        {
            Headphone = headphone;
            Ambient = ambient;
            UseHeadphones = useHeadphones;
        }
    }

    internal class ArduinoReader
    {
        public event EventHandler<LightChangedEventArgs> LightChanged;

        private SerialPort currentPort;

        private bool requestClose;

        public int Threshold { get; set; } = 150;

        public bool IsListening => currentPort != null;

        private Timer timer;

        private bool isReading;

        private readonly object lockObject = new object();

        public ArduinoReader()
        {
            Start();
        }

        public void Start()
        {
            timer?.Dispose();
            timer = new Timer(Timer_Callback, null, 500, 500);
        }

        public void Stop()
        {
            timer?.Dispose();
            timer = null;
            StopListening();
        }

        private void Timer_Callback(object state)
        {
            ReadValues();
        }

        private async Task StartListening()
        {
            requestClose = false;
            try
            {
                var ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    currentPort = new SerialPort(port, 9600);
                    if (await DetectArduino())
                    {
                        break;
                    }
                    else
                    {
                        currentPort = null;
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        public void StopListening()
        {
            if (currentPort == null)
            {
                return;
            }
            currentPort.Close();
            currentPort = null;
            requestClose = true;
        }

        private async void ReadValues()
        {
            lock (lockObject)
            {
                if (isReading)
                {
                    return;
                }
                isReading = true;
            }
            if (currentPort == null)
            {
                await StartListening();
            }
            var response = await GetNextMessage();
            if (string.IsNullOrEmpty(response) || !response.Contains('#'))
            {
                isReading = false;
                if (response == "close")
                {
                    StopListening();
                }
                return;
            }
            int headphoneLight = 0;
            int ambientLight = 0;
            int count = 0;
            var lightValues = response.Split('#').Skip(1);
            foreach (var pair in lightValues)
            {
                if (string.IsNullOrEmpty(pair))
                {
                    break;
                }
                try
                {
                    var values = pair.Split(';');
                    if (values.Length < 2 || string.IsNullOrEmpty(values[0]) || string.IsNullOrEmpty(values[1]))
                    {
                        break;
                    }
                    headphoneLight += int.Parse(values[0]);
                    ambientLight += int.Parse(values[1]);
                    count++;
                }
                catch (Exception)
                {
                    break;
                }
            }
            headphoneLight /= count;
            ambientLight /= count;

            bool headphonesOnStand = headphoneLight < ambientLight;
            if (headphonesOnStand)
            {
                if (ambientLight < 150)
                {
                    // Low light
                    var delta = ambientLight * .50;
                    var difference = Math.Abs(headphoneLight - ambientLight);
                    if (difference < delta)
                    {
                        headphonesOnStand = false;
                    }
                }
                else
                {
                    headphonesOnStand = headphoneLight < Threshold && headphoneLight < ambientLight;
                }

            }
            LightChanged?.Invoke(this, new LightChangedEventArgs(headphoneLight, ambientLight, !headphonesOnStand));
            isReading = false;
        }

        private async Task<byte?> ReadByteAsync(int timeout = 100)
        {
            while (timeout > 0 && currentPort?.BytesToRead == 0)
            {
                timeout--;
                await Task.Delay(1);
            }
            if (currentPort?.BytesToRead == 0)
            {
                return null;
            }
            return currentPort == null ? null : (byte?)currentPort.ReadByte();
        }

        private async Task<string> GetNextMessage(int timeout = 200)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                string msg = "";
                byte? nextByte;
                while ((nextByte = await ReadByteAsync()) != null)
                {
                    if (sw.ElapsedMilliseconds >= timeout)
                    {
                        break;
                    }
                    msg += Convert.ToChar(nextByte.GetValueOrDefault());
                }
                if (string.IsNullOrEmpty(msg))
                {
                    return "";
                }
                msg += "#";
                return msg.Substring(msg.IndexOf('#'));
            }
            catch (Exception ex)
            {
                return "close";
            }
        }

        private async Task<bool> Handshake()
        {
            var msg = await GetNextMessage();
            if (msg.Contains('#'))
            {
                return true;
            }
            return false;
        }

        private async Task<bool> DetectArduino()
        {
            Debug.WriteLine("DetectArduino Entry");
            try
            {
                currentPort.Handshake = System.IO.Ports.Handshake.None;
                currentPort?.Open();
                currentPort.RtsEnable = true;
                currentPort.DtrEnable = true;
                if (await Handshake())
                {
                    Debug.WriteLine("DetectArduino Exit");
                    return true;
                }
                currentPort?.Close();
                Debug.WriteLine("DetectArduino Exit");
                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine("DetectArduino Exit");
                return false;
            }
        }
    }
}

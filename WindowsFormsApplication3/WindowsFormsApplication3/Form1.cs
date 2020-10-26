using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;
//using Accord;
using System.Threading;
namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        private SerialPort myComPort = new SerialPort();
        private int time;
        private bool sleeping;
        private bool clicked;

        private static Random rnd = new Random();
        private static double audioValueMax = 0;
        private static double audioValueLast = 0;
        private static int audioCount = 0;
        private static int RATE = 44100;
        private static int BUFFER_SAMPLES = (int)Math.Pow(2, 11);
        private bool zactive = false;
        private bool zsent = false;
        private int twait = 10;
        private WaveInEvent waveIn = new WaveInEvent();
        public BufferedWaveProvider bwp;
        private const int minLength = 2;
        private const int maxLength = 16384;
        private const int minBits = 1;
        private const int maxBits = 14;
        private static int[][] reversedBits = new int[maxBits][];
        private static System.Numerics.Complex[,][] complexRotation = new System.Numerics.Complex[maxBits, 2][];
        private bool leagueActive = false;
        private Class2 data = Class2.init();
        private string curr = "0";
        private bool off = false;
        private bool initialized = false;
        public Form1()
        {

            
            InitializeComponent();
            waveIn.DeviceNumber = 0; // Use default input device TODO: Identify specifically Stereo mix.
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(RATE, 1);
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.BufferMilliseconds = (int)((double)BUFFER_SAMPLES / (double)RATE * 1000.0);
            bwp = new BufferedWaveProvider(waveIn.WaveFormat);
            bwp.BufferLength = BUFFER_SAMPLES * 2;
            bwp.DiscardOnBufferOverflow = true;
            this.waveIn.StartRecording();
            

            int[] bitRates = new int[10];
            string[] nameArray;

            // Find the COM ports on the system.
            clicked = false;
            nameArray = SerialPort.GetPortNames();
            Array.Sort(nameArray);

            // Fill a combo box with the port names.

            cmbPorts.DataSource = nameArray;
            cmbPorts.DropDownStyle = ComboBoxStyle.DropDownList;

            // Select a default port.

            cmbPorts.SelectedIndex = 1;
            
            time = 0;
            timer1.Start();
            System.Diagnostics.Debug.WriteLine("Initialized...");
        }

        private void OnDataAvailable(object sender, WaveInEventArgs args)
        {

            double max = 0;
            WaveInEventArgs bytes = args;
            bwp.AddSamples(args.Buffer, 0, args.BytesRecorded);
            
            
            byte[] buffer = args.Buffer;
            int bytesRecorded = args.BytesRecorded;
            int bufferIncrement = waveIn.WaveFormat.BlockAlign;
            int frameSize = BUFFER_SAMPLES;
            var audioBytes = new byte[frameSize];

            bwp.Read(audioBytes, 0, frameSize);
            int graphPointCount = audioBytes.Length / 2;
            double[] fft = new double[graphPointCount];
            double[] pcm = new double[graphPointCount];
            double[] fftReal = new double[graphPointCount / 2];
            if (audioBytes.Length == 0)
                return;
            if (audioBytes[frameSize - 2] == 0)
                return;
            for (int i = 0; i < graphPointCount; i++)
            {
                // read the int16 from the two bytes
                Int16 val = BitConverter.ToInt16(audioBytes, i * 2);

                // store the value in Ys as a percent (+/- 100% = 200%)
                pcm[i] = (double)(val) / Math.Pow(2, 16) * 200.0;
            }
            fft = FFT(pcm);
            double pcmPointSpacingMs = RATE / 1000;
            double fftMaxFreq = RATE / 2;
            double fftPointSpacingHz = fftMaxFreq / graphPointCount;
            
            Array.Copy(fft, fftReal, fftReal.Length);
            
            int l = 0;

            // average first 50 lines of FFT
            for (int index = 0; index < fftReal.Length; index++)
            {
                if (index <= 50)
                {
                    max = max + fftReal[index];
                    l++;
                }

            }
            max = max / l; 
            // find peaks.
            if (max > audioValueMax)
            {
                audioValueMax = max;
            }
            audioValueLast = max;
            audioCount += 1;
        }

        public void OpenComPort()
        {
            try
            {
                if (!myComPort.IsOpen)
                {
                    myComPort.PortName = cmbPorts.SelectedItem.ToString(); // Get the selected COM port from the combo box.

                    myComPort.BaudRate = 57600;

                    // Set other port parameters.
                    myComPort.DataBits = 8;
                    myComPort.Parity = Parity.None;
                    myComPort.StopBits = StopBits.One;
                    myComPort.Handshake = Handshake.None;
                    myComPort.Encoding = System.Text.Encoding.Default;
                    myComPort.ReadTimeout = 10000;
                    myComPort.WriteTimeout = 5000;
                    myComPort.RtsEnable = true;
                    myComPort.DtrEnable = true;


                    myComPort.Open();
                    sleeping = false;
                    System.Diagnostics.Debug.WriteLine("Connection Established");
                    if (!initialized)
                    {
                        initialized = true;
                        while (!myComPort.IsOpen) { }
                        zactive = false;
                        byte[] i = new byte[1];
                        i[0] = Convert.ToByte(255);
                        myComPort.Write(i, 0, 1);
                        SendCommand("1");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }

            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message);
            }

            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void CloseComPort()
        {
            try
            {
                using (myComPort)
                    if ((!(myComPort == null)))
                    {
                        // The COM port exists.
                        if (myComPort.IsOpen)
                        {
                            // Wait for the transmit buffer to empty.
                            while ((myComPort.BytesToWrite > 0))
                            {
                            }
                            myComPort.Close();
                            System.Diagnostics.Debug.WriteLine("Disconnected.");
                        }
                    }
            }
            catch (UnauthorizedAccessException ex)
            {
            }
        }

        /*
        Unused.
            
        */
        public void resetArduino()
        {
            myComPort.DtrEnable = true;
            if (!myComPort.IsOpen)
                myComPort.Open();
            Thread.Sleep(1000);
            myComPort.DtrEnable = false;
        }
        public void SendCommand(string command)
        {
            try
            {
                
                if (command == "Z")
                {
                    zactive = true;
                    zsent = true;
                    
                }
                if (command != "Z" && zactive == true)
                {
                    myComPort.DiscardOutBuffer();
                    myComPort.Close();
                    OpenComPort();
                    while (!myComPort.IsOpen) { }
                    zactive = false;
                    byte[] i = new byte[1];
                    i[0] = Convert.ToByte(255);
                    myComPort.Write(i,0,1);

                    
                }
                try { myComPort.WriteLine(command); }
                catch (IOException ex)
                {
                    sleep();
                    //MessageBox.Show(ex.Message);
                    unsleep();
                }
                

                System.Diagnostics.Debug.WriteLine("Sent Command: " + command);
            }


            catch (TimeoutException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void colorchange(Color c)
        {
            string cc;

            cc = string.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
            unsleep();
            SendCommand("RGB " + cc);
            
        }
        private void sleep()
        {
            if (sleeping == false)
            {
                CloseComPort();
                sleeping = true;
                timer1.Stop();
            }
        }

        private void unsleep()
        {
            if (sleeping == true)
            {
                timer1.Start();
                OpenComPort();
                sleeping = false;
                time = 0;
            }
            else
                time = 0;// reset timer
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string Incoming;
            if (!data.active)
            {
                data = Class2.init();
            }
            if (leagueActive) {

                
                GameState g = data.gameState;
                if (g.ActivePlayer != null)
                {
                    float hp = g.ActivePlayer.Stats.CurrentHealth;
                    //bool ded = g.ActivePlayer.IsDead;
                    float maxhp = g.ActivePlayer.Stats.MaxHealth;
                    float mana = g.ActivePlayer.Stats.ResourceValue;
                    float maxmana = g.ActivePlayer.Stats.ResourceMax;
                    string manat = g.ActivePlayer.Stats.ResourceType;
                    if ((hp / maxhp) < .3)
                    {

                        if (curr != "K")
                            SendCommand("K");
                        curr = "K";
                    }
                    else if ((mana / maxmana) < .3 && (manat == "MANA" || manat == "ENERGY"))
                    {
                        if (curr != "M")
                            SendCommand("M");
                        curr = "M";
                    }
                    else
                    {
                        if (curr != "1")
                            SendCommand("1");
                        curr = "1";
                    }
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("null");
                    if (curr != "1")
                        SendCommand("1");
                    curr = "1";
                }
               
            }
            else { 
            
                if (!zactive)
                {
                    time = time + 1;
                
                    if (time > 30)
                    {
                        sleep();
                        System.Diagnostics.Debug.WriteLine("sleep mode");
                        time = 0;
                        sleeping = true;
                    }
                } else
                {
                    if (zsent && twait > 0)
                    {
                        twait--;
                        if (twait == 0)
                        {
                            twait = 10;
                            zsent = false;
                        }
                    }
                    else
                    {
                        double i = audioValueLast * 1000;
                        byte[] output = new byte[1];
                        int x = mapEnforce((int)i, 0, 255, 0, 254);
                        output[0] = Convert.ToByte(x);
                        try {
                            myComPort.Write(output, 0, 1);
                        }
                        catch (TimeoutException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        catch (InvalidOperationException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        //System.Diagnostics.Debug.WriteLine("DataStream: " + output[0].ToString() +" ival: " +i);
                    }
                }
            }
            // Diagnostics.Debug.WriteLine("Timer: {0}", time)
            try
            {
                if (myComPort.IsOpen)
                {
                    Incoming = myComPort.ReadExisting();
                    if (Incoming != null && Incoming != "")
                        System.Diagnostics.Debug.WriteLine(Incoming);
                }
            }
            catch (TimeoutException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: Serial Port read timed out.");
            }
        }

        /// <summary>
        /// map integer values between a specific range of values and enforce them to remain in constraints
        /// example: mapEnforce(21, 0, 255, 255, 0) // returns 234
        /// example enforced: mapEnforce(300,0,255,0,1024) // returns 1024
        /// </summary>
        /// <param name="value">value to map</param>
        /// <param name="fromLow">lowest value</param>
        /// <param name="fromHigh">highest value</param>
        /// <param name="toLow">lowest mapped value</param>
        /// <param name="toHigh">highest mapped value</param>
        /// <returns>int mapped value</returns>
        private static int mapEnforce(int value, int fromLow, int fromHigh, int toLow, int toHigh)
        {
            if (value > fromHigh) value = fromHigh;
            if (value < fromLow) value = fromLow;
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            unsleep();
            SendCommand("-");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string send = "";
            bool doSend = true;
            unsleep();
            leagueActive = false;
            if (listBox1.SelectedIndex == 9)
                send = "K";
            else if (listBox1.SelectedIndex == 10)
                send = "Z";
            else if (listBox1.SelectedIndex == 11) { 
                leagueActive = true;
                doSend = false;
            }
            else
                send = System.Convert.ToString(listBox1.SelectedIndex + 1);
            if (doSend)
                SendCommand(send);
        }

        private void cmbPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            unsleep();
            CloseComPort();
            myComPort.PortName = cmbPorts.SelectedItem.ToString();
            OpenComPort();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            unsleep();
            SendCommand("C");
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ColorDialog cDialog = new ColorDialog();
            cDialog.Color = Color.Red;

            if ((cDialog.ShowDialog() == DialogResult.OK))
                colorchange(cDialog.Color);
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void TrackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            clicked = true;
        }

        private void TrackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            if (clicked)
            {
                string xx;
                xx = TrackBar1.Value.ToString();
                unsleep();
                SendCommand("B " + xx);
                clicked = false;
            }
        }
        public double[] FFT(double[] data)
        {
            double[] fft = new double[data.Length];
            System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[data.Length];
            for (int i = 0; i < data.Length; i++)
                fftComplex[i] = new System.Numerics.Complex(data[i], 0.0);
            aFFT(fftComplex, 1);
            for (int i = 0; i < data.Length; i++)
                fft[i] = fftComplex[i].Magnitude;
            return fft;
        }
        public static void aFFT(System.Numerics.Complex[] data, int direction)
        {
            int n = data.Length;
            int m = Log2(n);

            // reorder data first
            ReorderData(data);

            // compute FFT
            int tn = 1, tm;

            for (int k = 1; k <= m; k++)
            {
                System.Numerics.Complex[] rotation = GetComplexRotation(k, direction);

                tm = tn;
                tn <<= 1;

                for (int i = 0; i < tm; i++)
                {
                    System.Numerics.Complex t = rotation[i];

                    for (int even = i; even < n; even += tn)
                    {
                        int odd = even + tm;
                        System.Numerics.Complex ce = data[even];
                        System.Numerics.Complex co = data[odd];

                        double tr = co.Real * t.Real - co.Imaginary * t.Imaginary;
                        double ti = co.Real * t.Imaginary + co.Imaginary * t.Real;

                        data[even] += new System.Numerics.Complex(tr, ti);
                        data[odd] = new System.Numerics.Complex(ce.Real - tr, ce.Imaginary - ti);
                    }
                }
            }

            if (direction == 1)
            {
                for (int i = 0; i < data.Length; i++)
                    data[i] /= (double)n;
            }
        }
        private static int[] GetReversedBits(int numberOfBits)
        {
            if ((numberOfBits < minBits) || (numberOfBits > maxBits))
                throw new ArgumentOutOfRangeException();

            // check if the array is already calculated
            if (reversedBits[numberOfBits - 1] == null)
            {
                int n = Pow2(numberOfBits);
                int[] rBits = new int[n];

                // calculate the array
                for (int i = 0; i < n; i++)
                {
                    int oldBits = i;
                    int newBits = 0;

                    for (int j = 0; j < numberOfBits; j++)
                    {
                        newBits = (newBits << 1) | (oldBits & 1);
                        oldBits = (oldBits >> 1);
                    }
                    rBits[i] = newBits;
                }
                reversedBits[numberOfBits - 1] = rBits;
            }
            return reversedBits[numberOfBits - 1];
        }
        private static System.Numerics.Complex[] GetComplexRotation(int numberOfBits, int direction)
        {
            int directionIndex = (direction == 1) ? 0 : 1;

            // check if the array is already calculated
            if (complexRotation[numberOfBits - 1, directionIndex] == null)
            {
                int n = 1 << (numberOfBits - 1);
                double uR = 1.0;
                double uI = 0.0;
                double angle = System.Math.PI / n * (int)direction;
                double wR = System.Math.Cos(angle);
                double wI = System.Math.Sin(angle);
                double t;
                System.Numerics.Complex[] rotation = new System.Numerics.Complex[n];

                for (int i = 0; i < n; i++)
                {
                    rotation[i] = new System.Numerics.Complex(uR, uI);
                    t = uR * wI + uI * wR;
                    uR = uR * wR - uI * wI;
                    uI = t;
                }

                complexRotation[numberOfBits - 1, directionIndex] = rotation;
            }
            return complexRotation[numberOfBits - 1, directionIndex];
        }
        private static void ReorderData(System.Numerics.Complex[] data)
        {
            int len = data.Length;

            // check data length
            if ((len < minLength) || (len > maxLength) || (!IsPowerOf2(len)))
                throw new ArgumentException("Incorrect data length.");

            int[] rBits = GetReversedBits(Log2(len));

            for (int i = 0; i < len; i++)
            {
                int s = rBits[i];

                if (s > i)
                {
                    System.Numerics.Complex t = data[i];
                    data[i] = data[s];
                    data[s] = t;
                }
            }
        }
        public static int Log2(int x)
        {
            if (x <= 65536)
            {
                if (x <= 256)
                {
                    if (x <= 16)
                    {
                        if (x <= 4)
                        {
                            if (x <= 2)
                            {
                                if (x <= 1)
                                    return 0;
                                return 1;
                            }
                            return 2;
                        }
                        if (x <= 8)
                            return 3;
                        return 4;
                    }
                    if (x <= 64)
                    {
                        if (x <= 32)
                            return 5;
                        return 6;
                    }
                    if (x <= 128)
                        return 7;
                    return 8;
                }
                if (x <= 4096)
                {
                    if (x <= 1024)
                    {
                        if (x <= 512)
                            return 9;
                        return 10;
                    }
                    if (x <= 2048)
                        return 11;
                    return 12;
                }
                if (x <= 16384)
                {
                    if (x <= 8192)
                        return 13;
                    return 14;
                }
                if (x <= 32768)
                    return 15;
                return 16;
            }

            if (x <= 16777216)
            {
                if (x <= 1048576)
                {
                    if (x <= 262144)
                    {
                        if (x <= 131072)
                            return 17;
                        return 18;
                    }
                    if (x <= 524288)
                        return 19;
                    return 20;
                }
                if (x <= 4194304)
                {
                    if (x <= 2097152)
                        return 21;
                    return 22;
                }
                if (x <= 8388608)
                    return 23;
                return 24;
            }
            if (x <= 268435456)
            {
                if (x <= 67108864)
                {
                    if (x <= 33554432)
                        return 25;
                    return 26;
                }
                if (x <= 134217728)
                    return 27;
                return 28;
            }
            if (x <= 1073741824)
            {
                if (x <= 536870912)
                    return 29;
                return 30;
            }
            return 31;
        }
        public static int Pow2(int power)
        {
            return ((power >= 0) && (power <= 30)) ? (1 << power) : 0;
        }
        public static bool IsPowerOf2(int x)
        {
            return (x > 0) ? ((x & (x - 1)) == 0) : false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}

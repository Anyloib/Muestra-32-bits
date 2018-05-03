using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio;
using NAudio.Wave;
using NAudio.Dsp;


namespace Grabacion
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WaveIn wavein;
        WaveFormat formato;
        WaveFileWriter writer;
        AudioFileReader reader;
        WaveOutEvent waveOut;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void btniniciar_Click(object sender, RoutedEventArgs e)
        {
            
            wavein = new WaveIn();
            wavein.WaveFormat = new WaveFormat(44100, 16, 1);
            formato = wavein.WaveFormat;

            wavein.DataAvailable += OnDaraAvailable;
            wavein.RecordingStopped += OnRectodingStopped;
            writer =
                new WaveFileWriter("sonido.wav", formato);

            wavein.StartRecording();
        }

        void OnRectodingStopped(object sender, StoppedEventArgs e)
        {
            writer.Dispose();
        }

        void OnDaraAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesGrabados = e.BytesRecorded;

            double acumulador = 0;

            double nummuestras = bytesGrabados / 2;
            int exponente = 1;
            int numeroMuestraComplejas = 0;
            int bitsMaximas = 0;

            do //1200
            {
                bitsMaximas = (int) Math.Pow(2, exponente);
                exponente++;
            } while (bitsMaximas < nummuestras);

            //bitsMaximas = 2048
            //exponente = 12

            //numeroMuestraComplejas = 1024
            //exponente = 10

            exponente += 2;
            numeroMuestraComplejas = bitsMaximas / 2;

            Complex[] muestrasCompletas = new Complex[numeroMuestraComplejas]; 

            for (int i=0; i < bytesGrabados; i+=2)
            {
                short muestra = (short)(buffer[i + 1] << 8 | buffer[i]);

                float muestra32bits = (float)muestra / 32768.0f;
                slbvolumen.Value = Math.Abs(muestra32bits);
                if (i / 2 < numeroMuestraComplejas)
                {
                    muestrasCompletas[i / 2].X = muestra32bits;
                }


                //acumulador += muestra;
                //nummuestras++;
            }
            //double promedio = acumulador / nummuestras;
            //slbvolumen.Value = promedio;
            //writer.Write(buffer, 0, bytesGrabados);

            FastFourierTransform.FFT(true, exponente, muestrasCompletas);
            float[] valoresAbsolutos =
                new float[muestrasCompletas.Length];
           for(int i =0; i < muestrasCompletas.Length; i++)
            {
                valoresAbsolutos[i] = (float)Math.Sqrt((muestrasCompletas[i].X * muestrasCompletas[i].X) -
                    (muestrasCompletas[i].Y * muestrasCompletas[i].Y));
            }

           
        }

        private void btnfinalizar_Click(object sender, RoutedEventArgs e)
        {
            wavein.StopRecording();
        }

        private void btnReproducir_Click(object sender, RoutedEventArgs e)
        {
            reader = new AudioFileReader("sonido.wav");
            waveOut = new WaveOutEvent();
            waveOut.Init(reader);
            waveOut.Play();
        }

        private void slbvolumen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}

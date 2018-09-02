using System.Threading;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.Netduino;
using Microsoft.SPOT.Hardware;

namespace MQ3
{
    public class Program
    {
        public static void Main()
        {
            //create analog input
            AnalogInput MQ3sensor = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);

            //first run we need to calibrate sensors zeropoint in cleanair
            bool calibrating = true;
            double zeroPoint = 0;

            while (true)
            {
                // read the analog input
                int analogInputValue = MQ3sensor.ReadRaw();

                // output
                if (calibrating || analogInputValue<zeroPoint)
                {
                    Debug.Print("Calibrating...Raw input:" + analogInputValue);
                    zeroPoint = analogInputValue;
                    calibrating = false;
                }
                else
                {
                    Debug.Print("Alcohollevel: " + ugl(analogInputValue, calibrating, zeroPoint) + " µg/L" +
                                ", Raw input: " + analogInputValue.ToString());
                }
                // wait 1/4 second
                Thread.Sleep(250);
            }
        }
        

        //Calculate µg/L
        public static double ugl(double analogValue,bool calibrating, double zeroPoint)
        {
            //Now a little math...
            //The max value is 10 mg/L and the least value is 0.1 mg/L what can be measured
            //We have 12 bits, so there are 4096 possible values
            //When we use ((Max value-Least value)/possible values) * result + least value then we should get the right value
            //Least measured value: (9.9/4096.0) * 0 (Binairy 000000000000) + 0.1 = 0.1  mg/L
            //Max measured value: (9.9/4096) * 1024(Binairy 111111111111) + 0.1 = 10 mg/L

            double concentration = (9.9 / 4096.0) * analogValue + 0.1;

            //Now subtract zeroPoint from the concentration to remove unwanted measured gasses (the mq3 sensor measures different gasses, so that's why we need to calibrate in clean air)
            if (!calibrating)
            {
                double cleanAirConcentration = (9.9 / 4096.0) * zeroPoint + 0.1;
                concentration = concentration - cleanAirConcentration;
            }
            //now we have the result in mg/L but we need µg/L
            concentration = concentration * 1000;
            return concentration;
        }
    }
}
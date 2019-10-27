using SourceAFIS.Simple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Bionic
{
    class Program
    {
        private static string picturePath = "D:\\Card\\Data";
        private static string templatePath = "D:\\Card\\Data\\template";
        private static AfisEngine afisEngine = new AfisEngine();

        static void Main(string[] args)
        {
            Console.WriteLine("LibScanApi Demo");

            var accessor = new DeviceAccessor();
         
            // var cr = accessor.AccessCardReader();

            // cr.CardDetected += (sender, eventArgs) => Console.WriteLine($"Card detected: {eventArgs.SerialNumber:X10}");
            // cr.CardRemoved += (sender, eventArgs) => Console.WriteLine("Card removed");

            using (var device = accessor.AccessFingerprintDevice())
            {
                device.SwitchLedState(false, false);

                device.FingerDetected += (sender, eventArgs) =>
                {
                    Console.WriteLine("Finger Detected!");

                    device.SwitchLedState(true, false);

                    // Save fingerprint to temporary folder
                    var fingerprint = device.ReadFingerprint();
                    var tempFile = picturePath + "\\futronic.bmp";
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }

                    fingerprint.Save(tempFile);

                    Console.WriteLine("Saved to " + tempFile);

                    /*try
                    {
                        Fingerprint fp = new Fingerprint();
                        fp.AsBitmapSource = new BitmapImage(new Uri(tempFile, UriKind.RelativeOrAbsolute));
                        Person ps = new Person();
                        ps.Fingerprints.Add(fp);
                        afisEngine.Extract(ps);
                        File.WriteAllBytes(templatePath + "\\futronic" + ".tmpl", fp.AsIsoTemplate);

                        Console.WriteLine("Template saved successfully !");

                        // TODO Notify to RabbitMQ
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Fingerprint Extraction: " + ex.Message);
                    }*/

                    Fingerprint strfp = new Fingerprint();
                    strfp.AsIsoTemplate = File.ReadAllBytes(templatePath + "\\futronic" + ".tmpl");
                    Person storedPerson = new Person(strfp);

                    try
                    {
                        Fingerprint fp = new Fingerprint();
                        fp.AsBitmapSource = new BitmapImage(new Uri(tempFile, UriKind.RelativeOrAbsolute));
                        Person scannedPerson = new Person();
                        scannedPerson.Fingerprints.Add(fp);
                        afisEngine.Extract(scannedPerson);

                        float verify = afisEngine.Verify(storedPerson, scannedPerson);
                        if (verify != 0) {
                            Console.WriteLine("Success : " + verify);
                        } else {
                            Console.WriteLine("Failed : " + verify);
                        }
                        
                        // TODO Notify to RabbitMQ
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Fingerprint Extraction: " + ex.Message);
                    }
                };

                device.FingerReleased += (sender, eventArgs) =>
                {
                    Console.WriteLine("Finger Released!");

                    device.SwitchLedState(false, true);
                };

                Console.WriteLine("FingerprintDevice Opened");

                device.StartFingerDetection();
                device.SwitchLedState(false, true);

                Console.ReadLine();

                Console.WriteLine("Exiting...");

                device.SwitchLedState(false, false);
                device.Dispose();
            }
        }
    }
}
